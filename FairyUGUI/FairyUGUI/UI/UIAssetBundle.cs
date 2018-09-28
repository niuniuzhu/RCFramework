using Game.Loader;
using Logger = Core.Misc.Logger;

namespace FairyUGUI.UI
{
	public class UIAssetBundle : IUISource
	{
		public string assetBundlePath { get; private set; }

		public bool loaded => UIPackage.GetById( this.assetBundlePath ) != null;

		private bool _loading;

		public UIAssetBundle( string assetBundlePath )
		{
			this.assetBundlePath = assetBundlePath;
		}

		public void Load( UILoadCallback callback )
		{
			if ( this._loading || this.loaded )
				return;

			this._loading = true;

			UILoader loader = new UILoader( this.assetBundlePath, callback );
			loader.Load( this.OnComplete, null, this.OnError );
		}

		private void OnComplete( object sender, AssetsProxy assetsProxy, object data )
		{
			UILoader loader = ( UILoader )sender;
			UILoadCallback callback = ( UILoadCallback )loader.data;
			callback.Invoke();
		}

		private void OnError( object sender, string msg, object data )
		{
			Logger.Log( msg );
		}
	}
}