using Core.Xml;
using FairyUGUI.Core;
using FairyUGUI.UI.UGUIExt;
using FairyUGUI.Utils;
using System;
using UnityEngine;

namespace FairyUGUI.UI
{
	public class GGraph : GObject
	{
		private Shape _content;

		public bool enableDraw
		{
			get => this._content.enableDraw;
			set => this._content.enableDraw = value;
		}

		public float lineSize
		{
			get => this._content.lineSize;
			set => this._content.lineSize = value;
		}

		public Color lineColor
		{
			get => this._content.lineColor;
			set => this._content.lineColor = value;
		}

		public GraphGraphic.Type type
		{
			get => this._content.type;
			set => this._content.type = value;
		}

		public GGraph()
		{
			this.lineSize = 0;
		}

		protected override void InternalDispose()
		{
			this._content = null;

			base.InternalDispose();
		}

		protected override void CreateDisplayObject()
		{
			this.displayObject = this._content = new Shape( this );
		}

		/// <summary>
		/// Replace this object to another object in the display list.
		/// 在显示列表中，将指定对象取代这个图形对象。这个图形对象相当于一个占位的用途。
		/// </summary>
		/// <param name="target">Target object.</param>
		public void ReplaceMe( GObject target )
		{
			if ( this.parent == null )
				throw new Exception( "parent not set" );

			target.name = this.name;
			target.alpha = this.alpha;
			target.rotation = this.rotation;
			target.visible = this.visible;
			target.touchable = this.touchable;
			target.grayed = this.grayed;
			target.position = this.position;
			target.size = this.size;

			int index = this.parent.GetChildIndex( this );
			this.parent.AddChildAt( target, index );
			target.relations.CopyFrom( this.relations );

			this.parent.RemoveChild( this, true );
		}

		/// <summary>
		/// Add another object before this object.
		/// 在显示列表中，将另一个对象插入到这个对象的前面。
		/// </summary>
		/// <param name="target">Target object.</param>
		public void AddBeforeMe( GObject target )
		{
			if ( this.parent == null )
				throw new Exception( "parent not set" );

			int index = this.parent.GetChildIndex( this );
			this.parent.AddChildAt( target, index );
		}

		/// <summary>
		/// Add another object after this object.
		/// 在显示列表中，将另一个对象插入到这个对象的后面。
		/// </summary>
		/// <param name="target">Target object.</param>
		public void AddAfterMe( GObject target )
		{
			if ( this.parent == null )
				throw new Exception( "parent not set" );

			int index = this.parent.GetChildIndex( this );
			index++;
			this.parent.AddChildAt( target, index );
		}

		/// <summary>
		/// 设置内容为一个原生对象。这个图形对象相当于一个占位的用途。
		/// </summary>
		/// <param name="obj">原生对象</param>
		public void SetNativeObject( DisplayObject obj )
		{
			if ( obj == null )
				return;

			if ( this.displayObject == obj )
				return;

			if ( this.displayObject.parent != null )
				this.displayObject.parent.RemoveChild( this.displayObject, true );
			else
				this.displayObject.Dispose();

			obj.position = this.position;
			obj.size = this.size;
			obj.scale = this.scale;
			obj.color = this.color;
			obj.grayed = this.grayed;
			obj.rotationZ = this.rotation;
			obj.visible = this.visible;
			obj.touchable = this.touchable;
			obj.gOwner = this;

			this._content.Dispose();
			this._content = null;

			this.displayObject = obj;

		    this.parent?.ChildStateChanged( this );
		}

		internal override void SetupBeforeAdd( XML xml )
		{
			string mType = xml.GetAttribute( "type" );
			this.enableDraw = mType != null && mType != "empty";

			base.SetupBeforeAdd( xml );

			string str = xml.GetAttribute( "lineSize" );
			this.lineSize = str != null ? int.Parse( str ) : 1;

			str = xml.GetAttribute( "lineColor" );
			this.lineColor = str != null ? ToolSet.ConvertFromHtmlColor( str ) : Color.black;

			str = xml.GetAttribute( "fillColor" );
			this.color = str != null ? ToolSet.ConvertFromHtmlColor( str ) : Color.white;

			this.type = mType == "rect" ? GraphGraphic.Type.Rect : GraphGraphic.Type.Ellipse;
		}
	}
}