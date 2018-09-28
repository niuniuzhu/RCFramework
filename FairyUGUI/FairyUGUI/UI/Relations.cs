using System;
using System.Collections.Generic;
using Core.Xml;
using UnityEngine;

namespace FairyUGUI.UI
{
	public class Relations
	{
		private readonly GObject _owner;
		private readonly List<RelationItem> _items;

		//internal GObject handling;

		static readonly string[] RELATION_NAMES =
		{
			"left-left",//0
			"left-center",
			"left-right",
			"center-center",
			"right-left",
			"right-center",
			"right-right",
			"top-top",//7
			"top-middle",
			"top-bottom",
			"middle-middle",
			"bottom-top",
			"bottom-middle",
			"bottom-bottom",
			"width-width",//14
			"height-height",//15
			"leftext-left",//16
			"leftext-right",
			"rightext-left",
			"rightext-right",
			"topext-top",//20
			"topext-bottom",
			"bottomext-top",
			"bottomext-bottom"//23
		};

		static readonly char[] JOINT_CHAR0 = { ',' };

		public Relations( GObject owner )
		{
			this._owner = owner;
			this._items = new List<RelationItem>();
		}

		public void Add( GObject target, RelationType relationType, bool usePercent )
		{
			int count = this._items.Count;
			for ( int i = 0; i < count; i++ )
			{
				RelationItem item = this._items[i];
				if ( item.target == target )
				{
					item.Add( relationType, usePercent );
					return;
				}
			}
			RelationItem newItem = new RelationItem( this._owner );
			newItem.target = target;
			newItem.Add( relationType, usePercent );
			this._items.Add( newItem );
		}

		private void AddItems( GObject target, string sidePairs )
		{
			string[] arr = sidePairs.Split( JOINT_CHAR0 );

			RelationItem newItem = new RelationItem( this._owner );
			newItem.target = target;

			int cnt = arr.Length;
			for ( int i = 0; i < cnt; i++ )
			{
				string s = arr[i];
				if ( string.IsNullOrEmpty( s ) )
					continue;

				bool usePercent;
				if ( s[s.Length - 1] == '%' )
				{
					s = s.Substring( 0, s.Length - 1 );
					usePercent = true;
				}
				else
					usePercent = false;

				int j = s.IndexOf( "-", StringComparison.Ordinal );
				if ( j == -1 )
					s = s + "-" + s;

				int tid = Array.IndexOf( RELATION_NAMES, s );
				if ( tid == -1 )
					throw new ArgumentException( "invalid relation type: " + s );

				newItem.QuickAdd( ( RelationType )tid, usePercent );
			}

			this._items.Add( newItem );
		}

		public void Remove( GObject target, RelationType relationType )
		{
			int cnt = this._items.Count;
			int i = 0;
			while ( i < cnt )
			{
				RelationItem item = this._items[i];
				if ( item.target == target )
				{
					item.Remove( relationType );
					if ( item.isEmpty )
					{
						item.Dispose();
						this._items.RemoveAt( i );
						cnt--;
						continue;
					}
					i++;
				}
				i++;
			}
		}

		public bool Contains( GObject target )
		{
			foreach ( RelationItem item in this._items )
			{
				if ( item.target == target )
					return true;
			}
			return false;
		}

		public void ClearFor( GObject target )
		{
			int cnt = this._items.Count;
			int i = 0;
			while ( i < cnt )
			{
				RelationItem item = this._items[i];
				if ( item.target == target )
				{
					item.Dispose();
					this._items.RemoveAt( i );
					cnt--;
				}
				else
					i++;
			}
		}

		public void ClearAll()
		{
			foreach ( RelationItem item in this._items )
			{
				item.Dispose();
			}
			this._items.Clear();
		}

		public void CopyFrom( Relations source )
		{
			this.ClearAll();

			List<RelationItem> arr = source._items;
			foreach ( RelationItem ri in arr )
			{
				RelationItem item = new RelationItem( this._owner );
				item.CopyFrom( ri );
				this._items.Add( item );
			}
		}

		public void Dispose()
		{
			this.ClearAll();
		}

		public void OnOwnerSizeChanged( Vector2 deltaSize )
		{
			int count = this._items.Count;
			if ( count == 0 )
				return;

			for ( int i = 0; i < count; i++ )
			{
				RelationItem item = this._items[i];
				item.ApplyOnSelfSizeChanged( deltaSize );
			}
		}

		public void Setup( XML xml )
		{
			XMLList col = xml.Elements( "relation" );
			if ( col == null )
				return;

			foreach ( XML cxml in col )
			{
				string targetId = cxml.GetAttribute( "target" );
				GObject target;
				if ( this._owner.parent != null )
				{
					target = !string.IsNullOrEmpty( targetId ) ? this._owner.parent.GetChildById( targetId ) : this._owner.parent;
				}
				else
				{
					//call from component construction
					target = ( ( GComponent )this._owner ).GetChildById( targetId );
				}
				if ( target != null )
					this.AddItems( target, cxml.GetAttribute( "sidePair" ) );
			}
		}
	}
}
