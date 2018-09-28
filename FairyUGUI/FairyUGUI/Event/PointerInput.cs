using System.Collections.Generic;
using System.Text;
using FairyUGUI.Core;
using FairyUGUI.UI;
using UnityEngine;

namespace FairyUGUI.Event
{
	public class PointerInput
	{
		public const int K_MOUSE_LEFT_ID = 0;
		public const int K_MOUSE_RIGHT_ID = 1;
		public const int K_MOUSE_MIDDLE_ID = 2;

		public DisplayObject currentFocusedGameObject { get; private set; }

		private readonly MouseState _mouseState = new MouseState();
		private readonly Dictionary<int, PointerEventData> _pointerData = new Dictionary<int, PointerEventData>();
		private readonly List<DisplayObject> _tmpHovered = new List<DisplayObject>();
		private BaseEventData _baseEventData;

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach ( KeyValuePair<int, PointerEventData> kv in this._pointerData )
			{
				sb.AppendLine( kv.Value.ToString() );
			}
			return sb.ToString();
		}

		public bool IsMouseOverGameObject( int mouseId )
		{
			PointerEventData lastPointer = this.GetLastPointerEventData( mouseId );
			return lastPointer?.pointerEnter != null;
		}

		public bool IsPointerOverGameObject( int pointerId )
		{
			PointerEventData lastPointer = this.GetLastPointerEventData( pointerId );
			return lastPointer?.pointerEnter != null;
		}

		private void RemovePointerData( PointerEventData data )
		{
			this._pointerData.Remove( data.pointerId );
		}

		public void Process()
		{
			this.SendUpdateEventToSelectedObject();

			if ( !this.ProcessTouchEvents() && Input.mousePresent )
				this.ProcessMouseEvent();
		}

		private bool ProcessTouchEvents()
		{
			for ( int i = 0; i < Input.touchCount; ++i )
			{
				Touch touch = Input.GetTouch( i );

				if ( touch.type == TouchType.Indirect )
					continue;

				PointerEventData pointer = this.GetTouchPointerEventData( touch, out bool pressed, out bool released );

				this.ProcessTouchPress( pointer, pressed, released );

				if ( !released )
				{
					this.ProcessMove( pointer );
					this.ProcessDrag( pointer );
				}
				else
					this.RemovePointerData( pointer );
			}
			return Input.touchCount > 0;
		}

