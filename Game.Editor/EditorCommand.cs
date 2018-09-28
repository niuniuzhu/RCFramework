using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
	public class EditorCommand
	{
		[MenuItem( "AssetBundles/Build AssetBundles %#a" )]
		public static void AssetBundles_Build_AssetBundles()
		{
			AssetBundleBuilder.BuildAssetBundles();
		}

		[MenuItem( "AssetBundles/Build Player %#p" )]
		static void AssetBundles_Build_Player()
		{
			AssetBundleBuilder.BuildPlayer();
		}

		[MenuItem( "AssetBundles/Build Lua %#l" )]
		static void AssetBundles_Build_Lua()
		{
			if ( EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS )
				LuaBuilder.BuildIOS();
			else
				LuaBuilder.BuildWithoutJit();
		}

		[MenuItem( "Edit/Clear cache" )]
		static void ClearCache()
		{
			Caching.ClearCache();
		}
	}
}