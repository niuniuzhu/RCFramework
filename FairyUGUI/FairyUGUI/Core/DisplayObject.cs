using FairyUGUI.Event;
using FairyUGUI.UI;
using FairyUGUI.Utils;
using System.Collections.Generic;
using Game.Pool;
using UnityEngine;
using UnityEngine.UI;
using EventType = FairyUGUI.Event.EventType;
using Object = UnityEngine.Object;

namespace FairyUGUI.Core
{
	public class DisplayObject : EventDispatcher
	{
		internal string id;

		private string _name;
		public string name
		{
			get => this._name;
			set
			{
				if ( this._name == value )
					return;
				this._name = value;
				this.gameObject.name = this.GetType().Name + ( string.IsNullOrEmpty( this.name ) ? string.Empty : "@" + this._name );
			}
		}

		internal GObject gOwner { get; set; }

		internal int siblingIndex
		{
			get => this.rectTransform.GetSiblingIndex();
			set => this.rectTransform.SetSiblingIndex( value );
		}

		internal DisplayObject topmost
		{
			get
			{
				DisplayObject currentObject = this;
				while ( currentObject.parent != null )
					currentObject = currentObject.parent;
				return currentObject;
			}
		}

		internal Stage stage { get; set; }

		internal Container parent { get; private set; }

		public GameObject gameObject { get; private set; }

		public RectTransform rectTransform { get; private set; }

		internal RectTransform clipTransform;

		static readonly Vector2 DEFAULT_POVIT = new Vector2( 0, 1 );
		internal virtual Vector2 position
		{
			get
			{
				Vector2 pos = this.rectTransform.anchoredPosition;
				pos.y = -pos.y;
				return pos;
			}
			set
			{
				value.y = -value.y;
				if ( value == this.rectTransform.anchoredPosition )
					return;

				//if ( this.pivot != DEFAULT_POVIT )
				//{
				//	Vector2 p = this.pivot;
				//	Vector2 s = this.size;
				//	value.x += s.x * p.x;
				//	value.y -= s.y * ( 1 - p.y );
				//}
				this.rectTransform.anchoredPosition = value;
				this.HandlePositionChanged();
			}
		}

		internal virtual Vector3 localPosition
		{
			get => this.rectTransform.localPosition;
			set
			{
				if ( this.rectTransform.localPosition == value )
					return;
				this.rectTransform.localPosition = value;
				this.HandlePositionChanged();
			}
		}

		internal Vector2 anchorOffsetMin
		{
			get => this.rectTransform.offsetMin;
			set => this.rectTransform.offsetMin = value;
		}

		internal Vector2 anchorOffsetMax
		{
			get => this.rectTransform.offsetMax;
			set => this.rectTransform.offsetMax = value;
		}

		internal AnchorType anchor
		{
			set => ToolSet.SetAnchor( this.rectTransform, value );
		}

		internal virtual Vector2 pivot
		{
			get => this.rectTransform.pivot;
			set
			{
				if ( this.rectTransform.pivot == value )
					return;

				Vector2 s = this.rectTransform.rect.size;
				Vector2 deltaPivot = value - this.rectTransform.pivot;
				Vector2 deltaPosition = new Vector2( deltaPivot.x * s.x, deltaPivot.y * s.y );
				this.rectTransform.pivot = value;
				this.rectTransform.anchoredPosition += deltaPosition;
			}
		}

		internal virtual Vector2 size
		{
			get => this.rectTransform.sizeDelta;
			set
			{
				if ( this.rectTransform.sizeDelta == value )
					return;
				this.rectTransform.sizeDelta = value;
				this.HandleSizeChanged();
			}
		}

		internal virtual Vector2 scale
		{
			get => this.rectTransform.localScale;
			set
			{
				Vector3 newValue = new Vector3( value.x, value.y, 1 );
				if ( this.rectTransform.localScale == newValue )
					return;
				this.rectTransform.localScale = newValue;
				this.HandleSizeChanged();
			}
		}

		internal Rect rect => this.rectTransform.rect;

