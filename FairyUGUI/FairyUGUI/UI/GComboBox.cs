using Core.Xml;
using FairyUGUI.Core;
using FairyUGUI.Event;
using FairyUGUI.Utils;
using System;
using UnityEngine;
using EventType = FairyUGUI.Event.EventType;
using Logger = Core.Misc.Logger;

namespace FairyUGUI.UI
{
	public class GComboBox : GComponent
	{
		private const string UP = "up";
		private const string DOWN = "down";
		private const string OVER = "over";

		/// <summary>
		/// Text display in combobox.
		/// </summary>
		public override string text
		{
			get => this._titleObject?.text;
			set
			{
				if ( this._titleObject != null )
					this._titleObject.text = value;
			}
		}

		/// <summary>
		/// Text color
		/// </summary>
		public Color titleColor
		{
			get
			{
				if ( this._titleObject != null )
					return this._titleObject.color;
				return Color.black;
			}
			set
			{
				if ( this._titleObject != null )
					this._titleObject.color = value;
			}
		}

		/// <summary>
		/// Items to build up drop down list.
		/// </summary>
		public string[] items
		{
			get => this._items;
			set
			{
				if ( value == null )
					this._items = new string[0];
				else
					this._items = ( string[] ) value.Clone();
				if ( this._items.Length > 0 )
				{
					if ( this._selectedIndex >= this._items.Length )
						this._selectedIndex = this._items.Length - 1;
					else if ( this._selectedIndex == -1 )
						this._selectedIndex = 0;
					this.text = this._items[this._selectedIndex];
				}
				else
					this.text = string.Empty;
				this._itemsUpdated = true;
			}
		}

		/// <summary>
		/// Values, should be same size of the items. 
		/// </summary>
		public string[] values
		{
			get => this._values;
			set
			{
				if ( value == null )
					this._values = new string[0];
				else
					this._values = ( string[] ) value.Clone();
			}
		}

		private int _selectedIndex;
		/// <summary>
		/// Selected index.
		/// </summary>
		public int selectedIndex
		{
			get => this._selectedIndex;
			set
			{
				if ( this._selectedIndex == value )
					return;

				this._selectedIndex = value;
				if ( this.selectedIndex >= 0 && this.selectedIndex < this._items.Length )
					this.text = this._items[this._selectedIndex];
				else
					this.text = string.Empty;
			}
		}

		/// <summary>
		/// Selected value.
		/// </summary>
		public string value
		{
			get
			{
				if ( this._selectedIndex >= 0 && this._selectedIndex < this._values.Length )
					return this._values[this._selectedIndex];
				return null;
			}
			set => this.selectedIndex = Array.IndexOf( this._values, value );
		}

		public int visibleItemCount;
		public bool autoSizeDropdown;
		public PopupDirection popupDirection;
		public PopupConstraint popupConstraint;

		private bool _itemsUpdated;
		private Controller _buttonController;
		private string[] _items;
		private string[] _values;
		private bool _down;
		private bool _over;
		private GComponent _dropdown;
		private GList _list;
		private GTextField _titleObject;

		public EventListener onChanged { get; private set; }

		public GComboBox()
		{
			this.visibleItemCount = UIConfig.defaultComboBoxVisibleItemCount;
			this._itemsUpdated = true;
			this._selectedIndex = -1;
			this._items = new string[0];
			this._values = new string[0];

			this.onChanged = new EventListener( this, EventType.Changed );
		}

		protected override void InternalDispose()
		{
			if ( this._dropdown != null )
			{
				if ( this._dropdown.parent != null )
					GRoot.inst.HidePopup();
				this._dropdown.Dispose();
				this._dropdown = null;
			}

			this._list = null;
			this._titleObject = null;
			this._items = null;
			this._values = null;
			this._buttonController = null;

			base.InternalDispose();
		}

