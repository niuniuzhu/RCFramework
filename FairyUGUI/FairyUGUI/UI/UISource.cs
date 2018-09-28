namespace FairyUGUI.UI
{
	public class UISource : IUISource
	{
		public string assetPath { get; private set; }

		public bool loaded => UIPackage.GetById( this.assetPath ) != null;

		public UISource( string assetPath )
		{
			this.assetPath = assetPath;
		}

		public void Load( UILoadCallback callback )
		{
			UIPackage.AddPackage( this.assetPath );
			callback.Invoke();
		}
	}
}