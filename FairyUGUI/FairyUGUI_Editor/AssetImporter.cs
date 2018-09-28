using UnityEditor;
using UnityEngine;

namespace FairyUGUI_Editor
{
	public class AssetsImporter : AssetPostprocessor
	{
		private static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets,
			string[] movedFromAssetPaths )
		{
			if ( PlayerPrefs.HasKey( "ui_folder" ) )
			{
				foreach ( string assetPath in importedAssets )
				{
					if ( !assetPath.Contains( PlayerPrefs.GetString( "ui_folder" ) ) || !assetPath.Contains( "@sprites.bytes" ) )
						continue;

					AtlasGenerator.DoOne( assetPath );
				}
			}
			if ( PlayerPrefs.HasKey( "ui_bundle_folder" ) )
			{
				foreach ( string assetPath in importedAssets )
				{
					if ( !assetPath.Contains( PlayerPrefs.GetString( "ui_bundle_folder" ) ) || !assetPath.Contains( "@sprites.bytes" ) )
						continue;

					AtlasGenerator.DoOne( assetPath );
				}
			}
		}

		private static string GetFullAssetPath( string assetPath )
		{
			return Application.dataPath + "/../" + assetPath;
		}
	}
}