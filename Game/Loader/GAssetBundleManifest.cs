using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Loader
{
	public static class GAssetBundleManifest
	{
		private static string[] _bundles;
		private static string[] _variants;
		private static readonly Dictionary<string, string[]> DEPENDENCIES = new Dictionary<string, string[]>();
		private static readonly Dictionary<string, Hash128> HASHS = new Dictionary<string, Hash128>();
		private static readonly List<AssetBundleManifest> MANIFESTS = new List<AssetBundleManifest>();

		public static void Add( AssetBundleManifest manifest )
		{
			if ( !MANIFESTS.Contains( manifest ) )
				MANIFESTS.Add( manifest );
		}

		public static void Combine()
		{
			int count = MANIFESTS.Count;
			List<string> ns = new List<string>();
			List<string> vs = new List<string>();
			for ( int i = 0; i < count; i++ )
			{
				AssetBundleManifest manifest = MANIFESTS[i];
				string[] allAssetBundles = manifest.GetAllAssetBundles();
				string[] allVariants = manifest.GetAllAssetBundlesWithVariant();
				ns.AddRange( allAssetBundles );
				vs.AddRange( allVariants );
				foreach ( string n in allAssetBundles )
				{
					DEPENDENCIES[n] = manifest.GetAllDependencies( n );
					HASHS[n] = manifest.GetAssetBundleHash( n );
				}
			}
			_bundles = ns.ToArray();
			_variants = vs.ToArray();
			MANIFESTS.Clear();
		}

		public static string[] GetAllAssetBundles()
		{
			return _bundles;
		}

		public static string[] GetAllAssetBundlesWithVariant()
		{
			return _variants;
		}

		public static string[] GetAllDependencies( string assetBundleName )
		{
			string[] deps;
			DEPENDENCIES.TryGetValue( assetBundleName, out deps );
			return deps;
		}

		public static bool HashAssetBundleHash( string assetBundleName )
		{
			return HASHS.ContainsKey( assetBundleName );
		}

		public static Hash128 GetAssetBundleHash( string assetBundleName )
		{
			if ( !HASHS.ContainsKey( assetBundleName ) )
				throw new Exception( "Manifest do not contain assetbundle name:" + assetBundleName );
			return HASHS[assetBundleName];
		}
	}
}