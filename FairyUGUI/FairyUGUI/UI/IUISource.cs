namespace FairyUGUI.UI
{
	public delegate void UILoadCallback();

	public interface IUISource
	{
		bool loaded { get; }

		void Load( UILoadCallback callback );
	}
}
