using System.Collections.Generic;

namespace Game.Pool
{
	public abstract class StackPool<T> where T : new()
	{
		private static readonly Stack<T> POOL = new Stack<T>();

		public static T Get()
		{
			if ( POOL.Count > 0 )
				return POOL.Pop();
			return new T();
		}

		private static void Release( T value )
		{
			POOL.Push( value );
		}

		protected abstract void Reset();

		public void Release()
		{
			this.Reset();
			Release( ( T )( object )this );
		}
	}
}