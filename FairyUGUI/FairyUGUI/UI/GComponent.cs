using Core.Xml;
using FairyUGUI.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = Core.Misc.Logger;

namespace FairyUGUI.UI
{
	public class GComponent : GObject
	{
		public GObject this[string name] => this.GetChild( name );

		public GObject this[int index] => this.GetChildAt( index );

		/// <summary>
		/// Root container.
		/// </summary>
		public Container rootContainer { get; protected set; }

		/// <summary>
		/// Content container. If the component is not clipped, then container==rootContainer.
		/// </summary>
		public Container container { get; protected set; }

		/// <summary>
		/// Returns controller list.
		/// </summary>
		/// <returns>Controller list</returns>
		public List<Controller> controllers => this._controllers;

		protected readonly List<GObject> _children;
		/// <summary>
		/// The number of children of this component.
		/// </summary>
		public int numChildren => this._children.Count;

		/// <summary>
		/// If true, mouse/touch events cannot pass through the empty area of the component. Default is true.
		/// </summary>
		//public bool opaque
		//{
		//	get { return rootContainer.hitArea != null; }
		//	set
		//	{
		//		if ( value )
		//		{
		//			if ( rootContainer.hitArea == null )
		//				rootContainer.hitArea = new RectHitTest();
		//			UpdateHitArea();
		//		}
		//		else
		//			rootContainer.hitArea = null;
		//	}
		//}

		private int _sortingChildCount;

		private readonly List<Controller> _controllers;

		public int numControllers => this._controllers.Count;

		private readonly List<Transition> _transitions;

		private bool _boundsDirty;

		private Vector2 _contentSize;
		public Vector2 contentSize
		{
			get
			{
				if ( !this._boundsDirty )
					return this._contentSize;

				this._boundsDirty = false;
				float aw = 0, ah = 0;
				int cnt = this._children.Count;
				if ( cnt > 0 )
				{
					for ( int i = 0; i < cnt; ++i )
					{
						GObject child = this._children[i];
						Vector2 pos = child.position;
						Vector2 sz = child.actualSize;
						aw = Mathf.Max( aw, pos.x + sz.x );
						ah = Mathf.Max( ah, pos.y + sz.y );
					}
				}
				this._contentSize.x = aw;
				this._contentSize.y = ah;
				return this._contentSize;
			}
		}

		internal OverflowType overflow
		{
			get => this.displayObject.overflow;
			set => this.displayObject.overflow = value;
		}

		public ScrollView scrollView { get; private set; }

		public Layout layout;

		private GObject _modalWaitPane;

		public bool isModalWaiting => this._modalWaitPane?.parent != null;

		public GComponent()
		{
			this._children = new List<GObject>();
			this._controllers = new List<Controller>();
			this._transitions = new List<Transition>();
		}

		protected override void CreateDisplayObject()
		{
			this.displayObject = this.container = this.rootContainer = new Container( this );
		}

		protected override void InternalDispose()
		{
			this.RemoveFromParent();

			int count = this._children.Count;
			for ( int i = count - 1; i >= 0; --i )
			{
				GObject child = this._children[i];
				child.Dispose();
			}
			this._children.Clear();

			count = this._controllers.Count;
			for ( int i = 0; i < count; i++ )
				this._controllers[i].Dispose();
			this._controllers.Clear();

			count = this._transitions.Count;
			for ( int i = 0; i < count; i++ )
				this._transitions[i].Dispose();
			this._transitions.Clear();

			if ( this.layout != null )
			{
				this.layout.Dispose();
				this.layout = null;
			}

			this.rootContainer = null;
			this.container = null;

			base.InternalDispose(); //Dispose native tree first, avoid DisplayObject.RemoveFromParent call
		}

		#region Construct
		internal override void ConstructFromResource( PackageItem pkgItem )
		{
			this.packageItem = pkgItem;
			this.packageItem.Load();

			this.ConstructFromXML( this.packageItem.componentData );
			this.ConstructScroll( this.packageItem.componentData );
		}

