using Core.Math;
using Core.Xml;
using DG.Tweening;
using FairyUGUI.Core;
using FairyUGUI.Event;
using System;
using UnityEngine;
using EventType = FairyUGUI.Event.EventType;
using Rect = UnityEngine.Rect;

namespace FairyUGUI.UI
{
	public class GObject : EventDispatcher, ILayoutItem
	{
		/// <summary>
		/// GObject的id，仅作为内部使用。与name不同，id值是不会相同的。
		/// id is for internal use only.
		/// </summary>
		public string id { get; private set; }

		/// <summary>
		/// Name of the object.
		/// </summary>
		public string name;

		/// <summary>
		/// Resource url of this object.
		/// </summary>
		public string resourceURL
		{
			get
			{
				if ( this.packageItem != null )
					return UIPackage.URL_PREFIX + this.packageItem.owner.id + this.packageItem.id;
				return null;
			}
		}

		/// <summary>
		/// User defined data. 
		/// </summary>
		public object data;

		public GRoot root
		{
			get
			{
				GRoot gRoot = this as GRoot;
				if ( gRoot != null )
					return gRoot;

				GObject p = this.parent;
				while ( p != null )
				{
					gRoot = p as GRoot;
					if ( gRoot != null )
						return gRoot;
					p = p.parent;
				}
				return GRoot.inst;
			}
		}

		/// <summary>
		/// The source width of the object.
		/// </summary>
		public int sourceWidth { get; internal set; }

		/// <summary>
		/// The source height of the object.
		/// </summary>
		public int sourceHeight { get; internal set; }

		/// <summary>
		/// The initial width of the object.
		/// </summary>
		public int initWidth { get; internal set; }

		/// <summary>
		/// The initial height of the object.
		/// </summary>
		public int initHeight { get; internal set; }

		/// <summary>
		/// Group belonging to.
		/// </summary>
		public GGroup group;

		public bool ignoreLayout { get; set; }

		/// <summary>
		/// Parent object.
		/// </summary>
		public GComponent parent { get; internal set; }

		/// <summary>
		/// Lowlevel display object.
		/// </summary>
		public DisplayObject displayObject { get; protected set; }

		/// <summary>
		/// Gear to display controller.
		/// </summary>
		public GearDisplay gearDisplay { get; private set; }

		/// <summary>
		/// Gear to color controller
		/// </summary>
		public GearColor gearColor { get; private set; }

		/// <summary>
		/// Gear to xy controller.
		/// </summary>
		public GearXY gearXY { get; private set; }

		/// <summary>
		/// Gear to size controller.
		/// </summary>
		public GearSize gearSize { get; private set; }

		/// <summary>
		/// Gear to look controller.
		/// </summary>
		public GearLook gearLook { get; private set; }

		/// <summary>
		/// Gear to icon.
		/// </summary>
		public GearIcon gearIcon { get; private set; }

		/// <summary>
		/// Gear to text.
		/// </summary>
		public GearText gearText { get; private set; }

		internal PackageItem packageItem;

		protected internal bool _underConstruct;

		internal XML constructingData;

		int _sortingOrder;
		/// <summary>
		/// By default(when sortingOrder==0), object added to component is arrange by the added roder. 
		/// The bigger is the sorting order, the object is more in front.
		/// </summary>
		public int sortingOrder
		{
			get => this._sortingOrder;
			set
			{
				if ( value < 0 )
					value = 0;
				if ( this._sortingOrder != value )
				{
					int old = this._sortingOrder;
					this._sortingOrder = value;
					this.parent?.ChildSortingOrderChanged( this, old, this._sortingOrder );
				}
			}
		}

		public bool inContainer => this.displayObject.parent != null;

		private Vector2 _position;
		public virtual Vector2 position
		{
			get => this._position;
			set
			{
				if ( this._position == value )
					return;

				this._position = value;

				this.HandlePositionChanged();

				if ( this.gearXY.controller != null )
					this.gearXY.UpdateState();

				this.parent?.SetBoundsChangedFlag();

				this.onPositionChanged.Call();
			}
		}

		public Vector2 minSize;
		public Vector2 maxSize;

