using System.Collections.Generic;
using System.Text;
using FairyUGUI.Core;
using UnityEngine;

namespace FairyUGUI.Event
{
	public class PointerEventData : BaseEventData
	{
		public enum InputButton
		{
			Left = 0,
			Right = 1,
			Middle = 2
		}

		public enum FramePressState
		{
			Pressed,
			Released,
			PressedAndReleased,
			NotChanged
		}

		public DisplayObject pointerEnter { get; set; }

		private DisplayObject _pointerClick;
		public DisplayObject pointerClick
		{
			get => this._pointerClick;
			set
			{
				if ( this._pointerClick == value )
					return;

				this.lastClick = this._pointerClick;
				this._pointerClick = value;
			}
		}

		public DisplayObject lastClick { get; private set; }

		private DisplayObject _pointerDown;
		public DisplayObject pointerDown
		{
			get => this._pointerDown;
			set
			{
				if ( this._pointerDown == value )
					return;

				this.lastDown = this._pointerDown;
				this._pointerDown = value;
			}
		}

		public DisplayObject lastDown { get; private set; }

		public DisplayObject rawPointerPress { get; set; }

		public DisplayObject pointerDrag { get; set; }

		public RaycastResult pointerCurrentRaycast { get; set; }

		public RaycastResult pointerPressRaycast { get; set; }

		public readonly List<DisplayObject> hovered = new List<DisplayObject>();
		public readonly List<DisplayObject> clicked = new List<DisplayObject>();
		public readonly List<DisplayObject> released = new List<DisplayObject>();

		public bool eligibleForClick { get; set; }

		public int pointerId { get; set; }

		public Vector2 position { get; set; }

		public Vector2 delta { get; set; }

		public Vector2 pressPosition { get; set; }

		public float clickTime { get; set; }

		public int clickCount { get; set; }

		public Vector2 scrollDelta { get; set; }

		public bool useDragThreshold { get; set; }

		public bool dragging { get; set; }

		public InputButton button { get; set; }

		public PointerEventData()
		{
			this.eligibleForClick = false;

			this.pointerId = -1;
			this.position = Vector2.zero;
			this.delta = Vector2.zero;
			this.pressPosition = Vector2.zero;
			this.clickTime = 0.0f;
			this.clickCount = 0;

			this.scrollDelta = Vector2.zero;
			this.useDragThreshold = true;
			this.dragging = false;
			this.button = InputButton.Left;
		}

		public bool IsPointerMoving()
		{
			return this.delta.sqrMagnitude > 0.0f;
		}

		public bool IsScrolling()
		{
			return this.scrollDelta.sqrMagnitude > 0.0f;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendLine( "position: " + this.position );
			sb.AppendLine( "delta: " + this.delta );
			sb.AppendLine( "eligibleForClick: " + this.eligibleForClick );
			sb.AppendLine( "pointerClick: " + ( this.pointerClick == null ? string.Empty : this.pointerClick.name ) );
			sb.AppendLine( "lastPointerClick: " + ( this.lastClick == null ? string.Empty : this.lastClick.name ) );
			sb.AppendLine( "pointerDown: " + ( this.pointerDown == null ? string.Empty : this.pointerDown.name ) );
			sb.AppendLine( "lastPointerPress: " + ( this.lastDown == null ? string.Empty : this.lastDown.name ) );
			sb.AppendLine( "pointerEnter: " + ( this.pointerEnter == null ? string.Empty : this.pointerEnter.name ) );
			sb.AppendLine( "pointerDrag: " + ( this.pointerDrag == null ? string.Empty : this.pointerDrag.name ) );
			sb.AppendLine( "Use Drag Threshold: " + this.useDragThreshold );
			sb.AppendLine( "Current Rayast:" );
			sb.AppendLine( this.pointerCurrentRaycast.ToString() );
			sb.AppendLine( "Press Rayast:" );
			sb.AppendLine( this.pointerPressRaycast.ToString() );
			return sb.ToString();
		}
	}
}