		protected virtual void ConstructFromXML( XML xml )
		{
			this._underConstruct = true;

			string[] arr = xml.GetAttributeArray( "size" );
			this.sourceWidth = int.Parse( arr[0] );
			this.sourceHeight = int.Parse( arr[1] );
			this.initWidth = this.sourceWidth;
			this.initHeight = this.sourceHeight;

			this.size = new Vector2( this.sourceWidth, this.sourceHeight );

			arr = xml.GetAttributeArray( "restrictSize" );
			if ( arr != null )
			{
				this.minSize = new Vector2( int.Parse( arr[0] ), int.Parse( arr[2] ) );
				this.maxSize = new Vector2( int.Parse( arr[1] ), int.Parse( arr[3] ) );
			}

			//this.opaque = xml.GetAttributeBool( "opaque", true );
			//arr = xml.GetAttributeArray( "hitTest" );
			//if ( arr != null )
			//{
			//	PixelHitTestData hitTestData = _packageItem.owner.GetPixelHitTestData( arr[0] );
			//	if ( hitTestData != null )
			//		this.rootContainer.hitArea = new PixelHitTest( hitTestData, int.Parse( arr[1] ), int.Parse( arr[2] ) );
			//}

			XMLList col = xml.Elements( "controller" );
			foreach ( XML cxml in col )
			{
				Controller controller = new Controller();
				this.AddController( controller );
				controller.Setup( cxml );
			}

			XMLList transCol = xml.Elements( "transition" );
			foreach ( XML cxml in transCol )
			{
				Transition trans = new Transition( this );
				this.AddTransition( trans );
				trans.Setup( cxml );
			}

			XML listNode = xml.GetNode( "displayList" );
			if ( listNode != null )
			{
				col = listNode.Elements();
				foreach ( XML cxml in col )
				{
					GObject u = this.ConstructChild( cxml );
					if ( u == null )
						continue;

					u._underConstruct = true;
					u.constructingData = cxml;
					u.SetupBeforeAdd( cxml );
					this.AddChild( u );
				}
			}
			this.relations.Setup( xml );

			int cnt = this._children.Count;
			for ( int i = 0; i < cnt; i++ )
			{
				GObject u = this._children[i];
				u.relations.Setup( u.constructingData );
			}

			for ( int i = 0; i < cnt; i++ )
			{
				GObject u = this._children[i];
				u.SetupAfterAdd( u.constructingData );
				u._underConstruct = false;
				u.constructingData = null;
			}

			this.ApplyAllControllers();

			//arr = xml.GetAttributeArray( "clipSoftness" );
			//if ( arr != null )
			//	this.clipSoftness = new Vector2( int.Parse( arr[0] ), int.Parse( arr[1] ) );

			this._underConstruct = false;
		}

		private void ConstructScroll( XML xml )
		{
			string str = xml.GetAttribute( "overflow" );
			OverflowType mOverflow = str != null ? FieldTypes.ParseOverflowType( str ) : OverflowType.Visible;

			//str = xml.GetAttribute( "margin" );
			//if ( str != null )
			//	_margin.Parse( str );

			if ( mOverflow == OverflowType.Scroll )
			{
				str = xml.GetAttribute( "scroll" );
				ScrollType scroll = str != null ? FieldTypes.ParseScrollType( str ) : ScrollType.Vertical;

				str = xml.GetAttribute( "scrollBar" );
				ScrollBarDisplayType scrollBarDisplay = str != null ? FieldTypes.ParseScrollBarDisplayType( str ) : ScrollBarDisplayType.Default;

				int scrollBarFlags = xml.GetAttributeInt( "scrollBarFlags" );

				Margin scrollBarMargin = new Margin();
				str = xml.GetAttribute( "scrollBarMargin" );
				if ( str != null )
					FieldTypes.ParseMargin( str, ref scrollBarMargin );

				string vtScrollBarRes = null;
				string hzScrollBarRes = null;
				string[] arr = xml.GetAttributeArray( "scrollBarRes" );
				if ( arr != null )
				{
					vtScrollBarRes = arr[0];
					hzScrollBarRes = arr[1];
				}
				this.SetupScroll( mOverflow, scrollBarMargin, scroll, scrollBarDisplay, scrollBarFlags, vtScrollBarRes, hzScrollBarRes );
			}
			else
				this.overflow = mOverflow;
		}

