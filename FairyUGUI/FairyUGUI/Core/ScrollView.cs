using Core.Math;
using FairyUGUI.Event;
using FairyUGUI.UI;
using UnityEngine;
using Bounds = UnityEngine.Bounds;
using EventType = FairyUGUI.Event.EventType;
using Logger = Core.Misc.Logger;

namespace FairyUGUI.Core
{
	public class ScrollView : Container, IRaycastFilter
	{
		public enum MovementType
		{
			Unrestricted, // Unrestricted movement -- can scroll forever
			Elastic, // Restricted but flexible -- can go past the edges, but springs back in place
			Clamped, // Restricted movement where it's not possible to go past the edges
		}

		internal Container content { get; private set; }

		public MovementType movementType = UIConfig.defaultMovementType;
		public float elasticity = UIConfig.defaultScrollElasticity;
		public bool inertia = UIConfig.defaultScrollInertia;
		public float decelerationRate = UIConfig.defaultScrollDecelerationRate;
		public float scrollSensitivity = UIConfig.defaultScrollSensitivity;
		public bool contentSizeAutoFit = true;

		private GComponent _owner;
		private GScrollBar _vtScrollBar;
		private GScrollBar _hzScrollBar;
		private readonly Vector3[] _corners = new Vector3[4];
		private Vector2 _pointerStartLocalCursor;
		private Vector2 _contentStartPosition;
		private bool _dragging;
		private Bounds _viewBounds;
		private Bounds _contentBounds;

		private Vector2 _velocity;
		private Vector2 _prevPosition;
		private Bounds _prevViewBounds;
		private Bounds _prevContentBounds;
		private readonly bool _displayOnLeft;
		private readonly string _vtScrollBarRes;
		private readonly string _hzScrollBarRes;
		private readonly ScrollType _scrollType;
		private readonly ScrollBarDisplayType _scrollBarDisplay;
		private Margin _scrollBarMargin;
		private bool _contentSizeDirty;

		public Vector2 normalizedPosition
		{
			get => new Vector2( this.horizontalNormalizedPosition, this.verticalNormalizedPosition );
			set
			{
				this.SetNormalizedPosition( value.x, 0 );
				this.SetNormalizedPosition( value.y, 1 );
			}
		}

		public float horizontalNormalizedPosition
		{
			get
			{
				this.EnsureBoundsCorrect();
				return this.GetHorizontalNormalizedPosition();
			}
			set => this.SetNormalizedPosition( value, 0 );
		}

		public float verticalNormalizedPosition
		{
			get
			{
				this.EnsureBoundsCorrect();
				return this.GetVerticalNormalizedPosition();
			}
			set => this.SetNormalizedPosition( value, 1 );
		}

		public EventListener onChange { get; private set; }

		internal ScrollView( GComponent gOwner,
			ScrollType scrollType,
			Margin scrollBarMargin,
			ScrollBarDisplayType scrollBarDisplay,
			int flags,
			string vtScrollBarRes,
			string hzScrollBarRes )
			: base( null )
		{
			this._owner = gOwner;

			this.onChange = new EventListener( this, EventType.Changed );

			this.UnregisterAllEventTriggerTypes();
			this.RegisterEventTriggerType( EventTriggerType.Scroll );
			this.RegisterEventTriggerType( EventTriggerType.InitializePotentialDrag );
			this.RegisterEventTriggerType( EventTriggerType.BeginDrag );
			this.RegisterEventTriggerType( EventTriggerType.EndDrag );
			this.RegisterEventTriggerType( EventTriggerType.Drag );
			this.RegisterEventTriggerType( EventTriggerType.Drop );

			this._displayOnLeft = ( flags & 1 ) != 0;

			bool bouncebackEffect = UIConfig.defaultScrollBounceEffect;
			if ( ( flags & 64 ) != 0 )
				bouncebackEffect = true;
			else if ( ( flags & 128 ) != 0 )
				bouncebackEffect = false;

			this.movementType = bouncebackEffect ? MovementType.Elastic : MovementType.Clamped;
			this._scrollBarMargin = scrollBarMargin;
			this._vtScrollBarRes = vtScrollBarRes;
			this._hzScrollBarRes = hzScrollBarRes;
			this._scrollType = scrollType;

			if ( scrollBarDisplay == ScrollBarDisplayType.Default )
				scrollBarDisplay = Application.isMobilePlatform ? ScrollBarDisplayType.Auto : UIConfig.defaultScrollBarDisplay;
			this._scrollBarDisplay = scrollBarDisplay;
		}