		private void ProcessTouchPress( PointerEventData pointerEvent, bool pressed, bool released )
		{
			DisplayObject currentOverObj = pointerEvent.pointerCurrentRaycast.displayObject;

			// PointerDown notification
			if ( pressed )
			{
				pointerEvent.eligibleForClick = true;
				pointerEvent.delta = Vector2.zero;
				pointerEvent.dragging = false;
				pointerEvent.useDragThreshold = true;
				pointerEvent.pressPosition = pointerEvent.position;
				pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

				this.DeselectIfSelectionChanged( currentOverObj, pointerEvent );

				if ( pointerEvent.pointerEnter != currentOverObj )
				{
					this.HandlePointerExitAndEnter( pointerEvent, currentOverObj );
					pointerEvent.pointerEnter = currentOverObj;
				}

				DisplayObject newPressed = ExecuteEvents.ExecuteHierarchy( currentOverObj, pointerEvent,
																		   EventTriggerType.PointerDown );

				//先把能处理click事件的对象保存起来
				ExecuteEvents.GetEventHandlerChain( currentOverObj, EventTriggerType.PointerClick, pointerEvent.clicked );

				float time = Time.unscaledTime;

				if ( newPressed == pointerEvent.lastDown )
				{
					float diffTime = time - pointerEvent.clickTime;
					if ( diffTime < 0.3f )
						++pointerEvent.clickCount;
					else
						pointerEvent.clickCount = 1;

					pointerEvent.clickTime = time;
				}
				else
					pointerEvent.clickCount = 1;

				pointerEvent.pointerClick = pointerEvent.clicked.Count > 0 ? pointerEvent.clicked[0] : null;
				pointerEvent.pointerDown = newPressed;
				pointerEvent.rawPointerPress = currentOverObj;

				pointerEvent.clickTime = time;

				// 同时把drag对象保存起来
				pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler( currentOverObj, EventTriggerType.Drag );

				ExecuteEvents.ExecuteHierarchy( pointerEvent.pointerDrag, pointerEvent, EventTriggerType.InitializePotentialDrag );
			}

			// PointerUp notification
			if ( released )
			{
				ExecuteEvents.ExecuteHierarchy( pointerEvent.pointerDown, pointerEvent, EventTriggerType.PointerUp );

				// 获取当前over对象能接收click事件的链
				ExecuteEvents.GetEventHandlerChain( currentOverObj, EventTriggerType.PointerClick, pointerEvent.released );

				// 和之前的click链比较，如果存在则执行click事件
				if ( pointerEvent.eligibleForClick && pointerEvent.clicked.Count > 0 )
				{
					int count = pointerEvent.released.Count;
					for ( int i = 0; i < count; i++ )
					{
						DisplayObject pointerUpHandler = pointerEvent.released[i];
						if ( pointerEvent.clicked.Contains( pointerUpHandler ) )
							ExecuteEvents.Execute( pointerUpHandler, pointerEvent, EventTriggerType.PointerClick );
					}
				}
				// Drop events
				if ( pointerEvent.pointerDrag != null && pointerEvent.dragging )
					ExecuteEvents.ExecuteHierarchy( currentOverObj, pointerEvent, EventTriggerType.Drop );

				pointerEvent.eligibleForClick = false;
				pointerEvent.clicked.Clear();
				pointerEvent.released.Clear();
				pointerEvent.pointerDown = null;
				pointerEvent.rawPointerPress = null;

				if ( pointerEvent.pointerDrag != null && pointerEvent.dragging )
					ExecuteEvents.ExecuteHierarchy( pointerEvent.pointerDrag, pointerEvent, EventTriggerType.EndDrag );

				pointerEvent.dragging = false;
				pointerEvent.pointerDrag = null;

				// send exit events as we need to simulate this on touch up on touch device
				ExecuteEvents.ExecuteHierarchy( pointerEvent.pointerEnter, pointerEvent, EventTriggerType.PointerExit );
				pointerEvent.pointerEnter = null;
			}
		}

		private void ProcessDrag( PointerEventData pointerEvent )
		{
			if ( !pointerEvent.IsPointerMoving() ||
				Cursor.lockState == CursorLockMode.Locked ||
				pointerEvent.pointerDrag == null )
				return;

			if ( !pointerEvent.dragging &&
				 ShouldStartDrag( pointerEvent.pressPosition, pointerEvent.position, UIConfig.pixelDragThreshold,
								  pointerEvent.useDragThreshold ) )
			{
				ExecuteEvents.ExecuteHierarchy( pointerEvent.pointerDrag, pointerEvent, EventTriggerType.BeginDrag );
				pointerEvent.dragging = true;
			}

			// Drag notification
			if ( pointerEvent.dragging )
			{
				// Before doing drag we should cancel any pointer down state
				// And clear selection!
				//if ( pointerEvent.pointerDown != pointerEvent.pointerDrag )
				//{
				//	ExecuteEvents.Execute( pointerEvent.pointerDown, pointerEvent, ExecuteEvents.pointerUpHandler );

				//	pointerEvent.eligibleForClick = false;
				//	pointerEvent.pointerDown = null;
				//	pointerEvent.rawPointerPress = null;
				//}
				ExecuteEvents.ExecuteHierarchy( pointerEvent.pointerDrag, pointerEvent, EventTriggerType.Drag );
			}
		}

		private void ProcessMove( PointerEventData pointerEvent )
		{
			DisplayObject targetObj = ( Cursor.lockState == CursorLockMode.Locked ? null : pointerEvent.pointerCurrentRaycast.displayObject );
			this.HandlePointerExitAndEnter( pointerEvent, targetObj );
		}

