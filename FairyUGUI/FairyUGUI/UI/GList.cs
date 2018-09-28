using Core.Math;
using Core.Xml;
using FairyUGUI.Core;
using FairyUGUI.Event;
using System.Collections.Generic;
using UnityEngine;
using EventType = FairyUGUI.Event.EventType;

namespace FairyUGUI.UI
{
	public class GList : GComponent
	{
		private readonly GObjectPool _pool = new GObjectPool();
		private string _defaultItem;

		public ListSelectionMode selectionMode;

		private readonly List<int> _selectedIndices = new List<int>();
		public List<int> selectedIndices => new List<int>( this._selectedIndices );

		public GObject firstSelectedItem => this.GetChildAt( this.isVirtual ? this.VirtualIndexToRealIndex( this._selectedIndices[0] ) : this._selectedIndices[0] );

		private int _lastSelectedIndex = -1;

		public int numItems
		{
			get => this.isVirtual ? this._numVirtualItems : this._children.Count;
			set
			{
				if ( this.isVirtual )
					this._numVirtualItems = value;
				else
				{
					int cnt = this._children.Count;
					if ( value > cnt )
						for ( int i = cnt; i < value; i++ )
							this.AddItemFromPool();
					else
						this.RemoveChildrenToPool( value, cnt );
				}
			}
		}

		public bool isVirtual { get; private set; }

		private int _numVirtualItems;
		private Vector2 _virtualItemSize;
		private readonly List<int> _virtualItemRenderIndices = new List<int>();
		private readonly List<int> _lastVirtualItemRenderIndices = new List<int>();
		private readonly List<int> _indicesToAdd = new List<int>();
		private readonly List<int> _indicesToRemove = new List<int>();
		private readonly List<int> _tobeSelectedIndices = new List<int>();

		public EventListener onClickItem { get; private set; }

		public EventListener onVirtualItemChanged { get; private set; }

		public GList()
		{
			this.onClickItem = new EventListener( this, EventType.ItemClick );
			this.onVirtualItemChanged = new EventListener( this, EventType.VirtualItemChanged );
		}

		protected override void InternalDispose()
		{
			this._indicesToRemove.Clear();
			this._indicesToAdd.Clear();
			this._virtualItemRenderIndices.Clear();
			this._lastVirtualItemRenderIndices.Clear();
			this._tobeSelectedIndices.Clear();
			this.RemoveChildrenToPool();
			this._pool.Clear();

			base.InternalDispose();
		}

		public override GObject AddChildAt( GObject child, int index )
		{
			return this.AddItemFromPoolAt( child, index );
		}

		public override GObject RemoveChildAt( int index, bool dispose = false )
		{
			return this.RemoveChildToPoolAt( index );
		}

		public GObject GetFromPool( string url = null )
		{
			if ( string.IsNullOrEmpty( url ) )
				url = this._defaultItem;

			GObject ret = this._pool.GetObject( url );
			return ret;
		}

		private GObject ReturnToPool( GObject obj )
		{
			this._pool.ReturnObject( obj );
			return obj;
		}

		public GObject AddItemFromPool()
		{
			GObject child = this.GetFromPool();
			return this.AddItemFromPool( child );
		}

		public GObject AddItemFromPool( string url )
		{
			GObject child = this.GetFromPool( url );
			return this.AddItemFromPool( child );
		}

		public GObject AddItemFromPool( GObject child )
		{
			return this.AddItemFromPoolAt( child, this._children.Count );
		}

		public GObject AddItemFromPoolAt( int index )
		{
			GObject child = this.GetFromPool();
			return this.AddItemFromPoolAt( child, index );
		}

		public GObject AddItemFromPoolAt( GObject child, int index )
		{
			//if ( this.isVirtual )
			//{
			//	Logger.Warn( "AddChild not support when list set to virtual" );
			//	return null;
			//}

			child.onClick.Add( this.OnClickItem );

			return base.AddChildAt( child, index );
		}

		public void RemoveChildrenToPool( int beginIndex = 0, int endIndex = -1 )
		{
			if ( endIndex < 0 || endIndex >= this._children.Count )
				endIndex = this._children.Count - 1;

			for ( int i = beginIndex; i <= endIndex; ++i )
				this.RemoveChildToPoolAt( beginIndex );
		}

