namespace FairyUGUI.Event
{
	public enum EventTriggerType
	{
		None = -1,
		PointerEnter = 0,
		PointerExit = 1,
		PointerDown = 2,
		PointerUp = 3,
		PointerClick = 4,
		Drag = 5,
		Drop = 6,
		Scroll = 7,
		UpdateSelected = 8,
		Select = 9,
		Deselect = 10,
		Move = 11,
		InitializePotentialDrag = 12,
		BeginDrag = 13,
		EndDrag = 14
	}

	public enum EventType
	{
		None,
		Click,
		Scroll,
		TouchBegin,
		TouchEnd,
		RollOver,
		RollOut,
		Move,
		AddToStage,
		RemoveFromStage,
		Select,
		Deselect,
		UpdateSelected,
		Keydown,
		Keyup,
		LinkClick,
		PositionChanged,
		SizeChanged,
		CullChanged,
		BeginDrag,
		EndDrag,
		InitializeDrag,
		Drag,
		Drop,
		Changed,
		EndEdit,
		PlayEnd,
		Submit,
		Cancel,
		ItemClick,
		VirtualItemChanged
	}

	public static class EventTypeMapping
	{
		public static EventType MappingTriggerTypeToEventTpye( EventTriggerType triggerType )
		{
			switch ( triggerType )
			{
				case EventTriggerType.PointerEnter:
					return EventType.RollOver;
				case EventTriggerType.PointerExit:
					return EventType.RollOut;
				case EventTriggerType.PointerDown:
					return EventType.TouchBegin;
				case EventTriggerType.PointerUp:
					return EventType.TouchEnd;
				case EventTriggerType.PointerClick:
					return EventType.Click;
				case EventTriggerType.Drag:
					return EventType.Drag;
				case EventTriggerType.Drop:
					return EventType.Drop;
				case EventTriggerType.Scroll:
					return EventType.Scroll;
				case EventTriggerType.UpdateSelected:
					return EventType.UpdateSelected;
				case EventTriggerType.Select:
					return EventType.Select;
				case EventTriggerType.Deselect:
					return EventType.Deselect;
				case EventTriggerType.Move:
					return EventType.Move;
				case EventTriggerType.InitializePotentialDrag:
					return EventType.InitializeDrag;
				case EventTriggerType.BeginDrag:
					return EventType.BeginDrag;
				case EventTriggerType.EndDrag:
					return EventType.EndDrag;
			}
			return EventType.None;
		}

		public static EventTriggerType MappingEventTpyeToTriggerType( EventType eventType )
		{
			switch ( eventType )
			{
				case EventType.Click:
					return EventTriggerType.PointerClick;
				case EventType.Scroll:
					return EventTriggerType.Scroll;
				case EventType.TouchBegin:
					return EventTriggerType.PointerDown;
				case EventType.TouchEnd:
					return EventTriggerType.PointerUp;
				case EventType.RollOver:
					return EventTriggerType.PointerEnter;
				case EventType.RollOut:
					return EventTriggerType.PointerExit;
				case EventType.Move:
					return EventTriggerType.Move;
				case EventType.Select:
					return EventTriggerType.Select;
				case EventType.Deselect:
					return EventTriggerType.Deselect;
				case EventType.UpdateSelected:
					return EventTriggerType.UpdateSelected;
				case EventType.BeginDrag:
					return EventTriggerType.BeginDrag;
				case EventType.EndDrag:
					return EventTriggerType.EndDrag;
				case EventType.InitializeDrag:
					return EventTriggerType.InitializePotentialDrag;
				case EventType.Drag:
					return EventTriggerType.Drag;
				case EventType.Drop:
					return EventTriggerType.Drop;
			}
			return EventTriggerType.None;
		}
	}
}