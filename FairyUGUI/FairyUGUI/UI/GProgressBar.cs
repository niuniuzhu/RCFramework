using Core.Xml;
using FairyUGUI.Core;
using FairyUGUI.Event;

namespace FairyUGUI.UI
{
	public class GProgressBar : GComponent
	{
		protected Slider _content;

		public float minValue
		{
			get => this._content.minValue;
			set => this._content.minValue = value;
		}

		public float maxValue
		{
			get => this._content.maxValue;
			set => this._content.maxValue = value;
		}

		public float value
		{
			get => this._content.value;
			set => this._content.value = value;
		}

		public float normalizedValue
		{
			get => this._content.normalizedValue;
			set => this._content.normalizedValue = value;
		}

		public Slider.Direction direction
		{
			get => this._content.direction;
			set => this._content.direction = value;
		}

		public bool interactable
		{
			get => this._content.interactable;
			set => this._content.interactable = value;
		}

		public ProgressTitleType titleType
		{
			get => this._content.titleType;
			set => this._content.titleType = value;
		}

		public EventListener onChanged { get; private set; }

		public GProgressBar()
		{
			this.onChanged = new EventListener( this, EventType.Changed );
		}

		protected override void InternalDispose()
		{
			this._content = null;

			base.InternalDispose();
		}

		protected override void CreateDisplayObject()
		{
			this.rootContainer = this._content = new Slider( this );
			this.container = this.rootContainer;
			this.displayObject = this.rootContainer;
		}

		protected override void ConstructFromXML( XML cxml )
		{
			base.ConstructFromXML( cxml );

			this.InternalConstructFromXML( cxml );
		}

		protected virtual void InternalConstructFromXML( XML cxml )
		{
			XML xml = cxml.GetNode( "ProgressBar" );

			bool reverse = xml.GetAttributeBool( "reverse" );
			GObject bar = this.GetChild( "bar" );
			if ( bar == null )
			{
				bar = this.GetChild( "bar_v" );
				if ( bar != null )
					this._content.direction = reverse
						? Slider.Direction.BottomToTop
						: Slider.Direction.TopToBottom;
			}
			else
			{
				if ( reverse )
					this._content.direction = Slider.Direction.RightToLeft;
			}
			this._content.titleObject = this.GetChild( "title" ) as GTextField;
			this._content.barObject = bar;
			this._content.aniObject = this.GetChild( "ani" ) as GMovieClip;

			string str = xml.GetAttribute( "titleType" );
			this._content.titleType = str != null ? FieldTypes.ParseProgressTitleType( str ) : ProgressTitleType.Percent;
		}

		internal override void SetupAfterAdd( XML cxml )
		{
			base.SetupAfterAdd( cxml );

			XML xml = cxml.GetNode( "ProgressBar" );
			if ( xml != null )
			{
				this._content.maxValue = xml.GetAttributeInt( "max" );
				this._content.value = xml.GetAttributeInt( "value" );
			}
		}
	}
}