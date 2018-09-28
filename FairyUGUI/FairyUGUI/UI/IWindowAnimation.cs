using DG.Tweening;

namespace FairyUGUI.UI
{
	public interface IWindowAnimation
	{
		bool reverse { get; set; }

		bool keepOriginal { get; set; }

		float duration { get; set; }

		Ease ease { get; set; }

		Window window { get; set; }

		void Play( Window.AnimationCompleteHandler callback );

		void Cancel( bool aniComplete );
	}
}