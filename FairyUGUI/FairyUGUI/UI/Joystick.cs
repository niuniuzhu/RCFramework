using Core.Xml;
using DG.Tweening;
using FairyUGUI.Event;
using UnityEngine;
using EventType = FairyUGUI.Event.EventType;

namespace FairyUGUI.UI
{
	public class Joystick : GComponent
	{
		public float radius = 100f;

		public Vector2 center;

		public float resetDuration = 1.0f;

		public Vector2 touchPosition { set => this.axis = value - this.center; }

		private Vector2 _axis;
		private Vector2 axis
		{
			get => this._axis;
			set
			{
				float length = value.magnitude;
				Vector2 normalAxis = length < 0.0001f ? Vector2.zero : value / length;

				if ( this._core != null )
				{
					Vector2 touchAxis = value;
					if ( touchAxis.sqrMagnitude >= this.radius * this.radius )
						touchAxis = normalAxis * this.radius;

					this._core.position = touchAxis + this.center;
				}

				if ( this._axis == normalAxis )
					return;

				this._axis = normalAxis;

				this.onChanged.Call( this._axis );
			}
		}

		public Vector2 worldAxis => this.LocalToWorldDirection( this._axis );

		private GComponent _core;

		public string coreName
		{
			get => this._core?.name ?? string.Empty;
			set
			{
				this._core = this[value].asCom;
				if ( this._core != null )
					this._core.touchable = false;
			}
		}

		public EventListener onChanged { get; private set; }

		public Joystick()
		{
			this.enableDrag = true;

			this.onChanged = new EventListener( this, EventType.Changed );
		}

		protected override void ConstructFromXML( XML xml )
		{
			base.ConstructFromXML( xml );

			this.center = this.size * 0.5f;
		}

		public void Reset( bool fadeOut = false )
		{
			if ( fadeOut )
				DOTween.To( () => this._axis, v => this.axis = v, Vector2.zero, this.resetDuration ).SetTarget( this );
			else
				this.axis = Vector2.zero;
		}

		private void SetAxis( PointerEventData e )
		{
			this.touchPosition = this.ScreenToLocal( e.position );
		}

		protected override void OnPointerDown( BaseEventData e )
		{
			this.SetAxis( ( PointerEventData ) e );
		}

		protected override void OnDrag( BaseEventData e )
		{
			this.SetAxis( ( PointerEventData ) e );
		}

		protected override void OnPointerUp( BaseEventData eventData )
		{
			this.Reset( true );
		}

		protected override void InternalDispose()
		{
			base.InternalDispose();

			DOTween.Kill( this );
		}
	}
}