using Game.Pool;

namespace FairyUGUI.Event
{
	public class EventContext : StackPool<EventContext>
	{
		public BaseEventData eventData { get; internal set; }

		public EventType type { get; internal set; }

		public EventDispatcher sender { get; internal set; }

		public object initiator { get; internal set; }

		public object data;

		protected override void Reset()
		{
			this.eventData = null;
			this.sender = null;
			this.initiator = null;
		}

		public void StopPropagation()
		{
		    this.eventData?.StopPropagation();
		}

	    public void PreventDefault()
	    {
	        this.eventData?.PreventDefault();
	    }
	}
}
