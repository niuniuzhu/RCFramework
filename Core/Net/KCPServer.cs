﻿using Core.Misc;
using Core.Net.Protocol;
using Core.Structure;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Core.Net
{
	public sealed class KCPServer : INetServer
	{
		public event SocketEventHandler OnSocketEvent;

		private readonly int _maxClient;
		private Socket _socket;
		private SocketEvent? _closeEvent;
		private readonly SwitchQueue<ReceivedData> _receivedDatas = new SwitchQueue<ReceivedData>();
		private readonly NetworkUpdateContext _updateContext = new NetworkUpdateContext();
		private readonly List<KCPUserToken> _tokens = new List<KCPUserToken>();
		private readonly Dictionary<ushort, KCPUserToken> _idToTokens = new Dictionary<ushort, KCPUserToken>();
		private readonly KCPUserTokenPool _userTokenPool = new KCPUserTokenPool();

		internal KCPServer( int maxClient )
		{
			this._maxClient = maxClient;
		}

		internal void SendTo( KCPUserToken token, byte[] data, int offset, int size, EndPoint endPoint )
		{
			if ( this._socket == null )
				return;
			try
			{
				this._socket.SendTo( data, offset, size, SocketFlags.None, endPoint );
			}
			catch ( ObjectDisposedException )
			{

			}
			catch ( SocketException e )
			{
				token.MarkToDisconnect( e.ToString(), e.SocketErrorCode );
			}
		}

		public void Send( ushort tokenId, Packet packet )
		{
			if ( !this._idToTokens.TryGetValue( tokenId, out KCPUserToken token ) )
			{
				Logger.Warn( $"Usertoken {tokenId} not found" );
				return;
			}
			token.Send( packet );
		}

		public void Send( IEnumerable<ushort> tokenIds, Packet packet )
		{
			packet.OnSend();
			byte[] data = NetworkHelper.EncodePacket( packet );
			foreach ( ushort tokenId in tokenIds )
			{
				if ( !this._idToTokens.TryGetValue( tokenId, out KCPUserToken token ) )
				{
					Logger.Warn( $"Usertoken {tokenId} not found" );
					continue;
				}
				token.Send( data );
			}
		}

		public void Dispose()
		{
			this.Stop();
			this._userTokenPool.Dispose();
		}

		public void Stop()
		{
			Socket socket = this._socket;
			lock ( this._receivedDatas )
			{
				if ( this._socket == null )
					return;
				this._socket = null;
				this._receivedDatas.Clear();
			}

			int count = this._tokens.Count;
			for ( int i = 0; i < count; i++ )
			{
				KCPUserToken token = this._tokens[i];
				this.OnSocketEvent?.Invoke( new SocketEvent( SocketEvent.Type.Disconnect, "Server stoped", SocketError.Shutdown, token ) );
				token.Close();
				this._userTokenPool.Push( token );
			}
			this._tokens.Clear();
			this._idToTokens.Clear();

			socket.Shutdown( SocketShutdown.Both );
			socket.Close();
		}

		private void MarkToStop( string msg, SocketError errorCode )
		{
			if ( this._closeEvent != null )
				return;
			this._closeEvent = new SocketEvent( SocketEvent.Type.Close, msg, errorCode, null );
		}

		public void Start( int port )
		{
			this.Stop();
			this._socket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
			this._socket.Bind( new IPEndPoint( IPAddress.Any, port ) );
			this.StartReceive( null );
		}

		private void StartReceive( SocketAsyncEventArgs receiveEventArgs )
		{
			if ( this._socket == null )
				return;

			if ( receiveEventArgs == null )
			{
				receiveEventArgs = new SocketAsyncEventArgs();
				receiveEventArgs.SetBuffer( new byte[NetworkConfig.BUFFER_SIZE], 0, NetworkConfig.BUFFER_SIZE );
				receiveEventArgs.RemoteEndPoint = new IPEndPoint( IPAddress.Any, 0 );
				receiveEventArgs.Completed += this.OnReceiveComplete;
			}
			bool asyncResult;
			try
			{
				asyncResult = this._socket.ReceiveFromAsync( receiveEventArgs );
			}
			catch ( ObjectDisposedException )
			{
				return;
			}
			catch ( SocketException e )
			{
				this.MarkToStop( e.ToString(), e.SocketErrorCode );
				return;
			}
			if ( !asyncResult )
				this.ProcessReceive( receiveEventArgs );
		}

		private void OnReceiveComplete( object sender, SocketAsyncEventArgs receiveEventArgs )
		{
			this.ProcessReceive( receiveEventArgs );
		}

		private void ProcessReceive( SocketAsyncEventArgs receiveEventArgs )
		{
			if ( receiveEventArgs.SocketError == SocketError.Success &&
				 receiveEventArgs.BytesTransferred > 0 )
			{
				lock ( this._receivedDatas )
				{
					if ( this._socket != null )
						this._receivedDatas.Push( new ReceivedData( receiveEventArgs ) );
				}
			}
			this.StartReceive( receiveEventArgs );
		}

		private bool VerifyConnKey( byte[] buffer, ref int offset, ref int size )
		{
			if ( size < NetworkConfig.SIZE_OF_CONN_KEY )
				return false;

			uint key = ByteUtils.Decode32u( buffer, offset );
			if ( key != NetworkConfig.CONN_KEY )
				return false;

			offset += NetworkConfig.SIZE_OF_CONN_KEY;
			size -= NetworkConfig.SIZE_OF_CONN_KEY;
			return true;
		}

		private bool VerifyHandshake( byte[] buffer, ref int offset, ref int size )
		{
			if ( size < NetworkConfig.SIZE_OF_SIGNATURE )
				return false;

			ushort signature = ByteUtils.Decode16u( buffer, offset );
			if ( signature != NetworkConfig.HANDSHAKE_SIGNATURE )
				return false;

			offset += NetworkConfig.SIZE_OF_SIGNATURE;
			size -= NetworkConfig.SIZE_OF_SIGNATURE;
			return true;
		}

		private bool VerifyPeerId( byte[] data, ref int offset, ref int size, ref ushort id )
		{
			if ( size < NetworkConfig.SIZE_OF_PEER_ID )
				return false;

			ByteUtils.Decode16u( data, offset, ref id );
			offset += NetworkConfig.SIZE_OF_PEER_ID;
			size -= NetworkConfig.SIZE_OF_PEER_ID;
			return true;
		}

		private void CheckClientOverRange()
		{
			int over = this._tokens.Count - this._maxClient;
			for ( int i = 0; i < over; i++ )
			{
				KCPUserToken token = this._tokens[this._tokens.Count - 1];
				token.MarkToDisconnect( "Client overrange", SocketError.SocketError );
			}
		}

		private void UpdateClients()
		{
			int count = this._tokens.Count;
			for ( int i = 0; i < count; i++ )
				this._tokens[i].Update( this._updateContext );
		}

		private void CheckClientAlive()
		{
			for ( int i = this._tokens.Count - 1; i >= 0; --i )
				this._tokens[i].CheckAlive();
		}

		private void ProcessReceiveDatas()
		{
			this._receivedDatas.Switch();
			while ( !this._receivedDatas.isEmpty )
			{
				ReceivedData receivedData = this._receivedDatas.Pop();
				byte[] data = receivedData.data;
				int offset = 0;
				int size = data.Length;

				if ( !this.VerifyConnKey( data, ref offset, ref size ) )
					continue;

				if ( this.VerifyHandshake( data, ref offset, ref size ) )
				{
					KCPUserToken newToken = this._userTokenPool.Pop( this );
					this._tokens.Add( newToken );
					this._idToTokens.Add( newToken.id, newToken );
					newToken.OnConnected( receivedData.remoteEndPoint, TimeUtils.utcTime );
					this.OnSocketEvent?.Invoke( new SocketEvent( SocketEvent.Type.Accept,
																 $"Client connection accepted, Remote Address: {receivedData.remoteEndPoint}",
																 SocketError.Success, newToken ) );

					byte[] handshakeAckData = new byte[NetworkConfig.SIZE_OF_CONN_KEY + NetworkConfig.SIZE_OF_SIGNATURE + NetworkConfig.SIZE_OF_PEER_ID];
					int handshakeAckOffset = ByteUtils.Encode32u( handshakeAckData, 0, NetworkConfig.CONN_KEY );
					handshakeAckOffset += ByteUtils.Encode16u( handshakeAckData, handshakeAckOffset, NetworkConfig.HANDSHAKE_SIGNATURE );
					handshakeAckOffset += ByteUtils.Encode16u( handshakeAckData, handshakeAckOffset, newToken.id );
					newToken.SendDirect( handshakeAckData, 0, handshakeAckOffset );
					continue;
				}

				ushort peerId = 0;
				if ( !this.VerifyPeerId( data, ref offset, ref size, ref peerId ) )
				{
					continue;
				}

				if ( !this._idToTokens.TryGetValue( peerId, out KCPUserToken token ) )
				{
					continue;
				}

				token.ProcessData( data, offset, size, packet =>
				{
					if ( packet.module == NetworkConfig.INTERNAL_MODULE && packet.command == 0 )
						token.Send( new PacketHeartBeat( ( ( PacketHeartBeat )packet ).localTime ) );
					else
						this.OnSocketEvent?.Invoke( new SocketEvent( SocketEvent.Type.Receive, packet, token ) );
				} );
			}
		}

		public void Update( long dt )
		{
			this._updateContext.deltaTime = dt;
			this._updateContext.time += dt;

			this.ProcessReceiveDatas();
			this.CheckClientOverRange();
			this.CheckClientAlive();
			this.UpdateClients();

			int count = this._tokens.Count;
			for ( int i = 0; i < count; i++ )
			{
				KCPUserToken token = this._tokens[i];
				if ( token.disconnectEvent == null )
					continue;
				this.OnSocketEvent?.Invoke( token.disconnectEvent.Value );
				token.Close();
				this._tokens.RemoveAt( i );
				this._idToTokens.Remove( token.id );
				this._userTokenPool.Push( token );
				--i;
				--count;
			}
			if ( this._closeEvent != null )
			{
				this.OnSocketEvent?.Invoke( this._closeEvent.Value );
				this._closeEvent = null;
				this.Stop();
			}
		}
	}
}