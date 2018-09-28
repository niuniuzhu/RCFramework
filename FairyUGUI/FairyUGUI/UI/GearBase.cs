using Core.Xml;
using DG.Tweening;

namespace FairyUGUI.UI
{
	/// <summary>
	/// Gear is a connection between object and controller.
	/// </summary>
	public abstract class GearBase
	{
		public static bool disableAllTweenEffect = false;

		/// <summary>
		/// Use tween to apply change.
		/// </summary>
		public bool tween;

		/// <summary>
		/// Ease type.
		/// </summary>
		public Ease easeType;

		/// <summary>
		/// Tween duration in seconds.
		/// </summary>
		public float tweenTime;

		/// <summary>
		/// Tween delay in seconds.
		/// </summary>
		public float delay;

		protected readonly GObject _owner;
		protected Controller _controller;

		/// <summary>
		/// Controller object.
		/// </summary>
		public Controller controller
		{
			get => this._controller;
			set
			{
				if ( value != this._controller )
				{
					this._controller = value;
					if ( this._controller != null )
						this.Init();
				}
			}
		}

		protected GearBase( GObject owner )
		{
			this._owner = owner;
			this.easeType = Ease.OutQuad;
			this.tweenTime = 0.3f;
			this.delay = 0;
		}

		public void Setup( XML xml )
		{
			this._controller = this._owner.parent.GetController( xml.GetAttribute( "controller" ) );
			if ( this._controller == null )
				return;

			this.Init();

			string str = xml.GetAttribute( "tween" );
			if ( str != null )
				this.tween = true;

			str = xml.GetAttribute( "ease" );
			if ( str != null )
				this.easeType = FieldTypes.ParseEaseType( str );

			str = xml.GetAttribute( "duration" );
			if ( str != null )
				this.tweenTime = float.Parse( str );

			str = xml.GetAttribute( "delay" );
			if ( str != null )
				this.delay = float.Parse( str );

			if ( this is GearDisplay )
			{
				string[] pages = xml.GetAttributeArray( "pages" );
				if ( pages != null )
					( ( GearDisplay )this ).pages.AddRange( pages );
			}
			else
			{
				string[] pages = xml.GetAttributeArray( "pages" );
				string[] values = xml.GetAttributeArray( "values", '|' );

				if ( pages != null && values != null )
				{
					for ( int i = 0; i < values.Length; i++ )
					{
						str = values[i];
						if ( str != "-" )
							this.AddStatus( pages[i], str );
					}
				}
				str = xml.GetAttribute( "default" );
				if ( str != null )
					this.AddStatus( null, str );
			}
		}

		protected abstract void AddStatus( string pageId, string value );
		protected abstract void Init();

		/// <summary>
		/// Call when controller active page changed.
		/// </summary>
		public abstract void Apply();

		/// <summary>
		/// Call when object's properties changed.
		/// </summary>
		public abstract void UpdateState();
	}
}
