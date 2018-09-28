using System.Collections.Generic;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest
{
	public class ThreadTest
	{
		private readonly ITestOutputHelper _output;
		private readonly object _lockObj = new object();
		private static int _ii;
		private long _t;

		public ThreadTest( ITestOutputHelper output )
		{
			this._output = output;
		}

		[Fact]
		public void AsyncTestAsync()
		{
		}

		[Fact]
		public void LockTest()
		{
			for ( int i = 0; i < 100; i++ )
			{
				Thread t = new Thread( this.Worker );
				t.IsBackground = true;
				t.Start();
			}
		}

		private void Worker( object obj )
		{
			this._output.WriteLine( _ii++.ToString() );
		}

		[Fact]
		public void ListTest()
		{
			List<int> a = new List<int>();

			Thread t1 = new Thread( this.AddToList );
			t1.IsBackground = true;
			t1.Start( a );

			Thread t2 = new Thread( this.ListCount );
			t2.IsBackground = true;
			t2.Start( a );

			Thread.Sleep( int.MaxValue );
		}

		private void AddToList( object obj )
		{
			List<int> a = ( List<int> )obj;
			for ( int i = 0; i < 1000; i++ )
			{
				lock ( this._lockObj )
					a.Add( i );
				Thread.Sleep( 100 );
			}
		}

		private void ListCount( object obj )
		{
			List<int> a = ( List<int> )obj;
			while ( true )
			{
				lock ( this._lockObj )
					this._output.WriteLine( a.Count.ToString() );
				Thread.Sleep( 600 );
			}
		}
	}
}