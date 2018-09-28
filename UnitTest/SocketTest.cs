using Core.Net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using Core.Structure;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest
{
	public class SocketTest
	{
		private readonly ITestOutputHelper _output;

		private readonly SwitchQueue<byte[]> _data = new SwitchQueue<byte[]>();

		private readonly byte[] _socketBuffer;
		private readonly Dictionary<Socket, int> _socketToId = new Dictionary<Socket, int>();
		private int _disconnectCount;

		public SocketTest( ITestOutputHelper output )
		{
			this._socketBuffer = new byte[8 * 512];
			this._output = output;
		}

		private void Log( string msg )
		{
			this._output.WriteLine( msg );
		}

		[Fact]
		public void RPCTest()
		{
			Dictionary<string, MethodInfo> r2cToMethod = new Dictionary<string, MethodInfo>();

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach ( Assembly assembly in assemblies )
			{
				Type[] types = assembly.GetTypes();
				foreach ( Type type in types )
				{
					MethodInfo[] methodInfos = type.GetMethods( BindingFlags.NonPublic | BindingFlags.Instance );
					foreach ( MethodInfo methodInfo in methodInfos )
					{
						R2CAttribute attr = methodInfo.GetCustomAttribute<R2CAttribute>();
						if ( attr != null )
							r2cToMethod[attr.name] = methodInfo;
					}
				}
			}

			UdpClient client = new UdpClient( "127.0.0.1", 23000 );
			client.BeginReceive( this.OnReceived, client );
			byte[] bytes = Encoding.UTF8.GetBytes( "RPC_Test" );
			client.Send( bytes, bytes.Length );

			while ( true )
			{
				this._data.Switch();
				while ( !this._data.isEmpty )
				{
					byte[] data = this._data.Pop();
					string message = Encoding.UTF8.GetString( data );
					this._output.WriteLine( message );
					if ( r2cToMethod.TryGetValue( message, out MethodInfo methodInfo ) )
						methodInfo.Invoke( this, null );
				}
				Thread.Sleep( 50 );
			}
		}

		private void OnReceived( IAsyncResult ar )
		{
			UdpClient client = ( UdpClient )ar.AsyncState;
			IPEndPoint endPoint = new IPEndPoint( IPAddress.Any, 0 );
			byte[] data = client.EndReceive( ar, ref endPoint );
			if ( data.Length > 0 )
				this._data.Push( data );
		}

		[R2C( "Test" )]
		private void R2C_Test()
		{
			this._output.WriteLine( "R2C_Test invoke" );
		}

		[Fact]
		public void ConnectionTest()
		{
			for ( int i = 0; i < 12; i++ )
			{
				Socket socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
				socket.NoDelay = true;
				this._socketToId[socket] = i;
				this.Connect( socket, "127.0.0.1", 2551 );
			}
			Thread.Sleep( int.MaxValue );
		}

		private void Connect( Socket socket, string ip, int port )
		{
			try
			{
				socket.BeginConnect( ip, port, this.ConnectCallback, socket );
			}
			catch ( SocketException e )
			{
				this.Log( e.ToString() );
			}
		}

		private void ConnectCallback( IAsyncResult asyncConnect )
		{
			Socket socket = ( Socket )asyncConnect.AsyncState;
			try
			{
				socket.EndConnect( asyncConnect );
			}
			catch ( Exception e )
			{
				this.Log( $"{this._socketToId[socket]} connect error:{e}, local:{socket.LocalEndPoint}" );
				return;
			}

			this.Log( $"{this._socketToId[socket]} connected, local:{socket.LocalEndPoint}" );

			this.StartReceive( socket );
		}

		private void StartReceive( Socket socket )
		{
			try
			{
				socket.BeginReceive( this._socketBuffer, 0, 8 * NetworkConfig.BUFFER_SIZE, SocketFlags.None, this.ProcessReceive,
									 socket );
			}
			catch ( SocketException e )
			{
				this.Log( e.ToString() );
				socket.Shutdown( SocketShutdown.Both );
				socket.Close();
			}
		}

		private void ProcessReceive( IAsyncResult ar )
		{
			Socket socket = ( Socket )ar.AsyncState;
			int revCount;
			try
			{
				revCount = socket.EndReceive( ar );
			}
			catch ( SocketException e )
			{
				this.CloseSocket( socket, $"{this._socketToId[socket]} receive error:{e}, local:{socket.LocalEndPoint}" );
				return;
			}

			if ( revCount > 0 )
			{
				this.Log( "Rect:" + revCount );
				this.StartReceive( socket );
			}
			else
			{
				this.CloseSocket( socket, $"{this._socketToId[socket]} receive no data, local:{socket.LocalEndPoint}" );
			}
		}

		private void CloseSocket( Socket socket, string msg )
		{
			this.Log( msg );
			socket.Shutdown( SocketShutdown.Both );
			socket.Close();
			int count = Interlocked.Increment( ref this._disconnectCount );
			this.Log( $"count:{count}" );
		}
	}
}