		internal Vector2 sizeWithAnchors
		{
			set
			{
				this.rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, value.x );
				this.rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, value.y );
			}
		}

		internal Quaternion rotation
		{
			get => this.rectTransform.localRotation;
			set => this.rectTransform.localRotation = value;
		}

		internal float rotationZ
		{
			get => -this.rectTransform.localRotation.eulerAngles.z;
			set
			{
				Vector3 eulerAngles = this.rectTransform.localRotation.eulerAngles;
				this.rectTransform.localRotation = Quaternion.Euler( eulerAngles.x, eulerAngles.y, -value );
			}
		}

		private bool _touchable;
		internal virtual bool touchable
		{
			get => this._touchable;
			set
			{
				if ( this._touchable == value )
					return;
				this._touchable = value;
				this.HandleTouchableChanged();
			}
		}

		private bool _visible;
		internal bool visible
		{
			get => this._visible;
			set
			{
				if ( this._visible == value )
					return;

				this._visible = value;
				//if ( !value )
				//	this.gameObject.hideFlags |= HideFlags.HideInHierarchy;
				//else
				//	this.gameObject.hideFlags = HideFlags.None;
				this.gameObject.SetActive( this.parent != null && this._visible );
				this.HandleVisibleChanged();
			}
		}

		internal virtual int layer
		{
			get => this.gameObject.layer;
			set => this.gameObject.layer = value;
		}

		private bool _grayed;
		internal virtual bool grayed
		{
			get => this._grayed;
			set
			{
				if ( this._grayed == value )
					return;
				this._grayed = value;
				if ( this.material != null )
					this.material = this._grayed ? MaterialManager.EnableGrayed( this.material ) : MaterialManager.DisableGrayed( this.material );
				this.HandleGrayedChanged();
			}
		}

		internal Graphic graphic { get; set; }

		internal bool eventGraphicOnly { get; set; }

		private CanvasRenderer _canvasRenderer;

		private Color _color;
		internal virtual Color color
		{
			get => this._color;
			set
			{
				if ( this._color == value )
					return;
				this._color = value;
				if ( this.graphic == null )
					return;
				this.graphic.color = this._color;
			}
		}

		private BlendMode _blendMode;
		internal virtual BlendMode blendMode
		{
			get => this._blendMode;
			set
			{
				if ( this._blendMode == value )
					return;
				this._blendMode = value;
				if ( this.material != null )
				{
					this.material = MaterialManager.SetBlendMode( this.material, this._blendMode );
					this.material.ApplyBlendMode( this._blendMode );
				}
			}
		}

		private ColorFilter _colorFilter;
		internal virtual ColorFilter colorFilter
		{
			get => this._colorFilter;
			set
			{
				if ( this._colorFilter == value )
					return;
				this._colorFilter = value;
				if ( this.material != null )
				{
					if ( !this._colorFilter.isIdentity )
					{
						this.material = MaterialManager.EnableColorFilter( this.material );
						this.material.ApplyColorFilter( this._colorFilter );
					}
					else
						this.material = MaterialManager.DisableColorFilter( this.material );
				}
			}
		}

		private BlurFilter _blurFilter;
		internal virtual BlurFilter blurFilter
		{
			get => this._blurFilter;
			set
			{
				if ( this._blurFilter == value )
					return;
				this._blurFilter = value;
				if ( this.material != null )
				{
					if ( !this._blurFilter.isIdentity )
					{
						this.material = MaterialManager.EnableBlurFilter( this.material );
						this.material.ApplyBlurFilter( this._blurFilter );
					}
					else
						this.material = MaterialManager.DsiableBlurFilter( this.material );
				}
			}
		}

		private NMaterial _material;
		internal virtual NMaterial material
		{
			get => this._material;
			set
			{
				if ( this._material == value )
					return;
				this._material = value;
				if ( this.graphic == null )
					return;
				this.graphic.material = this._material == null ? null : this._material.material;
			}
		}

		internal virtual Shader shader
		{
			get => this.graphic != null ? this.graphic.material.shader : null;
			set
			{
				if ( this.graphic == null )
					return;
				this.graphic.material.shader = value;
			}
		}

		private Canvas _canvas;
		internal virtual int sortingOrder
		{
			set
			{
				if ( this.graphic == null )
					return;

				if ( value == -1 )
				{
					if ( this._canvas != null )
					{
						Object.Destroy( this._canvas );
						this._canvas = null;
					}
				}
				else
				{
					if ( this._canvas == null )
						this._canvas = this.AddComponent<Canvas>();

					this._canvas.overrideSorting = true;
					this._canvas.sortingOrder = value;
				}
			}
		}

		public enum CullState
		{
			Invaild,
			Culled,
			Overlaps
		}

		private CullState _cullState;
		internal CullState cullState
		{
			get => this._cullState;
			set
			{
				if ( this._cullState == value )
					return;
				this._cullState = value;
				if ( this._canvasRenderer != null )
				{
					this._canvasRenderer.cull = this._cullState == CullState.Culled;
					this.graphic.SetVerticesDirty();
				}
				this.onCullChanged.Call( this._cullState );
			}
		}

		private readonly List<DisplayObject> _clippers = new List<DisplayObject>();

		protected OverflowType _overflow;
		internal virtual OverflowType overflow
		{
			get => this._overflow;
			set => this._overflow = value;
		}

		private Rect _canvasRect;
		internal Rect canvasRect
		{
			get
			{
				if ( !this._shouldUpdateCanvasRect )
					return this._canvasRect;
				this._shouldUpdateCanvasRect = false;
				this._canvasRect = ClipHelper.GetCanvasRect( this, Stage.inst );
				return this._canvasRect;
			}
		}

		private bool _shouldUpdateCanvasRect;
		internal virtual bool shouldUpdateCanvasRect
		{
			set => this._shouldUpdateCanvasRect = value;
		}

		private bool _shouldUpdateClipping;
		internal virtual bool shouldUpdateClipping
		{
			set => this._shouldUpdateClipping = value;
		}

		internal EventListener onClick { get; private set; }
		internal EventListener onScroll { get; private set; }
		internal EventListener onTouchBegin { get; private set; }
		internal EventListener onTouchEnd { get; private set; }
		internal EventListener onRollOver { get; private set; }
		internal EventListener onRollOut { get; private set; }
		internal EventListener onInitializePotentialDrag { get; private set; }
		internal EventListener onBeginDrag { get; private set; }
		internal EventListener onEndDrag { get; private set; }
		internal EventListener onDrag { get; private set; }
		internal EventListener onDrop { get; private set; }
		internal EventListener onCancel { get; private set; }
		internal EventListener onSelect { get; private set; }
		internal EventListener onDeselect { get; private set; }
		internal EventListener onUpdateSelected { get; private set; }
		internal EventListener onAddedToStage { get; private set; }
		internal EventListener onRemovedFromStage { get; private set; }
		internal EventListener onPositionChanged { get; private set; }
		internal EventListener onSizeChanged { get; private set; }
		internal EventListener onCullChanged { get; private set; }

		internal DisplayObject( GObject owner )
		{
			this.gOwner = owner;
			this._touchable = true;
			this._visible = true;
			this._colorFilter.Reset();

			this.onClick = new EventListener( this, EventType.Click );
			this.onScroll = new EventListener( this, EventType.Scroll );
			this.onTouchBegin = new EventListener( this, EventType.TouchBegin );
			this.onTouchEnd = new EventListener( this, EventType.TouchEnd );
			this.onRollOver = new EventListener( this, EventType.RollOver );
			this.onRollOut = new EventListener( this, EventType.RollOut );
			this.onInitializePotentialDrag = new EventListener( this, EventType.InitializeDrag );
			this.onBeginDrag = new EventListener( this, EventType.BeginDrag );
			this.onEndDrag = new EventListener( this, EventType.EndDrag );
			this.onDrag = new EventListener( this, EventType.Drag );
			this.onDrop = new EventListener( this, EventType.Drop );
			this.onCancel = new EventListener( this, EventType.Cancel );
			this.onSelect = new EventListener( this, EventType.Select );
			this.onDeselect = new EventListener( this, EventType.Deselect );
			this.onUpdateSelected = new EventListener( this, EventType.UpdateSelected );
			this.onAddedToStage = new EventListener( this, EventType.AddToStage );
			this.onRemovedFromStage = new EventListener( this, EventType.RemoveFromStage );
			this.onPositionChanged = new EventListener( this, EventType.PositionChanged );
			this.onSizeChanged = new EventListener( this, EventType.SizeChanged );
			this.onCullChanged = new EventListener( this, EventType.CullChanged );

			this.CreateGameObject();
		}

		protected override void InternalDispose()
		{
			this.UnregisterAllEventTriggerTypes();
			this._eventTriggerTypes = null;

			if ( this.material != null )
			{
				this.material.pool.Release( this.material );
				this.material = null;
			}

			//this.RemoveFromParent(); //call by owner

			if ( this.graphic != null )
			{
				Object.DestroyImmediate( this.graphic );
				this.graphic = null;
			}

			if ( this.gameObject != null )
			{
				Object.DestroyImmediate( this.gameObject );
				this.gameObject = null;
			}

			this._clippers.Clear();
			this.clipTransform = null;
			this.rectTransform = null;
			this.gOwner = null;

			base.InternalDispose();
		}

		private void CreateGameObject()
		{
			this.gameObject =
				new GameObject( ( this.gOwner?.GetType().Name ?? this.GetType().Name ) +
							   ( string.IsNullOrEmpty( this.name ) ? "" : "@" + this._name ) );

			Object.DontDestroyOnLoad( this.gameObject );
			this.gameObject.layer = LayerMask.NameToLayer( Stage.LAYER_NAME );
			this.gameObject.hideFlags |= HideFlags.HideInHierarchy;
			this.gameObject.SetActive( false );

			this.rectTransform = this.AddComponent<RectTransform>();
			this.rectTransform.anchorMin = new Vector2( 0, 1 );
			this.rectTransform.anchorMax = new Vector2( 0, 1 );
			this.rectTransform.pivot = new Vector2( 0, 1 );
			this.rectTransform.sizeDelta = Vector2.zero;

			if ( DisplayOptions.defaultRoot != null )
				ToolSet.SetParent( this.rectTransform, DisplayOptions.defaultRoot[0] );

			this.OnGameObjectCreated();

			if ( this.graphic != null )
			{
				this._canvasRenderer = this.graphic.GetComponent<CanvasRenderer>();
				this.graphic.raycastTarget = false;
				this.material = MaterialManager.GetDefaultMaterial();

				if ( UIConfig.useCanvasSortingOrder )
				{
					this._canvas = this.AddComponent<Canvas>();
					this._canvas.overrideSorting = true;
				}
			}
		}

		protected virtual void OnGameObjectCreated()
		{
		}

		internal void SetParent( Container value )
		{
			if ( this.parent == value )
				return;

			this.parent = value;

			if ( this.parent != null )
			{
				this.gameObject.hideFlags &= ~HideFlags.HideInHierarchy;
				this.gameObject.SetActive( this._visible );

				this.rectTransform.SetParent( this.parent.rectTransform, false );
			}
			else
			{
				this.rectTransform.SetParent( null, false );
				this.gameObject.SetActive( false );
				this.gameObject.hideFlags |= HideFlags.HideInHierarchy;
			}
		}

		internal void RemoveFromParent()
		{
			this.parent?.RemoveChild( this );
		}

		internal T AddComponent<T>() where T : Component
		{
			return this.gameObject.AddComponent<T>();
		}

		internal T GetComponent<T>()
		{
			return this.gameObject.GetComponent<T>();
		}

		internal void GetComponentsInParent<T>( bool includeInactive, List<T> results )
		{
			this.gameObject.GetComponentsInParent( includeInactive, results );
		}

		internal void GetWorldCorners( Vector3[] corners )
		{
			this.rectTransform.GetWorldCorners( corners );
		}

		internal virtual void AddClipper( DisplayObject clipper )
		{
			if ( !this._clippers.Contains( clipper ) )
				this._clippers.Add( clipper );
		}

		internal virtual void RemoveClipper( DisplayObject clipper )
		{
			this._clippers.Remove( clipper );
		}

		private void SetClipStateDirty()
		{
			this.shouldUpdateCanvasRect = true;
			this.shouldUpdateClipping = true;
		}

		private void PerformClip()
		{
			if ( !this.visible )
				return;

			if ( !this._shouldUpdateClipping )
				return;

			this._shouldUpdateClipping = false;

			if ( this.stage == null || this._clippers.Count == 0 )
				return;

			List<Rect> parentMaskRects = ListPool<Rect>.Get();
			int count = this._clippers.Count;
			for ( int i = 0; i < count; i++ )
				parentMaskRects.Add( this._clippers[i].canvasRect );

			Rect clipRect = ClipHelper.FindCullAndClipWorldRect( this.canvasRect, parentMaskRects, out CullState mCullState );

			parentMaskRects.Clear();
			ListPool<Rect>.Release( parentMaskRects );

			if ( mCullState == CullState.Overlaps )
				this.EnableRectClipping( clipRect );
			else
				this.DisableRectClipping();
			this.cullState = mCullState;
		}

		private void EnableRectClipping( Rect clipRect )
		{
			if ( this._canvasRenderer != null )
				this._canvasRenderer.EnableRectClipping( clipRect );
		}

		private void DisableRectClipping()
		{
			if ( this._canvasRenderer != null )
				this._canvasRenderer.DisableRectClipping();
		}

		protected internal virtual void HandleAddToStage()
		{
			this.stage = this.parent.stage;
			DisplayObjectRegister.RegisterDisplayObject( this );

			DisplayObject p = this.parent;
			while ( p != null )
			{
				IRaycastFilter filter = p as IRaycastFilter;
				if ( filter != null )
					this.clipTransform = p.rectTransform;

				if ( p.overflow != OverflowType.Visible )
					this._clippers.Add( p );

				p = p.parent;
			}

			this.SetClipStateDirty();

			this.gOwner?.HandleAddToStage();

			this.onAddedToStage.Call();
		}

		protected internal virtual void HandleRemoveFromStage()
		{
			this.onRemovedFromStage.Call();

			this.gOwner?.HandleRemoveFromStage();

			this.clipTransform = null;
			this._clippers.Clear();
			this.DisableRectClipping();
			this.cullState = CullState.Invaild;

			DisplayObjectRegister.UnregisterDisplayObject( this );
			this.stage = null;
		}

		protected virtual void HandlePositionChanged()
		{
			this.SetClipStateDirty();
		}

		protected virtual void HandleSizeChanged()
		{
			this.SetClipStateDirty();
		}

		protected virtual void HandleTouchableChanged()
		{
		}

		protected virtual void HandleGrayedChanged()
		{
		}

		protected virtual void HandleVisibleChanged()
		{
		}

		internal virtual bool Raycast( Vector2 sp, Camera eventCamera )
		{
			if ( this.eventGraphicOnly && this.graphic == null )
				return false;
			if ( !RectTransformUtility.RectangleContainsScreenPoint( this.rectTransform, sp, eventCamera ) )
				return false;
			if ( this.clipTransform == null )//是否在scrollview内
				return true;
			return RectTransformUtility.RectangleContainsScreenPoint( this.clipTransform, sp, eventCamera );
		}

		protected internal virtual void Update( UpdateContext context )
		{
		}

		protected internal virtual void WillRenderCanvases()
		{
			this.PerformClip();
		}

		#region Event callbacks
		internal void TriggerEvent( BaseEventData eventData, EventTriggerType type )
		{
			switch ( type )
			{
				case EventTriggerType.PointerEnter:
					this.OnPointerEnter( eventData );
					this.onRollOver.SimpleCall( null, eventData );
					break;

				case EventTriggerType.PointerExit:
					this.OnPointerExit( eventData );
					this.onRollOut.SimpleCall( null, eventData );
					break;

				case EventTriggerType.PointerDown:
					this.OnPointerDown( eventData );
					this.onTouchBegin.SimpleCall( null, eventData );
					break;

				case EventTriggerType.PointerUp:
					this.OnPointerUp( eventData );
					this.onTouchEnd.SimpleCall( null, eventData );
					break;

				case EventTriggerType.PointerClick:
					this.OnPointerClick( eventData );
					this.onClick.SimpleCall( null, eventData );
					break;

				case EventTriggerType.Drag:
					this.OnDrag( eventData );
					this.onDrag.SimpleCall( null, eventData );
					break;

				case EventTriggerType.Drop:
					this.OnDrop( eventData );
					this.onDrop.SimpleCall( null, eventData );
					break;

				case EventTriggerType.Scroll:
					this.OnScroll( eventData );
					this.onScroll.SimpleCall( null, eventData );
					break;

				case EventTriggerType.UpdateSelected:
					this.OnUpdateSelected( eventData );
					this.onUpdateSelected.SimpleCall( null, eventData );
					break;

				case EventTriggerType.Select:
					this.OnSelect( eventData );
					this.onSelect.SimpleCall( null, eventData );
					break;

				case EventTriggerType.Deselect:
					this.OnDeselect( eventData );
					this.onDeselect.SimpleCall( null, eventData );
					break;

				case EventTriggerType.InitializePotentialDrag:
					this.OnInitializePotentialDrag( eventData );
					this.onInitializePotentialDrag.SimpleCall( null, eventData );
					break;

				case EventTriggerType.BeginDrag:
					this.OnBeginDrag( eventData );
					this.onBeginDrag.SimpleCall( null, eventData );
					break;

				case EventTriggerType.EndDrag:
					this.OnEndDrag( eventData );
					this.onEndDrag.SimpleCall( null, eventData );
					break;
			}
			this.gOwner?.TriggerEvent( eventData, type );
		}

		protected virtual void OnPointerDown( BaseEventData eventData )
		{
		}

		protected virtual void OnPointerUp( BaseEventData eventData )
		{
		}

		protected virtual void OnPointerEnter( BaseEventData eventData )
		{
		}

		protected virtual void OnPointerExit( BaseEventData eventData )
		{
		}

		protected virtual void OnPointerClick( BaseEventData eventData )
		{
		}

		protected virtual void OnBeginDrag( BaseEventData eventData )
		{
		}

		protected virtual void OnEndDrag( BaseEventData eventData )
		{
		}

		protected virtual void OnDrag( BaseEventData eventData )
		{
		}

		protected virtual void OnDrop( BaseEventData eventData )
		{
		}

		protected virtual void OnScroll( BaseEventData eventData )
		{
		}

		protected virtual void OnInitializePotentialDrag( BaseEventData eventData )
		{
		}

		protected virtual void OnSubmit( BaseEventData eventData )
		{
		}

		protected virtual void OnCancel( BaseEventData eventData )
		{
		}

		protected virtual void OnSelect( BaseEventData eventData )
		{
		}

		protected virtual void OnDeselect( BaseEventData eventData )
		{
		}

		protected virtual void OnUpdateSelected( BaseEventData eventData )
		{
		}
		#endregion

		#region Event register
		private Dictionary<EventTriggerType, bool> _eventTriggerTypes;

		internal void RegisterEventTriggerType( EventTriggerType eventTriggerType )
		{
			if ( this._eventTriggerTypes == null )
				this._eventTriggerTypes = new Dictionary<EventTriggerType, bool>();

			if ( this._eventTriggerTypes.ContainsKey( eventTriggerType ) )
				return;

			this._eventTriggerTypes[eventTriggerType] = true;
		}

		internal bool UnregisterEventTriggerType( EventTriggerType eventTriggerType )
		{
			if ( this._eventTriggerTypes == null )
				return false;
			bool result = this._eventTriggerTypes.Remove( eventTriggerType );
			if ( this._eventTriggerTypes.Count == 0 )
				this._eventTriggerTypes = null;
			return result;
		}

		internal void UnregisterAllEventTriggerTypes()
		{
			if ( this._eventTriggerTypes == null )
				return;
			this._eventTriggerTypes.Clear();
			this._eventTriggerTypes = null;
		}

		internal bool HasEventTriggerType( EventTriggerType eventTriggerType )
		{
			return this._eventTriggerTypes != null && this._eventTriggerTypes.ContainsKey( eventTriggerType );
		}

		internal bool IsTriggerTypesEmpty()
		{
			return this._eventTriggerTypes == null;
		}
		#endregion

		public override string ToString()
		{
			return this.name;
		}
	}
}