		protected override void InternalDispose()
		{
			if ( this._vtScrollBar != null )
			{
				this._owner.rootContainer.RemoveChild( this._vtScrollBar.displayObject );
				this._vtScrollBar.Dispose();
				this._vtScrollBar = null;
			}
			if ( this._hzScrollBar != null )
			{
				this._owner.rootContainer.RemoveChild( this._hzScrollBar.displayObject );
				this._hzScrollBar.Dispose();
				this._hzScrollBar = null;
			}
			this._vtScrollBar = null;
			this._hzScrollBar = null;
			this._owner = null;
			this.content = null;

			base.InternalDispose();
		}

		protected override void OnGameObjectCreated()
		{
			this.content = new Container( null );
			this.content.name = "Content";
			this.content.layer = LayerMask.NameToLayer( Stage.LAYER_NAME );
			this.content.UnregisterAllEventTriggerTypes();
			this.AddChild( this.content );
		}

		internal void SetupScrollBar()
		{
			if ( this._scrollBarDisplay != ScrollBarDisplayType.Hidden )
			{
				if ( this._scrollType == ScrollType.Both || this._scrollType == ScrollType.Vertical )
				{
					string res = string.IsNullOrEmpty( this._vtScrollBarRes ) ? UIConfig.verticalScrollBar : this._vtScrollBarRes;
					if ( !string.IsNullOrEmpty( res ) )
					{
						this._vtScrollBar = UIPackage.CreateObjectFromURL( res ).asScrollBar;
						if ( this._vtScrollBar == null )
							Logger.Warn( "FairyGUI: cannot create scrollbar from " + res );
						else
						{
							this._vtScrollBar.minValue = 0f;
							this._vtScrollBar.maxValue = 1f;
							this._vtScrollBar.direction = ScrollBar.Direction.BottomToTop;
							this._vtScrollBar.onChange.Add( this.SetVerticalNormalizedPosition );
							this._owner.rootContainer.AddChild( this._vtScrollBar.displayObject );
						}
					}
				}
				if ( this._scrollType == ScrollType.Both || this._scrollType == ScrollType.Horizontal )
				{
					string res = string.IsNullOrEmpty( this._hzScrollBarRes ) ? UIConfig.horizontalScrollBar : this._hzScrollBarRes;
					if ( !string.IsNullOrEmpty( res ) )
					{
						this._hzScrollBar = UIPackage.CreateObjectFromURL( res ).asScrollBar;
						if ( this._hzScrollBar == null )
							Logger.Warn( "FairyGUI: cannot create scrollbar from " + res );
						else
						{
							this._hzScrollBar.minValue = 0f;
							this._hzScrollBar.maxValue = 1f;
							this._hzScrollBar.direction = ScrollBar.Direction.LeftToRight;
							this._hzScrollBar.onChange.Add( this.SetHorizontalNormalizedPosition );
							this._owner.rootContainer.AddChild( this._hzScrollBar.displayObject );
						}
					}
				}
			}
			this.UpdateLayout();
		}

		private void UpdateLayout()
		{
			this.rectTransform.sizeDelta = this._owner.size;

			if ( this._vtScrollBar != null )
			{
				if ( this._displayOnLeft )
				{
					Vector2 pos = this.position;
					pos.x = this._vtScrollBar.size.x;
					this.position = pos;
				}
				else
				{
					Vector2 pos = this.position;
					pos.x = 0;
					this.position = pos;
				}
				Vector2 s = this.size;
				s.x -= this._vtScrollBar.size.x;
				this.rectTransform.sizeDelta = s;
			}
			if ( this._hzScrollBar != null )
			{
				Vector2 pos = this.position;
				pos.y = 0;
				this.position = pos;

				Vector2 s = this.size;
				s.y -= this._hzScrollBar.size.y;
				this.rectTransform.sizeDelta = s;
			}

			if ( this._vtScrollBar != null )
			{
				Vector2 s = this._vtScrollBar.size;
				s.y = this.size.y - this._scrollBarMargin.top - this._scrollBarMargin.bottom;
				this._vtScrollBar.size = s;

				if ( this._displayOnLeft )
					this._vtScrollBar.position = new Vector2( 0, this._scrollBarMargin.top );
				else
				{
					Vector2 pos = this._vtScrollBar.position;
					pos.x = this.size.x;
					pos.y = this._scrollBarMargin.top;
					this._vtScrollBar.position = pos;
				}
			}
			if ( this._hzScrollBar != null )
			{
				Vector2 s = this._hzScrollBar.size;
				s.x = this.size.x - this._scrollBarMargin.left - this._scrollBarMargin.right;
				this._hzScrollBar.size = s;

				Vector2 pos = this._hzScrollBar.position;
				pos.x = this._scrollBarMargin.left;
				pos.y = this.size.y;
				this._hzScrollBar.position = pos;
			}

			this.EnsureBoundsCorrect();
			this.onChange.Call( this.normalizedPosition );
			this.UpdatePrevData();
		}