		public void RemoveChildToPool( GObject child )
		{
			int index = this.GetChildIndex( child );
			this.RemoveChildToPoolAt( index );
		}

		public GObject RemoveChildToPoolAt( int index )
		{
			//if ( this.isVirtual )
			//{
			//	Logger.Warn( "RemoveChild not support when list set to virtual" );
			//	return null;
			//}

			GObject child = base.RemoveChildAt( index );

			GButton btn = child as GButton;
			if ( btn != null )
				btn.selected = false;

			child.onClick.Remove( this.OnClickItem );

			return this.ReturnToPool( child );
		}

		public void ScrollToIndex( int index )
		{
			index = Mathf.Clamp( index, 0, this.numItems - 1 );
			switch ( this.layout.type )
			{
				case LayoutType.SingleColumn:
				case LayoutType.FlowHorizontal:
					this.scrollView.verticalNormalizedPosition = this.numItems <= 1 ? 1 : 1f - ( float )index / ( this.numItems - 1 );
					break;
				case LayoutType.SingleRow:
				case LayoutType.FlowVertical:
					this.scrollView.horizontalNormalizedPosition = this.numItems <= 1 ? 1 : ( float )index / ( this.numItems - 1 );
					break;
			}
		}

		protected override void HandleSizeChanged()
		{
			base.HandleSizeChanged();

			if ( this.isVirtual )
				this.UpdateVirtualContentSize();
		}

		#region virtuallist support
		public void SetVirtual( int itemCount )
		{
			if ( this.isVirtual )
				return;

			this.RemoveChildrenToPool();
			this.isVirtual = true;
			this.layout.enable = false;
			GObject item = this.GetFromPool();
			this._virtualItemSize = item.size;
			this.ReturnToPool( item );
			this.numItems = itemCount;
			this.UpdateVirtualContentSize();
		    this.scrollView?.onChange.Add( this.OnScrollChanged );
		    this.UpdateVirtualList();
		}

		public void SetReal()//not fully support now
		{
			if ( !this.isVirtual )
				return;

			this.RemoveChildrenToPool();
			this._indicesToRemove.Clear();
			this._indicesToAdd.Clear();
			this._virtualItemRenderIndices.Clear();
			this._lastVirtualItemRenderIndices.Clear();
			this.isVirtual = false;
			this.layout.enable = true;
			this.layout.UpdateLayout();
			this._numVirtualItems = 0;
			this._virtualItemSize = Vector2.zero;
			if ( this.scrollView != null )
			{
				this.scrollView.onChange.Remove( this.OnScrollChanged );
				this.scrollView.contentSizeAutoFit = true;
			}
		}

		private void OnScrollChanged( EventContext context )
		{
			this.UpdateVirtualList();
		}

		private void UpdateVirtualContentSize()
		{
			Vector2 itemsSize = this.CalculateTotalItemSize();
			if ( this.scrollView != null )
			{
				this.scrollView.contentSizeAutoFit = false;
				this.scrollView.SetContentSize( itemsSize );
			}
			else
				this.size = itemsSize;
		}

		private Vector2 CalculateTotalItemSize()
		{
			Vector2 itemsSize = new Vector2();
			switch ( this.layout.type )
			{
				case LayoutType.SingleRow:
					this.CalHorizontalLayout( ref itemsSize );
					break;

				case LayoutType.SingleColumn:
					this.CalVerticalLayout( ref itemsSize );
					break;

				case LayoutType.FlowHorizontal:
					this.CalFlowHorizontalLayout( ref itemsSize );
					break;

				case LayoutType.FlowVertical:
					this.CalFlowVerticalLayout( ref itemsSize );
					break;
			}
			return itemsSize;
		}

		private void CalHorizontalLayout( ref Vector2 itemsSize )
		{
			itemsSize.x = ( this._virtualItemSize.x + this.layout.columnGap ) * this.numItems - this.layout.columnGap;
			itemsSize.y = this._virtualItemSize.y;
		}

		private void CalVerticalLayout( ref Vector2 itemsSize )
		{
			itemsSize.x = this._virtualItemSize.x;
			itemsSize.y = ( this._virtualItemSize.y + this.layout.lineGap ) * this.numItems - this.layout.lineGap;
		}

