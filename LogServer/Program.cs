using Core.Net;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LogServer
{
	class Program
	{
		static int Main( string[] args )
		{
			Server server = new Server();
			Console.ReadLine();
			return 0;
		}
	}

	class Server
	{
		private const int PORT = 23000;

		private readonly Socket _socket;

		public Server()
		{
			this._socket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
			this._socket.Bind( new IPEndPoint( IPAddress.Any, PORT ) );
			Console.WriteLine( $"Log server started, listening port: {PORT}" );
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
			catch ( SocketException e )
			{
				Console.WriteLine( e.ToString() );
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
			if ( receiveEventArgs.SocketError != SocketError.Success ||
				 receiveEventArgs.BytesTransferred < 0 )
			{
				this.StartReceive( receiveEventArgs );
				return;
			}

			string message = Encoding.UTF8.GetString( receiveEventArgs.Buffer, receiveEventArgs.Offset, receiveEventArgs.BytesTransferred );
			this.HandleMessage( receiveEventArgs, message );
			this.StartReceive( receiveEventArgs );
		}

		void HandleMessage( SocketAsyncEventArgs receiveEventArgs, string message )
		{
			if ( message.StartsWith( "RPC" ) )
			{
				string method = message.Substring( 4 );
				this._socket.SendTo( Encoding.UTF8.GetBytes( method ), receiveEventArgs.RemoteEndPoint );
			}
			else
				Console.WriteLine( message );
		}
	}
}