		/// <summary>
		/// actualSize = size * scale
		/// </summary>
		private Vector2 _actualSize;
		public Vector2 actualSize
		{
			get => this._actualSize;
			set
			{
				if ( this._actualSize == value )
					return;

				this._actualSize = value;
				this.size = Vector2.Scale( this._actualSize, new Vector2( 1 / this._scale.x, 1 / this._scale.y ) );
			}
		}

		private Vector2 _size;
		public virtual Vector2 size
		{
			get => this._size;
			set
			{
				if ( value.x < this.minSize.x )
					value.x = this.minSize.x;
				else if ( this.maxSize.x > 0 && value.x > this.maxSize.x )
					value.x = this.maxSize.x;
				if ( value.y < this.minSize.y )
					value.y = this.minSize.y;
				else if ( this.maxSize.x > 0 && value.y > this.maxSize.x )
					value.y = this.maxSize.x;

				if ( this._size == value )
					return;

				this._size = value;
				Vector2 oldSize = this._actualSize;
				this._actualSize = Vector2.Scale( this._size, this._scale );
				this.UpdateSizeInternal( this._actualSize - oldSize );
			}
		}

		private Vector2 _scale;
		public virtual Vector2 scale
		{
			get => this._scale;
			set
			{
				if ( this._scale == value )
					return;

				this._scale = value;
				Vector2 oldSize = this._actualSize;
				this._actualSize = Vector2.Scale( this._size, this._scale );
				this.UpdateSizeInternal( this._actualSize - oldSize );
			}
		}

		private float _rotation;
		/// <summary>
		/// The rotation around the z axis of the object in degrees.
		/// </summary>
		public virtual float rotation
		{
			get => this._rotation;
			set
			{
				if ( MathUtils.Abs( this._rotation - value ) < 0.001f )
					return;
				this._rotation = value;

				if ( this.gearLook.controller != null )
					this.gearLook.UpdateState();

				this.displayObject.rotationZ = this._rotation;
			}
		}

		private Vector2 _pivot = new Vector2( 0, 1 );
		public Vector2 pivot
		{
			get => this._pivot;
			set
			{
				if ( this._pivot == value )
					return;
				this._pivot = value;

				this.displayObject.pivot = this._pivot;
			}
		}

		protected bool _touchable;
		public virtual bool touchable
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

		public bool finalTouchable { get; private set; }

		private bool _visible;
		public virtual bool visible
		{
			get => this._visible;
			set
			{
				if ( this._visible == value )
					return;
				this._visible = value;

				this.HandleVisibleChanged();
			}
		}

		private bool _gearVisible;
		public virtual bool gearVisible
		{
			get => this._gearVisible;
			set
			{
				if ( this._gearVisible == value )
					return;
				this._gearVisible = value;

				this.HandleVisibleChanged();
			}
		}

		private int _ignoreGearVisible;
		internal int ignoreGearVisible
		{
			get => this._ignoreGearVisible;
			set
			{
				if ( this._ignoreGearVisible == value )
					return;
				this._ignoreGearVisible = value;

				this.HandleVisibleChanged();
			}
		}

		public bool finalVisible { get; private set; }

		private bool _grayed;
		public virtual bool grayed
		{
			get => this._grayed;
			set
			{
				if ( this._grayed == value )
					return;
				this._grayed = value;

				this.HandleGrayedChanged();

				if ( this.gearLook.controller != null )
					this.gearLook.UpdateState();
			}
		}

		public bool finalGrayed { get; private set; }

		private Color _color;
		public virtual Color color
		{
			get => this._color;
			set
			{
				if ( this._color == value )
					return;
				this._color = value;
				this.HandleColorChanged();

				if ( this.gearColor.controller != null )
					this.gearColor.UpdateState();
			}
		}

		public Color finalColor { get; protected set; }

		public virtual float alpha
		{
			get => this._color.a;
			set
			{
				if ( this._color.a == value )
					return;
				this._color.a = value;
				this.HandleColorChanged();

				if ( this.gearLook.controller != null )
					this.gearLook.UpdateState();
			}
		}

		public virtual BlendMode blendMode
		{
			get => this.displayObject.blendMode;
			set => this.displayObject.blendMode = value;
		}