		private void ProcessMouseEvent()
		{
			MouseState mouseData = this.GetMousePointerEventData();
			MouseButtonEventData leftButtonData = mouseData.GetButtonState( PointerEventData.InputButton.Left ).eventData;

			this.currentFocusedGameObject = leftButtonData.buttonData.pointerCurrentRaycast.displayObject;

			// Process the first mouse button fully
			this.ProcessMousePress( leftButtonData );
			this.ProcessMove( leftButtonData.buttonData );
			this.ProcessDrag( leftButtonData.buttonData );

			// Now process right / middle clicks
			this.ProcessMousePress( mouseData.GetButtonState( PointerEventData.InputButton.Right ).eventData );
			this.ProcessDrag( mouseData.GetButtonState( PointerEventData.InputButton.Right ).eventData.buttonData );
			this.ProcessMousePress( mouseData.GetButtonState( PointerEventData.InputButton.Middle ).eventData );
			this.ProcessDrag( mouseData.GetButtonState( PointerEventData.InputButton.Middle ).eventData.buttonData );

			if ( !Mathf.Approximately( leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f ) )
			{
				DisplayObject scrollHandler =
					ExecuteEvents.GetEventHandler( leftButtonData.buttonData.pointerCurrentRaycast.displayObject,
												   EventTriggerType.Scroll );
				ExecuteEvents.ExecuteHierarchy( scrollHandler, leftButtonData.buttonData, EventTriggerType.Scroll );
			}
		}

		/// <summary>
		///     Process the current mouse press.
		/// </summary>
		private void ProcessMousePress( MouseButtonEventData data )
		{
			PointerEventData pointerEvent = data.buttonData;
			DisplayObject currentOverObj = pointerEvent.pointerCurrentRaycast.displayObject;

			// PointerDown notification
			if ( data.PressedThisFrame() )
			{
				pointerEvent.eligibleForClick = true;
				pointerEvent.delta = Vector2.zero;
				pointerEvent.dragging = false;
				pointerEvent.useDragThreshold = true;
				pointerEvent.pressPosition = pointerEvent.position;
				pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

				this.DeselectIfSelectionChanged( currentOverObj, pointerEvent );

				DisplayObject newPressed = ExecuteEvents.ExecuteHierarchy( currentOverObj, pointerEvent,
																		   EventTriggerType.PointerDown );

				ExecuteEvents.GetEventHandlerChain( currentOverObj, EventTriggerType.PointerClick, pointerEvent.clicked );

				float time = Time.unscaledTime;

				if ( newPressed == pointerEvent.lastDown )
				{
					float diffTime = time - pointerEvent.clickTime;
					if ( diffTime < 0.3f )
						++pointerEvent.clickCount;
					else
						pointerEvent.clickCount = 1;

					pointerEvent.clickTime = time;
				}
				else
					pointerEvent.clickCount = 1;

				pointerEvent.pointerClick = pointerEvent.clicked.Count > 0 ? pointerEvent.clicked[0] : null;
				pointerEvent.pointerDown = newPressed;
				pointerEvent.rawPointerPress = currentOverObj;

				pointerEvent.clickTime = time;

				// Save the drag handler as well
				pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler( currentOverObj, EventTriggerType.Drag );

				ExecuteEvents.ExecuteHierarchy( pointerEvent.pointerDrag, pointerEvent, EventTriggerType.InitializePotentialDrag );
			}

			// PointerUp notification
			if ( data.ReleasedThisFrame() )
			{
				ExecuteEvents.ExecuteHierarchy( pointerEvent.pointerDown, pointerEvent, EventTriggerType.PointerUp );

				ExecuteEvents.GetEventHandlerChain( currentOverObj, EventTriggerType.PointerClick, pointerEvent.released );

				// Click events
				if ( pointerEvent.eligibleForClick && pointerEvent.clicked.Count > 0 )
				{
					int count = pointerEvent.released.Count;
					for ( int i = 0; i < count; i++ )
					{
						DisplayObject pointerUpHandler = pointerEvent.released[i];
						if ( pointerEvent.clicked.Contains( pointerUpHandler ) )
							ExecuteEvents.Execute( pointerUpHandler, pointerEvent, EventTriggerType.PointerClick );
					}
				}
				// Drop events
				if ( pointerEvent.pointerDrag != null && pointerEvent.dragging )
					ExecuteEvents.ExecuteHierarchy( currentOverObj, pointerEvent, EventTriggerType.Drop );

				pointerEvent.eligibleForClick = false;
				pointerEvent.clicked.Clear();
				pointerEvent.released.Clear();
				pointerEvent.pointerClick = null;
				pointerEvent.pointerDown = null;
				pointerEvent.rawPointerPress = null;

				if ( pointerEvent.pointerDrag != null && pointerEvent.dragging )
					ExecuteEvents.ExecuteHierarchy( pointerEvent.pointerDrag, pointerEvent, EventTriggerType.EndDrag );

				pointerEvent.dragging = false;
				pointerEvent.pointerDrag = null;

				// redo pointer enter / exit to refresh state
				// so that if we moused over somethign that ignored it before
				// due to having pressed on something else
				// it now gets it.
				if ( currentOverObj != pointerEvent.pointerEnter )
				{
					this.HandlePointerExitAndEnter( pointerEvent, null );
					this.HandlePointerExitAndEnter( pointerEvent, currentOverObj );
				}
			}
		}