		protected override void ConstructFromXML( XML cxml )
		{
			base.ConstructFromXML( cxml );

			XML xml = cxml.GetNode( "ComboBox" );

			this._buttonController = this.GetController( "button" );
			this._titleObject = this.GetChild( "title" ) as GTextField;

			string str = xml.GetAttribute( "dropdown" );
			if ( !string.IsNullOrEmpty( str ) )
			{
				this._dropdown = UIPackage.CreateObjectFromURL( str ) as GComponent;
				if ( this._dropdown == null )
				{
					Logger.Warn( "FairyGUI: " + this.resourceURL + " should be a component." );
					return;
				}

				this._list = this._dropdown.GetChild( "list" ) as GList;
				if ( this._list == null )
				{
					Logger.Warn( "FairyGUI: " + this.resourceURL + ": should container a list component named list." );
					return;
				}
				this._list.RemoveChildrenToPool();
				this._list.onClickItem.Add( this.OnClickItem );

				this._list.AddRelation( this._dropdown, RelationType.Width );
				this._list.RemoveRelation( this._dropdown, RelationType.Height );

				this._dropdown.AddRelation( this._list, RelationType.Height );
				this._dropdown.RemoveRelation( this._list, RelationType.Width );

				if ( this._list.scrollView != null )
					this._list.scrollView.movementType = ScrollView.MovementType.Clamped;
			}

			this.onRollOver.Add( this.OnRollOver );
			this.onRollOut.Add( this.OnRollOut );
			this.onTouchBegin.Add( this.OnTouchBegin );
			this.onTouchEnd.Add( this.OnTouchEnd );
		}

		internal override void SetupAfterAdd( XML cxml )
		{
			base.SetupAfterAdd( cxml );

			XML xml = cxml.GetNode( "ComboBox" );
			if ( xml == null )
				return;

			string str = xml.GetAttribute( "titleColor" );
			if ( str != null )
				this.titleColor = ToolSet.ConvertFromHtmlColor( str );
			this.visibleItemCount = xml.GetAttributeInt( "visibleItemCount", this.visibleItemCount );

			XMLList col = xml.Elements( "item" );
			this._items = new string[col.count];
			this._values = new string[col.count];
			int i = 0;
			foreach ( XML ix in col )
			{
				this._items[i] = ix.GetAttribute( "title" );
				this._values[i] = ix.GetAttribute( "value" );
				i++;
			}

			str = xml.GetAttribute( "title" );
			if ( !string.IsNullOrEmpty( str ) )
			{
				this.text = str;
				this._selectedIndex = Array.IndexOf( this._items, str );
			}
			else if ( this._items.Length > 0 )
			{
				this._selectedIndex = 0;
				this.text = this._items[0];
			}
			else
				this._selectedIndex = -1;
		}

		private void OnClickItem( EventContext context )
		{
			this._selectedIndex = this._list.GetChildIndex( ( GObject ) context.data );
			this.text = this._selectedIndex >= 0 ? this._items[this._selectedIndex] : string.Empty;

			GRoot.inst.HidePopup();

			this.onChanged.Call();
		}

		private void OnRollOver( EventContext context )
		{
			this._over = true;
			this.SetState( this._down ? DOWN : OVER );
		}

		private void OnRollOut( EventContext context )
		{
			this._over = false;
			this.SetState( UP );
		}

		private void OnTouchBegin( EventContext context )
		{
			this._down = true;
			this.SetState( DOWN );
		}

		private void OnTouchEnd( EventContext context )
		{
			this._down = false;
			this.SetState( this._over ? OVER : UP );
			this.ShowDropdown();
		}

		public void SetItem( int index, string text )
		{
			if ( index < 0 || index > this._items.Length - 1 )
				return;
			this._items[index] = text;
			if ( index == this._selectedIndex )
				this.text = this._items[this._selectedIndex];
		}

		private void SetState( string value )
		{
			if ( this._buttonController == null )
				return;
			this._buttonController.selectedPage = value;
		}

		private void ShowDropdown()
		{
			if ( this._list == null )
				return;

			if ( this._itemsUpdated )
				this.RenderDropdownList();

			this._list.ClearSelection();

			if ( this.autoSizeDropdown )
				this._dropdown.size = this.size;

			this.root.ShowPopup( this._dropdown, this, this.popupDirection, this.popupConstraint, this.OnDropdownHide );

			if ( this._itemsUpdated )
			{
				this._list.layout.ResizeToFit( this.visibleItemCount );
				this._itemsUpdated = false;
			}
		}

		private void OnDropdownHide()
		{
		}

		private void RenderDropdownList()
		{
			if ( this._list == null )
				return;

			this._list.RemoveChildrenToPool();
			int cnt = this._items.Length;
			for ( int i = 0; i < cnt; i++ )
			{
				GObject item = this._list.AddItemFromPool();
				item.text = this._items[i];
				item.name = i < this._values.Length ? this._values[i] : string.Empty;
			}
		}
	}
}