		public virtual ColorFilter colorFilter
		{
			get => this.displayObject.colorFilter;
			set
			{
				if ( this.displayObject.colorFilter == value )
					return;
				this.displayObject.colorFilter = value;

				this.HandleColorFilterChanged();
			}
		}

		public virtual BlurFilter blurFilter
		{
			get => this.displayObject.blurFilter;
			set
			{
				if ( this.displayObject.blurFilter == value )
					return;
				this.displayObject.blurFilter = value;

				this.HandleBlurFilterChanged();
			}
		}

		public virtual NMaterial material
		{
			get => this.displayObject.material;
			set => this.displayObject.material = value;
		}

		public virtual Shader shader
		{
			get => this.displayObject.shader;
			set => this.displayObject.shader = value;
		}

		public virtual string text
		{
			get { return string.Empty; }
			set { }
		}

		public virtual string icon
		{
			get { return string.Empty; }
			set { }
		}

		public Relations relations { get; private set; }

		public bool eventGraphicOnly
		{
			get => this.displayObject.eventGraphicOnly;
			set => this.displayObject.eventGraphicOnly = value;
		}

		private bool _enableDrag;
		public bool enableDrag
		{
			get => this._enableDrag;
			set
			{
				if ( this._enableDrag == value )
					return;
				this._enableDrag = value;

				if ( this._enableDrag )
				{
					this.displayObject.RegisterEventTriggerType( EventTriggerType.InitializePotentialDrag );
					this.displayObject.RegisterEventTriggerType( EventTriggerType.BeginDrag );
					this.displayObject.RegisterEventTriggerType( EventTriggerType.Drag );
					this.displayObject.RegisterEventTriggerType( EventTriggerType.EndDrag );
				}
				else
				{
					this.displayObject.UnregisterEventTriggerType( EventTriggerType.InitializePotentialDrag );
					this.displayObject.UnregisterEventTriggerType( EventTriggerType.BeginDrag );
					this.displayObject.UnregisterEventTriggerType( EventTriggerType.Drag );
					this.displayObject.UnregisterEventTriggerType( EventTriggerType.EndDrag );
				}
			}
		}

		private bool _enableDrop;
		public bool enableDrop
		{
			get => this._enableDrop;
			set
			{
				if ( this._enableDrop == value )
					return;
				this._enableDrop = value;

				if ( this._enableDrop )
					this.displayObject.RegisterEventTriggerType( EventTriggerType.Drop );
				else
					this.displayObject.UnregisterEventTriggerType( EventTriggerType.Drop );
			}
		}

		private bool _enableScroll;
		public bool enableScroll
		{
			get => this._enableScroll;
			set
			{
				if ( this._enableScroll == value )
					return;
				this._enableScroll = value;

				if ( this._enableScroll )
					this.displayObject.RegisterEventTriggerType( EventTriggerType.Scroll );
				else
					this.displayObject.UnregisterEventTriggerType( EventTriggerType.Scroll );
			}
		}

		public DisplayObject.CullState cullState => this.displayObject.cullState;

		[Flags]
		internal enum GearState
		{
			Position = 1 << 0,
			Size = 1 << 1,
			Color = 1 << 2,
			Look = 1 << 3,
			Animation = 1 << 4,
			Icon = 1 << 5,
			Text = 1 << 6
		}

		private GearState _gearState;

		public GImage asImage => this as GImage;

		public GComponent asCom => this as GComponent;

		public GButton asButton => this as GButton;

		public GLabel asLabel => this as GLabel;

		public GProgressBar asProgress => this as GProgressBar;

		public GSlider asSlider => this as GSlider;

		public GComboBox asComboBox => this as GComboBox;

		public GTextField asTextField => this as GTextField;

		public GRichTextField asRichTextField => this as GRichTextField;

		public GTextInput asTextInput => this as GTextInput;

		public GScrollBar asScrollBar => this as GScrollBar;

		public GLoader asLoader => this as GLoader;

		public GList asList => this as GList;

		public GGraph asGraph => this as GGraph;

		public GGroup asGroup => this as GGroup;

		public GMovieClip asMovieClip => this as GMovieClip;

