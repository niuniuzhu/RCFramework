using Core.Xml;
using FairyUGUI.Core;

namespace FairyUGUI.UI
{
	public class GSlider : GProgressBar
	{
		protected override void CreateDisplayObject()
		{
			base.CreateDisplayObject();
			this.interactable = true;
		}

		protected override void InternalConstructFromXML( XML cxml )
		{
			XML xml = cxml.GetNode( "Slider" );

			bool reverse = xml.GetAttributeBool( "reverse" );
			Slider.Direction dir = Slider.Direction.LeftToRight;
			GObject bar = this.GetChild( "bar" );
			if ( bar == null )
			{
				bar = this.GetChild( "bar_v" );
				if ( bar != null )
					dir = reverse
						? Slider.Direction.BottomToTop
						: Slider.Direction.TopToBottom;
			}
			else
			{
				if ( reverse )
					dir = Slider.Direction.RightToLeft;
			}
			this._content.titleObject = this.GetChild( "title" ) as GTextField;
			this._content.barObject = bar;
			this._content.aniObject = this.GetChild( "ani" ) as GMovieClip;
			this._content.gripObject = this.GetChild( "grip" );
			this._content.direction = dir;

			this._content.maxValue = xml.GetAttributeInt( "max" );
			this._content.value = xml.GetAttributeInt( "value" );

			string str = xml.GetAttribute( "titleType" );
			this._content.titleType = str != null ? FieldTypes.ParseProgressTitleType( str ) : ProgressTitleType.Percent;
		}

		internal override void SetupAfterAdd( XML cxml )
		{
			base.SetupAfterAdd( cxml );

			XML xml = cxml.GetNode( "Slider" );
			if ( xml != null )
			{
				this._content.maxValue = xml.GetAttributeInt( "max" );
				this._content.value = xml.GetAttributeInt( "value" );
			}
		}
	}
}