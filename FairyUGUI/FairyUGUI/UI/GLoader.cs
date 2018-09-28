using Core.Xml;
using FairyUGUI.Core;
using FairyUGUI.UI.UGUIExt;
using FairyUGUI.Utils;
using UnityEngine;

namespace FairyUGUI.UI
{
	public class GLoader : GObject, IAnimationGear
	{
		private static readonly GObjectPool ERROR_SIGN_POOL = new GObjectPool();

		public bool showErrorSign;

		public GearAnimation gearAnimation { get; private set; }

		private string _url;
		public string url
		{
			get => this._url;
			set
			{
				if ( this._url == value )
					return;

				this._url = value;
				this.LoadContent();
			}
		}

		public override NMaterial material
		{
			get => this._activeObject?.material;
			set
			{
				if ( this._activeObject != null )
					this._activeObject.material = value;
			}
		}

		public override Shader shader
		{
			get => this._activeObject?.shader;
			set
			{
				if ( this._activeObject != null )
					this._activeObject.shader = value;
			}
		}

		public override Color color
		{
			get => this._activeObject?.color ?? Color.black;
			set
			{
				if ( this._activeObject != null )
					this._activeObject.color = value;

				if ( this.gearColor.controller != null )
					this.gearColor.UpdateState();
			}
		}

		private FillType _fill;
		public FillType fill
		{
			get => this._fill;
			set
			{
				if ( this._fill != value )
				{
					this._fill = value;
					this.UpdateLayout();
				}
			}
		}

		public ImageEx.FillMethod fillMethod
		{
			get => this._image.fillMethod;
			set => this._image.fillMethod = value;
		}

		public int fillOrigin
		{
			get => this._image.fillOrigin;
			set => this._image.fillOrigin = value;
		}

		public bool fillClockwise
		{
			get => this._image.fillClockwise;
			set => this._image.fillClockwise = value;
		}

		public float fillAmount
		{
			get => this._image.fillAmount;
			set => this._image.fillAmount = value;
		}

		private bool _playing;
		public bool playing
		{
			get => this._playing;
			set
			{
				if ( this._playing != value )
				{
					this._playing = value;
					if ( this._movieClip != null )
					{
						this._movieClip.playing = value;
						if ( this.gearAnimation.controller != null )
							this.gearAnimation.UpdateState();
					}
				}
			}
		}

		private int _frame;
		public int frame
		{
			get => this._frame;
			set
			{
				this._frame = value;
				if ( this._movieClip != null )
				{
					this._movieClip.currentFrame = value;
					if ( this.gearAnimation.controller != null )
						this.gearAnimation.UpdateState();
				}
			}
		}
		private bool _autoSize;
		public bool autoSize
		{
			get => this._autoSize;
			set
			{
				if ( this._autoSize != value )
				{
					this._autoSize = value;
					this.UpdateLayout();
				}
			}
		}

		private AlignType _align;
		public AlignType align
		{
			get => this._align;
			set
			{
				if ( this._align != value )
				{
					this._align = value;
					this.UpdateLayout();
				}
			}
		}

		private VertAlignType _verticalAlign;
		public VertAlignType verticalAlign
		{
			get => this._verticalAlign;
			set
			{
				if ( this._verticalAlign != value )
				{
					this._verticalAlign = value;
					this.UpdateLayout();
				}
			}
		}

		private Image _image;
		public Image image => this._image;

		private MovieClip _movieClip;
		public MovieClip movieClip => this._movieClip;

		private NSprite _maskSprite;
		public NSprite maskSprite
		{
			get => this._maskSprite;
			set
			{
				if ( this._maskSprite == value )
					return;
				this._maskSprite = value;
				this.UpdateMask();
			}
		}

		private Container _content;
		private Image _activeObject;

		private Image activeObject
		{
			set
			{
				this._activeObject = value;
				this.UpdateMask();
				this.UpdateGrayed();
				this.UpdateLayout();
			}
		}

		private GImage _errorSign;
		private PackageItem _contentItem;

		public GLoader()
		{
			this._playing = true;
			this._url = string.Empty;
			this._align = AlignType.Left;
			this._verticalAlign = VertAlignType.Top;
			this.showErrorSign = true;

			this.gearAnimation = new GearAnimation( this );
		}

