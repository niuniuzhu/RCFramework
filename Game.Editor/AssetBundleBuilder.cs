using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Game.Editor
{
	public class AssetBundleBuilder
	{
		public static void BuildAssetBundles()
		{
			if ( !PlayerPrefs.HasKey( "assetbundle_output_path" ) || string.IsNullOrEmpty( PlayerPrefs.GetString( "assetbundle_output_path" ) ) )
				throw new Exception( "AssetBundle output path not set!" );

			string outputPath = Path.Combine( PlayerPrefs.GetString( "assetbundle_output_path" ),
				Application.productName + "_" + GetPlatformFolderForAssetBundles( EditorUserBuildSettings.activeBuildTarget ) );

			if ( !Directory.Exists( outputPath ) )
				Directory.CreateDirectory( outputPath );

			BuildPipeline.BuildAssetBundles( outputPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget );

			string runFile = Path.Combine( Application.dataPath, "run_after_bundle_build.bat" );
			if ( File.Exists( runFile ) )
			{
				Process proc = new Process
				{
					StartInfo =
					{
						FileName = runFile,
						Arguments = Application.dataPath.Replace( "/", "\\" ) + " " + GetPlatformFolderForAssetBundles( EditorUserBuildSettings.activeBuildTarget ),
					}
				};
				proc.Start();
				proc.WaitForExit();
				proc.Close();
			}
		}

		public static void BuildPlayer()
		{
			var outputPath = EditorUtility.SaveFolderPanel( "Choose Location of the Built Game", "", "" );
			if ( outputPath.Length == 0 )
				return;

			string[] levels = GetLevelsFromBuildSettings();
			if ( levels.Length == 0 )
			{
				Debug.Log( "Nothing to build." );
				return;
			}

			string targetName = Application.productName + GetBuildTargetFileExtension( EditorUserBuildSettings.activeBuildTarget );

			BuildOptions option = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
			BuildPipeline.BuildPlayer( levels, Path.Combine( outputPath, targetName ), EditorUserBuildSettings.activeBuildTarget, option | BuildOptions.CompressWithLz4 );
		}

		private static string GetBuildTargetFileExtension( BuildTarget target )
		{
			switch ( target )
			{
				case BuildTarget.Android:
					return ".apk";
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
					return ".exe";
				case BuildTarget.StandaloneOSX:
					return ".app";
				default:
					Debug.Log( "Target not implemented." );
					return null;
			}
		}

		static string[] GetLevelsFromBuildSettings()
		{
			int count = EditorBuildSettings.scenes.Length;
			List<string> levels = new List<string>();
			for ( int i = 0; i < count; ++i )
			{
				if ( EditorBuildSettings.scenes[i].enabled )
					levels.Add( EditorBuildSettings.scenes[i].path );
			}

			return levels.ToArray();
		}

		static string GetPlatformFolderForAssetBundles( BuildTarget target )
		{
			switch ( target )
			{
				case BuildTarget.Android:
					return "Android";
				case BuildTarget.iOS:
					return "iOS";
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
					return "Windows";
				case BuildTarget.StandaloneOSX:
					return "OSX";
				case BuildTarget.WebGL:
					return "WebGL";
				default:
					return null;
			}
		}
	}
}