		private void CalFlowHorizontalLayout( ref Vector2 itemsSize )
		{
			float xx = this.scrollView.size.x;
			if ( this.layout.lineItemCount > 0 )
				xx = Mathf.Min( xx, ( this._virtualItemSize.x + this.layout.columnGap ) * this.layout.lineItemCount );
			int hCount = ( int )( xx / ( this._virtualItemSize.x + this.layout.columnGap ) );
			int vCount = Mathf.CeilToInt( ( float )this.numItems / hCount );
			float yy = ( this._virtualItemSize.y + this.layout.lineGap ) * vCount - this.layout.lineGap;
			itemsSize.x = xx;
			itemsSize.y = yy;
		}

		private void CalFlowVerticalLayout( ref Vector2 itemsSize )
		{
			float yy = this.scrollView.size.y;
			if ( this.layout.lineItemCount > 0 )
				yy = Mathf.Min( yy, ( this._virtualItemSize.y + this.layout.lineGap ) * this.layout.lineItemCount );
			int vCount = ( int )( yy / ( this._virtualItemSize.y + this.layout.lineGap ) );
			int hCount = Mathf.CeilToInt( ( float )this.numItems / vCount );
			float xx = ( this._virtualItemSize.x + this.layout.columnGap ) * hCount - this.layout.columnGap;
			itemsSize.x = xx;
			itemsSize.y = yy;
		}

		private void UpdateVirtualList()
		{
			switch ( this.layout.type )
			{
				case LayoutType.SingleRow:
					this.UpdateHorizontalVirtualList();
					break;

				case LayoutType.SingleColumn:
					this.UpdateVerticalVirtualList();
					break;

				case LayoutType.FlowHorizontal:
					this.UpdateFlowHorizontalVirtualList();
					break;

				case LayoutType.FlowVertical:
					this.UpdateFlowVerticalVirtualList();
					break;
			}
		}

		private void UpdateHorizontalVirtualList()
		{
			Vector2 viewSize = this.scrollView.size;
			Vector3 contentPos = this.GetContentPositionRelativeToViewport();
			int beginIndex = ( int )( ( -contentPos.x ) / ( this._virtualItemSize.x + this.layout.columnGap ) );
			beginIndex = beginIndex < 0 ? 0 : beginIndex;
			int endIndex = ( int )( ( viewSize.x - contentPos.x ) / ( this._virtualItemSize.x + this.layout.columnGap ) );
			endIndex = Mathf.Min( endIndex, this.numItems - 1 );
			if ( beginIndex > this.numItems - 1 )
			{
				beginIndex = -1;
				endIndex = -2;
			}
			this.UpdateRenderItems( beginIndex, endIndex );
		}

		private void UpdateVerticalVirtualList()
		{
			Vector2 viewSize = this.scrollView.size;
			Vector3 contentPos = this.GetContentPositionRelativeToViewport();
			int beginIndex = ( int )( ( contentPos.y ) / ( this._virtualItemSize.y + this.layout.lineGap ) );
			beginIndex = beginIndex < 0 ? 0 : beginIndex;
			int endIndex = ( int )( ( contentPos.y + viewSize.y ) / ( this._virtualItemSize.y + this.layout.lineGap ) );
			endIndex = Mathf.Min( endIndex, this.numItems - 1 );
			if ( beginIndex > this.numItems - 1 )
			{
				beginIndex = -1;
				endIndex = -2;
			}
			this.UpdateRenderItems( beginIndex, endIndex );
		}

		private void UpdateFlowHorizontalVirtualList()
		{
			Vector2 viewSize = this.scrollView.size;
			Vector3 contentPos = this.GetContentPositionRelativeToViewport();
			int hCount = ( int )( viewSize.x / ( this._virtualItemSize.x + this.layout.columnGap ) );
			int beginIndex = hCount * ( int )( ( contentPos.y ) / ( this._virtualItemSize.y + this.layout.lineGap ) );
			beginIndex = beginIndex < 0 ? 0 : beginIndex;
			int endIndex = hCount * ( int )( ( contentPos.y + viewSize.y ) / ( this._virtualItemSize.y + this.layout.lineGap ) );
			endIndex += hCount - 1;
			endIndex = Mathf.Min( endIndex, this.numItems - 1 );
			if ( beginIndex > this.numItems - 1 )
			{
				beginIndex = -1;
				endIndex = -2;
			}
			this.UpdateRenderItems( beginIndex, endIndex );
		}