		public EventListener onClick { get; private set; }
		public EventListener onScroll { get; private set; }
		public EventListener onTouchBegin { get; private set; }
		public EventListener onTouchEnd { get; private set; }
		public EventListener onRollOver { get; private set; }
		public EventListener onRollOut { get; private set; }
		public EventListener onInitializePotentialDrag { get; private set; }
		public EventListener onBeginDrag { get; private set; }
		public EventListener onEndDrag { get; private set; }
		public EventListener onDrag { get; private set; }
		public EventListener onDrop { get; private set; }
		public EventListener onSelect { get; private set; }
		public EventListener onDeselect { get; private set; }
		public EventListener onUpdateSelected { get; private set; }
		public EventListener onAddedToStage { get; private set; }
		public EventListener onRemovedFromStage { get; private set; }
		public EventListener onKeyDown { get; private set; }
		public EventListener onPositionChanged { get; private set; }
		public EventListener onSizeChanged { get; private set; }
		public EventListener onCullChanged { get; private set; }

		public GObject()
		{
			this._scale = Vector2.one;
			this.finalVisible = this._visible = this._gearVisible = true;
			this.finalTouchable = this._touchable = true;
			this.finalColor = this._color = Color.white;

			this.relations = new Relations( this );

			this.gearDisplay = new GearDisplay( this );
			this.gearColor = new GearColor( this );
			this.gearXY = new GearXY( this );
			this.gearSize = new GearSize( this );
			this.gearLook = new GearLook( this );
			this.gearIcon = new GearIcon( this );
			this.gearText = new GearText( this );

			this.CreateDisplayObject();

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
			this.onSelect = new EventListener( this, EventType.Select );
			this.onDeselect = new EventListener( this, EventType.Deselect );
			this.onUpdateSelected = new EventListener( this, EventType.UpdateSelected );
			this.onAddedToStage = new EventListener( this, EventType.AddToStage );
			this.onRemovedFromStage = new EventListener( this, EventType.RemoveFromStage );
			this.onKeyDown = new EventListener( this, EventType.Keydown );
			this.onPositionChanged = new EventListener( this, EventType.PositionChanged );
			this.onSizeChanged = new EventListener( this, EventType.SizeChanged );
			this.onCullChanged = new EventListener( this, EventType.CullChanged );
		}

		protected override void InternalDispose()
		{
			this.RemoveFromParent();
			this.relations.Dispose();
			this.displayObject.Dispose();
			this.displayObject = null;

			base.InternalDispose();
		}

		protected virtual void CreateDisplayObject()
		{
			this.displayObject = new DisplayObject( this );
		}

		public void RemoveFromParent()
		{
			this.parent?.RemoveChild( this );
		}

		public void AddRelation( GObject target, RelationType relationType, bool usePercent = false )
		{
			this.relations.Add( target, relationType, usePercent );
		}

		public void RemoveRelation( GObject target, RelationType relationType )
		{
			this.relations.Remove( target, relationType );
		}

		public void Center( bool restraint = false )
		{
			GComponent r = this.parent ?? this.root;

			this.position = ( r.size - this.size ) * 0.5f;
			if ( restraint )
			{
				this.AddRelation( r, RelationType.Center_Center );
				this.AddRelation( r, RelationType.Middle_Middle );
			}
		}

		private void UpdateSizeInternal( Vector2 deltaSize )
		{
			this.HandleSizeChanged();

			if ( this.gearSize.controller != null )
				this.gearSize.UpdateState();

			if ( this.parent != null )
			{
				this.relations.OnOwnerSizeChanged( deltaSize );
				this.parent.SetBoundsChangedFlag();
			}

			this.onSizeChanged.Call();
		}

		protected internal virtual void HandleAddToStage()
		{
			this.HandleTouchableChanged();
			this.HandleGrayedChanged();
			this.HandleColorChanged();
			this.HandleColorFilterChanged();
			this.HandleVisibleChanged();
		}

		protected internal virtual void HandleRemoveFromStage()
		{
			if ( GRoot.inst.popup == this )
				GRoot.inst.HidePopup();
		}

		protected virtual void HandlePositionChanged()
		{
			this.displayObject.position = this._position;
		}

		protected virtual void HandleSizeChanged()
		{
			this.displayObject.size = this.size;
			this.displayObject.scale = this.scale;
		}

