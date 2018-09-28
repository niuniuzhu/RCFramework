using FairyUGUI.UI;
using UnityEngine;

namespace FairyUGUI.Core
{
	public class Layout
	{
		private GComponent _owner;

		private LayoutType _type;
		public LayoutType type
		{
			get => this._type;
			set
			{
				if ( this._type == value )
					return;
				this._type = value;
				this.InternalUpdateLayout();
			}
		}

		private bool _flexible;
		public bool flexible
		{
			get => this._flexible;
			set
			{
				if ( this._flexible == value )
					return;
				this._flexible = value;
				this.InternalUpdateLayout();
			}
		}

		private float _columnGap;
		public float columnGap
		{
			get => this._columnGap;
			set
			{
				if ( this._columnGap == value )
					return;
				this._columnGap = value;
				this.InternalUpdateLayout();
			}
		}

		private float _lineGap;
		public float lineGap
		{
			get => this._lineGap;
			set
			{
				if ( this._lineGap == value )
					return;
				this._lineGap = value;
				this.InternalUpdateLayout();
			}
		}

		private int _lineItemCount;
		public int lineItemCount
		{
			get => this._lineItemCount;
			set
			{
				if ( this._lineItemCount == value )
					return;
				this._lineItemCount = value;
				this.InternalUpdateLayout();
			}
		}

		public bool enable { get; set; }

		public Layout( GComponent owner )
		{
			this._owner = owner;
			this.enable = true;
		}

		public void Dispose()
		{
			this._owner = null;
		}

		public void ResizeToFit( int itemCount, int minSize = 0 )
		{
			int numChildren = this._owner.numChildren;
			if ( itemCount > numChildren )
				itemCount = numChildren;

			if ( itemCount == 0 )
			{
				Vector2 s = this._owner.size;
				if ( this.type == LayoutType.SingleColumn || this.type == LayoutType.FlowHorizontal )
					s.y = minSize;
				else
					s.x = minSize;
				this._owner.size = s;
			}
			else
			{
				int i = itemCount - 1;
				ILayoutItem obj = null;
				while ( i >= 0 )
				{
					obj = this._owner.GetChildAt( i );
					if ( obj.visible )
						break;
					i--;
				}
				Vector2 s = this._owner.size;
				if ( i < 0 )
				{
					if ( this.type == LayoutType.SingleColumn || this.type == LayoutType.FlowHorizontal )
						s.y = minSize;
					else
						s.x = minSize;
				}
				else
				{
					float size;
					if ( this.type == LayoutType.SingleColumn || this.type == LayoutType.FlowHorizontal )
					{
						size = obj.position.y + obj.size.y;
						if ( size < minSize )
							size = minSize;
						s.y = size;
					}
					else
					{
						size = obj.position.x + obj.size.x;
						if ( size < minSize )
							size = minSize;
						s.x = size;
					}
				}
				this._owner.size = s;
			}
		}

		private void InternalUpdateLayout()
		{
			if ( !this.enable )
				return;

			if ( this.flexible && this.type != LayoutType.FlowHorizontal && this.type != LayoutType.FlowVertical )
				this.MakeItemsFlexible();

			switch ( this.type )
			{
				case LayoutType.SingleRow:
					this.SetHorizontalLayout();
					break;

				case LayoutType.SingleColumn:
					this.SetVerticalLayout();
					break;

				case LayoutType.FlowHorizontal:
					this.SetFlowHorizontalLayout();
					break;

				case LayoutType.FlowVertical:
					this.SetFlowVerticalLayout();
					break;
			}
		}

		internal void UpdateLayout()
		{
			this.InternalUpdateLayout();
		}

		private void MakeItemsFlexible()
		{
			int count = this._owner.numChildren;
			for ( int i = 0; i < count; i++ )
			{
				ILayoutItem child = this._owner.GetChildAt( i );
				if ( child.ignoreLayout )
					continue;
				this.MakeItemFlexible( child );
			}
		}

		private void MakeItemFlexible( ILayoutItem child )
		{
			if ( this.type == LayoutType.SingleColumn )
			{
				Vector2 s = child.size;
				s.x = this._owner.size.x;
				child.size = s;
			}
			else if ( this.type == LayoutType.SingleRow )
			{
				Vector2 s = child.size;
				s.y = this._owner.size.y;
				child.size = s;
			}
		}

		private void SetHorizontalLayout()
		{
			float xx = 0;
			float yy = -float.MaxValue;
			int count = this._owner.numChildren;
			for ( int i = 0; i < count; i++ )
			{
				ILayoutItem child = this._owner.GetChildAt( i );
				if ( child.ignoreLayout )
					continue;
				child.position = new Vector2( xx, child.position.y );
				xx += child.size.x + this._columnGap;
				yy = yy > child.size.y ? yy : child.size.y;
			}
		}

		private void SetVerticalLayout()
		{
			float xx = -float.MaxValue;
			float yy = 0;
			int count = this._owner.numChildren;
			for ( int i = 0; i < count; i++ )
			{
				ILayoutItem child = this._owner.GetChildAt( i );
				if ( child.ignoreLayout )
					continue;
				child.position = new Vector2( child.position.x, yy );
				yy += child.size.y + this._lineGap;
				xx = xx > child.size.x ? xx : child.size.x;
			}
		}

		private void SetFlowHorizontalLayout()
		{
			float xx = 0;
			float yy = 0;
			int numCol = 0;
			int count = this._owner.numChildren;
			Vector2 ownerSize = this._owner.scrollView?.size ?? this._owner.size;
			for ( int i = 0; i < count; i++ )
			{
				ILayoutItem child = this._owner.GetChildAt( i );
				if ( child.ignoreLayout )
					continue;
				child.position = new Vector2( xx, yy );
				xx += child.size.x + this._columnGap;

				++numCol;

				if ( i != count - 1 )
				{
					if ( numCol == this._lineItemCount ||
						xx + this._owner.GetChildAt( i + 1 ).size.x > ownerSize.x )
					{
						numCol = 0;
						xx = 0;
						yy += child.size.y + this._lineGap;
					}
				}
			}
		}

		private void SetFlowVerticalLayout()
		{
			float xx = 0;
			float yy = 0;
			int numLine = 0;
			int count = this._owner.numChildren;
			Vector2 ownerSize = this._owner.scrollView?.size ?? this._owner.size;
			for ( int i = 0; i < count; i++ )
			{
				ILayoutItem child = this._owner.GetChildAt( i );
				if ( child.ignoreLayout )
					continue;
				child.position = new Vector2( xx, yy );
				yy += child.size.y + this._lineGap;
				++numLine;

				if ( i != count - 1 )
				{
					if ( numLine == this._lineItemCount ||
						yy + this._owner.GetChildAt( i + 1 ).size.y > ownerSize.y )
					{
						numLine = 0;
						yy = 0;
						xx += child.size.x + this._columnGap;
					}
				}
			}
		}
	}
}