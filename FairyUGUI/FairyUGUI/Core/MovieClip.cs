using FairyUGUI.Event;
using FairyUGUI.UI;
using UnityEngine;
using EventType = FairyUGUI.Event.EventType;

namespace FairyUGUI.Core
{
	public class MovieClip : Image
	{
		public struct Frame
		{
			internal Rect rect;
			internal float addDelay;
			internal NSprite sprite;
		}

		internal float interval;
		internal bool swing;
		internal float repeatDelay;

		internal EventListener onPlayEnd { get; private set; }

		internal int frameCount => this.frames.Length;

		internal Frame[] frames { get; private set; }

		internal PlayState playState { get; private set; }

		private int _start;
		private int _end;
		private int _times;
		private int _endAt;
		private int _status; //0-none, 1-next loop, 2-ending, 3-ended

		private bool _playing;
		internal bool playing
		{
			get => this._playing;
			set => this._playing = value;
		}

		private int _currentFrame;
		internal int currentFrame
		{
			get => this._currentFrame;
			set
			{
				if ( this._currentFrame == value )
					return;
				this._currentFrame = value;
				if ( this._currentFrame >= this.frameCount )
					this._currentFrame %= this.frameCount;
				this.playState.currrentFrame = this._currentFrame;
				this.DrawFrame();
			}
		}

		internal MovieClip( GObject owner )
			: base( owner )
		{
			this.playState = new PlayState();
			this.interval = 0.1f;
			this._playing = true;

			this.onPlayEnd = new EventListener( this, EventType.PlayEnd );
		}

		protected override void InternalDispose()
		{
			this.frames = null;

			base.InternalDispose();
		}

		internal void SetData( PackageItem pkgItem )
		{
			this.frames = pkgItem.frames;
			this.swing = pkgItem.swing;
			this.interval = pkgItem.interval;
			this.repeatDelay = pkgItem.repeatDelay;

			this.SetPlaySettings();
			this.playState.Rewind();
			this.DrawFrame();
		}

		//从start帧开始，播放到end帧（-1表示结尾），重复times次（0表示无限循环），循环结束后，停止在endAt帧（-1表示参数end）
		internal void SetPlaySettings( int start = 0, int end = -1, int times = 0, int endAt = -1 )
		{
			this._start = start;
			this._end = end;
			this._times = times;
			this._endAt = endAt;
			if ( this._end == -1 || this._end > this.frameCount - 1 )
				this._end = this.frameCount - 1;
			if ( this._endAt == -1 || this._endAt > this.frameCount - 1 )
				this._endAt = this._end;
			this.currentFrame = start;
			this._status = 0;
		}

		private void DrawFrame()
		{
			if ( this._currentFrame < this.frameCount )
			{
				Frame frame = this.frames[this._currentFrame];
				this.customRect = frame.rect;
				this.nSprite = frame.sprite;
			}
		}

		protected internal override void Update( UpdateContext context )
		{
			base.Update( context );

			if ( this._playing && this.frameCount != 0 && this._status != 3 )
			{
				this.playState.Update( this, context );
				if ( this._currentFrame != this.playState.currrentFrame )
				{
					if ( this._status == 1 )
					{
						this._currentFrame = this._start;
						this.playState.currrentFrame = this._currentFrame;
						this._status = 0;
					}
					else if ( this._status == 2 )
					{
						this._currentFrame = this._endAt;
						this.playState.currrentFrame = this._currentFrame;
						this._status = 3;

						this.onPlayEnd.Call();
					}
					else
					{
						this._currentFrame = this.playState.currrentFrame;
						if ( this._currentFrame == this._end )
						{
							if ( this._times > 0 )
							{
								this._times--;
								this._status = this._times == 0 ? 2 : 1;
							}
						}
					}
					this.DrawFrame();
				}
			}
		}
	}
}