namespace Game.Task
{
	public class TimerEntry
	{
		public delegate void TimerHandler( int index, float dt, object param );
		public delegate void CompleteHandler();
		public TimerHandler timerCallback { get; private set; }

		private readonly float _interval;
		private readonly int _repeat;
		private readonly CompleteHandler _completCallback;
		private readonly object _param;
		private float _sum;
		private int _count;

		public bool finished { get; private set; }

		public TimerEntry( float interval, int repeat, bool startImmediately, TimerHandler timerCallback, CompleteHandler completCallback, object param = null )
		{
			this._interval = interval;
			this._repeat = repeat;
			this.timerCallback = timerCallback;
			this._completCallback = completCallback;
			this._param = param;
			if ( startImmediately )
			{
			    this.timerCallback?.Invoke( this._count, 0, this._param );

			    ++this._count;

				if ( this._repeat > 0 && this._count == this._repeat )
				{
					this.finished = true;
				    this._completCallback?.Invoke();
				}
			}
		}

		internal void OnUpdate( float deltaTime )
		{
			if ( this.finished )
				return;

			this._sum += deltaTime;
			while ( this._sum >= this._interval )
			{
			    this.timerCallback?.Invoke( this._count, this._interval, this._param );

			    ++this._count;

				this._sum -= this._interval;

				if ( this._repeat > 0 && this._count == this._repeat )
				{
					this.finished = true;

				    this._completCallback?.Invoke();

				    return;
				}
			}
		}
	}
}