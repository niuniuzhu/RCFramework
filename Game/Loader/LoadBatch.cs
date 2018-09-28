using Core.Misc;
using System.Collections.Generic;

namespace Game.Loader
{
	public class LoadBatch
	{
		private readonly List<IBatchLoader> _loaders = new List<IBatchLoader>();
		private readonly Dictionary<IBatchLoader, float> _progress = new Dictionary<IBatchLoader, float>();

		public BatchSingleCompleteHandler singleCompleteHandler;
		public CompleteHandler completeHandler;
		public BatchProgressHandler progressHandler;
		public ErrorHandler errorHandler;

		private readonly bool _useWWW;
		private readonly bool _sync;
		private readonly bool _fromCache;
		private bool _started;

		public int numItems => this._loaders.Count;

		public int numLoaded { get; private set; }

		public object data;

		public LoadBatch( bool useWWW = false, bool fromCache = true, bool sync = false, object data = null )
		{
			this._useWWW = useWWW;
			this._fromCache = fromCache;
			this._sync = sync;
			this.data = data;
		}

		public void Add( IBatchLoader loader )
		{
			if ( this._started )
			{
				Logger.Warn( "Loader must be added before start" );
				return;
			}

			if ( this._progress.ContainsKey( loader ) )
				return;

			this._loaders.Add( loader );
			this._progress[loader] = 0f;
		}

		public void Remove( IBatchLoader loader )
		{
			if ( this._started )
			{
				Logger.Warn( "Loader must be removed before start" );
				return;
			}

			if ( !this._progress.ContainsKey( loader ) )
				return;

			this._loaders.Remove( loader );
			this._progress.Remove( loader );
		}

		public void Start( CompleteHandler completeHandler, BatchProgressHandler progressHandler, ErrorHandler errorHandler,
						   BatchSingleCompleteHandler singleCompleteHandler )
		{
			if ( this._started )
				return;

			this._started = true;
			this.completeHandler = completeHandler;
			this.progressHandler = progressHandler;
			this.errorHandler = errorHandler;
			this.singleCompleteHandler = singleCompleteHandler;

			this.numLoaded = 0;
			int count = this._loaders.Count;
			if ( count > 0 )
			{
				for ( int i = 0; i < count; i++ )
				{
					IBatchLoader loader = this._loaders[i];
					loader.Load( this.OnLoadComplete, this.OnLoadProgress, this.OnLoadError, this._useWWW, this._fromCache, this._sync );
				}
			}
			else
			{
				this.completeHandler?.Invoke( this );
			}
		}

		public void Cancel()
		{
			int count = this._loaders.Count;
			for ( int i = 0; i < count; i++ )
			{
				IBatchLoader loader = this._loaders[i];
				loader.Cancel();
				this._progress[loader] = 0f;
			}
			this._started = false;
		}

		private void OnLoadComplete( object sender, AssetsProxy assetsProxy, object data )
		{
			IBatchLoader loader = ( IBatchLoader )sender;
			this._progress[loader] = 1f;

			this.singleCompleteHandler?.Invoke( this, assetsProxy, loader );

			++this.numLoaded;

			if ( this._loaders.Count == this.numLoaded )
			{
				this._loaders.Clear();
				this.completeHandler?.Invoke( this );
			}
		}

		private void OnLoadProgress( object sender, float progress )
		{
			IBatchLoader loader = ( IBatchLoader )sender;
			this._progress[loader] = progress;
			float sum = 0f;
			foreach ( KeyValuePair<IBatchLoader, float> kv in this._progress )
				sum += kv.Value;
			this.progressHandler?.Invoke( this, sum / this.numItems, loader );
		}

		private void OnLoadError( object sender, string msg, object data )
		{
			IBatchLoader loader = ( IBatchLoader )sender;
			this._progress[loader] = 1f;

			this.errorHandler?.Invoke( this, msg );

			++this.numLoaded;

			if ( this._loaders.Count == this.numLoaded )
			{
				this._loaders.Clear();
				this.completeHandler?.Invoke( this );
			}
		}
	}
}