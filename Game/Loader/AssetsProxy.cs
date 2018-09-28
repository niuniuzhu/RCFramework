using System;
using UnityEngine;

namespace Game.Loader
{
	public class AssetsProxy
	{
		public AssetBundle assetBundle { get; private set; }

		public string name { get; private set; }

		private readonly bool _autoUnload;

		public AssetsProxy( string name, AssetBundle assetBundle, bool autoUnload )
		{
			this.name = name;
			this.assetBundle = assetBundle;
			this._autoUnload = autoUnload;
		}

		public bool Destroy( bool unloadAllLoadedObjects )
		{
			if ( !this._autoUnload )
				return false;
			this.assetBundle.Unload( unloadAllLoadedObjects );
			return true;
		}

		public T LoadAsset<T>( string name ) where T : UnityEngine.Object
		{
			return this.assetBundle.LoadAsset<T>( name );
		}

		public UnityEngine.Object LoadAsset( string name )
		{
			return this.assetBundle.LoadAsset( name );
		}

		public UnityEngine.Object LoadAsset( string name, Type type )
		{
			return this.assetBundle.LoadAsset( name, type );
		}
	}
}