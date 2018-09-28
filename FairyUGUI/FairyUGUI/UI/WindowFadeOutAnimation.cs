using DG.Tweening;

namespace FairyUGUI.UI
{
	public class WindowFadeAnimation : IWindowAnimation
	{
		public bool reverse { get; set; }

		public bool keepOriginal { get; set; }

		public float duration { get; set; }

		public Ease ease { get; set; }

		public Window window { get; set; }

		public WindowFadeAnimation()
		{
			this.duration = 0.3f;
			this.ease = Ease.OutQuad;
		}

		public void Play( Window.AnimationCompleteHandler callback )
		{
			if ( !this.reverse )
			{
				if ( !this.keepOriginal )
					this.window.alpha = 0f;
				this.window.TweenFade( 1f, this.duration ).SetEase( this.ease ).OnComplete( callback.Invoke );
			}
			else
			{
				if ( !this.keepOriginal )
					this.window.alpha = 1f;
				this.window.TweenFade( 0f, this.duration ).SetEase( this.ease ).OnComplete( callback.Invoke );
			}
		}

		public void Cancel( bool aniComplete )
		{
			DOTween.Kill( this.window, aniComplete );
		}
	}
}