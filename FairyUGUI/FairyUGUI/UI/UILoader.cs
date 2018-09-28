using Game.Loader;
using UnityEngine;

namespace FairyUGUI.UI
{
	public class UILoader : IBatchLoader
	{
		public AssetsCompleteHandler completeHandler { get; set; }
		public ProgressHandler progressHandler { get; set; }
		public ErrorHandler errorHandler { get; set; }

		public object data;

		private readonly string _assetBundleName;
		private AssetBundle _dec;
		private AssetBundle _res;
		private LoadBatch _batch;

		public bool loaded => UIPackage.GetById( this._assetBundleName ) != null;

		public UILoader( string assetBundleName, object data = null )
		{
			this._assetBundleName = assetBundleName;
			this.data = data;
		}

		public void Load( AssetsCompleteHandler completeHandler, ProgressHandler progressHandler, ErrorHandler errorHandler,
						  bool useWWW = false, bool fromCache = true, bool sync = false )
		{
			this.completeHandler = completeHandler;
			this.progressHandler = progressHandler;
			this.errorHandler = errorHandler;

			this.Cancel();

			this._batch = new LoadBatch( fromCache, sync );
			this._batch.Add( new AssetsLoader( this._assetBundleName, string.Empty, null, AssetsLoader.CacheType.NoCache ) );
			this._batch.Add( new AssetsLoader( this._assetBundleName + "_res", string.Empty, null, AssetsLoader.CacheType.NoCache ) );
			this._batch.Start( this.OnInternalComplete, this.OnInternalProgress, this.OnInternalError,
							   this.OnInternalSingleComplete );
		}

		private void OnInternalSingleComplete( object sender, AssetsProxy assetsProxy, IBatchLoader loader, object o )
		{
			if ( assetsProxy.name == this._assetBundleName )
				this._dec = assetsProxy.assetBundle;
			else
				this._res = assetsProxy.assetBundle;
		}

		private void OnInternalProgress( object sender, float progress, IBatchLoader loader )
		{
			this.progressHandler?.Invoke( this, progress );
		}

		private void OnInternalComplete( object sender, object o )
		{
			this._batch = null;

			if ( !this.loaded )
			{
				UIPackage.AddPackage( this._dec, this._res ).customId = this._assetBundleName;
				this._dec = this._res = null;
			}

			this.completeHandler?.Invoke( this, null );
		}

		private void OnInternalError( object sender, string msg, object o )
		{
			this.errorHandler?.Invoke( this, msg );
		}

		public void Cancel()
		{
			if ( this._batch != null )
			{
				this._batch.Cancel();
				this._batch = null;
			}
		}
	}
}