		internal virtual void HandleControllerChanged( Controller c )
		{
			if ( this.gearDisplay.controller == c )
				this.gearDisplay.Apply();
			if ( this.gearColor.controller == c )
				this.gearColor.Apply();
			if ( this.gearXY.controller == c )
				this.gearXY.Apply();
			if ( this.gearSize.controller == c )
				this.gearSize.Apply();
			if ( this.gearLook.controller == c )
				this.gearLook.Apply();
			if ( this.gearIcon.controller == c )
				this.gearIcon.Apply();
			if ( this.gearText.controller == c )
				this.gearText.Apply();
		}

		protected internal virtual void HandleTouchableChanged()
		{
			if ( this.displayObject.stage == null )
				return;

			this.finalTouchable = this.touchable && ( this.parent == null || this.parent.finalTouchable );

			this.displayObject.touchable = this.finalTouchable;
		}

		protected internal virtual void HandleGrayedChanged()
		{
			if ( this.displayObject.stage == null )
				return;

			this.finalGrayed = this.grayed || ( this.parent != null && this.parent.finalGrayed );

			this.displayObject.grayed = this.finalGrayed;
		}

		protected internal virtual void HandleColorChanged()
		{
			if ( this.displayObject.stage == null )
				return;

			this.finalColor = this.color;

			if ( this.group != null )
				this.finalColor *= this.group.finalColor;

			if ( this.parent != null )
				this.finalColor *= this.parent.finalColor;

			this.displayObject.color = this.finalColor;
		}

		protected internal virtual void HandleColorFilterChanged()
		{
			if ( this.displayObject.stage == null )
				return;

			if ( this.parent != null )
				this.displayObject.colorFilter = this.colorFilter * this.parent.colorFilter;
		}

		protected internal virtual void HandleBlurFilterChanged()
		{
			if ( this.displayObject.stage == null )
				return;

			if ( this.parent != null )
				this.displayObject.blurFilter = this.blurFilter + this.parent.blurFilter;
		}

		protected internal virtual void HandleVisibleChanged()
		{
			if ( this.displayObject.stage == null )
				return;

			this.finalVisible = this.visible && ( this.ignoreGearVisible > 0 || this.gearVisible ) &&
								( this.group == null || this.group.finalVisible )
								&& ( this.parent == null || this.parent.finalVisible );

			this.displayObject.visible = this.finalVisible;
		}

		internal void SetGearState( GearState gearState, bool value )
		{
			if ( value )
				this._gearState |= gearState;
			else
				this._gearState &= ~gearState;
		}

		internal bool TestGearState( GearState gearState )
		{
			return ( this._gearState & gearState ) > 0;
		}

		internal void FitToDisplayObject()
		{
			Rect rect = this.displayObject.rect;
			if ( this._size != rect.size )
			{
				Vector2 oldSize = this._size;

				this._size = rect.size;

				if ( this.parent != null )
				{
					this.relations.OnOwnerSizeChanged( this._size - oldSize );
					this.parent.SetBoundsChangedFlag();
				}

				this.onSizeChanged.Call();
			}
		}

		public void GetWorldCorners( Vector3[] worldCorners )
		{
			this.displayObject.rectTransform.GetWorldCorners( worldCorners );
		}

		public Vector3 WorldToLocal( Vector3 p )
		{
			Vector3 p1 = this.displayObject.rectTransform.InverseTransformPoint( p );
			p1.y = -p1.y;
			return p1;
		}

		public Vector3 LocalToWorld( Vector3 p )
		{
			p.y = -p.y;
			return this.displayObject.rectTransform.TransformPoint( p );
		}

		public Vector3 WorldToLocalDirection( Vector3 p )
		{
			Vector3 p1 = this.displayObject.rectTransform.InverseTransformDirection( p );
			p1.y = -p1.y;
			return p1;
		}

		public Vector3 LocalToWorldDirection( Vector3 p )
		{
			p.y = -p.y;
			return this.displayObject.rectTransform.TransformDirection( p );
		}

		public Vector3 ScreenToLocal( Vector2 p )
		{
			Vector3 point = Stage.inst.eventCamera.ScreenToWorldPoint( new Vector3( p.x, p.y, Stage.inst.planeDistance ) );
			return this.WorldToLocal( point );
		}