		private void UpdateFlowVerticalVirtualList()
		{
			Vector2 viewSize = this.scrollView.size;
			Vector3 contentPos = this.GetContentPositionRelativeToViewport();
			int vCount = ( int )( viewSize.y / ( this._virtualItemSize.y + this.layout.lineGap ) );
			int beginIndex = vCount * ( int )( ( -contentPos.x ) / ( this._virtualItemSize.x + this.layout.columnGap ) );
			beginIndex = beginIndex < 0 ? 0 : beginIndex;
			int endIndex = vCount * ( int )( ( viewSize.x - contentPos.x ) / ( this._virtualItemSize.x + this.layout.columnGap ) );
			endIndex += vCount - 1;
			endIndex = Mathf.Min( endIndex, this.numItems - 1 );
			if ( beginIndex > this.numItems - 1 )
			{
				beginIndex = -1;
				endIndex = -2;
			}
			this.UpdateRenderItems( beginIndex, endIndex );
		}

		private void UpdateRenderItems( int beginIndex, int endIndex )
		{
			this._virtualItemRenderIndices.Clear();
			this._indicesToAdd.Clear();
			this._indicesToRemove.Clear();
			for ( int i = beginIndex; i <= endIndex; i++ )
			{
				this._virtualItemRenderIndices.Add( i );
				if ( !this._lastVirtualItemRenderIndices.Contains( i ) )
					this._indicesToAdd.Add( i );
			}
			int count = this._lastVirtualItemRenderIndices.Count;
			for ( int i = 0; i < count; i++ )
			{
				int j = this._lastVirtualItemRenderIndices[i];
				if ( !this._virtualItemRenderIndices.Contains( j ) )
					this._indicesToRemove.Add( j );
			}
			this.RenderItems();
			this._lastVirtualItemRenderIndices.Clear();
			this._lastVirtualItemRenderIndices.AddRange( this._virtualItemRenderIndices );
		}

		private void RenderItems()
		{
			int offset = this._lastVirtualItemRenderIndices.Count == 0 ? 0 : this._lastVirtualItemRenderIndices[0];
			int count = this._indicesToRemove.Count;
			while ( count > 0 )
			{
				int index = this._indicesToRemove[0];
				GObject item = this.RemoveChildToPoolAt( index - offset );
				VirtualItemChangedInfo info = new VirtualItemChangedInfo();
				info.index = index;
				info.add = false;
				info.item = item;
				this.onVirtualItemChanged.Call( info );
				--count;
			}
			offset = this._virtualItemRenderIndices.Count == 0 ? 0 : this._virtualItemRenderIndices[0];
			count = this._indicesToAdd.Count;
			for ( int i = 0; i < count; i++ )
			{
				int index = this._indicesToAdd[i];
				GObject item = this.AddItemFromPoolAt( index - offset );
				if ( this._tobeSelectedIndices.Contains( index ) )
				{
					this.SetItemSelected( index - offset );
					this._tobeSelectedIndices.Remove( index );
				}
				item.position = this.CalVirtualItemPosition( index, item.position );
				VirtualItemChangedInfo info = new VirtualItemChangedInfo();
				info.index = index;
				info.add = true;
				info.item = item;
				this.onVirtualItemChanged.Call( info );
			}
		}

		private Vector2 CalVirtualItemPosition( int index, Vector2 position )
		{
			switch ( this.layout.type )
			{
				case LayoutType.SingleRow:
					position.x = index * ( this._virtualItemSize.x + this.layout.columnGap );
					break;

				case LayoutType.SingleColumn:
					position.y = index * ( this._virtualItemSize.y + this.layout.lineGap );
					break;

				case LayoutType.FlowHorizontal:
					{
						Vector2 viewSize = this.scrollView.size;
						int hCount = ( int )( viewSize.x / ( this._virtualItemSize.x + this.layout.columnGap ) );
						int xIndex = index % hCount;
						int yIndex = index / hCount;
						position.x = xIndex * ( this._virtualItemSize.x + this.layout.columnGap );
						position.y = yIndex * ( this._virtualItemSize.y + this.layout.lineGap );
					}
					break;

				case LayoutType.FlowVertical:
					{
						Vector2 viewSize = this.scrollView.size;
						int vCount = ( int )( viewSize.y / ( this._virtualItemSize.y + this.layout.lineGap ) );
						int xIndex = index / vCount;
						int yIndex = index % vCount;
						position.x = xIndex * ( this._virtualItemSize.x + this.layout.columnGap );
						position.y = yIndex * ( this._virtualItemSize.y + this.layout.lineGap );
					}
					break;
			}
			return position;
		}

