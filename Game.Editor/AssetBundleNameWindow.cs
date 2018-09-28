using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
	public class AssetBundleNameWindow : EditorWindow
	{
		private string _prefix;

		[MenuItem( "Assets/Asset Bundle Name Window" )]
		public static void ShowWindow()
		{
			EditorWindow window = GetWindow<AssetBundleNameWindow>( "Asset Bundle Name Window" );
			window.maxSize = new Vector2( 290, 400 );
			window.minSize = new Vector2( 290, 400 );
		}

		private void OnInspectorUpdate()
		{
			this.Repaint();
		}

		private void OnGUI()
		{
			this._prefix = EditorGUILayout.TextField( "前缀:", this._prefix );
			if ( GUILayout.Button( "确定" ) )
			{
				Object[] selectedObjects = Selection.objects;
				foreach ( Object o in selectedObjects )
				{
					string assetPath = AssetDatabase.GetAssetPath( o );
					string fullAssetPath = this.GetFullAssetPath( assetPath );
					AssetImporter importer = AssetImporter.GetAtPath( assetPath );
					importer.assetBundleName = ( this._prefix + Path.GetFileNameWithoutExtension( fullAssetPath ) ).ToLower();
				}
			}
			if ( GUILayout.Button( "确定2" ) )
			{
				Object[] selectedObjects = Selection.GetFiltered( typeof( Object ), SelectionMode.DeepAssets );
				foreach ( Object o in selectedObjects )
				{
					string assetPath = AssetDatabase.GetAssetPath( o );
					AssetImporter importer = AssetImporter.GetAtPath( assetPath );
					importer.assetBundleName = this._prefix.ToLower();
				}
			}
			if ( GUILayout.Button( "清除" ) )
			{
				Object[] selectedObjects = Selection.GetFiltered( typeof( Object ), SelectionMode.DeepAssets );
				foreach ( Object o in selectedObjects )
				{
					string assetPath = AssetDatabase.GetAssetPath( o );
					AssetImporter importer = AssetImporter.GetAtPath( assetPath );
					importer.assetBundleName = string.Empty;
				}
			}
			AssetDatabase.SaveAssets();
		}

		private string GetFullAssetPath( string assetPath )
		{
			return Application.dataPath + "/../" + assetPath;
		}
	}
}