		private void SetHorizontalNormalizedPosition( EventContext context )
		{
			this.SetNormalizedPosition( ( float )context.data, 0 );
		}

		private void SetVerticalNormalizedPosition( EventContext context )
		{
			this.SetNormalizedPosition( ( float )context.data, 1 );
		}

		private void SetNormalizedPosition( float value, int axis )
		{
			this.EnsureBoundsCorrect();
			// How much the content is larger than the view.
			float hiddenLength = this._contentBounds.size[axis] - this._viewBounds.size[axis];
			// Where the position of the lower left corner of the content bounds should be, in the space of the view.
			float contentBoundsMinPosition = this._viewBounds.min[axis] - value * hiddenLength;
			// The new content localPosition, in the space of the view.
			float newLocalPosition = this.content.rectTransform.localPosition[axis] + contentBoundsMinPosition - this._contentBounds.min[axis];

			Vector3 localPos = this.content.localPosition;
			if ( Mathf.Abs( localPos[axis] - newLocalPosition ) > 0.01f )
			{
				localPos[axis] = newLocalPosition;
				this.content.localPosition = localPos;
				this._velocity[axis] = 0;
				this.EnsureBoundsCorrect();
			}
		}

		protected override void OnScroll( BaseEventData eventData )
		{
			PointerEventData e = ( PointerEventData )eventData;

			this.UpdateBounds();

			Vector2 delta = e.scrollDelta;
			delta.y = -delta.y;
			if ( this._scrollType == ScrollType.Vertical )
			{
				if ( Mathf.Abs( delta.x ) > Mathf.Abs( delta.y ) )
					delta.y = delta.x;
				delta.x = 0;
			}
			else if ( this._scrollType == ScrollType.Horizontal )
			{
				if ( Mathf.Abs( delta.y ) > Mathf.Abs( delta.x ) )
					delta.x = delta.y;
				delta.y = 0;
			}

			Vector2 pos = this.content.rectTransform.anchoredPosition;
			pos += delta * this.scrollSensitivity;
			if ( this.movementType == MovementType.Clamped )
				pos += this.CalculateOffset( pos - this.content.rectTransform.anchoredPosition );

			this.SetContentAnchoredPosition( pos );
			this.UpdateBounds();

			eventData.StopPropagation();
		}

		protected override void OnInitializePotentialDrag( BaseEventData eventData )
		{
			this._velocity = Vector2.zero;
		}

		protected override void OnBeginDrag( BaseEventData eventData )
		{
			PointerEventData e = ( PointerEventData )eventData;

			this.UpdateBounds();
			RectTransformUtility.ScreenPointToLocalPointInRectangle( this.rectTransform, e.position, Stage.inst.eventCamera, out this._pointerStartLocalCursor );
			this._contentStartPosition = this.content.rectTransform.anchoredPosition;
			this._dragging = true;

			eventData.StopPropagation();
		}

		protected override void OnEndDrag( BaseEventData eventData )
		{
			this._dragging = false;

			eventData.StopPropagation();
		}

		protected override void OnDrag( BaseEventData eventData )
		{
			PointerEventData e = ( PointerEventData )eventData;

			Vector2 localCursor;
			if ( !RectTransformUtility.ScreenPointToLocalPointInRectangle( this.rectTransform, e.position, Stage.inst.eventCamera, out localCursor ) )
				return;

			this.UpdateBounds();

			Vector2 pointerDelta = localCursor - this._pointerStartLocalCursor;
			Vector2 contentPos = this._contentStartPosition + pointerDelta;

			// Offset to get content into place in the view.
			Vector2 offset = this.CalculateOffset( contentPos - this.content.rectTransform.anchoredPosition );
			contentPos += offset;
			if ( this.movementType == MovementType.Elastic )
			{
				if ( MathUtils.Abs( offset.x ) > 0.01f )
					contentPos.x = contentPos.x - RubberDelta( offset.x, this._viewBounds.size.x );
				if ( MathUtils.Abs( offset.y ) > 0.01f )
					contentPos.y = contentPos.y - RubberDelta( offset.y, this._viewBounds.size.y );
			}

			this.SetContentAnchoredPosition( contentPos );

			eventData.StopPropagation();
		}

