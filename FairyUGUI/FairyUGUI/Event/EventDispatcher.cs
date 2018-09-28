using System.Collections.Generic;
using FairyUGUI.Core;
using FairyUGUI.UI;
using Game.Pool;

namespace FairyUGUI.Event
{
	public class EventDispatcher
	{
		internal bool disposed { get; private set; }

		private Dictionary<EventType, EventBridge> _dic;

		internal EventBridge GetEventBridge( EventType strType )
		{
			if ( this._dic == null )
				this._dic = new Dictionary<EventType, EventBridge>();

			if ( !this._dic.TryGetValue( strType, out EventBridge bridge ) )
			{
				bridge = new EventBridge( this );
				this._dic[strType] = bridge;
			}
			return bridge;
		}

		private EventBridge TryGetEventBridge( EventType strType )
		{
			if ( this._dic == null )
				return null;

			this._dic.TryGetValue( strType, out EventBridge bridge );
			return bridge;
		}

		internal EventBridge TryGetEventBridge( EventTriggerType triggerType )
		{
			if ( this._dic == null )
				return null;

			EventType strType = EventTypeMapping.MappingTriggerTypeToEventTpye( triggerType );

			this._dic.TryGetValue( strType, out EventBridge bridge );
			return bridge;
		}

		public void DispatchEvent( EventType strType, object data = null )
		{
			this.InternalCall( this.TryGetEventBridge( strType ), strType, data );
		}

		public void DispatchEvent( EventContext context )
		{
			EventBridge bridge = this.TryGetEventBridge( context.type );
			EventBridge gBridge = null;
			if ( ( this is DisplayObject ) && ( ( DisplayObject )this ).gOwner != null )
				gBridge = ( ( DisplayObject )this ).gOwner.TryGetEventBridge( context.type );

			EventDispatcher savedSender = context.sender;

			if ( bridge != null && !bridge.isEmpty )
			{
				bridge.CallCaptureInternal( context );
				bridge.CallInternal( context );
			}

			if ( gBridge != null && !gBridge.isEmpty )
			{
				gBridge.CallCaptureInternal( context );
				gBridge.CallInternal( context );
			}

			context.sender = savedSender;
		}

		internal void InternalSimpleCall( EventBridge bridge, EventType type, object data = null, BaseEventData eventData = null )
		{
			if ( bridge == null || bridge.isEmpty )
				return;

			EventContext context = EventContext.Get();
			context.initiator = this;
			context.eventData = eventData;
			context.data = data;

			bridge.CallCaptureInternal( context );
			bridge.CallInternal( context );

			context.Release();
		}

		internal void InternalCall( EventBridge bridge, EventType type, object data = null, BaseEventData eventData = null )
		{
			EventBridge gBridge = null;
			if ( ( this is DisplayObject ) && ( ( DisplayObject )this ).gOwner != null )
				gBridge = ( ( DisplayObject )this ).gOwner.TryGetEventBridge( type );

			bool b1 = bridge != null && !bridge.isEmpty;
			bool b2 = gBridge != null && !gBridge.isEmpty;
			if ( b1 || b2 )
			{
				EventContext context = EventContext.Get();
				context.initiator = this;
				context.eventData = eventData;
				context.data = data;

				if ( b1 )
				{
					bridge.CallCaptureInternal( context );
					bridge.CallInternal( context );
				}

				if ( b2 )
				{
					gBridge.CallCaptureInternal( context );
					gBridge.CallInternal( context );
				}

				context.Release();
			}
		}

		public void BroadcastEvent( EventType strType, object data )
		{
			EventContext context = EventContext.Get();
			context.initiator = this;
			context.type = strType;
			context.data = data;

			List<EventBridge> bubbleChain = ListPool<EventBridge>.Get();

			if ( this is Container container )
				GetChildEventBridges( strType, container, bubbleChain );
			else
			{
				if ( this is GComponent component )
					GetChildEventBridges( strType, component, bubbleChain );
			}

			int length = bubbleChain.Count;
			for ( int i = 0; i < length; ++i )
				bubbleChain[i].CallInternal( context );

			bubbleChain.Clear();
			ListPool<EventBridge>.Release( bubbleChain );

			context.Release();
		}

		static void GetChildEventBridges( EventType strType, Container container, List<EventBridge> bridges )
		{
			EventBridge bridge = container.TryGetEventBridge( strType );
			if ( bridge != null )
				bridges.Add( bridge );
			if ( container.gOwner != null )
			{
				bridge = container.gOwner.TryGetEventBridge( strType );
				if ( bridge != null && !bridge.isEmpty )
					bridges.Add( bridge );
			}

			int count = container.numChildren;
			for ( int i = 0; i < count; ++i )
			{
				DisplayObject obj = container.GetChildAt( i );
				if ( obj is Container container1 )
					GetChildEventBridges( strType, container1, bridges );
				else
				{
					bridge = obj.TryGetEventBridge( strType );
					if ( bridge != null && !bridge.isEmpty )
						bridges.Add( bridge );

					if ( obj.gOwner != null )
					{
						bridge = obj.gOwner.TryGetEventBridge( strType );
						if ( bridge != null && !bridge.isEmpty )
							bridges.Add( bridge );
					}
				}
			}
		}

		static void GetChildEventBridges( EventType strType, GComponent component, List<EventBridge> bridges )
		{
			EventBridge bridge = component.TryGetEventBridge( strType );
			if ( bridge != null )
				bridges.Add( bridge );

			int count = component.numChildren;
			for ( int i = 0; i < count; ++i )
			{
				GObject obj = component.GetChildAt( i );
				GComponent component1 = obj as GComponent;
				if ( component1 != null )
					GetChildEventBridges( strType, component1, bridges );
				else
				{
					bridge = obj.TryGetEventBridge( strType );
					if ( bridge != null )
						bridges.Add( bridge );
				}
			}
		}

		public void RemoveEventListeners()
		{
			if ( this._dic == null )
				return;

			foreach ( KeyValuePair<EventType, EventBridge> kv in this._dic )
				kv.Value.Clear();
		}

		public void RemoveEventListeners( EventType type )
		{
			if ( this._dic == null )
				return;

			if ( this._dic.TryGetValue( type, out EventBridge bridge ) )
				bridge.Clear();
		}

		public void Dispose()
		{
			if ( this.disposed )
				return;

			this.InternalDispose();
			this.RemoveEventListeners();
			this._dic = null;
			this.disposed = true;
		}

		protected virtual void InternalDispose()
		{
		}
	}
}