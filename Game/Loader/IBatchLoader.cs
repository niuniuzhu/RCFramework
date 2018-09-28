namespace Game.Loader
{
	public interface IBatchLoader
	{
		void Load( AssetsCompleteHandler completeHandler, ProgressHandler progressHandler, ErrorHandler errorHandler, bool useWWW, bool fromCache, bool sync );

		void Cancel();

		AssetsCompleteHandler completeHandler { get; set; }

		ProgressHandler progressHandler { get; set; }

		ErrorHandler errorHandler { get; set; }
	}
}