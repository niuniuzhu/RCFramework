using FairyUGUI.Core;
using System.Collections.Generic;

namespace FairyUGUI.Event
{
	public static class ExecuteEvents
	{
		private static readonly List<DisplayObject> INTERNAL_TRANSFORM_LIST = new List<DisplayObject>();

		public static bool Execute( DisplayObject target, BaseEventData eventData, EventTriggerType type )
		{
			if ( target == null || target.stage == null || !target.visible || !target.touchable ||
				!target.HasEventTriggerType( type ) )
				return false;

			eventData.initiator = target;
			eventData.sender = target;
			eventData.eventPhase = BaseEventData.EventPhase.Bubble;
			target.TriggerEvent( eventData, type );
			return true;
		}

		public static DisplayObject ExecuteHierarchy( DisplayObject root, BaseEventData eventData, EventTriggerType type )
		{
			GetEventHandlerChain( root, type, INTERNAL_TRANSFORM_LIST );

			eventData.eventPhase = BaseEventData.EventPhase.Bubble;
			eventData.initiator = null;

			int count = INTERNAL_TRANSFORM_LIST.Count;
			for ( int i = 0; i < count; i++ )
			{
				DisplayObject target = INTERNAL_TRANSFORM_LIST[i];

				if ( eventData.initiator == null )
					eventData.initiator = target;

				eventData.sender = target;
				target.TriggerEvent( eventData, type );

				if ( eventData.stopPropagation )
					break;
			}
			return eventData.initiator;
		}

		/// <summary>
		/// Bubble the specified event on the game object, figuring out which object will actually receive the event.
		/// </summary>
		public static DisplayObject GetEventHandler( DisplayObject root, EventTriggerType type )
		{
			if ( root == null )
				return null;

			DisplayObject d = root;
			while ( d != null )
			{
				if ( d.stage != null && d.visible && d.touchable && d.HasEventTriggerType( type ) )
					return d;
				d = d.parent;
			}
			return null;
		}

		public static void GetEventHandlerChain( DisplayObject root, EventTriggerType type, IList<DisplayObject> handlerChain )
		{
			handlerChain.Clear();
			if ( root == null )
				return;

			DisplayObject d = root;
			while ( d != null )
			{
				if ( d.stage != null && d.visible && d.touchable && d.HasEventTriggerType( type ) )
					handlerChain.Add( d );
				d = d.parent;
			}
		}
	}
}