		private GObject ConstructChild( XML xml )
		{
			string pkgId = xml.GetAttribute( "pkg" );
			UIPackage thisPkg = this.packageItem.owner;
			UIPackage pkg;
			if ( pkgId != null && pkgId != thisPkg.id )
				pkg = UIPackage.GetById( pkgId );
			else
				pkg = thisPkg;

			if ( pkg != null )
			{
				string src = xml.GetAttribute( "src" );
				if ( src != null )
				{
					PackageItem pi = pkg.GetItem( src );
					if ( pi != null )
						return pkg.CreateObject( pi, null );
				}
			}

			return UIObjectFactory.NewObject( xml.name, xml.GetAttributeBool( "input" ) );
		}

		protected void SetupScroll( OverflowType overflowType, Margin scrollBarMargin,
			ScrollType scrollType, ScrollBarDisplayType scrollBarDisplay, int flags,
			String vtScrollBarRes, String hzScrollBarRes )
		{
			this.scrollView = new ScrollView( this, scrollType, scrollBarMargin, scrollBarDisplay,
				flags, vtScrollBarRes, hzScrollBarRes );
			this.scrollView.layer = LayerMask.NameToLayer( Stage.LAYER_NAME );
			this.scrollView.overflow = overflowType;

			int count = this.container.numChildren;
			while ( count > 0 )
			{
				this.scrollView.content.AddChild( this.container.GetChildAt( 0 ) );
				--count;
			}

			this.rootContainer.AddChild( this.scrollView );
			this.scrollView.SetupScrollBar();
			this.scrollView.size = this.size;

			this.container = this.scrollView.content;
		}

		#endregion

		/// <summary>
		/// Add a child to the component. It will be at the frontmost position.
		/// </summary>
		/// <param name="child">A child object</param>
		/// <returns>GObject</returns>
		public GObject AddChild( GObject child )
		{
			this.AddChildAt( child, this._children.Count );
			return child;
		}

		/// <summary>
		/// Adds a child to the component at a certain index.
		/// </summary>
		/// <param name="child">A child object</param>
		/// <param name="index">Index</param>
		/// <returns>GObject</returns>
		public virtual GObject AddChildAt( GObject child, int index )
		{
			if ( child == null || child.disposed )
				return child;

			if ( child.parent == this )
				this.SetChildIndex( child, index );
			else
			{
				int count = this._children.Count;
				index = Mathf.Clamp( index, 0, count );

				child.RemoveFromParent();
				child.parent = this;

				int cnt = this._children.Count;
				if ( child.sortingOrder != 0 )
				{
					this._sortingChildCount++;
					index = this.GetInsertPosForSortingChild( child );
				}
				else if ( this._sortingChildCount > 0 )
				{
					if ( index > ( cnt - this._sortingChildCount ) )
						index = cnt - this._sortingChildCount;
				}

				if ( index == cnt )
					this._children.Add( child );
				else
					this._children.Insert( index, child );

				this.ChildStateChanged( child );
			}
			return child;
		}

		/// <summary>
		/// Removes a child from the component. If the object is not a child, nothing happens. 
		/// </summary>
		/// <param name="child">A child object</param>
		/// <param name="dispose">If true, the child will be disposed right away.</param>
		/// <returns>GObject</returns>
		public GObject RemoveChild( GObject child, bool dispose = false )
		{
			if ( child == null )
				return null;

			int childIndex = this._children.IndexOf( child );
			this.RemoveChildAt( childIndex, dispose );
			return child;
		}

