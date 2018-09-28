using System.Collections.Generic;

namespace Game.Pool
{
	public static class ListPool<T>
	{
		private static readonly ObjectPool<List<T>> LIST_POOL = new ObjectPool<List<T>>();

		public static List<T> Get()
		{
			return LIST_POOL.Get();
		}

		public static void Release( List<T> toRelease )
		{
			LIST_POOL.Release( toRelease );
		}
	}
}