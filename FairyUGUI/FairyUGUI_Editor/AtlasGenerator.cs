using Core.Xml;
using FairyUGUI.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace FairyUGUI_Editor
{
	public static class AtlasGenerator
	{
		public static void GenAtlas()
		{
			if ( !PlayerPrefs.HasKey( "ui_folder" ) )
				throw new Exception( "UI folder not set" );

			AssetDatabase.ImportAsset( PlayerPrefs.GetString( "ui_folder" ), ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ImportRecursive );
		}

		public static void DoOne( string assetpath )
		{
			if ( string.IsNullOrEmpty( assetpath ) )
				return;

			string fullpath = GetFullAssetPath( assetpath );
			string dir = Path.GetDirectoryName( fullpath );
			string name = Path.GetFileNameWithoutExtension( fullpath );
			string[] pair = name.Split( '@' );
			name = pair[0];

			string def = DecodeDesc( File.ReadAllBytes( Path.Combine( dir, name + ".bytes" ) ) );
			string spriteDef = File.ReadAllText( Path.Combine( dir, name + "@sprites.bytes" ) );
			string atlasPrefix = assetpath.Substring( 0, assetpath.LastIndexOf( '/' ) ) + "/" + name + "@atlas";
			GenAtlas( def, atlasPrefix, spriteDef );

			Debug.Log( $"Atlas:{assetpath} Generation Done" );
		}

		public static void Do( string path )
		{
			path = GetFullAssetPath( path );
			DirectoryInfo di = new DirectoryInfo( path );
			FileInfo[] fis = di.GetFiles( "*.bytes" );
			int count = fis.Length;
			for ( int i = 0; i < count; i++ )
			{
				FileInfo fi = fis[i];
				if ( fi.Name.Contains( "@sprites" ) )
				{
					string[] pair = fi.Name.Split( '@' );
					string s0 = pair[0];
					string dir = fi.FullName.Replace( fi.Name, "" );
					string atlasPrefix = dir + s0 + "@atlas";

					string def = DecodeDesc( File.ReadAllBytes( dir + s0 + ".bytes" ) );
					string spriteDef = File.ReadAllText( fi.FullName );
					GenAtlas( def, atlasPrefix, spriteDef );
				}
			}

			Debug.Log( "Atlas Generation Done" );
		}

		private static void GenAtlas( string def, string atlasPrefix, string spritePath )
		{
			Dictionary<string, Texture2D> texMap = new Dictionary<string, Texture2D>();
			Dictionary<string, List<SpriteMetaData>> smdMap = new Dictionary<string, List<SpriteMetaData>>();

			StringReader sr = new StringReader( spritePath );
			string s;
			while ( ( s = sr.ReadLine() ) != null )
			{
				if ( s.StartsWith( "//" ) )
					continue;
				string[] pair = s.Split( ' ' );
				string name = pair[0];
				string page = pair[1];
				int x = int.Parse( pair[2] );
				int y = int.Parse( pair[3] );
				int width = int.Parse( pair[4] );
				int height = int.Parse( pair[5] );

				//Debug.Log( "processing:" + name );

				string atlasPath = atlasPrefix + ( page == "-1" ? "_" + name : page ) + ".png";

				Texture2D texture;
				if ( !texMap.TryGetValue( atlasPath, out texture ) )
				{
					texture = AssetDatabase.LoadAssetAtPath<Texture2D>( atlasPath );
					texMap[atlasPath] = texture;
					smdMap[atlasPath] = new List<SpriteMetaData>();
				}

				SpriteMetaData smd = new SpriteMetaData();
				Rect rect = new Rect();
				rect.width = width;
				rect.height = height;
				rect.x = x;
				rect.y = texture.height - y - height;
				smd.rect = rect;
				smd.name = name;
				smd.border = GetBorder( def, name );
				smd.alignment = 0;
				smd.pivot = new Vector2( 0, 0 );
				smdMap[atlasPath].Add( smd );
			}
			sr.Close();
			foreach ( KeyValuePair<string, Texture2D> kv in texMap )
			{
				string atlasPath = kv.Key;

				TextureImporter texImporter = ( TextureImporter ) AssetImporter.GetAtPath( atlasPath );
				texImporter.spriteImportMode = SpriteImportMode.Multiple;
				//texImporter.spriteImportMode = SpriteMeshType.FullRect;
				texImporter.mipmapEnabled = false;
				texImporter.filterMode = FilterMode.Bilinear;
				texImporter.anisoLevel = 1;
				texImporter.textureCompression = TextureImporterCompression.Uncompressed;
				texImporter.alphaIsTransparency = false;
				texImporter.isReadable = false;
				texImporter.wrapMode = TextureWrapMode.Clamp;
				texImporter.spritesheet = smdMap[atlasPath].ToArray();

				string alphaTexPath = atlasPath.Substring( 0, atlasPath.Length - 4 ) + "!a.png";
				texImporter = AssetImporter.GetAtPath( alphaTexPath ) as TextureImporter;
				if ( texImporter != null )
				{
					texImporter.spriteImportMode = SpriteImportMode.Single;
					texImporter.mipmapEnabled = false;
					texImporter.filterMode = FilterMode.Bilinear;
					texImporter.anisoLevel = 1;
					texImporter.textureCompression = TextureImporterCompression.Uncompressed;
					texImporter.alphaIsTransparency = false;
					texImporter.isReadable = false;
					texImporter.wrapMode = TextureWrapMode.Clamp;
				}

				AssetDatabase.SaveAssets();
				AssetDatabase.ImportAsset( atlasPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport );

				//StripColor( atlasPath );
			}
		}

		private static void StripColor( string atlasPath )
		{
			TextureImporter texImporter = ( TextureImporter ) AssetImporter.GetAtPath( atlasPath );

			TextureImporterSettings settings = new TextureImporterSettings();
			texImporter.ReadTextureSettings( settings );
			settings.readable = true;
			texImporter.SetTextureSettings( settings );
			AssetDatabase.ImportAsset( atlasPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport );

			Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>( atlasPath );
			Color[] colors = texture.GetPixels();
			int count = colors.Length;
			for ( int i = 0; i < count; i++ )
			{
				if ( colors[i].a < 0.1f )
					colors[i] = Color.clear;
			}
			texture.SetPixels( colors );
			texture.Apply( false, false );
			byte[] bytes = texture.EncodeToPNG();
			File.WriteAllBytes( GetFullAssetPath( atlasPath ), bytes );

			settings.readable = false;
			texImporter.SetTextureSettings( settings );
			AssetDatabase.ImportAsset( atlasPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport );
		}

		private static string DecodeDesc( byte[] descBytes )
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine( "<?xml version=\"1.0\" encoding=\"utf-8\"?>" );
			sb.AppendLine( "<root>" );

			if ( descBytes.Length < 4
				|| descBytes[0] != 0x50 || descBytes[1] != 0x4b || descBytes[2] != 0x03 || descBytes[3] != 0x04 )
			{
				string source = Encoding.UTF8.GetString( descBytes );
				int curr = 0;
				string fn;
				int size;
				while ( true )
				{
					int pos = source.IndexOf( "|", curr, StringComparison.Ordinal );
					if ( pos == -1 )
						break;
					fn = source.Substring( curr, pos - curr );
					curr = pos + 1;
					pos = source.IndexOf( "|", curr, StringComparison.Ordinal );
					size = int.Parse( source.Substring( curr, pos - curr ) );
					curr = pos + 1;
					sb.Append( source.Substring( curr, size ) );
					curr += size;
				}
			}
			else
			{
				ZipReader zip = new ZipReader( descBytes );
				ZipReader.ZipEntry entry = new ZipReader.ZipEntry();
				while ( zip.GetNextEntry( entry ) )
				{
					if ( entry.isDirectory )
						continue;

					sb.Append( Encoding.UTF8.GetString( zip.GetEntryData( entry ) ) );
				}
			}

			sb.AppendLine();
			sb.Append( "</root>" );
			return sb.ToString();
		}

		private static Vector4 GetBorder( string def, string name )
		{
			XML xml = new XML( def );
			XML resources = xml.GetNode( "packageDescription" ).GetNode( "resources" );
			XMLList nodes = resources.Elements( "image" );
			foreach ( XML node in nodes )
			{
				string id = node.GetAttribute( "id" );
				if ( !id.Equals( name ) )
					continue;
				if ( node.GetAttribute( "scale", string.Empty ) != "9grid" )
					break;
				string[] s = node.GetAttributeArray( "size" );
				string[] v = node.GetAttributeArray( "scale9grid" );
				float left = float.Parse( v[0] );
				float top = float.Parse( v[1] );
				float width = float.Parse( v[2] );
				float height = float.Parse( v[3] );
				float right = float.Parse( s[0] ) - ( left + width );
				float bottom = float.Parse( s[1] ) - ( top + height );
				return new Vector4( left, bottom, right, top );
			}
			return Vector4.zero;
		}

		private static string GetDataPathFromFullName( string fullName )
		{
			int pos = fullName.IndexOf( "Assets", StringComparison.Ordinal );
			return fullName.Substring( pos ).Replace( '\\', '/' );
		}

		private static void DoAssetReimport( string path, ImportAssetOptions options )
		{
			try
			{
				AssetDatabase.StartAssetEditing();
				AssetDatabase.ImportAsset( path, options );
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}
		}

		private static string GetFullAssetPath( string assetPath )
		{
			return Application.dataPath + "/../" + assetPath;
		}
	}
}