namespace FairyUGUI.Event
{
	public delegate void EventHandler( EventContext context );

	public class EventListener
	{
		private readonly EventDispatcher _owner;
		private readonly EventType _type;
		private readonly EventBridge _bridge;

		public EventListener( EventDispatcher owner, EventType type )
		{
			this._owner = owner;
			this._type = type;
			this._bridge = this._owner.GetEventBridge( this._type );
		}

		public void Add( EventHandler handler )
		{
			this._bridge.Add( handler );
		}

		public void Remove( EventHandler handler )
		{
			this._bridge.Remove( handler );
		}

		public void Clear()
		{
			this._bridge.Clear();
		}

		public void SimpleCall( object data = null, BaseEventData eventData = null )
		{
			this._owner.InternalSimpleCall( this._bridge, this._type, data, eventData );
		}

		public void Call( object data = null, BaseEventData eventData = null )
		{
			this._owner.InternalCall( this._bridge, this._type, data, eventData );
		}

		public void BroadcastCall( object data = null )
		{
			this._owner.BroadcastEvent( this._type, data );
		}
	}
}
