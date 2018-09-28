using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
	public class ProjectSettings : EditorWindow
	{
		[MenuItem( "Settings/Project settings" )]
		public static void ShowWindow()
		{
			ProjectSettings window = GetWindow<ProjectSettings>( "Project settings" );
			window.maxSize = new Vector2( 440, 136 );
			window.minSize = new Vector2( 440, 136 );
		}

		private void OnInspectorUpdate()
		{
			this.Repaint();
		}

		private void OnGUI()
		{
			string uiFolder = EditorGUILayout.TextField( "UI Folder", PlayerPrefs.GetString( "ui_folder" ) );
			if ( GUI.changed )
				PlayerPrefs.SetString( "ui_folder", uiFolder );

			string uiBundleFolder = EditorGUILayout.TextField( "UI Bundle Folder", PlayerPrefs.GetString( "ui_bundle_folder" ) );
			if ( GUI.changed )
				PlayerPrefs.SetString( "ui_bundle_folder", uiBundleFolder );

			string abOutputPath = EditorGUILayout.TextField( "AssetBundle output path", PlayerPrefs.GetString( "assetbundle_output_path" ) );
			if ( GUI.changed )
				PlayerPrefs.SetString( "assetbundle_output_path", abOutputPath );

			string luaPath = EditorGUILayout.TextField( "Lua path", PlayerPrefs.GetString( "lua_path" ) );
			if ( GUI.changed )
				PlayerPrefs.SetString( "lua_path", luaPath );

			string luajitPath = EditorGUILayout.TextField( "Luajit path", PlayerPrefs.GetString( "luajit_path" ) );
			if ( GUI.changed )
				PlayerPrefs.SetString( "luajit_path", luajitPath );

			string luacPath = EditorGUILayout.TextField( "Luac path", PlayerPrefs.GetString( "luac_path" ) );
			if ( GUI.changed )
				PlayerPrefs.SetString( "luac_path", luacPath );
		}
	}
}