		/// <summary>
		/// Removes a child at a certain index. Children above the child will move down.
		/// </summary>
		/// <param name="index">Index</param>
		/// <param name="dispose">If true, the child will be disposed right away.</param>
		/// <returns>GObject</returns>
		public virtual GObject RemoveChildAt( int index, bool dispose = false )
		{
			if ( index < 0 || index >= this.numChildren )
			{
				Logger.Log( $"index:{index} out of range:{this.numChildren - 1}" );
				return null;
			}

			GObject child = this._children[index];

			child.parent = null;

			if ( child.sortingOrder != 0 )
				this._sortingChildCount--;

			this._children.RemoveAt( index );

			if ( child.inContainer )
				this.container.RemoveChild( child.displayObject );

			if ( dispose )
				child.Dispose();

			if ( this.layout != null && this.displayObject.stage != null )
				this.layout.UpdateLayout();

			this.SetBoundsChangedFlag();

			return child;
		}

		/// <summary>
		/// Removes a range of children from the container (endIndex included). 
		/// </summary>
		/// <param name="beginIndex">Begin index.</param>
		/// <param name="endIndex">End index.(Included).</param>
		/// <param name="dispose">If true, the child will be disposed right away.</param>
		public void RemoveChildren( int beginIndex = 0, int endIndex = -1, bool dispose = false )
		{
			if ( endIndex < 0 || endIndex >= this.numChildren )
				endIndex = this.numChildren - 1;

			for ( int i = beginIndex; i <= endIndex; ++i )
				this.RemoveChildAt( beginIndex, dispose );
		}

		internal void ChildStateChanged( GObject child )
		{
			int index = 0;
			int cnt = this._children.Count;
			for ( int i = 0; i < cnt; i++ )
			{
				GObject g = this._children[i];
				if ( g == child )
					break;

				if ( g.inContainer )
					index++;
			}
			this.container.AddChildAt( child.displayObject, index );

			if ( this.layout != null && this.displayObject.stage != null )
				this.layout.UpdateLayout();

			this.SetBoundsChangedFlag();
		}

		private int GetInsertPosForSortingChild( GObject target )
		{
			int cnt = this._children.Count;
			int i;
			for ( i = 0; i < cnt; i++ )
			{
				GObject child = this._children[i];
				if ( child == target )
					continue;

				if ( target.sortingOrder < child.sortingOrder )
					break;
			}
			return i;
		}

		/// <summary>
		/// Returns a child object at a certain index. If index out of bounds, exception raised.
		/// </summary>
		/// <param name="index">Index</param>
		/// <returns>A child object.</returns>
		public GObject GetChildAt( int index )
		{
			if ( index >= 0 && index < this.numChildren )
				return this._children[index];
			throw new Exception( "Invalid child index: " + index + ">" + this.numChildren );
		}

		/// <summary>
		/// Returns a child object with a certain name.
		/// </summary>
		/// <param name="name">Name</param>
		/// <returns>A child object. Null if not found.</returns>
		public GObject GetChild( string name )
		{
			int cnt = this._children.Count;
			for ( int i = 0; i < cnt; ++i )
			{
				if ( this._children[i].name == name )
					return this._children[i];
			}

			return null;
		}

		/// <summary>
		/// Returns a copy of all children with an array.
		/// </summary>
		/// <returns>An array contains all children</returns>
		public GObject[] GetChildren()
		{
			return this._children.ToArray();
		}

		/// <summary>
		/// Returns a visible child object with a certain name.
		/// </summary>
		/// <param name="name">Name</param>
		/// <returns>A child object. Null if not found.</returns>
		public GObject GetVisibleChild( string name )
		{
			int cnt = this._children.Count;
			for ( int i = 0; i < cnt; ++i )
			{
				GObject child = this._children[i];
				if ( child.finalVisible && child.name == name )
					return child;
			}

			return null;
		}