		private void SetContentAnchoredPosition( Vector2 pos )
		{
			pos.y = -pos.y;
			if ( this._scrollType != ScrollType.Both && this._scrollType != ScrollType.Horizontal )
				pos.x = this.content.position.x;
			if ( this._scrollType != ScrollType.Both && this._scrollType != ScrollType.Vertical )
				pos.y = this.content.position.y;

			if ( pos != this.content.position )
			{
				this.content.position = pos;
				this.UpdateBounds();
			}
		}

		private Vector2 CalculateOffset( Vector2 delta )
		{
			Vector2 offset = Vector2.zero;
			if ( this.movementType == MovementType.Unrestricted )
				return offset;

			Vector2 min = this._contentBounds.min;
			Vector2 max = this._contentBounds.max;

			if ( this._scrollType == ScrollType.Both || this._scrollType == ScrollType.Horizontal )
			{
				min.x += delta.x;
				max.x += delta.x;
				if ( min.x > this._viewBounds.min.x )
					offset.x = this._viewBounds.min.x - min.x;
				else if ( max.x < this._viewBounds.max.x )
					offset.x = this._viewBounds.max.x - max.x;
			}

			if ( this._scrollType == ScrollType.Both || this._scrollType == ScrollType.Vertical )
			{
				min.y += delta.y;
				max.y += delta.y;
				if ( max.y < this._viewBounds.max.y )
					offset.y = this._viewBounds.max.y - max.y;
				else if ( min.y > this._viewBounds.min.y )
					offset.y = this._viewBounds.min.y - min.y;
			}

			return offset;
		}

		private void UpdateBounds()
		{
			this._viewBounds.min = this.rect.min;
			this._viewBounds.max = this.rect.max;

			this.FitForContentSize();
			if ( this._contentSizeDirty || this._viewBounds != this._prevViewBounds || this.content.rectTransform.anchoredPosition != this._prevPosition )
			{
				this._contentSizeDirty = false;
				this._contentBounds = this.GetBounds();
			}

			if ( this._viewBounds == this._prevViewBounds && this._contentBounds == this._prevContentBounds )
				return;

			Vector3 contentSize = this._contentBounds.size;
			Vector3 contentPos = this._contentBounds.center;
			Vector2 contentPivot = this.content.pivot;

			AdjustBounds( ref this._viewBounds, ref contentPivot, ref contentSize, ref contentPos );

			this._contentBounds.size = contentSize;
			this._contentBounds.center = contentPos;

			if ( this.movementType == MovementType.Clamped )
			{
				// Adjust content so that content bounds bottom (right side) is never higher (to the left) than the view bounds bottom (right side).
				//                                       top (left side) is never lower (to the right) than the view bounds top (left side).
				// All this can happen if content has shrunk.
				// This works because content size is at least as big as view size (because of the call to InternalUpdateBounds above).
				Vector2 delta = Vector2.zero;
				if ( this._viewBounds.max.x > this._contentBounds.max.x )
				{
					delta.x = MathUtils.Min( this._viewBounds.min.x - this._contentBounds.min.x, this._viewBounds.max.x - this._contentBounds.max.x );
				}
				else if ( this._viewBounds.min.x < this._contentBounds.min.x )
				{
					delta.x = MathUtils.Max( this._viewBounds.min.x - this._contentBounds.min.x, this._viewBounds.max.x - this._contentBounds.max.x );
				}

				if ( this._viewBounds.min.y < this._contentBounds.min.y )
				{
					delta.y = MathUtils.Max( this._viewBounds.min.y - this._contentBounds.min.y, this._viewBounds.max.y - this._contentBounds.max.y );
				}
				else if ( this._viewBounds.max.y > this._contentBounds.max.y )
				{
					delta.y = MathUtils.Min( this._viewBounds.min.y - this._contentBounds.min.y, this._viewBounds.max.y - this._contentBounds.max.y );
				}
				if ( delta.sqrMagnitude > float.Epsilon )
				{
					contentPos = this.content.position + delta;
					if ( this._scrollType != ScrollType.Both && this._scrollType != ScrollType.Horizontal )
						contentPos.x = this.content.position.x;
					if ( this._scrollType != ScrollType.Both && this._scrollType != ScrollType.Vertical )
						contentPos.y = this.content.position.y;

					AdjustBounds( ref this._viewBounds, ref contentPivot, ref contentSize, ref contentPos );
				}
			}
		}

