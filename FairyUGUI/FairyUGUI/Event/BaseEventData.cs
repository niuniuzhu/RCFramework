using FairyUGUI.Core;

namespace FairyUGUI.Event
{
	public class BaseEventData
	{
		public enum EventPhase
		{
			Bubble,
			Capture
		}

		public DisplayObject selectedObject
		{
			get => EventSystem.instance.currentSelectedGameObject;
			set => EventSystem.instance.SetSelectedGameObject( value, this );
		}

		public EventTriggerType type { get; internal set; }

		public DisplayObject initiator { get; internal set; }

		public DisplayObject sender { get; internal set; }

		public EventPhase eventPhase { get; internal set; }

		public bool stopPropagation { get; private set; }

		public bool preventDefault { get; private set; }

		public virtual void Reset()
		{
			this.initiator = null;
			this.stopPropagation = false;
			this.preventDefault = false;
			this.sender = null;
		}

		public virtual void PreventDefault()
		{
			this.preventDefault = true;
			//this.initiator = null;
			//this.sender = null;
		}

		public void StopPropagation()
		{
			this.stopPropagation = true;
		}
	}
}