using UnityEngine;

namespace FairyUGUI.Core
{
	public class PlayState
	{
		internal bool reachEnding { get; private set; } //是否已播放到结尾
		internal bool reversed { get; private set; } //是否已反向播放
		internal int repeatedCount { get; private set; } //重复次数

		internal bool ignoreTimeScale = true; //是否忽略TimeScale的影响，即在TimeScale改变后依然保持原有的播放速度

		int _curFrame; //当前帧
		float _lastTime;
		float _curFrameDelay; //当前帧延迟

		internal void Update( MovieClip mc, UpdateContext context )
		{
			float time = Time.time;
			float elapsed = time - this._lastTime;
			if ( this.ignoreTimeScale && Time.timeScale != 0 )
				elapsed /= Time.timeScale;
			this._lastTime = time;

			this.reachEnding = false;
			this._curFrameDelay += elapsed;
			float interval = mc.interval + mc.frames[this._curFrame].addDelay + ( ( this._curFrame == 0 && this.repeatedCount > 0 ) ? mc.repeatDelay : 0 );
			if ( this._curFrameDelay < interval )
				return;

			this._curFrameDelay = 0;
			if ( mc.swing )
			{
				if ( this.reversed )
				{
					this._curFrame--;
					if ( this._curFrame < 0 )
					{
						this._curFrame = Mathf.Min( 1, mc.frameCount - 1 );
						this.repeatedCount++;
						this.reversed = !this.reversed;
					}
				}
				else
				{
					this._curFrame++;
					if ( this._curFrame > mc.frameCount - 1 )
					{
						this._curFrame = Mathf.Max( 0, mc.frameCount - 2 );
						this.repeatedCount++;
						this.reachEnding = true;
						this.reversed = !this.reversed;
					}
				}
			}
			else
			{
				this._curFrame++;
				if ( this._curFrame > mc.frameCount - 1 )
				{
					this._curFrame = 0;
					this.repeatedCount++;
					this.reachEnding = true;
				}
			}
		}

		internal int currrentFrame
		{
			get => this._curFrame;
			set
			{
				this._curFrame = value;
				this._curFrameDelay = 0;
			}
		}

		internal void Rewind()
		{
			this._curFrame = 0;
			this._curFrameDelay = 0;
			this.reversed = false;
			this.reachEnding = false;
		}

		internal void Reset()
		{
			this._curFrame = 0;
			this._curFrameDelay = 0;
			this.repeatedCount = 0;
			this.reachEnding = false;
			this.reversed = false;
		}

		internal void Copy( PlayState src )
		{
			this._curFrame = src._curFrame;
			this._curFrameDelay = src._curFrameDelay;
			this.repeatedCount = src.repeatedCount;
			this.reachEnding = src.reachEnding;
			this.reversed = src.reversed;
		}
	}
}