		protected override void InternalDispose()
		{
			this._activeObject = null;//no need to dispose,because it is a child of this object
			this._contentItem = null;

			base.InternalDispose();
		}

		protected override void CreateDisplayObject()
		{
			this.displayObject = this._content = new Container( this );
		}

		private void LoadContent()
		{
			this.ClearContent();

			if ( string.IsNullOrEmpty( this._url ) )
				return;

			if ( this._url.StartsWith( UIPackage.URL_PREFIX ) )
				this.LoadFromPackage( this._url );
			else
				this.LoadExternal();
		}

		private void LoadFromPackage( string itemURL )
		{
			this._contentItem = UIPackage.GetItemByURL( itemURL );

			if ( this._contentItem == null )
			{
				this.SetErrorState();
				return;
			}
			this._contentItem.Load();
			switch ( this._contentItem.type )
			{
				case PackageItemType.Image:
					if ( this._image == null )
					{
						this._image = new Image( null );
						this._image.grayed = this.grayed;
						this._content.AddChild( this._image );
					}
					this._image.nSprite = this._contentItem.sprite;
					this._image.size = new Vector2( this._contentItem.width, this._contentItem.height );
					this.activeObject = this._image;
					return;

				case PackageItemType.MovieClip:
					if ( this._movieClip == null )
					{
						this._movieClip = new MovieClip( null );
						this._movieClip.grayed = this.grayed;
						this._content.AddChild( this._movieClip );
					}

					this._movieClip.SetData( this._contentItem );
					this._movieClip.playing = this._playing;
					this._movieClip.currentFrame = this._frame;
					this._movieClip.size = new Vector2( this._contentItem.width, this._contentItem.height );
					this.activeObject = this._movieClip;
					return;
			}
			this.SetErrorState();
		}

		protected virtual void LoadExternal()
		{
			Texture2D tex = ( Texture2D ) Resources.Load( this.url, typeof( Texture2D ) );
			if ( tex != null )
				this.OnExternalLoadSuccess( tex );
			else
				this.OnExternalLoadFailed();
		}

		protected virtual void FreeExternal( NSprite sprite )
		{
		}

		public void OnExternalLoadSuccess( Texture2D texture )
		{
			if ( this.disposed )
				return;

			if ( this._image == null )
			{
				this._image = new Image( null );
				this._image.grayed = this.grayed;
				this._content.AddChild( this._image );
			}
			NTexture nTexture = new NTexture( texture, null, new[] { ToolSet.CreateSpriteFromTexture( texture ) } );
			this._image.nSprite = nTexture.GetSprite( nTexture.name );
			this._image.SetNativeSize();
			this.activeObject = this._image;
		}

		public void OnExternalLoadFailed()
		{
			if ( this.disposed )
				return;

			this.SetErrorState();
		}

		private void SetErrorState()
		{
			if ( !this.showErrorSign )
				return;

			if ( this._errorSign == null )
			{
				if ( UIConfig.loaderErrorSign != null )
					this._errorSign = ERROR_SIGN_POOL.GetObject( UIConfig.loaderErrorSign ).asImage;
			}

			if ( this._errorSign != null )
			{
				this._errorSign.size = this.size;
				this._errorSign.grayed = this.grayed;
				this._content.AddChild( this._errorSign.displayObject );
				this.activeObject = this._errorSign.content;
			}
		}

		private void ClearContent()
		{
			this.ClearErrorState();

			if ( this._image != null )
			{
				if ( this._image.nSprite != null )
					this.FreeExternal( this._image.nSprite );

				this._image.RemoveFromParent();
				this._image.Dispose();
				this._image = null;
			}
			if ( this._movieClip != null )
			{
				this._movieClip.RemoveFromParent();
				this._movieClip.Dispose();
				this._movieClip = null;
			}
			this._activeObject = null;
			this._contentItem = null;
		}

		private void ClearErrorState()
		{
			if ( this._errorSign != null )
			{
				this._content.RemoveChild( this._errorSign.displayObject );
				ERROR_SIGN_POOL.ReturnObject( this._errorSign );
				this._errorSign = null;
			}
		}

