using DG.Tweening;
using UnityEngine;

namespace FairyUGUI.UI
{
	public class WindowScaleAnimation : IWindowAnimation
	{
		public bool reverse { get; set; }

		public bool keepOriginal { get; set; }

		public float duration { get; set; }

		public Ease ease { get; set; }

		public Window window { get; set; }

		public WindowScaleAnimation()
		{
			this.duration = 0.3f;
			this.ease = Ease.OutQuad;
		}

		public void Play( Window.AnimationCompleteHandler callback )
		{
			if ( !this.reverse )
			{
				if ( !this.keepOriginal )
					this.window.scale = new Vector2( 0.1f, 0.1f );
				this.window.pivot = new Vector2( 0.5f, 0.5f );
				this.window.TweenScale( Vector2.one, this.duration ).SetEase( this.ease ).OnComplete( () =>
				{
					this.window.pivot = new Vector2( 0, 1 );
					callback.Invoke();
				} );
			}
			else
			{
				if ( !this.keepOriginal )
					this.window.scale = Vector2.one;
				this.window.pivot = new Vector2( 0.5f, 0.5f );
				this.window.TweenScale( new Vector2( 0.1f, 0.1f ), this.duration ).SetEase( this.ease ).OnComplete( () =>
				{
					this.window.pivot = new Vector2( 0, 1 );
					callback.Invoke();
				} );
			}
		}

		public void Cancel( bool aniComplete )
		{
			DOTween.Kill( this.window, aniComplete );
		}
	}
}