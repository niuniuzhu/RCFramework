using Core.Xml;
using FairyUGUI.Core;
using FairyUGUI.Core.Fonts;
using FairyUGUI.Utils;
using Game.Misc;
using UnityEngine;

namespace FairyUGUI.UI
{
	public class GTextField : GObject, ITextColorGear
	{
		protected TextField _content;

		public override bool touchable
		{
			get => false;
			set => base.touchable = false;
		}

		public int fontSize
		{
			get => this._content.fontSize;
			set => this._content.fontSize = value;
		}

		public override string text
		{
			get => this._content.text;
			set => this._content.text = value;
		}

		private string _fontName;
		public string fontName
		{
			get => this._fontName;
			set
			{
				if ( string.IsNullOrEmpty( value ) )
					value = UIConfig.defaultFont;

				if ( this._fontName == value )
					return;

				this._fontName = value;

				BaseFont font = FontManager.GetFont( this._fontName );

				this._content.font = font;
			}
		}

		public Color textColor { get => this._content.color; set => this._content.color = value; }

		public Color strokeColor { get => this._content.outlineColor; set => this._content.outlineColor = value; }

		public bool isBitmapFont => this._content.isBitmapFont;

		public float lineSpacing
		{
			get => this._content.lineSpacing;
			set => this._content.lineSpacing = value;
		}

		public AutoSizeType autoSize
		{
			get => this._content.autoSize;
			set => this._content.autoSize = value;
		}

		public virtual bool supportRichText
		{
			get => this._content.supportRichText;
			set => this._content.supportRichText = value;
		}

		public bool ubbEnabled
		{
			get => this._content.ubbEnabled;
			set => this._content.ubbEnabled = value;
		}

		protected override void CreateDisplayObject()
		{
			this.displayObject = this._content = new TextField( this );
			this.fontName = UIConfig.defaultFont;
		}

		public void SetAlign( AlignType align, VertAlignType verticalAlign )
		{
			switch ( align )
			{
				case AlignType.Left:
					{
						switch ( verticalAlign )
						{
							case VertAlignType.Top:
								this._content.alignment = TextAnchor.UpperLeft;
								break;
							case VertAlignType.Middle:
								this._content.alignment = TextAnchor.MiddleLeft;
								break;
							case VertAlignType.Bottom:
								this._content.alignment = TextAnchor.LowerLeft;
								break;
						}
					}
					break;

				case AlignType.Center:
					{
						switch ( verticalAlign )
						{
							case VertAlignType.Top:
								this._content.alignment = TextAnchor.UpperCenter;
								break;
							case VertAlignType.Middle:
								this._content.alignment = TextAnchor.MiddleCenter;
								break;
							case VertAlignType.Bottom:
								this._content.alignment = TextAnchor.LowerCenter;
								break;
						}
					}
					break;

				case AlignType.Right:
					{
						switch ( verticalAlign )
						{
							case VertAlignType.Top:
								this._content.alignment = TextAnchor.UpperRight;
								break;
							case VertAlignType.Middle:
								this._content.alignment = TextAnchor.MiddleRight;
								break;
							case VertAlignType.Bottom:
								this._content.alignment = TextAnchor.LowerRight;
								break;
						}
					}
					break;
			}
		}

		public void SetFontStyle( bool italic, bool bold )
		{
			if ( italic && !bold )
				this._content.fontStyle = FontStyle.Italic;
			else if ( !italic && bold )
				this._content.fontStyle = FontStyle.Bold;
			else if ( italic )
				this._content.fontStyle = FontStyle.BoldAndItalic;
		}

		public void SetShaodw( Color shadowColor, Vector2 shadowOffset )
		{
			this._content.SetShaodw( shadowColor, shadowOffset );
		}

		public void SetStrock( Color strokeColor, Vector2 outlineOffset )
		{
			this._content.SetStrock( strokeColor, outlineOffset );
		}

		internal override void SetupAfterAdd( XML xml )
		{
			base.SetupAfterAdd( xml );

			string str = xml.GetAttribute( "font", string.Empty );
			this.fontName = str;

			str = xml.GetAttribute( "fontSize" );
			if ( str != null )
				this.fontSize = int.Parse( str );

			str = xml.GetAttribute( "color" );
			Color c = str != null ? ToolSet.ConvertFromHtmlColor( str ) : ( this.isBitmapFont ? Color.white : Color.black );
			c.a = this.alpha;
			this.color = c;

			str = xml.GetAttribute( "align", string.Empty );
			AlignType align = FieldTypes.ParseAlign( str );

			str = xml.GetAttribute( "vAlign", string.Empty );
			VertAlignType verticalAlign = FieldTypes.ParseVerticalAlign( str );

			this.SetAlign( align, verticalAlign );

			str = xml.GetAttribute( "leading" );
			if ( str != null )
				this.lineSpacing = int.Parse( str );

			//str = xml.GetAttribute( "letterSpacing" );
			//if ( str != null )
			//	this.letterSpacing = int.Parse( str );

			this.supportRichText = false;
			this.ubbEnabled = xml.GetAttributeBool( "ubb" );

			//_textFormat.underline = xml.GetAttributeBool( "underline", false );
			bool italic = xml.GetAttributeBool( "italic" );
			bool bold = xml.GetAttributeBool( "bold" );
			this.SetFontStyle( italic, bold );

			str = xml.GetAttribute( "shadowColor" );
			if ( str != null )
			{
				Color shadowColor = ToolSet.ConvertFromHtmlColor( str );
				Vector2 shadowOffset = xml.GetAttributeVector( "shadowOffset" );
				this.SetShaodw( shadowColor, shadowOffset );
			}

			str = xml.GetAttribute( "strokeColor" );
			if ( str != null )
			{
				Color strokeColor = ToolSet.ConvertFromHtmlColor( str );
				int strokeSize = xml.GetAttributeInt( "strokeSize", 1 );
				this.SetStrock( strokeColor, new Vector2( strokeSize, strokeSize ) );
			}

			str = xml.GetAttribute( "text" );
			if ( !string.IsNullOrEmpty( str ) )
				this.text = str;

			str = xml.GetAttribute( "autoSize" );
			this.autoSize = str != null ? FieldTypes.ParseAutoSizeType( str ) : AutoSizeType.Both;
		}

	}
}