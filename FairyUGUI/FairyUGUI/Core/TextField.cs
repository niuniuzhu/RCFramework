using Core.Misc;
using FairyUGUI.Core.Fonts;
using FairyUGUI.UI;
using FairyUGUI.UI.UGUIExt;
using FairyUGUI.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FairyUGUI.Core
{
	public class TextField : DisplayObject
	{
		protected TextEx _nText;

		private BaseFont _font;
		internal BaseFont font
		{
			get => this._font;
			set
			{
				if ( this._font == value )
					return;
				this._font = value;
				this._nText.baseFont = this._font;
				this._nText.font = this._font.font;
				this.UpdateLineHeight();
				this.FitSize( this._nText.text );

				if ( this.material != null )
				{
					this.material = this._font.mainTexture.associatedAlphaSplitTexture != null
						? MaterialManager.EnableAlphaTexture( this.material )
						: MaterialManager.DisableAlphaTexture( this.material );
				}
			}
		}

		internal bool isBitmapFont => !this._nText.baseFont.isDynamic;

		internal override Vector2 size
		{
			set
			{
				if ( this.autoSize == AutoSizeType.Both || this.autoSize == AutoSizeType.Height )
					return;
				base.size = value;
			}
		}

		private string _text = string.Empty;
		internal virtual string text
		{
			get => this._text;
			set
			{
				if ( this._text == value )
					return;
				this._text = value;
				this._nText.text = this.ubbEnabled ? UBBParser.Parse( this._text ) : this._text;
				this.FitSize( this._nText.text );
			}
		}

		internal int fontSize
		{
			get => this._nText.fontSize;
			set
			{
				if ( this._nText.fontSize == value )
					return;
				this._nText.fontSize = value;
				this.UpdateLineHeight();
				this.FitSize( this._nText.text );
			}
		}

		internal TextAnchor alignment
		{
			get => this._nText.alignment;
			set
			{
				if ( this._nText.alignment == value )
					return;
				this._nText.alignment = value;
				this.FitSize( this._nText.text );
			}
		}

		private float _lineSpacing;
		internal float lineSpacing
		{
			get => this._lineSpacing;
			set
			{
				if ( Mathf.Approximately( this._lineSpacing, value ) )
					return;
				this._lineSpacing = value;
				this.UpdateLineHeight();
				this.FitSize( this._nText.text );
			}
		}

		private AutoSizeType _autoSize;
		internal AutoSizeType autoSize
		{
			get => this._autoSize;
			set
			{
				if ( this._autoSize == value )
					return;
				this._autoSize = value;
				this.FitSize( this._nText.text );
			}
		}

		internal FontStyle fontStyle
		{
			get => this._nText.fontStyle;
			set
			{
				if ( this._nText.fontStyle == value )
					return;
				this._nText.fontStyle = value;
				this.FitSize( this._nText.text );
			}
		}

		protected bool _supportRichText;
		internal virtual bool supportRichText
		{
			get => this._supportRichText;
			set
			{
				if ( this._supportRichText == value )
					return;
				this._supportRichText = value;
				this._nText.supportRichText = this._supportRichText || this._ubbEnabled;
				this.FitSize( this._nText.text );
			}
		}

		protected bool _ubbEnabled;
		internal virtual bool ubbEnabled
		{
			get => this._ubbEnabled;
			set
			{
				if ( this._ubbEnabled == value )
					return;
				this._ubbEnabled = value;
				if ( this._ubbEnabled )
					this.supportRichText = true;
				this._nText.text = this.ubbEnabled ? UBBParser.Parse( this._text ) : this._text;
				this.FitSize( this._nText.text );
			}
		}

		public bool outline { get => this._nText.outline; set => this._nText.outline = value; }

		public Color outlineColor { get => this._nText.outlineColor; set => this._nText.outlineColor = value; }

		public Vector2 outlineDistance { get => this._nText.outlineDistance; set => this._nText.outlineDistance = value; }

		internal TextField( GObject owner )
			: base( owner )
		{
		}

		protected override void OnGameObjectCreated()
		{
			GameObject textGo = new GameObject( "Text" );
			Object.DontDestroyOnLoad( textGo );
			textGo.layer = LayerMask.NameToLayer( Stage.LAYER_NAME );

			RectTransform textRectTransform = textGo.AddComponent<RectTransform>();
			ToolSet.SetAnchor( textRectTransform, AnchorType.Top_Left );
			textRectTransform.pivot = new Vector2( 0, 1 );
			textRectTransform.sizeDelta = this.size;
			textRectTransform.SetParent( this.rectTransform, false );

			this.graphic = this._nText = textGo.AddComponent<TextEx>();

			this._nText.fontStyle = FontStyle.Normal;
			this._nText.alignment = TextAnchor.UpperLeft;
			this._nText.supportRichText = false;
		}

		protected override void InternalDispose()
		{
			Object.DestroyImmediate( this._nText.gameObject );
			this._nText = null;

			base.InternalDispose();
		}

		protected void FitSize( string str )
		{
			if ( this.autoSize == AutoSizeType.None )
				return;

			float preferredWidth = this._nText.rectTransform.rect.size.x;

			if ( string.IsNullOrEmpty( str ) )
			{
				if ( this.autoSize == AutoSizeType.Both )
					preferredWidth = 0;
				base.size = new Vector2( preferredWidth, 0 );
				this.gOwner.FitToDisplayObject();
				return;
			}

			TextGenerationSettings settings;
			if ( this.autoSize == AutoSizeType.Both )
			{
				settings = this._nText.GetGenerationSettings( Vector2.zero );
				preferredWidth = this._nText.cachedTextGeneratorForLayout.GetPreferredWidth( str, settings ) / this._nText.pixelsPerUnit;
			}

			settings = this._nText.GetGenerationSettings( new Vector2( preferredWidth, 0.0f ) );
			float preferredHeight = this._nText.cachedTextGeneratorForLayout.GetPreferredHeight( str, settings ) / this._nText.pixelsPerUnit;

			BitmapFont bmf = this._font as BitmapFont;
			if ( bmf != null )
			{
				preferredHeight = 0f;
				int lineCount = this._nText.cachedTextGeneratorForLayout.lineCount;
				for ( int i = 0; i < lineCount; i++ )
				{
					if ( i > 0 )
						preferredHeight += this._lineSpacing;
					preferredHeight += bmf.lineHeight;
				}
			}

			base.size = new Vector2( preferredWidth, preferredHeight );
			this.gOwner.FitToDisplayObject();
		}

		protected override void HandleSizeChanged()
		{
			base.HandleSizeChanged();

			this._nText.rectTransform.sizeDelta = this.size;
		}

		internal void SetShaodw( Color shadowColor, Vector2 shadowOffset )
		{
			this._nText.shadow = true;
			this._nText.shadowColor = shadowColor;
			this._nText.shadowDistance = shadowOffset;
		}

		internal void SetStrock( Color strokeColor, Vector2 outlineOffset )
		{
			this._nText.outline = true;
			this._nText.outlineColor = strokeColor;
			this._nText.outlineDistance = outlineOffset;
		}

		private void UpdateLineHeight()
		{
			BitmapFont bmf = this._font as BitmapFont;
			if ( bmf != null )
				this._nText.lineSpacing = ( bmf.lineHeight + this._lineSpacing ) * 10;
			else
				this._nText.lineSpacing = 1f + ( this._lineSpacing / this._nText.font.lineHeight );
		}
	}
}