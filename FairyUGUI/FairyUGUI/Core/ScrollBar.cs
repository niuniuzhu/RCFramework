using FairyUGUI.Event;
using FairyUGUI.UI;
using FairyUGUI.UI.UGUIExt;
using UnityEngine;
using EventType = FairyUGUI.Event.EventType;

namespace FairyUGUI.Core
{
	public class ScrollBar : Container
	{
		public enum Direction
		{
			LeftToRight,
			RightToLeft,
			BottomToTop,
			TopToBottom
		}

		private enum Axis
		{
			Horizontal = 0,
			Vertical = 1
		}

		private float _minValue;
		internal float minValue
		{
			get => this._minValue;
			set
			{
				if ( value >= this._maxValue )
					value = this._maxValue - 0.1f;

				if ( this._minValue == value )
					return;

				this._minValue = value;
				this.UpdateVisuals();
			}
		}

		private float _maxValue = 1f;
		internal float maxValue
		{
			get => this._maxValue;
			set
			{
				if ( value <= this._minValue )
					value = this._minValue + 0.1f;

				if ( this._maxValue == value )
					return;

				this._maxValue = value;
				this.UpdateVisuals();
			}
		}

		private float _value;
		internal float value
		{
			get => this._value;
			set => this.Set( value );
		}

		internal float normalizedValue
		{
			get
			{
				if ( Mathf.Approximately( this._minValue, this._maxValue ) )
					return 0;
				return Mathf.InverseLerp( this._minValue, this._maxValue, this._value );
			}
			set => this.value = Mathf.Lerp( this._minValue, this._maxValue, value );
		}

		private float _visualSize;
		internal float visualSize
		{
			get => this._visualSize;
			set
			{
				value = Mathf.Clamp01( value );
				if ( this._visualSize == value )
					return;
				this._visualSize = value;
				this.UpdateVisuals();
			}
		}

		private Direction _direction;
		internal Direction direction
		{
			get => this._direction;
			set
			{
				if ( this._direction == value )
					return;
				this._direction = value;
				this.UpdateVisuals();
			}
		}

		private Vector2? _pointerDownEventData;
		private Vector2 _offset;

		internal GObject gripObject;
		internal GObject barObject;

		private Axis axis => ( this._direction == Direction.LeftToRight || this._direction == Direction.RightToLeft ) ? Axis.Horizontal : Axis.Vertical;

		private bool reverseValue => this._direction == Direction.RightToLeft || this._direction == Direction.BottomToTop;

		public EventListener onChanged { get; private set; }

		internal ScrollBar( GObject owner )
			: base( owner )
		{
			this.RegisterEventTriggerType( EventTriggerType.InitializePotentialDrag );
			this.RegisterEventTriggerType( EventTriggerType.BeginDrag );
			this.RegisterEventTriggerType( EventTriggerType.Drag );

			this.onChanged = new EventListener( this, EventType.Changed );
		}

		protected override void InternalDispose()
		{
			this._pointerDownEventData = null;
			this.gripObject = null;
			this.barObject = null;

			base.InternalDispose();
		}

		protected override void OnInitializePotentialDrag( BaseEventData eventData )
		{
			PointerEventData e = ( PointerEventData )eventData;
			e.useDragThreshold = false;
		}

		protected override void OnBeginDrag( BaseEventData eventData )
		{
			this._pointerDownEventData = null;

			PointerEventData e = ( PointerEventData )eventData;
			this._offset = Vector2.zero;
			if ( RectTransformUtility.RectangleContainsScreenPoint( this.gripObject.displayObject.rectTransform, e.position, Stage.inst.eventCamera ) )
			{
				Vector2 localMousePos;
				if ( RectTransformUtility.ScreenPointToLocalPointInRectangle( this.gripObject.displayObject.rectTransform, e.position, Stage.inst.eventCamera, out localMousePos ) )
					this._offset = localMousePos - this.gripObject.displayObject.rect.center;
			}

			eventData.StopPropagation();
		}

		protected override void OnDrag( BaseEventData eventData )
		{
			PointerEventData e = ( PointerEventData )eventData;
			this.UpdateDrag( e );

			eventData.StopPropagation();
		}

