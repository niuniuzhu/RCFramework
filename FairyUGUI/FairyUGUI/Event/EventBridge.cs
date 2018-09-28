namespace FairyUGUI.Event
{
	class EventBridge
	{
		private readonly EventDispatcher _owner;
		private EventHandler _handler;
		private EventHandler _captureHandler;

		public bool dispatching { get; private set; }

		public EventBridge( EventDispatcher owner )
		{
			this._owner = owner;
		}

		public void AddCapture( EventHandler callback )
		{
			this._captureHandler -= callback;
			this._captureHandler += callback;
		}

		public void RemoveCapture( EventHandler callback )
		{
			this._captureHandler -= callback;
		}

		public void Add( EventHandler callback )
		{
			this._handler -= callback;
			this._handler += callback;
		}

		public void Remove( EventHandler callback )
		{
			this._handler -= callback;
		}

		public bool isEmpty => this._handler == null && this._captureHandler == null;

		public void Clear()
		{
			this._handler = null;
			this._captureHandler = null;
		}

		public void CallInternal( EventContext context )
		{
			if ( this.dispatching )
				return;

			this.dispatching = true;
			context.sender = this._owner;
			try
			{
			    this._handler?.Invoke( context );
			}
			finally
			{
				this.dispatching = false;
			}
		}

		public void CallCaptureInternal( EventContext context )
		{
			if ( this._captureHandler == null )
				return;

			if ( this.dispatching )
				return;

			this.dispatching = true;
			context.sender = this._owner;
			try
			{
				this._captureHandler.Invoke( context );
			}
			finally
			{
				this.dispatching = false;
			}
		}
	}
}
