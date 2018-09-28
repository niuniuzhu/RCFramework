using Core.Xml;
using FairyUGUI.Core;
using FairyUGUI.Event;
using Logger = Core.Misc.Logger;

namespace FairyUGUI.UI
{
	public class GScrollBar : GComponent
	{
		public ScrollBar content { get; private set; }

		public float minValue
		{
			get => this.content.minValue;
			set => this.content.minValue = value;
		}

		public float maxValue
		{
			get => this.content.maxValue;
			set => this.content.maxValue = value;
		}

		public float value
		{
			get => this.content.value;
			set => this.content.value = value;
		}

		public float normalizedValue
		{
			get => this.content.normalizedValue;
			set => this.content.normalizedValue = value;
		}

		public ScrollBar.Direction direction
		{
			get => this.content.direction;
			set => this.content.direction = value;
		}

		internal float visualSize
		{
			get => this.content.visualSize;
			set => this.content.visualSize = value;
		}

		public EventListener onChange { get; private set; }

		public GScrollBar()
		{
			this.ignoreLayout = true;

			this.onChange = new EventListener( this, EventType.Changed );
		}

		protected override void CreateDisplayObject()
		{
			this.displayObject = this.container = this.rootContainer = this.content = new ScrollBar( this );
		}

		protected override void InternalDispose()
		{
			this.content = null;

			base.InternalDispose();
		}

		protected override void ConstructFromXML( XML cxml )
		{
			base.ConstructFromXML( cxml );

			//XML xml = cxml.GetNode( "ScrollBar" );
			//if ( xml != null )
			//	_fixedGripSize = xml.GetAttributeBool( "fixedGripSize" );

			this.content.gripObject = this.GetChild( "grip" );
			if ( this.content.gripObject == null )
				Logger.Warn( "FairyGUI: " + this.resourceURL + " should define grip" );

			this.content.barObject = this.GetChild( "bar" );
			if ( this.content.barObject == null )
				Logger.Warn( "FairyGUI: " + this.resourceURL + " should define bar" );
		}
	}
}