		private void HandlePointerExitAndEnter( PointerEventData currentPointerData, DisplayObject newEnterTarget )
		{
			// if we have no target / pointerEnter has been deleted
			// just send exit events to anything we are tracking
			// then exit
			if ( newEnterTarget == null || currentPointerData.pointerEnter == null )
			{
				int count = currentPointerData.hovered.Count;
				for ( int i = 0; i < count; ++i )
					ExecuteEvents.Execute( currentPointerData.hovered[i], currentPointerData, EventTriggerType.PointerExit );

				currentPointerData.hovered.Clear();

				if ( newEnterTarget == null )
				{
					currentPointerData.pointerEnter = null;
					return;
				}
			}

			// if we have not changed hover target
			if ( currentPointerData.pointerEnter == newEnterTarget )
				return;

			DisplayObject commonRoot = FindRoot( currentPointerData.pointerEnter, newEnterTarget );
			DisplayObject displayObject;
			// and we already an entered object from last time
			if ( currentPointerData.pointerEnter != null )
			{
				// send exit handler call to all elements in the chain
				// until we reach the new target, or null!
				displayObject = currentPointerData.pointerEnter;

				while ( displayObject != null )
				{
					// if we reach the common root break out!
					if ( commonRoot != null && commonRoot == displayObject )
						break;

					ExecuteEvents.Execute( displayObject, currentPointerData, EventTriggerType.PointerExit );
					currentPointerData.hovered.Remove( displayObject );
					displayObject = displayObject.parent;
				}
			}

			// now issue the enter call up to but not including the common root
			currentPointerData.pointerEnter = newEnterTarget;
			displayObject = newEnterTarget;
			while ( displayObject != null && displayObject != commonRoot )
			{
				ExecuteEvents.Execute( displayObject, currentPointerData, EventTriggerType.PointerEnter );
				this._tmpHovered.Add( displayObject );
				displayObject = displayObject.parent;
			}
			if ( this._tmpHovered.Count > 0 )
			{
				currentPointerData.hovered.InsertRange( 0, this._tmpHovered );
				this._tmpHovered.Clear();
			}
		}

		private bool SendUpdateEventToSelectedObject()
		{
			if ( EventSystem.instance.currentSelectedGameObject == null )
				return false;

			BaseEventData data = this.GetBaseEventData();
			ExecuteEvents.Execute( EventSystem.instance.currentSelectedGameObject, data, EventTriggerType.UpdateSelected );
			return data.stopPropagation;
		}