		private Vector3 GetContentPositionRelativeToViewport()
		{
			Vector3 worldPos = this.scrollView.content.rectTransform.TransformPoint( Vector3.zero );
			return this.scrollView.rectTransform.InverseTransformPoint( worldPos );
		}

		#endregion

		#region selection
		private void OnClickItem( EventContext context )
		{
			GObject item = ( GObject )context.sender;

			this.SetSelectionOnEvent( item );

			this.onClickItem.Call( item );
		}

		private int RealIndexToVirtualIndex( int index )
		{
			return this._virtualItemRenderIndices.Count > 0 ? index + this._virtualItemRenderIndices[0] : 0;
		}

		private int VirtualIndexToRealIndex( int index )
		{
			return this._virtualItemRenderIndices.Count > 0 ? index - this._virtualItemRenderIndices[0] : 0;
		}

		private void SetSelectionOnEvent( GObject item )
		{
			if ( this.selectionMode == ListSelectionMode.None )
				return;

			int index = this.GetChildIndex( item );
			if ( this.isVirtual )
				index = this.RealIndexToVirtualIndex( index );

			if ( this.selectionMode == ListSelectionMode.Single )
			{
				this.SetSelection( index, false );
				this._lastSelectedIndex = index;
			}
			else
			{
				if ( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift ) )
				{
					if ( this._lastSelectedIndex != -1 )
					{
						int min = MathUtils.Min( this._lastSelectedIndex, index );
						int max = MathUtils.Max( this._lastSelectedIndex, index );
						max = MathUtils.Min( max, this.numItems - 1 );

						int[] excepts = new int[max - min + 1];
						for ( int i = min; i <= max; i++ )
							excepts[i - min] = i;
						this.ClearSelectionExcept( excepts );
						for ( int i = min; i <= max; i++ )
							this.AddSelection( i, false );
					}
					else
					{
						this.AddSelection( index, false );
						this._lastSelectedIndex = index;
					}
				}
				else if ( Input.GetKey( KeyCode.LeftCommand ) || Input.GetKey( KeyCode.RightCommand ) ||
					Input.GetKey( KeyCode.LeftControl ) || Input.GetKey( KeyCode.RightControl ) ||
					this.selectionMode == ListSelectionMode.Multiple_SingleClick )
				{
					if ( this.ContainSelectedIndex( index ) )
					{
						this.ClearSelection( index );
						this._lastSelectedIndex = this._selectedIndices.Count > 0
							? this._selectedIndices[this._selectedIndices.Count - 1]
							: -1;
					}
					else
					{
						this.AddSelection( index, false );
						this._lastSelectedIndex = index;
					}
				}
				else
				{
					this.SetSelection( index, false );
					this._lastSelectedIndex = index;
				}
			}
		}

		public void ClearSelectionExcept( int[] indices )
		{
			int count = this._selectedIndices.Count;
			int c2 = indices.Length;
			for ( int i = 0; i < count; i++ )
			{
				int j = this._selectedIndices[i];
				bool contain = false;
				for ( int l = 0; l < c2; l++ )
				{
					if ( indices[l] == j )
					{
						contain = true;
						break;
					}
				}
				if ( contain )
					continue;

				this._selectedIndices.RemoveAt( i );
				this._tobeSelectedIndices.Remove( j );
				--count;
				--i;

				int k = j;
				if ( this.isVirtual )
				{
					if ( this._virtualItemRenderIndices.Count > 0 )
					{
						if ( this._virtualItemRenderIndices.Contains( j ) )
							k = j - this._virtualItemRenderIndices[0];
						else
							return;
					}
				}
				this.SetItemDeselected( k );
			}
		}

		public void ClearSelectionExcept( int index )
		{
			this.ClearSelectionExcept( new[] { index } );
		}

		public void ClearSelection()
		{
			int cnt = this._children.Count;
			for ( int i = 0; i < cnt; i++ )
			{
				GButton obj = this._children[i].asButton;
				if ( obj != null )
					obj.selected = false;
			}
			this._tobeSelectedIndices.Clear();
			this._selectedIndices.Clear();
		}