		protected override void OnPointerDown( BaseEventData eventData )
		{
			PointerEventData e = ( PointerEventData )eventData;
			this._pointerDownEventData = e.position;
		}

		protected override void OnPointerUp( BaseEventData eventData )
		{
			this._pointerDownEventData = null;
		}

		private void Set( float input, bool sendCallback = true )
		{
			float currentValue = Mathf.Clamp( input, this._minValue, this._maxValue );

			if ( this._value == currentValue )
				return;

			this._value = currentValue;

			this.UpdateVisuals();

			if ( sendCallback )
				this.onChanged.Call( this._value );
		}

		private void UpdateDrag( PointerEventData eventData )
		{
			Vector2 localCursor;
			if ( !RectTransformUtility.ScreenPointToLocalPointInRectangle( this.rectTransform, eventData.position, Stage.inst.eventCamera, out localCursor ) )
				return;

			Vector2 handleCenterRelativeToContainerCorner = localCursor - this._offset - this.rect.position;
			Vector2 handleCorner = handleCenterRelativeToContainerCorner - ( this.gripObject.displayObject.size ) * 0.5f;

			float parentSize = this.axis == 0 ? this.size.x : this.size.y;
			float remainingSize = parentSize * ( 1 - this._visualSize );
			if ( remainingSize <= 0 )
				return;

			switch ( this._direction )
			{
				case Direction.LeftToRight:
					this.Set( handleCorner.x / remainingSize );
					break;
				case Direction.RightToLeft:
					this.Set( 1f - ( handleCorner.x / remainingSize ) );
					break;
				case Direction.BottomToTop:
					this.Set( handleCorner.y / remainingSize );
					break;
				case Direction.TopToBottom:
					this.Set( 1f - ( handleCorner.y / remainingSize ) );
					break;
			}
		}

		private void UpdateVisuals()
		{
			if ( this.barObject == null || this.gripObject == null )
				return;

			float movement = this.normalizedValue * ( 1 - this._visualSize );

			if ( this.reverseValue )
				movement = 1 - movement - this._visualSize;

			GImage image = this.barObject as GImage;
			if ( image != null && image.fillMethod != ImageEx.FillMethod.None )
				image.fillAmount = movement;
			else
			{
				GLoader loader = this.barObject as GLoader;
				if ( loader != null && loader.fillMethod != ImageEx.FillMethod.None )
					loader.fillAmount = movement;
				else
				{
					if ( this.direction == Direction.LeftToRight || this.direction == Direction.RightToLeft )
					{
						Vector2 pos = this.gripObject.position;
						pos.x = this.barObject.position.x + this.barObject.size.x * movement;
						this.gripObject.position = pos;

						Vector2 s = this.gripObject.size;
						s.x = this.barObject.size.x * this._visualSize;
						this.gripObject.size = s;
					}
					else
					{
						Vector2 pos = this.gripObject.position;
						pos.y = this.barObject.position.y + this.barObject.size.y * movement;
						this.gripObject.position = pos;

						Vector2 s = this.gripObject.size;
						s.y = this.barObject.size.y * this._visualSize;
						this.gripObject.size = s;
					}
				}
			}
		}

		protected override void HandleSizeChanged()
		{
			this.UpdateVisuals();
		}

		protected internal override void Update( UpdateContext context )
		{
			base.Update( context );

			if ( this._pointerDownEventData != null )
			{
				if ( !RectTransformUtility.RectangleContainsScreenPoint( this.gripObject.displayObject.rectTransform, this._pointerDownEventData.Value, Stage.inst.eventCamera ) )
				{
					Vector2 localMousePos;
					if ( RectTransformUtility.ScreenPointToLocalPointInRectangle( this.gripObject.displayObject.rectTransform,
						this._pointerDownEventData.Value, Stage.inst.eventCamera, out localMousePos ) )
					{
						float axisCoordinate = this.axis == 0 ? localMousePos.x : localMousePos.y;
						if ( axisCoordinate < 0 )
							this.value -= this._visualSize;
						else
							this.value += this._visualSize;
					}
				}
			}
		}
	}
}