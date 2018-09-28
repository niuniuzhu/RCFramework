using Core.Xml;
using FairyUGUI.Core;
using FairyUGUI.UI.UGUIExt;
using FairyUGUI.Utils;
using UnityEngine;

namespace FairyUGUI.UI
{
	public class GImage : GObject
	{
		public Image content { get; private set; }

		public override bool touchable
		{
			get => false;
			set => base.touchable = false;
		}

		/// <summary>
		/// Flip type.
		/// </summary>
		/// <seealso cref="FlipType"/>
		public FlipType flipType
		{
			get => this.content.flipType;
			set => this.content.flipType = value;
		}

		/// <summary>
		/// Fill method.
		/// </summary>
		public ImageEx.FillMethod fillMethod
		{
			get => this.content.fillMethod;
			set => this.content.fillMethod = value;
		}

		/// <summary>
		/// Fill origin.
		/// </summary>
		public int fillOrigin
		{
			get => this.content.fillOrigin;
			set => this.content.fillOrigin = value;
		}

		/// <summary>
		/// Fill clockwise if true.
		/// </summary>
		public bool fillClockwise
		{
			get => this.content.fillClockwise;
			set => this.content.fillClockwise = value;
		}

		/// <summary>
		/// Fill amount. (0~1)
		/// </summary>
		public float fillAmount
		{
			get => this.content.fillAmount;
			set => this.content.fillAmount = value;
		}

		public ImageScaleMode scaleMode
		{
			get => this.content.scaleMode;
			set => this.content.scaleMode = value;
		}

		public NSprite nSprite
		{
			get => this.content.nSprite;
			set => this.content.nSprite = value;
		}

		public NSprite maskSprite
		{
			get => this.content.maskSprite;
			set => this.content.maskSprite = value;
		}

		protected override void CreateDisplayObject()
		{
			this.displayObject = this.content = new Image( this );
		}

		public void SetNativeSize()
		{
			this.content.SetNativeSize();
		}

		public void MakeSizeFitForContainer()
		{
			this.size = new Vector2( this.sourceWidth, this.sourceHeight );
		}

		internal override void ConstructFromResource( PackageItem pkgItem )
		{
			base.ConstructFromResource( pkgItem );
			this.packageItem.Load();

			this.sourceWidth = this.packageItem.width;
			this.sourceHeight = this.packageItem.height;
			this.initWidth = this.sourceWidth;
			this.initHeight = this.sourceHeight;

			this.content.nSprite = this.packageItem.sprite;
			this.content.scaleMode = this.packageItem.scaleMode;

			this.MakeSizeFitForContainer();
		}

		internal override void SetupBeforeAdd( XML xml )
		{
			base.SetupBeforeAdd( xml );

			string str = xml.GetAttribute( "color" );
			if ( str != null )
			{
				Color c = ToolSet.ConvertFromHtmlColor( str );
				c.a = this.alpha;
				this.color = c;
			}

			str = xml.GetAttribute( "flip" );
			if ( str != null )
				this.flipType = FieldTypes.ParseFlipType( str );

			str = xml.GetAttribute( "fillMethod" );
			if ( str != null )
			{
				this.scaleMode = ImageScaleMode.Filled;
				this.fillOrigin = xml.GetAttributeInt( "fillOrigin" );
				this.fillClockwise = xml.GetAttributeBool( "fillClockwise", true );
				this.fillAmount = ( float )xml.GetAttributeInt( "fillAmount", 100 ) / 100;
				this.fillMethod = FieldTypes.ParseFillMethod( str );
			}
		}
	}
}
