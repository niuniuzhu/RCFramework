using FairyUGUI.Event;
using FairyUGUI.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyUGUI.Core
{
	public class Container : DisplayObject
	{
		private readonly List<DisplayObject> _children = new List<DisplayObject>();

		internal int numChildren => this._children.Count;

		internal override OverflowType overflow
		{
			set
			{
				if ( this._overflow == value )
					return;
				this._overflow = value;

				if ( this._overflow != OverflowType.Visible )
				{
					int count = this._children.Count;
					for ( int i = 0; i < count; i++ )
						this._children[i].AddClipper( this );
				}
				else
				{
					int count = this._children.Count;
					for ( int i = 0; i < count; i++ )
						this._children[i].RemoveClipper( this );
				}
			}
		}

		internal override bool shouldUpdateCanvasRect
		{
			set
			{
				base.shouldUpdateCanvasRect = value;
				int count = this._children.Count;
				for ( int i = 0; i < count; i++ )
					this._children[i].shouldUpdateCanvasRect = value;
			}
		}

		internal override bool shouldUpdateClipping
		{
			set
			{
				base.shouldUpdateClipping = value;
				int count = this._children.Count;
				for ( int i = 0; i < count; i++ )
					this._children[i].shouldUpdateClipping = value;
			}
		}

		internal Container( GObject owner )
			: base( owner )
		{
			this.name = "Container";

			this.RegisterEventTriggerType( EventTriggerType.PointerClick );
			this.RegisterEventTriggerType( EventTriggerType.PointerDown );
			this.RegisterEventTriggerType( EventTriggerType.PointerUp );
			this.RegisterEventTriggerType( EventTriggerType.PointerEnter );
			this.RegisterEventTriggerType( EventTriggerType.PointerExit );
		}

		protected override void InternalDispose()
		{
			this._children.Clear();

			base.InternalDispose();
		}

		internal DisplayObject AddChild( DisplayObject child )
		{
			this.AddChildAt( child, this._children.Count );
			return child;
		}

		internal DisplayObject AddChildAt( DisplayObject child, int index )
		{
			int count = this._children.Count;
			index = Mathf.Clamp( index, 0, count );

			if ( child.parent == this )
				this.SetChildIndex( child, index );
			else
			{
				child.RemoveFromParent();
				child.SetParent( this );
				child.siblingIndex = index;

				this._children.Add( child );
				this._children.Sort( CompareSiblingIndex );

				if ( this.stage != null )//可能父对象还没有添加到舞台哦
					child.HandleAddToStage();
			}
			return child;
		}

		internal bool Contains( DisplayObject child )
		{
			return this._children.Contains( child );
		}

		internal DisplayObject GetChildAt( int index )
		{
			return this._children[index];
		}

		internal DisplayObject GetChild( string name )
		{
			int cnt = this._children.Count;
			for ( int i = 0; i < cnt; ++i )
			{
				if ( this._children[i].name == name )
					return this._children[i];
			}

			return null;
		}

		internal int GetChildIndex( DisplayObject child )
		{
			return this._children.IndexOf( child );
		}

		internal DisplayObject RemoveChild( DisplayObject child, bool dispose = false )
		{
			if ( child.parent != this )
				throw new Exception( "obj is not a child" );

			int i = this._children.IndexOf( child );
			return i >= 0 ? this.RemoveChildAt( i, dispose ) : null;
		}

		internal DisplayObject RemoveChildAt( int index, bool dispose = false )
		{
			if ( index < 0 || index >= this._children.Count )
				return null;
			DisplayObject child = this._children[index];

			if ( this.stage != null )
				child.HandleRemoveFromStage();

			this._children.Remove( child );

			if ( !dispose )
				child.SetParent( null );
			else
				child.Dispose();

			return child;
		}

		internal void RemoveChildren( int beginIndex = 0, int endIndex = int.MaxValue, bool dispose = false )
		{
			if ( endIndex < 0 || endIndex >= this.numChildren )
				endIndex = this.numChildren - 1;

			for ( int i = beginIndex; i <= endIndex; ++i )
				this.RemoveChildAt( beginIndex, dispose );
		}

		internal int SetChildIndex( DisplayObject child, int index )
		{
			int oldIndex = this._children.IndexOf( child );
			if ( oldIndex == index ) return index;
			if ( oldIndex == -1 ) throw new ArgumentException( "Not a child of this container" );

			child.siblingIndex = index;

			this._children.Sort( CompareSiblingIndex );
			return child.siblingIndex;
		}

		private static int CompareSiblingIndex( DisplayObject x, DisplayObject y )
		{
			int i0 = x.siblingIndex;
			int i1 = y.siblingIndex;
			if ( i0 > i1 )
				return 1;
			if ( i0 < i1 )
				return -1;
			return 0;
		}

		internal override void AddClipper( DisplayObject clipper )
		{
			base.AddClipper( clipper );
			int count = this._children.Count;
			for ( int i = 0; i < count; i++ )
			{
				DisplayObject child = this._children[i];
				child.AddClipper( clipper );
			}
		}

		internal override void RemoveClipper( DisplayObject clipper )
		{
			base.RemoveClipper( clipper );
			int count = this._children.Count;
			for ( int i = 0; i < count; i++ )
			{
				DisplayObject child = this._children[i];
				child.RemoveClipper( clipper );
			}
		}

		protected internal override void HandleAddToStage()
		{
			base.HandleAddToStage();
			int count = this._children.Count;
			for ( int i = 0; i < count; i++ )
				this._children[i].HandleAddToStage();
		}

		protected internal override void HandleRemoveFromStage()
		{
			base.HandleRemoveFromStage();
			int count = this._children.Count;
			for ( int i = 0; i < count; i++ )
				this._children[i].HandleRemoveFromStage();
		}

		protected internal override void Update( UpdateContext context )
		{
			base.Update( context );

			int cnt = this._children.Count;
			for ( int i = 0; i < cnt; i++ )
			{
				DisplayObject child = this._children[i];
				if ( UIConfig.useCanvasSortingOrder )
				{
					child.sortingOrder = context.sortingOrder;
					context.sortingOrder += 3;
				}
				//else
				//	child.sortingOrder = -1;
				child.Update( context );
			}
		}

		protected internal override void WillRenderCanvases()
		{
			base.WillRenderCanvases();

			int cnt = this._children.Count;
			for ( int i = 0; i < cnt; i++ )
			{
				DisplayObject child = this._children[i];
				child.WillRenderCanvases();
			}
		}
	}
}