		private void DeselectIfSelectionChanged( DisplayObject currentOverObj, BaseEventData pointerEvent )
		{
			// Selection tracking
			DisplayObject selectHandlerObj = ExecuteEvents.GetEventHandler( currentOverObj, EventTriggerType.Select );
			// if we have clicked something new, deselect the old thing
			// leave 'selection handling' up to the press event though.
			if ( selectHandlerObj != EventSystem.instance.currentSelectedGameObject )
				EventSystem.instance.SetSelectedGameObject( selectHandlerObj, pointerEvent );
		}

		private PointerEventData GetTouchPointerEventData( Touch input, out bool pressed, out bool released )
		{
			bool created = this.GetPointerData( input.fingerId, out PointerEventData pointerData, true );

			pointerData.Reset();

			pressed = created || ( input.phase == TouchPhase.Began );
			released = ( input.phase == TouchPhase.Canceled ) || ( input.phase == TouchPhase.Ended );

			if ( created )
				pointerData.position = input.position;

			if ( pressed )
				pointerData.delta = Vector2.zero;
			else
				pointerData.delta = input.position - pointerData.position;

			pointerData.position = input.position;

			pointerData.button = PointerEventData.InputButton.Left;

			EventSystem.instance.Raycast( pointerData, out RaycastResult result );
			pointerData.pointerCurrentRaycast = result;
			return pointerData;
		}

		public PointerEventData GetLastPointerEventData( int id )
		{
			this.GetPointerData( id, out PointerEventData data, false );
			return data;
		}

		private BaseEventData GetBaseEventData()
		{
			if ( this._baseEventData == null )
				this._baseEventData = new BaseEventData();

			this._baseEventData.Reset();
			return this._baseEventData;
		}

		private bool GetPointerData( int id, out PointerEventData data, bool create )
		{
			if ( !this._pointerData.TryGetValue( id, out data ) && create )
			{
				data = new PointerEventData { pointerId = id };
				this._pointerData.Add( id, data );
				return true;
			}
			return false;
		}

		private MouseState GetMousePointerEventData()
		{
			// Populate the left button...
			bool created = this.GetPointerData( K_MOUSE_LEFT_ID, out PointerEventData leftData, true );

			leftData.Reset();

			if ( created )
				leftData.position = Input.mousePosition;

			Vector2 pos = Input.mousePosition;
			if ( Cursor.lockState == CursorLockMode.Locked )
			{
				// We don't want to do ANY cursor-based interaction when the mouse is locked
				leftData.position = new Vector2( -1.0f, -1.0f );
				leftData.delta = Vector2.zero;
			}
			else
			{
				leftData.delta = pos - leftData.position;
				leftData.position = pos;
			}

			leftData.scrollDelta = Input.mouseScrollDelta;
			leftData.button = PointerEventData.InputButton.Left;
			EventSystem.instance.Raycast( leftData, out RaycastResult result );
			leftData.pointerCurrentRaycast = result;

			// copy the apropriate data into right and middle slots
			this.GetPointerData( K_MOUSE_RIGHT_ID, out PointerEventData rightData, true );
			this.CopyFromTo( leftData, rightData );
			rightData.button = PointerEventData.InputButton.Right;

			this.GetPointerData( K_MOUSE_MIDDLE_ID, out PointerEventData middleData, true );
			this.CopyFromTo( leftData, middleData );
			middleData.button = PointerEventData.InputButton.Middle;

			this._mouseState.SetButtonState( PointerEventData.InputButton.Left, StateForMouseButton( 0 ), leftData );
			this._mouseState.SetButtonState( PointerEventData.InputButton.Right, StateForMouseButton( 1 ), rightData );
			this._mouseState.SetButtonState( PointerEventData.InputButton.Middle, StateForMouseButton( 2 ), middleData );

			return this._mouseState;
		}

