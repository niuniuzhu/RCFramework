using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Loader
{
	public static class AssetsManager
	{
		public const int MAX_CONCURRENT = 2;

		internal static string assetBundlePath = string.Empty;

		public static int numLoading { get; internal set; }

		private static readonly Dictionary<string, AssetsProxy> ASSETS_PROXIES = new Dictionary<string, AssetsProxy>();
		private static readonly HashSet<string> LOADING_ASSET_BUNDLES = new HashSet<string>();

		private static string[] _variants = { };
		public static string[] variants
		{
			get => _variants;
			set => _variants = value;
		}

		private static string _relativeResUrl = string.Empty;
		private static string _relativeResPath = string.Empty;
		private static string _platformFolderForAssetBundles = string.Empty;

		public static string relativeResUrl => _relativeResUrl;
		public static string relativeResPath => _relativeResPath;
		public static string platformFolderForAssetBundles => _platformFolderForAssetBundles;

		private static bool _init;

		public static void Init( string assetBundlePath = "" )
		{
			if ( _init )
				return;

			_init = true;

			AssetsManager.assetBundlePath = string.IsNullOrEmpty( assetBundlePath )
				? ( ( Application.isMobilePlatform || Application.isConsolePlatform )
					? Application.persistentDataPath
					: Application.streamingAssetsPath )
				: assetBundlePath;

			_platformFolderForAssetBundles = LoaderUtil.GetAssetBundleOutputFolderName( Application.platform );
			_relativeResPath = LoaderUtil.GetRelativePath();
			if ( _relativeResPath.StartsWith( "jar:file://" ) || _relativeResPath.StartsWith( "http://" ) || _relativeResPath.StartsWith( "https://" ) )
				_relativeResUrl = _relativeResPath;
			else
				_relativeResUrl = "file://" + _relativeResPath;
		}

		internal static bool IsLoadingLocked( string name )
		{
			return LOADING_ASSET_BUNDLES.Contains( name );
		}

		internal static void LockLoading( string name )
		{
			LOADING_ASSET_BUNDLES.Add( name );
		}

		internal static void UnlockLoading( string name )
		{
			LOADING_ASSET_BUNDLES.Remove( name );
		}

		internal static void AddAssetBundle( AssetsProxy assetsProxy )
		{
			ASSETS_PROXIES.Add( assetsProxy.name, assetsProxy );
		}

		public static AssetsProxy GetAssetBundle( string name )
		{
			ASSETS_PROXIES.TryGetValue( name, out AssetsProxy assetsProxy );
			return assetsProxy;
		}

		public static Shader GetShader( string assetName )
		{
			return LoadAsset<Shader>( "shader", assetName );
		}

		public static T LoadAsset<T>( string bundleName, string assetName ) where T : Object
		{
			AssetsProxy assetsProxy = GetAssetBundle( bundleName );
			if ( assetsProxy != null )
				return LoadAsset<T>( assetsProxy, assetName );
			return null;
		}

		public static T LoadAsset<T>( AssetsProxy assetsProxy, string assetName ) where T : Object
		{
			return assetsProxy.LoadAsset<T>( assetName );
		}

		public static Object LoadAsset( string bundleName, string assetName )
		{
			AssetsProxy assetsProxy = GetAssetBundle( bundleName );
			return assetsProxy?.LoadAsset( assetName );
		}

		public static Object LoadAsset( AssetsProxy assetsProxy, string assetName )
		{
			return assetsProxy.LoadAsset( assetName );
		}

		public static Object LoadAsset( string bundleName, string assetName, Type type )
		{
			AssetsProxy assetsProxy = GetAssetBundle( bundleName );
			return assetsProxy?.LoadAsset( assetName, type );
		}

		public static Object LoadAsset( AssetsProxy assetsProxy, string assetName, Type type )
		{
			return assetsProxy.LoadAsset( assetName, type );
		}

		public static void UnloadAssetBundle( string name, bool unloadAllLoadedObjects )
		{
			if ( !ASSETS_PROXIES.ContainsKey( name ) )
				return;
			AssetsProxy assetsProxy = ASSETS_PROXIES[name];
			if ( assetsProxy.Destroy( unloadAllLoadedObjects ) )
				ASSETS_PROXIES.Remove( name );
		}

		public static void UnloadAllAssetBundle( bool unloadAllLoadedObjects )
		{
			List<string> tobeRemove = new List<string>();
			foreach ( KeyValuePair<string, AssetsProxy> kv in ASSETS_PROXIES )
			{
				if ( kv.Value.Destroy( unloadAllLoadedObjects ) )
					tobeRemove.Add( kv.Key );
			}
			foreach ( string key in tobeRemove )
				ASSETS_PROXIES.Remove( key );
		}

		internal static string RemapVariantName( string assetBundleName )
		{
			string[] bundlesWithVariant = GAssetBundleManifest.GetAllAssetBundlesWithVariant();

			if ( Array.IndexOf( bundlesWithVariant, assetBundleName ) < 0 )
				return assetBundleName;

			string[] split = assetBundleName.Split( '.' );

			int bestFit = int.MaxValue;
			int bestFitIndex = -1;
			for ( int i = 0; i < bundlesWithVariant.Length; i++ )
			{
				string[] curSplit = bundlesWithVariant[i].Split( '.' );
				if ( curSplit[0] != split[0] )
					continue;

				int found = Array.IndexOf( _variants, curSplit[1] );
				if ( found != -1 && found < bestFit )
				{
					bestFit = found;
					bestFitIndex = i;
				}
			}

			return bestFitIndex != -1 ? bundlesWithVariant[bestFitIndex] : assetBundleName;
		}

		public static void LoadManifest( CompleteHandler completeCallback, ErrorHandler errorCallback, bool useWWW, params string[] manifests )
		{
			LoadBatch batch = new LoadBatch( useWWW, false, false, new ArrayList { completeCallback, errorCallback } );
			int count = manifests.Length;
			for ( int i = 0; i < count; i++ )
			{
				string bundleName = $"{manifests[i]}_{_platformFolderForAssetBundles}";
				batch.Add( new AssetsLoader( bundleName, "AssetBundleManifest", null, AssetsLoader.CacheType.Always, true ) );
			}
			batch.Start( OnManifestsLoadComplete, OnManifestLoadProgress, OnManifestsLoadError, OnManifestLoadComplete );
		}

		private static void OnManifestLoadProgress( object sender, float progress, IBatchLoader loader )
		{

		}

		private static void OnManifestsLoadError( object sender, string msg, object data )
		{
			LoadBatch batch = ( LoadBatch )sender;
			ErrorHandler callback = ( ErrorHandler )( ( ArrayList )batch.data )[1];
			callback?.Invoke( null, msg );
		}

		private static void OnManifestLoadComplete( object sender, AssetsProxy assetsProxy, IBatchLoader batchLoader, object data )
		{
			AssetsLoader loader = ( AssetsLoader )batchLoader;
			AssetBundleManifest manifest = assetsProxy.LoadAsset<AssetBundleManifest>( loader.assetName );
			GAssetBundleManifest.Add( manifest );
		}

		private static void OnManifestsLoadComplete( object sender, object data )
		{
			LoadBatch batch = ( LoadBatch )sender;

			GAssetBundleManifest.Combine();

			CompleteHandler callback = ( CompleteHandler )( ( ArrayList )batch.data )[0];
			callback?.Invoke( null );
		}
	}
}