using Game.Task;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Loader
{
	public class AssetsLoader : IBatchLoader
	{
		public enum CacheType
		{
			AutoUnload,
			Always,
			NoCache
		}

		public AssetsCompleteHandler completeHandler { get; set; }
		public ProgressHandler progressHandler { get; set; }
		public ErrorHandler errorHandler { get; set; }

		public string assetBundleName { get; private set; }

		public string assetName { get; private set; }

		public object data;

		protected readonly bool _isManifest;
		private readonly CacheType _cacheType;
		protected bool _canceled;

		public AssetsLoader( string assetBundleName, string assetName = "", object data = null,
							 CacheType cacheType = CacheType.AutoUnload, bool isManifest = false )
		{
			this.assetBundleName = assetBundleName;
			this.assetName = assetName;
			this.data = data;
			this._cacheType = cacheType;
			this._isManifest = isManifest;
		}

		public virtual void Load( AssetsCompleteHandler completeHandler, ProgressHandler progressHandler,
								  ErrorHandler errorHandler, bool useWWW = false, bool fromCache = true, bool sync = false )
		{
			this.completeHandler = completeHandler;
			this.progressHandler = progressHandler;
			this.errorHandler = errorHandler;

			AssetsProxy assetsProxy = AssetsManager.GetAssetBundle( this.assetBundleName );
			if ( assetsProxy != null )
			{
				this.completeHandler?.Invoke( this, assetsProxy );
				return;
			}

			if ( Application.isEditor )
				fromCache = false;

			this._canceled = false;

			if ( !this._isManifest )
			{
				string[] dependencies = GAssetBundleManifest.GetAllDependencies( this.assetBundleName );
				if ( dependencies != null &&
					 dependencies.Length > 0 )
				{
					int count = dependencies.Length;
					List<string> dependenciesNotLoaded = new List<string>();
					for ( int i = 0; i < count; i++ )
					{
						dependencies[i] = AssetsManager.RemapVariantName( dependencies[i] );
						AssetsProxy dAssetsProxy = AssetsManager.GetAssetBundle( dependencies[i] );
						if ( dAssetsProxy == null )
							dependenciesNotLoaded.Add( dependencies[i] );
					}
					if ( dependenciesNotLoaded.Count == 0 )
						this.LoadAsset( useWWW, fromCache, sync );
					else
						SyncTask.Create( this.LoadDependenciesInternal( dependenciesNotLoaded, useWWW, fromCache, sync ) );
				}
				else
					this.LoadAsset( useWWW, fromCache, sync );
			}
			else
				this.LoadAsset( useWWW, fromCache, sync );
		}

		protected virtual IEnumerator LoadDependenciesInternal( List<string> dependencies, bool useWWW, bool fromCache, bool sync )
		{
			int count = dependencies.Count;
			for ( int i = 0; i < count; i++ )
			{
				string mAssetBundleName = dependencies[i];

				while ( AssetsManager.IsLoadingLocked( mAssetBundleName ) )
				{
					if ( this._canceled )
						yield break;

					yield return 0;
				}

				AssetsProxy assetsProxy = AssetsManager.GetAssetBundle( mAssetBundleName );
				if ( assetsProxy != null ) continue;

				AssetsManager.LockLoading( mAssetBundleName );
				if ( useWWW )
				{
					while ( AssetsManager.numLoading >= AssetsManager.MAX_CONCURRENT )
					{
						if ( this._canceled )
						{
							AssetsManager.UnlockLoading( mAssetBundleName );
							yield break;
						}

						yield return 0;
					}

					string url = AssetsManager.relativeResUrl + mAssetBundleName;

					WWW www = fromCache
						          ? WWW.LoadFromCacheOrDownload( url,
						                                         GAssetBundleManifest.GetAssetBundleHash(
							                                         mAssetBundleName ), 0 )
						          : new WWW( url );

					++AssetsManager.numLoading;

					while ( !www.isDone )
					{
						if ( this._canceled )
						{
							--AssetsManager.numLoading;
							AssetsManager.UnlockLoading( mAssetBundleName );
							yield break;
						}
						this.progressHandler?.Invoke( this, ( ( i + www.progress ) * 0.9f / ( count + 1 ) ) * 0.3f );
						yield return 0;
					}

					--AssetsManager.numLoading;

					if ( this._canceled )
					{
						www.Dispose();
						AssetsManager.UnlockLoading( mAssetBundleName );
						yield break;
					}

					this.progressHandler?.Invoke( this, ( ( i + 1f ) * 0.9f / ( count + 1 ) ) * 0.3f );

					if ( !string.IsNullOrEmpty( www.error ) )
						this.errorHandler?.Invoke( this, www.error );
					else
					{
						if ( sync )
							www.assetBundle.LoadAllAssets();
						else
						{
							AssetBundleRequest request = www.assetBundle.LoadAllAssetsAsync();
							while ( !request.isDone )
							{
								this.progressHandler?.Invoke( this, ( 0.9f + request.progress * 0.1f ) * 0.3f );
								yield return 0;
							}
						}

						this.progressHandler?.Invoke( this, 1f );

						assetsProxy = new AssetsProxy( mAssetBundleName, www.assetBundle, true );

						AssetsManager.AddAssetBundle( assetsProxy );
					}
					www.Dispose();
				}
				else
				{
					AssetBundle assetBundle;
					string url = AssetsManager.relativeResPath + mAssetBundleName;
					if ( sync )
					{
						assetBundle = AssetBundle.LoadFromFile( url );
					}
					else
					{
						AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromFileAsync( url );
						while ( !assetBundleCreateRequest.isDone )
						{
							if ( this._canceled )
							{
								AssetsManager.UnlockLoading( mAssetBundleName );
								yield break;
							}
							this.progressHandler?.Invoke( this, ( ( i + assetBundleCreateRequest.progress ) * 0.9f / ( count + 1 ) ) * 0.3f );
							yield return 0;
						}
						assetBundle = assetBundleCreateRequest.assetBundle;
					}

					this.progressHandler?.Invoke( this, ( ( i + 1f ) * 0.9f / ( count + 1 ) ) * 0.3f );

					if ( assetBundle == null )
						this.errorHandler?.Invoke( this, $"Failed to load {url}!" );
					else
					{
						if ( sync )
							assetBundle.LoadAllAssets();
						else
						{
							AssetBundleRequest request = assetBundle.LoadAllAssetsAsync();
							while ( !request.isDone )
							{
								this.progressHandler?.Invoke( this, ( 0.9f + request.progress * 0.1f ) * 0.3f );
								yield return 0;
							}
						}

						this.progressHandler?.Invoke( this, 1f );

						assetsProxy = new AssetsProxy( mAssetBundleName, assetBundle, true );

						AssetsManager.AddAssetBundle( assetsProxy );
					}
				}
				AssetsManager.UnlockLoading( mAssetBundleName );
			}

			if ( this._canceled )
				yield break;

			this.LoadAsset( useWWW, fromCache, sync );
		}

		protected void LoadAsset( bool useWWW, bool fromCache, bool sync )
		{
			SyncTask.Create(
				this.LoadAssetInternal(
					this._isManifest ? this.assetBundleName : AssetsManager.RemapVariantName( this.assetBundleName ),
					this.assetName, useWWW, fromCache, sync ) );
		}

		protected virtual IEnumerator LoadAssetInternal( string assetBundleName, string assetName, bool useWWW, bool fromCache,
														 bool sync )
		{
			if ( !this._isManifest &&
				 !GAssetBundleManifest.HashAssetBundleHash( assetBundleName ) )
			{
				this.errorHandler?.Invoke( this, "Manifest do not contain assetbundle name:" + assetBundleName );

				yield break;
			}

			while ( AssetsManager.IsLoadingLocked( assetBundleName ) )
			{
				if ( this._canceled )
					yield break;

				yield return 0;
			}

			AssetsProxy assetsProxy = AssetsManager.GetAssetBundle( assetBundleName );
			if ( assetsProxy != null )
			{
				this.completeHandler?.Invoke( this, assetsProxy );
			}
			else
			{
				AssetsManager.LockLoading( assetBundleName );
				if ( useWWW )
				{
					while ( AssetsManager.numLoading >= AssetsManager.MAX_CONCURRENT )
					{
						if ( this._canceled )
						{
							AssetsManager.UnlockLoading( assetBundleName );
							yield break;
						}
						yield return 0;
					}

					string url = AssetsManager.relativeResUrl + assetBundleName;

					WWW www = fromCache && !this._isManifest
						          ? WWW.LoadFromCacheOrDownload( url,
						                                         GAssetBundleManifest.GetAssetBundleHash( assetBundleName ),
						                                         0 )
						          : new WWW( url );

					++AssetsManager.numLoading;

					while ( !www.isDone )
					{
						if ( this._canceled )
						{
							--AssetsManager.numLoading;
							AssetsManager.UnlockLoading( assetBundleName );
							yield break;
						}
						this.progressHandler?.Invoke( this, www.progress * 0.9f * 0.7f + 0.3f );
						yield return 0;
					}

					--AssetsManager.numLoading;

					if ( this._canceled )
					{
						www.Dispose();
						AssetsManager.UnlockLoading( assetBundleName );
						yield break;
					}

					this.progressHandler?.Invoke( this, 0.9f * 0.7f + 0.3f );

					if ( !string.IsNullOrEmpty( www.error ) )
						this.errorHandler?.Invoke( this, www.error );
					else
					{
						if ( sync )
							www.assetBundle.LoadAllAssets();
						else
						{
							AssetBundleRequest request = www.assetBundle.LoadAllAssetsAsync();
							while ( !request.isDone )
							{
								//由于加载请求无法中断，这里不能cancel，否则造成bundle加载到内存，但却没有AssetsProxy
								this.progressHandler?.Invoke( this, ( 0.9f + request.progress * 0.1f ) * 0.7f + 0.3f );
								yield return 0;
							}
						}

						this.progressHandler?.Invoke( this, 1f );

						assetsProxy = new AssetsProxy( assetBundleName, www.assetBundle,
						                               this._cacheType == CacheType.AutoUnload );

						if ( this._cacheType != CacheType.NoCache )
							AssetsManager.AddAssetBundle( assetsProxy );

						this.completeHandler?.Invoke( this, assetsProxy );
					}
					www.Dispose();
				}
				else
				{
					AssetBundle assetBundle;
					string url = AssetsManager.relativeResPath + assetBundleName;
					if ( sync )
					{
						assetBundle = AssetBundle.LoadFromFile( url );
					}
					else
					{
						AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromFileAsync( url );
						while ( !assetBundleCreateRequest.isDone )
						{
							if ( this._canceled )
							{
								AssetsManager.UnlockLoading( assetBundleName );
								yield break;
							}
							this.progressHandler?.Invoke( this, assetBundleCreateRequest.progress * 0.9f * 0.7f + 0.3f );
							yield return 0;
						}
						assetBundle = assetBundleCreateRequest.assetBundle;
					}

					this.progressHandler?.Invoke( this, 0.9f * 0.7f + 0.3f );

					if ( assetBundle == null )
						this.errorHandler?.Invoke( this, $"Failed to load {url}!" );
					else
					{
						if ( sync )
							assetBundle.LoadAllAssets();
						else
						{
							AssetBundleRequest request = assetBundle.LoadAllAssetsAsync();
							while ( !request.isDone )
							{
								this.progressHandler?.Invoke( this, ( 0.9f + request.progress * 0.1f ) * 0.7f + 0.3f );
								yield return 0;
							}
						}

						this.progressHandler?.Invoke( this, 1f );

						assetsProxy = new AssetsProxy( assetBundleName, assetBundle,
						                               this._cacheType == CacheType.AutoUnload );

						if ( this._cacheType != CacheType.NoCache )
							AssetsManager.AddAssetBundle( assetsProxy );

						this.completeHandler?.Invoke( this, assetsProxy );
					}
				}
				AssetsManager.UnlockLoading( assetBundleName );
			}
		}

		public virtual void Cancel()
		{
			this._canceled = true;
		}
	}
}