		/// <summary>
		/// Returns a child object belong to a group with a certain name.
		/// </summary>
		/// <param name="group">A group object</param>
		/// <param name="name">Name</param>
		/// <returns>A child object. Null if not found.</returns>
		public GObject GetChildInGroup( GGroup group, string name )
		{
			if ( group == null )
				return null;

			int cnt = this._children.Count;
			for ( int i = 0; i < cnt; ++i )
			{
				GObject child = this._children[i];
				if ( child.group == group && child.name == name )
					return child;
			}

			return null;
		}

		internal GObject GetChildById( string id )
		{
			int cnt = this._children.Count;
			for ( int i = 0; i < cnt; ++i )
			{
				if ( this._children[i].id == id )
					return this._children[i];
			}

			return null;
		}

		/// <summary>
		/// Returns the index of a child within the container, or "-1" if it is not found.
		/// </summary>
		/// <param name="child">A child object</param>
		/// <returns>Index of the child. -1 If not found.</returns>
		public int GetChildIndex( GObject child )
		{
			return this._children.IndexOf( child );
		}

		/// <summary>
		/// Moves a child to a certain index. Children at and after the replaced position move up.
		/// </summary>
		/// <param name="child">A Child</param>
		/// <param name="index">Index</param>
		public void SetChildIndex( GObject child, int index )
		{
			if ( child == null )
				return;

			int oldIndex = this._children.IndexOf( child );
			if ( oldIndex == -1 )
				throw new ArgumentException( "Not a child of this container" );

			if ( child.sortingOrder != 0 ) //no effect
				return;

			//确保索引不能超越设置了sortingOrder的子对象后
			int cnt = this._children.Count;
			if ( this._sortingChildCount > 0 )
			{
				if ( index > ( cnt - this._sortingChildCount - 1 ) )
					index = cnt - this._sortingChildCount - 1;
			}

			this.InternalSetChildIndex( child, oldIndex, index );
		}

		/// <summary>
		/// Swaps the indexes of two children. 
		/// </summary>
		/// <param name="child1">A child object</param>
		/// <param name="child2">A child object</param>
		public void SwapChildren( GObject child1, GObject child2 )
		{
			int index1 = this._children.IndexOf( child1 );
			int index2 = this._children.IndexOf( child2 );
			if ( index1 == -1 || index2 == -1 )
				throw new Exception( "Not a child of this container" );
			this.SwapChildrenAt( index1, index2 );
		}

		/// <summary>
		///  Swaps the indexes of two children.
		/// </summary>
		/// <param name="index1">index of first child</param>
		/// <param name="index2">index of second child</param>
		public void SwapChildrenAt( int index1, int index2 )
		{
			GObject child1 = this._children[index1];
			GObject child2 = this._children[index2];

			this.SetChildIndex( child1, index2 );
			this.SetChildIndex( child2, index1 );
		}

		internal void ChildSortingOrderChanged( GObject child, int oldValue, int newValue )
		{
			if ( newValue == 0 )
			{
				this._sortingChildCount--;
				this.SetChildIndex( child, this._children.Count );
			}
			else
			{
				if ( oldValue == 0 )
					this._sortingChildCount++;

				int oldIndex = this._children.IndexOf( child );
				int index = this.GetInsertPosForSortingChild( child );
				if ( oldIndex < index )
					this.InternalSetChildIndex( child, oldIndex, index - 1 );//往后移动的话，由于先移除，队列会往前移1，所以要减去1
				else
					this.InternalSetChildIndex( child, oldIndex, index );
			}
		}

