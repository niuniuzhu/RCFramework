namespace Game.Task
{
	public class ScheduleEntry
	{
		public delegate void ScheduleHandler( int index, float dt, object param );
		public delegate void CompleteHandler();

		public ScheduleHandler scheduleCallback { get; private set; }

		private readonly float[] _times;
		private readonly CompleteHandler _completCallback;
		private readonly object _param;
		private int _index;

		public bool finished { get; private set; }

		private float _timeStamp;

		public ScheduleEntry( float[] times, ScheduleHandler scheduleCallback, CompleteHandler completCallback, object param = null )
		{
			this._times = times;
			this.scheduleCallback = scheduleCallback;
			this._completCallback = completCallback;
			this._timeStamp = 0f;
			this._param = param;
		}

		internal void OnUpdate( float deltaTime )
		{
			if ( this.finished )
				return;

			this._timeStamp += deltaTime;
			int count = this._times.Length;
			for ( int i = this._index; i < count; i++ )
			{
				float t = this._times[i];
				if ( this._timeStamp >= t )
				{
				    this.scheduleCallback?.Invoke( this._index, i == 0 ? t : t - this._times[i - 1], this._param );
				    ++this._index;
				}
				else
					break;
			}
			if ( this._index == count )
			{
				this.finished = true;

			    this._completCallback?.Invoke();
			}
		}
	}
}