using Core.Misc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Core.Crypto;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
	public static class LuaBuilder
	{
		public static readonly byte[] AES_KEY = Convert.FromBase64String( "bZdEWqA/z5mfkeUYsRfiEA==" );
		public static readonly byte[] AES_IV = Convert.FromBase64String( "+qe0mOBjiaSU8YJwteeQ3Q==" );

		public static void BuildWithoutJit()
		{
			string luaPath = PlayerPrefs.GetString( "lua_path" );

			DirectoryInfo di = new DirectoryInfo( luaPath );
			if ( string.IsNullOrEmpty( luaPath ) || !di.Exists )
				throw new Exception( "Lua path not exist." );
			FileInfo[] fis = di.GetFiles( "*.lua", SearchOption.AllDirectories );
			List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();
			foreach ( FileInfo fi in fis )
			{
				if ( fi.Extension != ".lua" )
					continue;

				string subPath = fi.DirectoryName.Replace( luaPath, string.Empty );
				string toPath = Application.dataPath.Replace( "/", "\\" ) + "\\Sources\\Lua" + subPath;
				string destName = fi.Name.Replace( ".lua", ".bytes" );
				string outputPath = toPath + "\\" + destName;

				StreamReader sr = fi.OpenText();
				tasks.Add( sr.ReadToEndAsync().ContinueWith( ( task, o ) =>
				 {
					 byte[] encrypted = AesUtil.AesEncrypt( Encoding.UTF8.GetBytes( task.Result ), AES_KEY, AES_IV );
					 File.WriteAllBytes( ( string )o, encrypted );
					 UnityEngine.Debug.Log( "Done:" + o );
				 }, outputPath ) );
			}
			System.Threading.Tasks.Task.WaitAll( tasks.ToArray() );
			AssetDatabase.Refresh();
		}

		public static void Build()
		{
			string luaCompilerPath = PlayerPrefs.GetString( "luajit_path" );
			string luaPath = PlayerPrefs.GetString( "lua_path" );

			DirectoryInfo di = new DirectoryInfo( luaCompilerPath );
			if ( string.IsNullOrEmpty( luaCompilerPath ) || !di.Exists )
				throw new Exception( "Luajit path not exist." );

			di = new DirectoryInfo( luaPath );
			if ( string.IsNullOrEmpty( luaPath ) || !di.Exists )
				throw new Exception( "Lua path not exist." );

			//md5
			Hashtable md5Map;
			string v = luaPath + "\\ver.txt";
			if ( File.Exists( v ) )
			{
				string vJson = File.ReadAllText( v );
				md5Map = ( Hashtable )MiniJSON.JsonDecode( vJson );
			}
			else
				md5Map = new Hashtable();

			string driver = luaCompilerPath.Substring( 0, luaCompilerPath.IndexOf( "\\", StringComparison.Ordinal ) ).ToLower();
			StringBuilder sb = new StringBuilder();
			sb.AppendLine( "@echo off" );
			sb.AppendLine( driver );
			sb.AppendLine( "cd " + luaCompilerPath );

			FileInfo[] fis = di.GetFiles( "*.lua", SearchOption.AllDirectories );
			foreach ( FileInfo fi in fis )
			{
				if ( fi.Extension != ".lua" )
					continue;
				string md5Digest = MD5Util.GetMd5HexDigest( fi );
				string subPath = fi.DirectoryName.Replace( luaPath, string.Empty );
				string toPath = Application.dataPath.Replace( "/", "\\" ) + "\\Sources\\Lua" + subPath;
				string destName = fi.Name.Replace( ".lua", ".bytes" );
				string dest = toPath + "\\" + destName;

				string key = subPath + "\\" + fi.Name;
				if ( md5Map.ContainsKey( key ) &&
					( string )md5Map[key] == md5Digest &&
					File.Exists( dest ) )
				{
					continue;
				}
				md5Map[key] = md5Digest;

				if ( !Directory.Exists( toPath ) )
					Directory.CreateDirectory( toPath );

				string cmd = $"luajit -b {fi.FullName} {dest}";

				sb.AppendLine( cmd );
				sb.AppendLine( "echo Compiled: " + subPath + "\\" + destName );

				//string bundlePath = subPath.Replace( "\\", "/" ) + "/";
				//assets.Add( "Assets/Sources/Lua" + bundlePath + destName );
				////assetNames.Add( "lua" + bundlePath + fi.Name.Replace( fi.Extension, string.Empty ) );
				////打成一个包
				//bundleNames.Add( "lua" );
			}

			string bat = luaPath + "\\Build.bat";
			File.WriteAllText( bat, sb.ToString() );

			RunCmd( bat );

			AssetDatabase.Refresh();

			List<string> assets = new List<string>();
			string assetPath = Application.dataPath.Replace( "/", "\\" ) + "\\Sources\\Lua";
			DirectoryInfo di2 = new DirectoryInfo( assetPath );
			DirectoryInfo[] di2S = di2.GetDirectories();
			foreach ( DirectoryInfo di3 in di2S )
			{
				assets.Add( "Assets/Sources/Lua/" + di3.Name );
			}

			FileInfo[] fi2S = di2.GetFiles();
			foreach ( FileInfo fi2 in fi2S )
			{
				if ( fi2.Extension == ".bytes" )
					assets.Add( "Assets/Sources/Lua/" + fi2.Name );
			}

			for ( int i = 0; i < assets.Count; i++ )
			{
				string assetPath2 = assets[i];
				WriteAssetBundleName( assetPath2, "lua" );
			}

			AssetDatabase.Refresh();

			string nv = MiniJSON.JsonEncode( md5Map );
			File.WriteAllText( v, nv );
		}

		private static void RunCmd( string cmd )
		{
			Process proc = new Process
			{
				StartInfo =
				{
					FileName = cmd,
				}
			};
			proc.Start();
			proc.WaitForExit();
			proc.Close();
		}

		public static void BuildIOS2()
		{
			//string luaCompilerPath = PlayerPrefs.GetString( "luac_path" );
			string luaCompilerPath = PlayerPrefs.GetString( "luajit_path" );
			string luaPath = PlayerPrefs.GetString( "lua_path" );

			DirectoryInfo di = new DirectoryInfo( luaCompilerPath );
			if ( string.IsNullOrEmpty( luaCompilerPath ) || !di.Exists )
				throw new Exception( "Luajit path not exist." );

			di = new DirectoryInfo( luaPath );
			if ( string.IsNullOrEmpty( luaPath ) || !di.Exists )
				throw new Exception( "Lua path not exist." );

			//md5
			Hashtable md5Map;
			string v = luaPath + "/ver.txt";
			if ( File.Exists( v ) )
			{
				string vJson = File.ReadAllText( v );
				md5Map = ( Hashtable )MiniJSON.JsonDecode( vJson );
			}
			else
				md5Map = new Hashtable();

			StringBuilder sb = new StringBuilder();
			//sb.AppendLine ("#!/bin/sh");
			FileInfo[] fis = di.GetFiles( "*.lua", SearchOption.AllDirectories );
			foreach ( FileInfo fi in fis )
			{
				if ( fi.Extension != ".lua" )
					continue;
				string md5Digest = MD5Util.GetMd5HexDigest( fi );
				string subPath = fi.DirectoryName.Replace( luaPath, string.Empty );
				string toPath = Application.dataPath + "/Sources/Lua" + subPath;
				string destName = fi.Name.Replace( ".lua", ".bytes" );
				string dest = toPath + "/" + destName;

				string key = subPath + "/" + fi.Name;
				if ( md5Map.ContainsKey( key ) &&
					( string )md5Map[key] == md5Digest &&
					File.Exists( dest ) )
				{
					continue;
				}
				md5Map[key] = md5Digest;

				if ( !Directory.Exists( toPath ) )
					Directory.CreateDirectory( toPath );

				string text = File.ReadAllText( fi.FullName );
				byte[] bytes = Encoding.UTF8.GetBytes( text );
				File.WriteAllText( fi.FullName, Encoding.UTF8.GetString( bytes ) );

				//string cmd = string.Format( "cp -f {0} {1}", fi.FullName, dest );
				string cmd = string.Format( "{0}/luajit -b {1} {2}", luaCompilerPath, fi.FullName, dest );
				//string cmd = string.Format( "{0}/luac -o {1} {2}", luaCompilerPath, dest, fi.FullName );

				sb.AppendLine( cmd );
				sb.AppendLine( "echo Compiled: " + subPath + "/" + destName );

				File.WriteAllBytes( dest, bytes );
			}

			const string bat = "Build.command";
			File.WriteAllText( luaPath + "/" + bat, sb.ToString() );

			RunCmd2( luaPath, bat );
			AssetDatabase.Refresh();

			List<string> assets = new List<string>();
			string assetPath = Application.dataPath + "/Sources/Lua";
			DirectoryInfo di2 = new DirectoryInfo( assetPath );
			DirectoryInfo[] di2S = di2.GetDirectories();
			foreach ( DirectoryInfo di3 in di2S )
			{
				assets.Add( "Assets/Sources/Lua/" + di3.Name );
			}

			FileInfo[] fi2S = di2.GetFiles();
			foreach ( FileInfo fi2 in fi2S )
			{
				if ( fi2.Extension == ".bytes" )
					assets.Add( "Assets/Sources/Lua/" + fi2.Name );
			}

			for ( int i = 0; i < assets.Count; i++ )
			{
				string assetPath2 = assets[i];
				WriteAssetBundleName( assetPath2, "lua" );
			}

			AssetDatabase.Refresh();

			string nv = MiniJSON.JsonEncode( md5Map );
			File.WriteAllText( v, nv );
		}

		private static void RunCmd2( string path, string fn )
		{
			ProcessStartInfo pst = new ProcessStartInfo();
			pst.FileName = "open";
			pst.WorkingDirectory = path;
			pst.Arguments = fn;
			pst.WindowStyle = ProcessWindowStyle.Normal;
			pst.CreateNoWindow = true;
			Process proc = new Process();
			proc.StartInfo = pst;
			proc.Start();
			proc.WaitForExit();
			proc.Close();
		}

		public static void BuildIOS()
		{
			//string luaCompilerPath = PlayerPrefs.GetString( "luac_path" );
			string luaCompilerPath = PlayerPrefs.GetString( "luajit_path" );
			string luaPath = PlayerPrefs.GetString( "lua_path" );

			DirectoryInfo di = new DirectoryInfo( luaCompilerPath );
			if ( string.IsNullOrEmpty( luaCompilerPath ) || !di.Exists )
				throw new Exception( "Luajit path not exist." );

			di = new DirectoryInfo( luaPath );
			if ( string.IsNullOrEmpty( luaPath ) || !di.Exists )
				throw new Exception( "Lua path not exist." );

			//md5
			Hashtable md5Map;
			string v = luaPath + "/ver.txt";
			if ( File.Exists( v ) )
			{
				string vJson = File.ReadAllText( v );
				md5Map = ( Hashtable )MiniJSON.JsonDecode( vJson );
			}
			else
				md5Map = new Hashtable();

			FileInfo[] fis = di.GetFiles( "*.lua", SearchOption.AllDirectories );
			foreach ( FileInfo fi in fis )
			{
				string md5Digest = MD5Util.GetMd5HexDigest( fi );
				string subPath = fi.DirectoryName.Replace( luaPath, string.Empty );
				string toPath = Application.dataPath + "/Sources/Lua" + subPath;
				string destName = fi.Name.Replace( ".lua", ".bytes" );
				string dest = toPath + "/" + destName;

				string key = subPath + "/" + fi.Name;
				if ( md5Map.ContainsKey( key ) &&
					( string )md5Map[key] == md5Digest &&
					File.Exists( dest ) )
				{
					continue;
				}
				md5Map[key] = md5Digest;

				if ( !Directory.Exists( toPath ) )
					Directory.CreateDirectory( toPath );

				string text = File.ReadAllText( fi.FullName );
				byte[] bytes = Encoding.UTF8.GetBytes( text );
				bytes = AesUtil.AesEncrypt( bytes );
				File.WriteAllBytes( dest, bytes );
			}

			AssetDatabase.Refresh();

			List<string> assets = new List<string>();
			string assetPath = Application.dataPath + "/Sources/Lua";
			DirectoryInfo di2 = new DirectoryInfo( assetPath );
			DirectoryInfo[] di2S = di2.GetDirectories();
			foreach ( DirectoryInfo di3 in di2S )
			{
				assets.Add( "Assets/Sources/Lua/" + di3.Name );
			}

			FileInfo[] fi2S = di2.GetFiles();
			foreach ( FileInfo fi2 in fi2S )
			{
				if ( fi2.Extension == ".bytes" )
					assets.Add( "Assets/Sources/Lua/" + fi2.Name );
			}

			for ( int i = 0; i < assets.Count; i++ )
			{
				string assetPath2 = assets[i];
				WriteAssetBundleName( assetPath2, "lua" );
			}

			AssetDatabase.Refresh();

			string nv = MiniJSON.JsonEncode( md5Map );
			File.WriteAllText( v, nv );
		}

		public static void WriteAssetBundleName( string assetPath, string assetBundleName )
		{
			AssetImporter importer = AssetImporter.GetAtPath( assetPath );
			importer.assetBundleName = assetBundleName.ToLower();
			AssetDatabase.SaveAssets();
			//DoAssetReimport( assetPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport );
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
	}
}