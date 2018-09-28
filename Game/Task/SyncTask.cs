using System.Collections;

namespace Game.Task
{
	public class SyncTask : IEnumerator
	{
		public static SyncTask Create( IEnumerator c, bool autoStart = true )
		{
			SyncTask task = new SyncTask( c, autoStart );
			return task;
		}

		private readonly TaskState _task;

		public bool running => this._task.running;

		public bool paused => this._task.paused;

		public delegate void CompleteHandler( bool manual );

		public CompleteHandler _completeInvoker;
		public event CompleteHandler OnComplete
		{
			add => this._completeInvoker += value;
			remove => this._completeInvoker -= value;
		}

		private SyncTask( IEnumerator c, bool autoStart = true )
		{
			this._task = new TaskState( c );
			this._task.OnComplete += this.TaskComplete;
			if ( autoStart )
				this.Start();
		}

		public void Start()
		{
			this._task.Start();
		}

		public void Stop()
		{
			this._task.Stop();
		}

		public void Pause()
		{
			this._task.Pause();
		}

		public void Resume()
		{
			this._task.Resume();
		}

		private void TaskComplete( bool manual )
		{
			this._task.OnComplete -= this.TaskComplete;
            this._completeInvoker?.Invoke(manual);
        }

		public static SyncTask Create( object delayCallback )
		{
			throw new System.NotImplementedException();
		}

		public bool MoveNext()
		{
			return this._task.MoveNext();
		}

		public void Reset()
		{
			this._task.Reset();
		}

		public object Current => this._task.Current;
	}

	class TaskState : IEnumerator
	{
		internal bool running { get; private set; }

		internal bool paused { get; private set; }

		internal delegate void FinishedHandler( bool manual );

		private FinishedHandler _completeInvoker;
		internal event FinishedHandler OnComplete
		{
			add => this._completeInvoker += value;
			remove => this._completeInvoker -= value;
		}

		internal readonly IEnumerator coroutine;
		private bool _stopped;

		internal TaskState( IEnumerator c )
		{
			this.coroutine = c;
		}

		internal void Pause()
		{
			this.paused = true;
		}

		internal void Resume()
		{
			this.paused = false;
		}

		internal void Start()
		{
			this.running = true;
			TaskManager.instance.StartCoroutine( this.CallWrapper() );
		}

		internal void Stop()
		{
			this._stopped = true;
			this.running = false;
		}

		private IEnumerator CallWrapper()
		{
			yield return null;
			IEnumerator e = this.coroutine;
			while ( this.running )
			{
				if ( this.paused )
					yield return null;
				else
				{
					if ( e != null && e.MoveNext() )
						yield return e.Current;
					else
						this.running = false;
				}
			}

            this._completeInvoker?.Invoke(this._stopped);
        }

		public bool MoveNext()
		{
			return this.coroutine.MoveNext();
		}

		public void Reset()
		{
			this.coroutine.Reset();
		}

		public object Current => this.coroutine.Current;
	}
}