		private static void AdjustBounds( ref Bounds viewBounds, ref Vector2 contentPivot, ref Vector3 contentSize, ref Vector3 contentPos )
		{
			// Make sure content bounds are at least as large as view by adding padding if not.
			// One might think at first that if the content is smaller than the view, scrolling should be allowed.
			// However, that's not how scroll views normally work.
			// Scrolling is *only* possible when content is *larger* than view.
			// We use the pivot of the content rect to decide in which directions the content bounds should be expanded.
			// E.g. if pivot is at top, bounds are expanded downwards.
			// This also works nicely when ContentSizeFitter is used on the content.
			Vector3 excess = viewBounds.size - contentSize;
			if ( excess.x > 0 )
			{
				contentPos.x -= excess.x * ( contentPivot.x - 0.5f );
				contentSize.x = viewBounds.size.x;
			}
			if ( excess.y > 0 )
			{
				contentPos.y -= excess.y * ( contentPivot.y - 0.5f );
				contentSize.y = viewBounds.size.y;
			}
		}

		private Bounds GetBounds()
		{
			Vector3 vMin = new Vector3( float.MaxValue, float.MaxValue, float.MaxValue );
			Vector3 vMax = new Vector3( float.MinValue, float.MinValue, float.MinValue );

			Matrix4x4 toLocal = this.rectTransform.worldToLocalMatrix;
			this.content.GetWorldCorners( this._corners );
			for ( int j = 0; j < 4; j++ )
			{
				Vector3 v = toLocal.MultiplyPoint3x4( this._corners[j] );
				vMin = Vector3.Min( v, vMin );
				vMax = Vector3.Max( v, vMax );
			}

			Bounds bounds = new Bounds( vMin, Vector3.zero );
			bounds.Encapsulate( vMax );
			return bounds;
		}

		private static float RubberDelta( float overStretching, float viewSize )
		{
			return ( 1 - ( 1 / ( ( Mathf.Abs( overStretching ) * 0.55f / viewSize ) + 1 ) ) ) * viewSize * Mathf.Sign( overStretching );
		}

		internal void SetContentSize( Vector2 contentSize )
		{
			if ( this.contentSizeAutoFit )
			{
				Logger.Warn( "Content of ScrollView is driven by itself" );
				return;
			}
			if ( this.content.size != contentSize )
			{
				this._contentSizeDirty = true;
				this.content.size = contentSize;
			}
		}

		private void FitForContentSize()
		{
			if ( !this.contentSizeAutoFit )
				return;

			Vector2 oldContentSize = this.content.size;
			this.content.size = this._owner.contentSize;
			this._contentSizeDirty = oldContentSize != this.content.size;
		}

		private void EnsureBoundsCorrect()
		{
			this.UpdateBounds();
			this.UpdateScrollbars( this.CalculateOffset( Vector2.zero ) );
		}

		protected internal override void Update( UpdateContext context )
		{
			base.Update( context );

			this.UpdateBounds();

			float deltaTime = Time.unscaledDeltaTime;
			Vector2 offset = this.CalculateOffset( Vector2.zero );
			if ( !this._dragging && ( offset != Vector2.zero || this._velocity != Vector2.zero ) )
			{
				Vector2 contentPos = this.content.rectTransform.anchoredPosition;
				for ( int axis = 0; axis < 2; axis++ )
				{
					// Apply spring physics if movement is elastic and content has an offset from the view.
					if ( this.movementType == MovementType.Elastic && offset[axis] != 0 )
					{
						float speed = this._velocity[axis];
						float current = contentPos[axis];
						contentPos[axis] = Mathf.SmoothDamp( current,
							current + offset[axis],
							ref speed, this.elasticity, Mathf.Infinity, deltaTime );
						if ( Mathf.Abs( speed ) < 1 )
							speed = 0;
						this._velocity[axis] = speed;
					}
					// Else move content according to velocity with deceleration applied.
					else if ( this.inertia )
					{
						this._velocity[axis] *= Mathf.Pow( this.decelerationRate, deltaTime );
						if ( Mathf.Abs( this._velocity[axis] ) < 1 )
							this._velocity[axis] = 0;
						contentPos[axis] += this._velocity[axis] * deltaTime;
					}
					// If we have neither elaticity or friction, there shouldn't be any velocity.
					else
						this._velocity[axis] = 0;
				}

				if ( this.movementType == MovementType.Clamped )
				{
					offset = this.CalculateOffset( contentPos - this.content.rectTransform.anchoredPosition );
					contentPos += offset;
				}

				this.SetContentAnchoredPosition( contentPos );
			}

			if ( this._dragging && this.inertia )
			{
				Vector3 newVelocity = ( this.content.rectTransform.anchoredPosition - this._prevPosition ) / deltaTime;
				this._velocity = Vector3.Lerp( this._velocity, newVelocity, deltaTime * 10 );
			}

			bool viewBoundsChanged = this._viewBounds != this._prevViewBounds;
			bool contentBoundsChanged = this._contentBounds != this._prevContentBounds;
			bool positionChanged = this.content.rectTransform.anchoredPosition != this._prevPosition;

			if ( viewBoundsChanged || contentBoundsChanged || positionChanged )
			{
				this.UpdateScrollbars( offset );
				this.onChange.Call( this.normalizedPosition );
				this.UpdatePrevData();
			}
		}

