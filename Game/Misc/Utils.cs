using System;
using System.IO;
using Core.Misc;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Misc
{
	public static class Utils
	{
		public static string MakeRidFromID( string id )
		{
			return id + "@" + GuidHash.GetString();
		}

		public static string GetIDFromRID( string rid )
		{
			int pos = rid.IndexOf( "@", StringComparison.Ordinal );
			string id = pos != -1 ? rid.Substring( 0, pos ) : rid;
			return id;
		}

		public static void AddChild( Transform parent, Transform child, bool worldPositionStays, bool autoRenameLayer, bool deep )
		{
			child.SetParent( parent, worldPositionStays );
			if ( autoRenameLayer )
			{
				int layer = parent.gameObject.layer;
				child.gameObject.layer = layer;
				if ( deep )
				{
					Transform[] trs = child.GetComponentsInChildren<Transform>( true );
					foreach ( Transform tr in trs )
						tr.gameObject.layer = layer;
				}
			}
		}

		public static void SetLayer( GameObject go, int layer )
		{
			Transform[] transforms = go.GetComponentsInChildren<Transform>( true );
			foreach ( Transform t in transforms )
				t.gameObject.layer = layer;
		}

		public static void SetLayer( GameObject go, string name )
		{
			SetLayer( go, LayerMask.NameToLayer( name ) );
		}

		public static void SetShadowMode( GameObject go, ShadowCastingMode shadowCastingMode )
		{
			Renderer[] renderers = go.GetComponentsInChildren<Renderer>( true );
			foreach ( Renderer renderer in renderers )
				renderer.shadowCastingMode = shadowCastingMode;
		}

		public static void SetReceivedShadow( GameObject go, bool value )
		{
			Renderer[] renderers = go.GetComponentsInChildren<Renderer>( true );
			foreach ( Renderer renderer in renderers )
				renderer.receiveShadows = value;
		}

		public static string GetTerminalID()
		{
			string perPath = Application.persistentDataPath;
			if ( string.IsNullOrEmpty( perPath ) )
				perPath = Application.dataPath;
			if ( !string.IsNullOrEmpty( perPath ) ) //有持久数据目录
			{
				string file = Path.Combine( perPath, "terminal" );
				string tid = null;
				if ( File.Exists( file ) )
					tid = File.ReadAllText( file ).Trim();
				if ( string.IsNullOrEmpty( tid ) )
				{
					tid = Guid.NewGuid().ToString();
					File.WriteAllText( file, tid );
				}
				return tid;
			}
			return Guid.NewGuid().ToString();
		}
	}
}