		private static PointerEventData.FramePressState StateForMouseButton( int buttonId )
		{
			bool pressed = Input.GetMouseButtonDown( buttonId );
			bool released = Input.GetMouseButtonUp( buttonId );
			if ( pressed && released )
				return PointerEventData.FramePressState.PressedAndReleased;
			if ( pressed )
				return PointerEventData.FramePressState.Pressed;
			if ( released )
				return PointerEventData.FramePressState.Released;
			return PointerEventData.FramePressState.NotChanged;
		}

		private static bool ShouldStartDrag( Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold )
		{
			if ( !useDragThreshold )
				return true;

			return ( pressPos - currentPos ).sqrMagnitude >= threshold * threshold;
		}

		private void CopyFromTo( PointerEventData from, PointerEventData to )
		{
			to.position = from.position;
			to.delta = from.delta;
			to.scrollDelta = from.scrollDelta;
			to.pointerCurrentRaycast = from.pointerCurrentRaycast;
			to.pointerEnter = from.pointerEnter;
		}

		private static DisplayObject FindRoot( DisplayObject d1, DisplayObject d2 )
		{
			if ( d1 == null || d2 == null )
				return null;

			DisplayObject t1 = d1;
			while ( t1 != null )
			{
				DisplayObject t2 = d2;
				while ( t2 != null )
				{
					if ( t1 == t2 )
						return t1;
					t2 = t2.parent;
				}
				t1 = t1.parent;
			}
			return null;
		}

		protected void ClearSelection()
		{
			BaseEventData baseEventData = this.GetBaseEventData();

			foreach ( PointerEventData pointer in this._pointerData.Values )
				this.HandlePointerExitAndEnter( pointer, null );

			this._pointerData.Clear();
			EventSystem.instance.SetSelectedGameObject( null, baseEventData );
		}

		private class ButtonState
		{
			private PointerEventData.InputButton _button = PointerEventData.InputButton.Left;
			public MouseButtonEventData eventData { get; set; }

			public PointerEventData.InputButton button
			{
				get => this._button;
				set => this._button = value;
			}
		}

		private class MouseState
		{
			private readonly List<ButtonState> _trackedButtons = new List<ButtonState>();

			public bool AnyPressesThisFrame()
			{
				int count = this._trackedButtons.Count;
				for ( int i = 0; i < count; i++ )
				{
					if ( this._trackedButtons[i].eventData.PressedThisFrame() )
						return true;
				}
				return false;
			}

			public bool AnyReleasesThisFrame()
			{
				int count = this._trackedButtons.Count;
				for ( int i = 0; i < count; i++ )
				{
					if ( this._trackedButtons[i].eventData.ReleasedThisFrame() )
						return true;
				}
				return false;
			}

			public ButtonState GetButtonState( PointerEventData.InputButton button )
			{
				ButtonState tracked = null;
				int count = this._trackedButtons.Count;
				for ( int i = 0; i < count; i++ )
				{
					if ( this._trackedButtons[i].button == button )
					{
						tracked = this._trackedButtons[i];
						break;
					}
				}

				if ( tracked == null )
				{
					tracked = new ButtonState { button = button, eventData = new MouseButtonEventData() };
					this._trackedButtons.Add( tracked );
				}
				return tracked;
			}

			public void SetButtonState( PointerEventData.InputButton button, PointerEventData.FramePressState stateForMouseButton,
										PointerEventData data )
			{
				ButtonState toModify = this.GetButtonState( button );
				toModify.eventData.buttonState = stateForMouseButton;
				toModify.eventData.buttonData = data;
			}
		}

		private class MouseButtonEventData
		{
			public PointerEventData buttonData;
			public PointerEventData.FramePressState buttonState;

			public bool PressedThisFrame()
			{
				return this.buttonState == PointerEventData.FramePressState.Pressed ||
					   this.buttonState == PointerEventData.FramePressState.PressedAndReleased;
			}

			public bool ReleasedThisFrame()
			{
				return this.buttonState == PointerEventData.FramePressState.Released ||
					   this.buttonState == PointerEventData.FramePressState.PressedAndReleased;
			}
		}
	}
}