		private float GetHorizontalNormalizedPosition()
		{
			if ( this._contentBounds.size.x <= this._viewBounds.size.x )
				return ( this._viewBounds.min.x > this._contentBounds.min.x ) ? 1 : 0;
			return ( this._viewBounds.min.x - this._contentBounds.min.x ) / ( this._contentBounds.size.x - this._viewBounds.size.x );
		}

		private float GetVerticalNormalizedPosition()
		{
			if ( this._contentBounds.size.y <= this._viewBounds.size.y )
				return ( this._viewBounds.min.y > this._contentBounds.min.y ) ? 1 : 0;
			return ( this._viewBounds.min.y - this._contentBounds.min.y ) / ( this._contentBounds.size.y - this._viewBounds.size.y );
		}

		private void UpdateScrollbars( Vector2 offset )
		{
			if ( this._hzScrollBar != null )
			{
				this._hzScrollBar.visualSize = this._contentBounds.size.x > 0
					? Mathf.Clamp01( ( this._viewBounds.size.x - Mathf.Abs( offset.x ) ) / this._contentBounds.size.x ) : 1;
				this._hzScrollBar.value = this.GetHorizontalNormalizedPosition();
			}

			if ( this._vtScrollBar != null )
			{
				this._vtScrollBar.visualSize = this._contentBounds.size.y > 0
					? Mathf.Clamp01( ( this._viewBounds.size.y - Mathf.Abs( offset.y ) ) / this._contentBounds.size.y ) : 1;
				this._vtScrollBar.value = this.GetVerticalNormalizedPosition();
			}

			if ( this._scrollBarDisplay == ScrollBarDisplayType.Auto )
			{
				if ( this._hzScrollBar != null )
					this._hzScrollBar.visible = this._contentBounds.size.x > this._viewBounds.size.x;
				if ( this._vtScrollBar != null )
					this._vtScrollBar.visible = this._contentBounds.size.y > this._viewBounds.size.y;
			}
			else if ( this._scrollBarDisplay == ScrollBarDisplayType.Visible )
			{
				if ( this._hzScrollBar != null )
					this._hzScrollBar.visible = true;
				if ( this._vtScrollBar != null )
					this._vtScrollBar.visible = true;
			}
		}

		private void UpdatePrevData()
		{
			this._prevPosition = this.content.rectTransform.anchoredPosition;
			this._prevViewBounds = this._viewBounds;
			this._prevContentBounds = this._contentBounds;
		}

		protected override void HandleSizeChanged()
		{
			base.HandleSizeChanged();

			this.UpdateLayout();
		}

		protected override void HandleTouchableChanged()
		{
			base.HandleTouchableChanged();

			if ( this._vtScrollBar != null )
				this._vtScrollBar.touchable = this.touchable;

			if ( this._hzScrollBar != null )
				this._hzScrollBar.touchable = this.touchable;
		}

		protected override void HandleGrayedChanged()
		{
			base.HandleGrayedChanged();

			if ( this._vtScrollBar != null )
				this._vtScrollBar.grayed = this.grayed;

			if ( this._hzScrollBar != null )
				this._hzScrollBar.grayed = this.grayed;
		}

		protected override void HandleVisibleChanged()
		{
			base.HandleVisibleChanged();

			this.content.visible = this.visible;
		}
	}
}