		private void UpdateMask()
		{
			if ( this._activeObject != null )
				this._activeObject.maskSprite = this._maskSprite;
		}

		private void UpdateGrayed()
		{
			if ( this._activeObject != null )
				this._activeObject.grayed = this.displayObject.grayed;
		}

		protected internal override void HandleGrayedChanged()
		{
			base.HandleGrayedChanged();

			this.UpdateGrayed();
		}

		internal override void HandleControllerChanged( Controller c )
		{
			base.HandleControllerChanged( c );

			if ( this.gearAnimation.controller == c )
				this.gearAnimation.Apply();
		}

		protected override void HandleSizeChanged()
		{
			this.UpdateLayout();

			base.HandleSizeChanged();
		}

		private void UpdateLayout()
		{
			if ( this._activeObject == null )
			{
				if ( this._autoSize )
					this.size = new Vector2( 50, 30 );
				return;
			}

			if ( this._autoSize )
			{
				this.size = this._activeObject == null ? new Vector2( 50, 30 ) : this._activeObject.size;
				Image img = this._activeObject;
				if ( img != null )
					img.SetNativeSize();
				else
					this._activeObject.scale = Vector2.one;
			}
			else
			{
				float sx = 1, sy = 1;
				float w = this._activeObject.size.x;
				float h = this._activeObject.size.y;
				if ( this._fill == FillType.Scale || this._fill == FillType.ScaleFree )
				{
					sx = this.size.x / w;
					sy = this.size.y / h;

					if ( sx != 1 || sy != 1 )
					{
						if ( this._fill == FillType.Scale )
						{
							if ( sx > sy )
								sx = sy;
							else
								sy = sx;
						}
						w *= sx;
						h *= sy;
					}
				}

				this._activeObject.scale = new Vector2( sx, sy );

				float nx;
				float ny;
				switch ( this._align )
				{
					case AlignType.Center:
						nx = ( this.size.x - w ) / 2;
						break;

					case AlignType.Right:
						nx = this.size.x - w;
						break;

					default:
						nx = 0;
						break;
				}
				switch ( this._verticalAlign )
				{
					case VertAlignType.Middle:
						ny = ( this.size.y - h ) * 0.5f;
						break;

					case VertAlignType.Bottom:
						ny = this.size.y - h;
						break;

					default:
						ny = 0;
						break;
				}

				this._activeObject.position = new Vector2( nx, ny );
			}
		}

		internal override void SetupBeforeAdd( XML xml )
		{
			base.SetupBeforeAdd( xml );

			string str = xml.GetAttribute( "url" );
			if ( str != null )
				this._url = str;

			str = xml.GetAttribute( "align" );
			if ( str != null )
				this._align = FieldTypes.ParseAlign( str );

			str = xml.GetAttribute( "vAlign" );
			if ( str != null )
				this._verticalAlign = FieldTypes.ParseVerticalAlign( str );

			str = xml.GetAttribute( "fill" );
			if ( str != null )
				this._fill = FieldTypes.ParseFillType( str );

			this._autoSize = xml.GetAttributeBool( "autoSize" );

			str = xml.GetAttribute( "errorSign" );
			if ( str != null )
				this.showErrorSign = str == "true";

			this._playing = xml.GetAttributeBool( "playing", true );

			str = xml.GetAttribute( "color" );
			if ( str != null )
				this.color = ToolSet.ConvertFromHtmlColor( str );

			str = xml.GetAttribute( "fillMethod" );
			if ( str != null )
			{
				this.fillOrigin = xml.GetAttributeInt( "fillOrigin" );
				this.fillClockwise = xml.GetAttributeBool( "fillClockwise", true );
				this.fillAmount = ( float ) xml.GetAttributeInt( "fillAmount", 100 ) / 100;
				this.fillMethod = FieldTypes.ParseFillMethod( str );
			}

			if ( this._url != null )
				this.LoadContent();
		}

		internal override void SetupAfterAdd( XML xml )
		{
			base.SetupAfterAdd( xml );

			XML cxml = xml.GetNode( "gearAni" );
			if ( cxml != null )
				this.gearAnimation.Setup( cxml );
		}
	}
}