		public void ClearSelection( int index )
		{
			if ( index < 0 || index >= this.numItems )
				return;

			if ( !this.ContainSelectedIndex( index ) )
				return;

			this._tobeSelectedIndices.Remove( index );
			this._selectedIndices.Remove( index );

			if ( this.isVirtual )
			{
				if ( this.IsVirtualIndexInView( index ) )
				{
					index = this.VirtualIndexToRealIndex( index );
					this.SetItemDeselected( index );
				}
			}
		}

		public bool ContainSelectedIndex( int index )
		{
			return this._selectedIndices.Contains( index );
		}

		public void SetSelection( int index, bool scrollItToView )
		{
			this.ClearSelectionExcept( index );
			this.AddSelection( index, scrollItToView );
		}

		public void AddSelection( IEnumerable<int> indices )
		{
			foreach ( int index in indices )
				this.AddSelection( index, false );
		}

		public void AddSelection( int index, bool scrollItToView )
		{
			if ( this.selectionMode == ListSelectionMode.None )
				return;

			if ( index < 0 || index >= this.numItems )
				return;

			if ( this.ContainSelectedIndex( index ) )
				return;

			this._selectedIndices.Add( index );

			if ( scrollItToView )
				this.ScrollToIndex( index );

			if ( this.isVirtual )
			{
				if ( this.IsVirtualIndexInView( index ) )
				{
					index = this.VirtualIndexToRealIndex( index );
					this.SetItemSelected( index );
				}
				else
				{
					if ( !this._tobeSelectedIndices.Contains( index ) )
						this._tobeSelectedIndices.Add( index );
				}
			}
		}

		private bool IsVirtualIndexInView( int index )
		{
			return this._virtualItemRenderIndices.Contains( index );
		}

		private void SetItemSelected( int index )
		{
			GButton obj = this.GetChildAt( index ).asButton;
			if ( obj != null && !obj.selected )
				obj.selected = true;
		}

		private void SetItemDeselected( int index )
		{
			GButton obj = this.GetChildAt( index ).asButton;
			if ( obj != null && obj.selected )
				obj.selected = false;
		}
		#endregion

		internal override void SetupBeforeAdd( XML xml )
		{
			base.SetupBeforeAdd( xml );

			string str = xml.GetAttribute( "layout" );
			this.layout = new Layout( this );
			this.layout.type = str != null ? FieldTypes.ParseListLayoutType( str ) : LayoutType.SingleColumn;
			this.layout.lineItemCount = xml.GetAttributeInt( "lineItemCount" );
			this.layout.lineGap = xml.GetAttributeInt( "lineGap" );
			this.layout.columnGap = xml.GetAttributeInt( "colGap" );
			this.layout.flexible = xml.GetAttributeBool( "autoItemSize", true );

			this._defaultItem = xml.GetAttribute( "defaultItem" );

			str = xml.GetAttribute( "selectionMode" );
			this.selectionMode = str != null ? FieldTypes.ParseListSelectionMode( str ) : ListSelectionMode.Single;

			//arr = xml.GetAttributeArray( "clipSoftness" );
			//if ( arr != null )
			//	this.clipSoftness = new Vector2( int.Parse( arr[0] ), int.Parse( arr[1] ) );

			XMLList col = xml.Elements( "item" );
			foreach ( XML ix in col )
			{
				string url = ix.GetAttribute( "url" );
				if ( string.IsNullOrEmpty( url ) )
					url = this._defaultItem;
				if ( string.IsNullOrEmpty( url ) )
					continue;

				GObject obj = this.GetFromPool( url );
				if ( obj != null )
				{
					this.AddChild( obj );
					GButton btn = obj as GButton;
					if ( btn != null )
					{
						btn.text = ix.GetAttribute( "title" );
						btn.icon = ix.GetAttribute( "icon" );
					}
					else
					{
						GLabel label = obj as GLabel;
						if ( label != null )
						{
							label.text = ix.GetAttribute( "title" );
							label.icon = ix.GetAttribute( "icon" );
						}
					}
				}
			}

			str = xml.GetAttribute( "overflow" );
			OverflowType mOverflow = str != null ? FieldTypes.ParseOverflowType( str ) : OverflowType.Visible;

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

				//str = xml.GetAttribute( "margin" );
				//if ( str != null )
				//	_margin.Parse( str );

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
	}

	public class VirtualItemChangedInfo
	{
		public int index;
		public bool add;
		public GObject item;
	}
}