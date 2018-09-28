using Game.Task;
using System.Collections;
using System.Collections.Generic;
using Game.Misc;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Loader
{
	public class SceneLoader : AssetsLoader
	{
		private readonly LoadSceneMode _loadSceneMode;
		private AsyncOperation _asyncOperation;

		public SceneLoader( string assetBundleName, string level, LoadSceneMode loadSceneMode, object data = null )
			: base( assetBundleName, level, data )
		{
			this._loadSceneMode = loadSceneMode;
		}

		public override void Load( AssetsCompleteHandler completeHandler, ProgressHandler progressHandler,
								   ErrorHandler errorHandler, bool useWWW = false, bool fromCache = true, bool sync = false )
		{
			this.completeHandler = completeHandler;
			this.progressHandler = progressHandler;
			this.errorHandler = errorHandler;
			this._canceled = false;

			if ( string.IsNullOrEmpty( this.assetBundleName ) )
			{
				SyncTask.Create( this.LoadSceneAsync( this.assetName, this._loadSceneMode, sync ) );
			}
			else
			{
				if ( Application.isEditor )
					fromCache = false;
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
		}

		protected override IEnumerator LoadAssetInternal( string assetBundleName, string level, bool useWWW, bool fromCache, bool sync )
		{
			while ( AssetsManager.IsLoadingLocked( assetBundleName ) )
				yield return 0;

			AssetsManager.LockLoading( assetBundleName );

			if ( useWWW )
			{
				string url = AssetsManager.relativeResUrl + assetBundleName;
				WWW www = fromCache && !this._isManifest
							  ? WWW.LoadFromCacheOrDownload( url, GAssetBundleManifest.GetAssetBundleHash( assetBundleName ), 0 )
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
				{
					this.errorHandler?.Invoke( this, www.error );
				}
				else
				{
					AssetBundle assetBundle = www.assetBundle; //必须先载入到内存

					if ( sync )
						SceneManager.LoadScene( level, this._loadSceneMode );
					else
						yield return SyncTask.Create( this.HandleLoadSceneAsync( level ) );

					www.assetBundle.Unload( false );
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
					assetBundleCreateRequest.allowSceneActivation = false;
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
						SceneManager.LoadScene( level, this._loadSceneMode );
					else
						yield return SyncTask.Create( this.HandleLoadSceneAsync( level ) );

					this.progressHandler?.Invoke( this, 1f );
					this.completeHandler?.Invoke( this, null );
					assetBundle.Unload( false );
				}
			}

			AssetsManager.UnlockLoading( assetBundleName );
		}

		private IEnumerator LoadSceneAsync( string assetName, LoadSceneMode loadSceneMode, bool sync )
		{
			if ( sync )
				SceneManager.LoadScene( assetName, loadSceneMode );
			else
				yield return SyncTask.Create( this.HandleLoadSceneAsync( assetName ) );

			this.progressHandler?.Invoke( this, 1f );
			this.completeHandler?.Invoke( this, null );
		}

		private IEnumerator HandleLoadSceneAsync( string level )
		{
			this._asyncOperation = SceneManager.LoadSceneAsync( level, this._loadSceneMode );
			this._asyncOperation.allowSceneActivation = false;
			while ( this._asyncOperation.progress < 0.9f )
			{
				this.progressHandler?.Invoke( this, ( this._asyncOperation.progress * 0.1f + 0.9f ) * 0.7f + 0.3f );
				yield return 0;
			}
		}

		public void BeginSceneActivation( CompleteHandler callback, object param = null )
		{
			if ( this._asyncOperation != null )
				SyncTask.Create( this.HandleSceneActivation( callback, param ) );
		}

		private IEnumerator HandleSceneActivation( CompleteHandler callback, object param )
		{
			this._asyncOperation.allowSceneActivation = true;
			while ( !this._asyncOperation.isDone )
				yield return 0;
			this._asyncOperation = null;
			callback.Invoke( this, param );
		}

		public Scene GetScene()
		{
			return SceneManager.GetSceneByName( this.assetName );
		}

		public bool ActiveScene()
		{
			return SceneManager.SetActiveScene( this.GetScene() );
		}

		public override void Cancel()
		{
			base.Cancel();

			this._asyncOperation = null;
		}

		public static void UnloadScene( string scaneName, CompleteHandler completeCallback, object param = null )
		{
			AsyncOperation ao = SceneManager.UnloadSceneAsync( scaneName );
			SyncTask.Create( HandleUnloadScene( ao, completeCallback, param ) );
		}

		public static void UnloadScene( Scene scene, CompleteHandler completeCallback, object param = null )
		{
			AsyncOperation ao = SceneManager.UnloadSceneAsync( scene );
			SyncTask.Create( HandleUnloadScene( ao, completeCallback, param ) );
		}

		private static IEnumerator HandleUnloadScene( AsyncOperation ao, CompleteHandler completeCallback, object param )
		{
			while ( !ao.isDone )
			{
				yield return 0;
			}
			completeCallback?.Invoke( null, param );
		}
	}
}