		public Vector2 LocalToScreen( Vector3 p )
		{
			Vector3 point = this.LocalToWorld( p );
			point = Stage.inst.eventCamera.WorldToScreenPoint( point );
			return new Vector2( point.x, point.y );
		}

		internal virtual void ConstructFromResource( PackageItem pkgItem )
		{
			this.packageItem = pkgItem;
		}

		internal virtual void SetupBeforeAdd( XML xml )
		{
			this.id = xml.GetAttribute( "id" );
			this.name = xml.GetAttribute( "name" );

			this.displayObject.id = this.id;
			this.displayObject.name = this.name;

			string[] arr = xml.GetAttributeArray( "xy" );
			if ( arr != null )
				this.position = new Vector2( int.Parse( arr[0] ), int.Parse( arr[1] ) );

			arr = xml.GetAttributeArray( "size" );
			if ( arr != null )
			{
				this.initWidth = int.Parse( arr[0] );
				this.initHeight = int.Parse( arr[1] );
				this.size = new Vector2( this.initWidth, this.initHeight );
			}

			arr = xml.GetAttributeArray( "restrictSize" );
			if ( arr != null )
			{
				this.minSize = new Vector2( int.Parse( arr[0] ), int.Parse( arr[2] ) );
				this.maxSize = new Vector2( int.Parse( arr[1] ), int.Parse( arr[3] ) );
			}

			arr = xml.GetAttributeArray( "scale" );
			if ( arr != null )
				this.scale = new Vector2( float.Parse( arr[0] ), float.Parse( arr[1] ) );

			string str = xml.GetAttribute( "rotation" );
			if ( str != null )
				this.rotation = int.Parse( str );

			arr = xml.GetAttributeArray( "pivot" );
			if ( arr != null )
			{
				float f1 = float.Parse( arr[0] );
				float f2 = float.Parse( arr[1] );
				//处理旧版本的兼容性(旧版本发布的值是坐标值，新版本是比例，一般都小于2）
				if ( f1 > 2 )
				{
					if ( this.sourceWidth != 0 )
						f1 = f1 / this.sourceWidth;
					else
						f1 = 0;
				}
				if ( f2 > 2 )
				{
					if ( this.sourceHeight != 0 )
						f2 = f2 / this.sourceHeight;
					else
						f2 = 0;
				}
				this.pivot = new Vector2( f1, 1.0f - f2 );
			}

			str = xml.GetAttribute( "blend" );
			if ( str != null )
				this.blendMode = FieldTypes.ParseBlendMode( str );

			str = xml.GetAttribute( "filter" );
			if ( str != null )
			{
				switch ( str )
				{
					case "color":
						arr = xml.GetAttributeArray( "filterData" );
						this.colorFilter = new ColorFilter(
							float.Parse( arr[0] ), float.Parse( arr[1] ),
							float.Parse( arr[2] ), float.Parse( arr[3] ) );
						break;

					case "blur":
						arr = xml.GetAttributeArray( "filterData" );
						this.blurFilter = new BlurFilter( float.Parse( arr[0] ) );
						break;
				}
			}

			//str = xml.GetAttribute( "tooltips" );
			//if ( str != null )
			//	this.tooltips = str;
		}

		internal virtual void SetupAfterAdd( XML xml )
		{
			string str = xml.GetAttribute( "group" );
			if ( str != null )
				this.group = this.parent.GetChildById( str ).asGroup;

			str = xml.GetAttribute( "alpha" );
			if ( str != null )
				this.alpha = float.Parse( str );

			this.touchable = xml.GetAttributeBool( "touchable", true );
			this.visible = xml.GetAttributeBool( "visible", true );
			this.grayed = xml.GetAttributeBool( "grayed" );

			XML cxml = xml.GetNode( "gearDisplay" );
			if ( cxml != null )
				this.gearDisplay.Setup( cxml );

			cxml = xml.GetNode( "gearColor" );
			if ( cxml != null )
				this.gearColor.Setup( cxml );

			cxml = xml.GetNode( "gearXY" );
			if ( cxml != null )
				this.gearXY.Setup( cxml );

			cxml = xml.GetNode( "gearSize" );
			if ( cxml != null )
				this.gearSize.Setup( cxml );

			cxml = xml.GetNode( "gearLook" );
			if ( cxml != null )
				this.gearLook.Setup( cxml );

			cxml = xml.GetNode( "gearIcon" );
			if ( cxml != null )
				this.gearIcon.Setup( cxml );

			cxml = xml.GetNode( "gearText" );
			if ( cxml != null )
				this.gearText.Setup( cxml );
		}

