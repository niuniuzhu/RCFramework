namespace Game.Loader
{
	public delegate void HttpCompleteHandler( object sender, byte[] bytes, string text, object data = null );
	public delegate void AssetsCompleteHandler( object sender, AssetsProxy assetsProxy, object data = null );
	public delegate void BatchSingleCompleteHandler( object sender, AssetsProxy assetsProxy, IBatchLoader loader, object data = null );
	public delegate void CompleteHandler( object sender, object data = null );
	public delegate void ErrorHandler( object sender, string msg, object data = null );
	public delegate void ProgressHandler( object sender, float progress );
	public delegate void BatchProgressHandler( object sender, float progress, IBatchLoader loader );
}