		private void InternalSetChildIndex( GObject child, int oldIndex, int index )
		{
			int cnt = this._children.Count;

			index = Mathf.Clamp( index, 0, cnt );

			if ( oldIndex == index )
				return;

			this._children.RemoveAt( oldIndex );
			if ( index >= cnt )
				this._children.Add( child );
			else
				this._children.Insert( index, child );

			if ( child.inContainer )
			{
				int displayIndex = 0;
				for ( int i = 0; i < index; i++ )
				{
					GObject g = this._children[i];
					if ( g.inContainer )
						displayIndex++;
				}
				this.container.SetChildIndex( child.displayObject, displayIndex );
			}

			if ( this.layout != null && this.displayObject.stage != null )
				this.layout.UpdateLayout();

			this.SetBoundsChangedFlag();
		}

		internal void AdjustRadioGroupDepth( GObject obj, Controller c )
		{
			int cnt = this._children.Count;
			int i;
			int myIndex = -1, maxIndex = -1;
			for ( i = 0; i < cnt; i++ )
			{
				GObject child = this._children[i];
				if ( child == obj )
				{
					myIndex = i;
				}
				else if ( ( child is GButton )
					&& ( ( GButton ) child ).relatedController == c )
				{
					if ( i > maxIndex )
						maxIndex = i;
				}
			}
			if ( myIndex < maxIndex )
				this.SwapChildrenAt( myIndex, maxIndex );
		}

		protected internal virtual void SetBoundsChangedFlag()
		{
			this._boundsDirty = true;
		}

		/// <summary>
		/// Adds a controller to the container.
		/// </summary>
		/// <param name="controller">Controller object</param>
		public void AddController( Controller controller )
		{
			controller.parent?.RemoveController( controller );

			this._controllers.Add( controller );
			controller.parent = this;
		}

		/// <summary>
		/// Returns a controller object  at a certain index.
		/// </summary>
		/// <param name="index">Index</param>
		/// <returns>Controller object.</returns>
		public Controller GetControllerAt( int index )
		{
			return this._controllers[index];
		}

		/// <summary>
		/// Returns a controller object with a certain name.
		/// </summary>
		/// <param name="name">Name</param>
		/// <returns>Controller object. Null if not found.</returns>
		public Controller GetController( string name )
		{
			int cnt = this._controllers.Count;
			for ( int i = 0; i < cnt; ++i )
			{
				Controller c = this._controllers[i];
				if ( c.name == name )
					return c;
			}

			return null;
		}

		/// <summary>
		/// Removes a controller from the container. 
		/// </summary>
		/// <param name="c">Controller object.</param>
		public void RemoveController( Controller c )
		{
			int index = this._controllers.IndexOf( c );
			if ( index == -1 )
				throw new Exception( "controller not exists: " + c.name );

			c.parent = null;
			this._controllers.RemoveAt( index );

			int cnt = this._children.Count;
			for ( int i = 0; i < cnt; ++i )
			{
				GObject child = this._children[i];
				child.HandleControllerChanged( c );
			}
		}

		internal void ApplyController( Controller c )
		{
			int cnt = this._children.Count;
			for ( int i = 0; i < cnt; ++i )
			{
				GObject child = this._children[i];
				child.HandleControllerChanged( c );
			}
		}

		private void ApplyAllControllers()
		{
			int cnt = this._controllers.Count;
			for ( int i = 0; i < cnt; ++i )
			{
				Controller controller = this._controllers[i];
				this.ApplyController( controller );
			}
		}

		public void AddTransition( Transition trans )
		{
			this._transitions.Add( trans );
		}

		public bool RemoveTransition( Transition trans )
		{
			bool result = this._transitions.Remove( trans );
			return result;
		}

		/// <summary>
		/// Returns a transition object  at a certain index.
		/// </summary>
		/// <param name="index">Index</param>
		/// <returns>transition object.</returns>
		public Transition GetTransitionAt( int index )
		{
			return this._transitions[index];
		}