		#region Event callbacks
		public void TriggerEvent( BaseEventData eventData, EventTriggerType type )
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

		#region Tween Support
		public Tweener TweenMove( Vector2 endValue, float duration, bool snapping )
		{
			return DOTween.To( () => this.position, x => this.position = x, endValue, duration )
				.SetOptions( snapping )
				.SetUpdate( true )
				.SetTarget( this );
		}

		public Tweener TweenMoveX( float endValue, float duration, bool snapping )
		{
			return DOTween.To( () => this.position.x, x =>
			{
				Vector2 p = this.position;
				p.x = x;
				this.position = p;
			},
				endValue, duration )
				.SetOptions( snapping )
				.SetUpdate( true )
				.SetTarget( this );
		}

		public Tweener TweenMoveY( float endValue, float duration, bool snapping )
		{
			return DOTween.To( () => this.position.y, x =>
			{
				Vector2 p = this.position;
				p.y = x;
				this.position = p;
			}, endValue, duration )
				.SetOptions( snapping )
				.SetUpdate( true )
				.SetTarget( this );
		}

		public Tweener TweenMove( Vector2 endValue, float duration )
		{
			return DOTween.To( () => this.position, x => this.position = x, endValue, duration )
				.SetOptions( true )
				.SetUpdate( true )
				.SetTarget( this );
		}

		public Tweener TweenMoveX( float endValue, float duration )
		{
			return DOTween.To( () => this.position.x, x =>
			{
				Vector2 p = this.position;
				p.x = x;
				this.position = p;
			},
				endValue, duration )
				.SetOptions( true )
				.SetUpdate( true )
				.SetTarget( this );
		}

		public Tweener TweenMoveY( float endValue, float duration )
		{
			return DOTween.To( () => this.position.y, x =>
			{
				Vector2 p = this.position;
				p.y = x;
				this.position = p;
			}, endValue, duration )
				.SetOptions( true )
				.SetUpdate( true )
				.SetTarget( this );
		}

		public Tweener TweenScale( Vector2 endValue, float duration )
		{
			return DOTween.To( () => this.scale, x => this.scale = x, endValue, duration )
				.SetUpdate( true )
				.SetTarget( this );
		}

		public Tweener TweenScaleX( float endValue, float duration )
		{
			return DOTween.To( () => this.scale.x, x =>
			{
				Vector2 p = this.scale;
				p.x = x;
				this.scale = p;
			}, endValue, duration )
				.SetUpdate( true )
				.SetTarget( this );
		}

		public Tweener TweenScaleY( float endValue, float duration )
		{
			return DOTween.To( () => this.scale.y, x =>
			{
				Vector2 p = this.scale;
				p.y = x;
				this.scale = p;
			}, endValue, duration )
				.SetUpdate( true )
				.SetTarget( this );
		}

		public Tweener TweenResize( Vector2 endValue, float duration, bool snapping )
		{
			return DOTween.To( () => this.size, x => this.size = x, endValue, duration )
				.SetOptions( snapping )
				.SetUpdate( true )
				.SetTarget( this );
		}

		public Tweener TweenResize( Vector2 endValue, float duration )
		{
			return DOTween.To( () => this.size, x => this.size = x, endValue, duration )
				.SetOptions( true )
				.SetUpdate( true )
				.SetTarget( this );
		}

		public Tweener TweenFade( float endValue, float duration )
		{
			return DOTween.To( () => this.alpha, x => this.alpha = x, endValue, duration )
				.SetUpdate( true )
				.SetTarget( this );
		}

		public Tweener TweenRotate( float endValue, float duration )
		{
			return DOTween.To( () => this.rotation, x => this.rotation = x, endValue, duration )
				.SetUpdate( true )
				.SetTarget( this );
		}
		#endregion
	}
}