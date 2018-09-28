using System.Collections.Generic;
using Game.Misc;
using UnityEngine;

namespace Game.Pool
{
	public class GameObjectPool
	{
		private static GameObjectPool _instance;

		public static GameObjectPool instance => _instance ?? ( _instance = new GameObjectPool() );

		private readonly Dictionary<string, SpawnPool> _spawnPools = new Dictionary<string, SpawnPool>();

		protected internal readonly Transform _root;

		private GameObjectPool()
		{
			this._root = new GameObject( "Pool" ).transform;
			//Object.DontDestroyOnLoad( this._root.gameObject );
		}

		public SpawnPool CreateSpawnPool( string id, Transform prefab, int maxCount = -1, int preloadAmount = 1 )
		{
			SpawnPool spawnPool = new SpawnPool( id, prefab, maxCount, preloadAmount, this );
			this._spawnPools.Add( id, spawnPool );
			return spawnPool;
		}

		public SpawnPool GetSpawnPool( string id )
		{
			SpawnPool spawnPool;
			this._spawnPools.TryGetValue( id, out spawnPool );
			return spawnPool;
		}

		public bool HasSpawnPool( string id )
		{
			return this._spawnPools.ContainsKey( id );
		}

		public void DespawnAll()
		{
			Dictionary<string, SpawnPool>.ValueCollection vc = this._spawnPools.Values;
			foreach ( SpawnPool spawnPool in vc )
				spawnPool.DespawnAll();
		}

		public void ClearDespawned()
		{
			List<string> tobeRemoved = new List<string>();
			foreach ( KeyValuePair<string, SpawnPool> kv in this._spawnPools )
			{
				SpawnPool spawnPool = kv.Value;
				spawnPool.DisposeDespawned();
				//如果已经没有已经孵化的对象则移走该spawnPool
				if ( spawnPool.prefab == null )
					tobeRemoved.Add( kv.Key );
			}
			int count = tobeRemoved.Count;
			for ( int i = 0; i < count; i++ )
			{
				string key = tobeRemoved[i];
				this._spawnPools.Remove( key );
			}
		}
	}

	public sealed class SpawnPool
	{
		private readonly List<IPoolGameObject> _despawned = new List<IPoolGameObject>();
		private readonly List<IPoolGameObject> _spawned = new List<IPoolGameObject>();

		public string id { get; private set; }

		public Transform prefab { get; private set; }

		public readonly int preloadAmount;

		public readonly int maxCount;

		private readonly GameObjectPool _pool;

		internal SpawnPool( string id, Transform prefab, int maxCount, int preloadAmount, GameObjectPool pool )
		{
			this.id = id;
			this.prefab = prefab;
			this.preloadAmount = preloadAmount;
			this.maxCount = maxCount;
			this._pool = pool;
			//this.Create();
		}

		internal void Create()
		{
			for ( int i = 0; i < this.preloadAmount; ++i )
				this.InstantiatePrefab();
		}

		private bool InstantiatePrefab()
		{
			int total = this._despawned.Count + this._spawned.Count;
			if ( this.maxCount != -1 && total >= this.maxCount )
				return false;
			GameObject go = Object.Instantiate( this.prefab.gameObject, this.prefab.transform.localPosition, this.prefab.transform.localRotation );
			go.name = this.prefab.name;
			go.SetActive( false );
			Utils.AddChild( this._pool._root, go.transform, false, false, false );
			this._despawned.Add( new PoolGameObject( this, go.transform ) );
			return true;
		}

		public bool CanSpawn()
		{
			if ( this._despawned.Count == 0 )
			{
				int total = this._despawned.Count + this._spawned.Count;
				if ( this.maxCount != -1 && total >= this.maxCount )
					return false;
			}
			return true;
		}

		public IPoolGameObject Spawn( Transform parent = null )
		{
			bool result = true;
			bool createFromPool = true;
			if ( this._despawned.Count == 0 )
			{
				result = this.InstantiatePrefab();
				createFromPool = false;
			}

			if ( !result )
				return null;

			IPoolGameObject po = this._despawned[0];
			po.Active( createFromPool );
			this._despawned.RemoveAt( 0 );
			this._spawned.Add( po );

			if ( parent != null )
			{
				Transform t = po.transform;
				t.parent = parent;
				t.localPosition = Vector3.zero;
				t.localScale = Vector3.one;
				t.localRotation = Quaternion.identity;
			}
			return po;
		}

		public bool Despawn( IPoolGameObject po )
		{
			if ( po == null )
				return false;

			if ( !this._spawned.Contains( po ) )
				return false;

			po.Deactive();
			Utils.AddChild( this._pool._root, po.transform, false, false, false );
			this._spawned.Remove( po );
			this._despawned.Add( po );
			return true;
		}

		public void DespawnAll()
		{
			foreach ( IPoolGameObject po in this._spawned )
			{
				po.Deactive();
				Utils.AddChild( this._pool._root, po.transform, false, false, false );
			}
			this._despawned.AddRange( this._spawned );
			this._spawned.Clear();
		}

		internal void DisposeDespawned()
		{
			foreach ( IPoolGameObject po in this._despawned )
				Object.Destroy( po.transform.gameObject );
			this._despawned.Clear();
			if ( this._spawned.Count == 0 )
				this.prefab = null;
		}

		//internal void Dispose()
		//{
		//	this.DespawnAll();
		//	foreach ( IPoolGameObject po in this._despawned )
		//		Object.Destroy( po.transform.gameObject );
		//	foreach ( IPoolGameObject po in this._spawned )
		//		Object.Destroy( po.transform.gameObject );
		//	this.prefab = null;
		//	this._despawned.Clear();
		//	this._spawned.Clear();
		//}
	}
}