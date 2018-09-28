using Core.Math;
using FairyUGUI.Event;
using FairyUGUI.UI;
using FairyUGUI.UI.UGUIExt;
using UnityEngine;
using EventType = FairyUGUI.Event.EventType;

namespace FairyUGUI.Core
{
	public class Slider : Container
	{
		public enum Direction
		{
			LeftToRight = 0,
			RightToLeft = 1,
			BottomToTop = 2,
			TopToBottom = 3,
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

		private ProgressTitleType _titleType;

		internal ProgressTitleType titleType
		{
			get => this._titleType;
			set
			{
				if ( this._titleType == value )
					return;
				this._titleType = value;
				this.UpdateTitle();
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

		internal bool interactable;

		internal GObject gripObject;
		internal GTextField titleObject;
		internal GMovieClip aniObject;
		private GObject _barObject;
		private GGraph _track;
		internal GObject barObject
		{
			get => this._barObject;
			set
			{
				if ( this._barObject == value )
					return;
				this._barObject = value;

				if ( this._barObject != null )
				{
					this._track = new GGraph();
					this._track.touchable = false;
					this._barObject.parent.AddChildAt( this._track, this._barObject.parent.GetChildIndex( this._barObject ) );
					this._track.size = this._barObject.size;
					this._track.position = this._barObject.position;
					this._track.AddRelation( this._barObject.parent, RelationType.Size );
				}
			}
		}

		private bool _canDrag;

		private Axis axis => ( this._direction == Direction.LeftToRight || this._direction == Direction.RightToLeft ) ? Axis.Horizontal : Axis.Vertical;

		private bool reverseValue => this._direction == Direction.RightToLeft || this._direction == Direction.BottomToTop;

		public EventListener onChanged { get; private set; }

		internal Slider( GObject owner )
			: base( owner )
		{
			this.onChanged = new EventListener( this, EventType.Changed );

			this.RegisterEventTriggerType( EventTriggerType.Drag );
		}

		protected override void InternalDispose()
		{
			this.gripObject = null;
			this.aniObject = null;
			this.titleObject = null;
			this._barObject = null;
			this._track = null;
			this._canDrag = false;

			base.InternalDispose();
		}

		protected override void OnPointerDown( BaseEventData eventData )
		{
			if ( !this.interactable )
				return;

			PointerEventData e = ( PointerEventData )eventData;

			RectTransform clickRect = this._track.displayObject.rectTransform;
			if ( !RectTransformUtility.RectangleContainsScreenPoint( clickRect, e.position, Stage.inst.eventCamera ) )
				return;

			this._canDrag = true;

			this.UpdateDrag( e, Stage.inst.eventCamera );
		}

		protected override void OnPointerUp( BaseEventData eventData )
		{
			this._canDrag = false;
		}

		protected override void OnDrag( BaseEventData eventData )
		{
			if ( !this._canDrag )
				return;

			this.UpdateDrag( ( PointerEventData )eventData, Stage.inst.eventCamera );

			eventData.StopPropagation();
		}

		private void UpdateDrag( PointerEventData eventData, Camera eventCamera )
		{
			if ( this._track == null )
				return;

			RectTransform clickRect = this._track.displayObject.rectTransform;
			if ( !( clickRect.rect.size[( int )this.axis] > 0 ) )
				return;

			Vector2 localCursor;
			if ( !RectTransformUtility.ScreenPointToLocalPointInRectangle( clickRect, eventData.position, eventCamera, out localCursor ) )
				return;

			localCursor -= clickRect.rect.position;

			float val = Mathf.Clamp01( localCursor[( int )this.axis] / clickRect.rect.size[( int )this.axis] );
			if ( this.axis == Axis.Vertical )
				val = 1 - val;
			this.normalizedValue = ( this.reverseValue ? 1f - val : val );
		}

		private void UpdateAniObject()
		{
			if ( this.aniObject != null )
				this.aniObject.frame = Mathf.RoundToInt( this.normalizedValue * 100 );
		}

		private void UpdateTitle()
		{
			if ( this.titleObject == null )
				return;

			switch ( this._titleType )
			{
				case ProgressTitleType.Percent:
					this.titleObject.text = MathUtils.Round( this.normalizedValue * 100 ) + "%";
					break;
				case ProgressTitleType.ValueAndMax:
					this.titleObject.text = this.value + "/" + this._maxValue;
					break;
				case ProgressTitleType.Value:
					this.titleObject.text = string.Empty + this.value;
					break;
				case ProgressTitleType.Max:
					this.titleObject.text = string.Empty + this._maxValue;
					break;
			}
		}

		private void Set( float input, bool sendCallback = true )
		{
			float currentValue = Mathf.Clamp( input, this._minValue, this._maxValue );

			if ( this._value == currentValue )
				return;

			this._value = currentValue;

			this.UpdateVisuals();
			this.UpdateAniObject();
			this.UpdateTitle();

			if ( sendCallback )
				this.onChanged.Call( this._value );
		}

		private void UpdateVisuals()
		{
			if ( this.barObject == null )
				return;

			float v = this.normalizedValue;
			bool isReversed = this.reverseValue;

			GImage image = this.barObject as GImage;
			if ( image != null && image.fillMethod != ImageEx.FillMethod.None )
				image.fillAmount = isReversed ? 1 - v : v;
			else
			{
				GLoader loader = this.barObject as GLoader;
				if ( loader != null && loader.fillMethod != ImageEx.FillMethod.None )
					loader.fillAmount = isReversed ? 1 - v : v;
				else
				{
					if ( this.direction == Direction.LeftToRight || this.direction == Direction.RightToLeft )
					{
						Vector2 s = this.barObject.size;
						s.x = this._track.size.x * v;
						this.barObject.size = s;

						Vector2 pos = this.barObject.position;
						pos.x = isReversed ? this._track.position.x + ( this._track.size.x - s.x ) : this._track.position.x;
						this.barObject.position = pos;
					}
					else
					{
						Vector2 s = this.barObject.size;
						s.y = this._track.size.y * v;
						this.barObject.size = s;

						Vector2 pos = this.barObject.position;
						pos.y = isReversed ? this._track.position.y + ( this._track.size.y - s.y ) : this._track.position.y;
						this.barObject.position = pos;
					}
				}
			}
		}

		protected override void HandleSizeChanged()
		{
			base.HandleSizeChanged();

			this.UpdateVisuals();
		}
	}
}