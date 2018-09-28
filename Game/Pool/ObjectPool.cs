using Core.Misc;
using System.Collections.Generic;

namespace Game.Pool
{
	public class ObjectPool<T> where T : new()
	{
		private readonly Stack<T> _stack = new Stack<T>();

		public int countAll { get; private set; }

		public int countActive => this.countAll - this.countInactive;

		public int countInactive => this._stack.Count;

		public T Get()
		{
			T element;
			if ( this._stack.Count == 0 )
			{
				element = new T();
				this.countAll++;
			}
			else
			{
				element = this._stack.Pop();
			}
			return element;
		}

		public void Release( T element )
		{
			if ( this._stack.Count > 0 && ReferenceEquals( this._stack.Peek(), element ) )
				Logger.Error( "Internal error. Trying to destroy object that is already released to pool." );
			this._stack.Push( element );
		}
	}
}