		/// <summary>
		/// Returns a transition object at a certain name. 
		/// </summary>
		/// <param name="name">Name</param>
		/// <returns>Transition Object</returns>
		public Transition GetTransition( string name )
		{
			int cnt = this._transitions.Count;
			for ( int i = 0; i < cnt; ++i )
			{
				Transition trans = this._transitions[i];
				if ( trans.name == name )
					return trans;
			}

			return null;
		}

		public int TransitionCount()
		{
			return this._transitions.Count;
		}

		public void ShowModalWait()
		{
			if ( UIConfig.globalModalWaiting != null )
			{
				if ( this._modalWaitPane == null )
					this._modalWaitPane = UIPackage.CreateObjectFromURL( UIConfig.globalModalWaiting );
				this._modalWaitPane.size = this.size;
				this._modalWaitPane.AddRelation( this, RelationType.Size );
				this.AddChild( this._modalWaitPane );
			}
		}

		public void CloseModalWait()
		{
			if ( this._modalWaitPane != null && this._modalWaitPane.parent != null )
				this.RemoveChild( this._modalWaitPane );
		}

		protected internal override void HandleAddToStage()
		{
			this.layout?.UpdateLayout();

			base.HandleAddToStage();
			int cnt = this._transitions.Count;
			for ( int i = 0; i < cnt; ++i )
			{
				Transition trans = this._transitions[i];
				if ( trans.autoPlay )
					trans.Play( trans.autoPlayRepeat, trans.autoPlayDelay );
			}
		}

		protected internal override void HandleRemoveFromStage()
		{
			int cnt = this._transitions.Count;
			for ( int i = 0; i < cnt; ++i )
			{
				Transition trans = this._transitions[i];
				trans.Stop();
			}

			base.HandleRemoveFromStage();
		}

		protected override void HandleSizeChanged()
		{
			base.HandleSizeChanged();

			if ( this.layout != null && this.displayObject.stage != null )
				this.layout.UpdateLayout();

			if ( this.scrollView != null )
				this.scrollView.size = this.size;
		}

		protected internal override void HandleTouchableChanged()
		{
			base.HandleTouchableChanged();

			if ( this.scrollView != null )
				this.scrollView.touchable = this.displayObject.touchable;

			int count = this._children.Count;
			for ( int i = 0; i < count; i++ )
			{
				GObject child = this._children[i];
				child.HandleTouchableChanged();
			}
		}

		protected internal override void HandleGrayedChanged()
		{
			base.HandleGrayedChanged();

			if ( this.scrollView != null )
				this.scrollView.grayed = this.displayObject.grayed;

			int cnt = this._children.Count;
			for ( int i = 0; i < cnt; ++i )
			{
				GObject child = this._children[i];
				child.HandleGrayedChanged();
			}
		}

		protected internal override void HandleColorChanged()
		{
			base.HandleColorChanged();
			int count = this._children.Count;
			for ( int i = 0; i < count; i++ )
			{
				GObject child = this._children[i];
				child.HandleColorChanged();
			}
		}

		protected internal override void HandleColorFilterChanged()
		{
			base.HandleColorFilterChanged();
			int count = this._children.Count;
			for ( int i = 0; i < count; i++ )
			{
				GObject child = this._children[i];
				child.HandleColorFilterChanged();
			}
		}

		protected internal override void HandleBlurFilterChanged()
		{
			base.HandleBlurFilterChanged();
			int count = this._children.Count;
			for ( int i = 0; i < count; i++ )
			{
				GObject child = this._children[i];
				child.HandleBlurFilterChanged();
			}
		}

		protected internal override void HandleVisibleChanged()
		{
			base.HandleVisibleChanged();

			if ( this.scrollView != null )
				this.scrollView.visible = this.displayObject.visible;

			int count = this._children.Count;
			for ( int i = 0; i < count; i++ )
			{
				GObject child = this._children[i];
				child.HandleVisibleChanged();
			}
		}
	}
}