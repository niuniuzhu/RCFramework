using System;
using System.Collections.Generic;
using Core.Xml;
using FairyUGUI.Event;

namespace FairyUGUI.UI
{
	/// <summary>
	/// Controller class.
	/// 控制器类。控制器的创建和设计需通过编辑器完成，不建议使用代码创建。
	/// 最常用的方法是通过selectedIndex获得或改变控制器的活动页面。如果要获得控制器页面改变的通知，使用onChanged事件。
	/// </summary>
	public class Controller : EventDispatcher
	{
		/// <summary>
		/// Name of the controller
		/// 控制器名称。
		/// </summary>
		public string name;

		/// <summary>
		/// When controller page changed.
		/// 当控制器活动页面改变时，此事件被触发。
		/// </summary>
		public EventListener onChanged { get; private set; }

		internal GComponent parent;
		internal bool autoRadioGroupDepth;

		private readonly List<string> _pageIds;
		/// <summary>
		/// Page count of this controller.
		/// 获得页面数量。
		/// </summary>
		public int pageCount => this._pageIds.Count;

		internal string selectedPageId
		{
			get
			{
				if ( this._selectedIndex == -1 )
					return null;
				return this._pageIds[this._selectedIndex];
			}
			set
			{
				int i = this._pageIds.IndexOf( value );
				this.selectedIndex = i;
			}
		}

		internal string oppositePageId
		{
			set
			{
				int i = this._pageIds.IndexOf( value );
				if ( i > 0 )
					this.selectedIndex = 0;
				else if ( this._pageIds.Count > 1 )
					this.selectedIndex = 1;
			}
		}

		internal string previousPageId
		{
			get
			{
				if ( this._previousIndex == -1 )
					return null;
				return this._pageIds[this._previousIndex];
			}
		}

		private int _selectedIndex;
		/// <summary>
		/// Current page index.
		/// 获得或设置当前活动页面索引。
		/// </summary>
		public int selectedIndex
		{
			get => this._selectedIndex;
			set
			{
				if ( this._selectedIndex != value )
				{
					if ( value > this._pageIds.Count - 1 )
						throw new IndexOutOfRangeException( "" + value );

					this._previousIndex = this._selectedIndex;
					this._selectedIndex = value;
					this.parent.ApplyController( this );

					this.onChanged.Call();

					if ( this._playingTransition != null )
					{
						this._playingTransition.Stop();
						this._playingTransition = null;
					}

					if ( this._pageTransitions != null )
					{
						foreach ( PageTransition pt in this._pageTransitions )
						{
							if ( pt.toIndex == this._selectedIndex && ( pt.fromIndex == -1 || pt.fromIndex == this._previousIndex ) )
							{
								this._playingTransition = this.parent.GetTransition( pt.transitionName );
								break;
							}
						}

						//	if ( this._playingTransition != null )
						//		this._playingTransition.Play( () => { this._playingTransition = null; } );
					}
				}
			}
		}

		/// <summary>
		/// Current page name.
		/// 获得当前活动页面名称
		/// </summary>
		public string selectedPage
		{
			get
			{
				if ( this._selectedIndex == -1 )
					return null;
				return this._pageNames[this._selectedIndex];
			}
			set
			{
				int i = this._pageNames.IndexOf( value );
				if ( i == -1 )
					i = 0;
				this.selectedIndex = i;
			}
		}

		private int _previousIndex;
		/// <summary>
		/// Previouse page index.
		/// 获得上次活动页面索引
		/// </summary>
		public int previsousIndex => this._previousIndex;

		/// <summary>
		/// Previous page name.
		/// 获得上次活动页面名称。
		/// </summary>
		public string previousPage
		{
			get
			{
				if ( this._previousIndex == -1 )
					return null;
				return this._pageNames[this._previousIndex];
			}
		}

		private readonly List<string> _pageNames;
		private List<PageTransition> _pageTransitions;
		private Transition _playingTransition;

		private static uint _nextPageId;

		public Controller()
		{
			this._pageIds = new List<string>();
			this._pageNames = new List<string>();
			this._selectedIndex = -1;
			this._previousIndex = -1;

			this.onChanged = new EventListener( this, EventType.Changed );
		}

		protected override void InternalDispose()
		{
			this.parent = null;

			base.InternalDispose();
		}

		/// <summary>
		/// Set current page index, no onChanged event.
		/// 通过索引设置当前活动页面，和selectedIndex的区别在于，这个方法不会触发onChanged事件。
		/// </summary>
		/// <param name="value">Page index</param>
		public void SetSelectedIndex( int value )
		{
			if ( this._selectedIndex != value )
			{
				if ( value > this._pageIds.Count - 1 )
					throw new IndexOutOfRangeException( "" + value );

				this._previousIndex = this._selectedIndex;
				this._selectedIndex = value;
				this.parent.ApplyController( this );

				//if ( this._playingTransition != null )
				//{
				//	this._playingTransition.Stop();
				//	this._playingTransition = null;
				//}
			}
		}

		/// <summary>
		/// Set current page by name, no onChanged event.
		/// 通过页面名称设置当前活动页面，和selectedPage的区别在于，这个方法不会触发onChanged事件。
		/// </summary>
		/// <param name="value">Page name</param>
		public void SetSelectedPage( string value )
		{
			int i = this._pageNames.IndexOf( value );
			if ( i == -1 )
				i = 0;
			this.SetSelectedIndex( i );
		}

		/// <summary>
		/// Get page name by an index.
		/// 通过页面索引获得页面名称。
		/// </summary>
		/// <param name="index">Page index</param>
		/// <returns>Page Name</returns>
		public string GetPageName( int index )
		{
			return this._pageNames[index];
		}

		/// <summary>
		/// Get page id by name
		/// </summary>
		/// <param name="aName"></param>
		/// <returns></returns>
		public string GetPageIdByName( string aName )
		{
			int i = this._pageNames.IndexOf( aName );
			if ( i != -1 )
				return this._pageIds[i];
			return null;
		}

		/// <summary>
		/// Add a new page to this controller.
		/// </summary>
		/// <param name="name">Page name</param>
		public void AddPage( string name )
		{
			if ( name == null )
				name = string.Empty;

			this.AddPageAt( name, this._pageIds.Count );
		}

		/// <summary>
		/// Add a new page to this controller at a certain index.
		/// </summary>
		/// <param name="name">Page name</param>
		/// <param name="index">Insert position</param>
		public void AddPageAt( string name, int index )
		{
			string nid = "_" + ( _nextPageId++ );
			if ( index == this._pageIds.Count )
			{
				this._pageIds.Add( nid );
				this._pageNames.Add( name );
			}
			else
			{
				this._pageIds.Insert( index, nid );
				this._pageNames.Insert( index, name );
			}
		}

		/// <summary>
		/// Remove a page.
		/// </summary>
		/// <param name="name">Page name</param>
		public void RemovePage( string name )
		{
			int i = this._pageNames.IndexOf( name );
			if ( i != -1 )
			{
				this._pageIds.RemoveAt( i );
				this._pageNames.RemoveAt( i );
				if ( this._selectedIndex >= this._pageIds.Count )
					this.selectedIndex = this._selectedIndex - 1;
				else
					this.parent.ApplyController( this );
			}
		}

		/// <summary>
		/// Removes a page at a certain index.
		/// </summary>
		/// <param name="index"></param>
		public void RemovePageAt( int index )
		{
			this._pageIds.RemoveAt( index );
			this._pageNames.RemoveAt( index );
			if ( this._selectedIndex >= this._pageIds.Count )
				this.selectedIndex = this._selectedIndex - 1;
			else
				this.parent.ApplyController( this );
		}

		/// <summary>
		/// Remove all pages.
		/// </summary>
		public void ClearPages()
		{
			this._pageIds.Clear();
			this._pageNames.Clear();
			if ( this._selectedIndex != -1 )
				this.selectedIndex = -1;
			else
				this.parent.ApplyController( this );
		}

		/// <summary>
		/// Check if the controller has a page.
		/// </summary>
		/// <param name="aName">Page name.</param>
		/// <returns></returns>
		public bool HasPage( string aName )
		{
			return this._pageNames.IndexOf( aName ) != -1;
		}

		internal int GetPageIndexById( string aId )
		{
			return this._pageIds.IndexOf( aId );
		}

		internal string GetPageNameById( string aId )
		{
			int i = this._pageIds.IndexOf( aId );
			if ( i != -1 )
				return this._pageNames[i];
			return null;
		}

		internal string GetPageId( int index )
		{
			return this._pageIds[index];
		}

		public void Setup( XML xml )
		{
			this.name = xml.GetAttribute( "name" );
			this.autoRadioGroupDepth = xml.GetAttributeBool( "autoRadioGroupDepth" );

			string[] arr = xml.GetAttributeArray( "pages" );
			if ( arr != null )
			{
				int cnt = arr.Length;
				for ( int i = 0; i < cnt; i += 2 )
				{
					this._pageIds.Add( arr[i] );
					this._pageNames.Add( arr[i + 1] );
				}
			}

			arr = xml.GetAttributeArray( "transitions" );
			if ( arr != null )
			{
				this._pageTransitions = new List<PageTransition>();

				int cnt = arr.Length;
				for ( int i = 0; i < cnt; i++ )
				{
					string str = arr[i];

					PageTransition pt = new PageTransition();
					int k = str.IndexOf( "=", StringComparison.Ordinal );
					pt.transitionName = str.Substring( k + 1 );
					str = str.Substring( 0, k );
					k = str.IndexOf( "-", StringComparison.Ordinal );
					pt.toIndex = int.Parse( str.Substring( k + 1 ) );
					str = str.Substring( 0, k );
					if ( str == "*" )
						pt.fromIndex = -1;
					else
						pt.fromIndex = int.Parse( str );
					this._pageTransitions.Add( pt );
				}
			}

			if ( this.parent != null && this._pageIds.Count >= 0 )
				this._selectedIndex = 0;
			else
				this._selectedIndex = -1;
		}
	}

	class PageTransition
	{
		public string transitionName;
		public int fromIndex;
		public int toIndex;
	}
}
