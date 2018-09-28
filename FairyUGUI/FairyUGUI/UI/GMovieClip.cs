using Core.Xml;
using FairyUGUI.Core;
using FairyUGUI.Event;
using FairyUGUI.Utils;
using UnityEngine;
using EventType = FairyUGUI.Event.EventType;

namespace FairyUGUI.UI
{
	public class GMovieClip : GImage, IAnimationGear
	{
		private MovieClip _content;

		public bool playing
		{
			get => this._content.playing;
			set
			{
				if ( this._content.playing != value )
				{
					this._content.playing = value;
					if ( this.gearAnimation.controller != null )
						this.gearAnimation.UpdateState();
				}
			}
		}

		public int frame
		{
			get => this._content.currentFrame;
			set
			{
				if ( this._content.currentFrame != value )
				{
					this._content.currentFrame = value;
					if ( this.gearAnimation.controller != null )
						this.gearAnimation.UpdateState();
				}
			}
		}

		public float interval
		{
			get => this._content.interval;
			set => this._content.interval = value;
		}

		public bool swing
		{
			get => this._content.swing;
			set => this._content.swing = value;
		}

		public float repeatDelay
		{
			get => this._content.repeatDelay;
			set => this._content.repeatDelay = value;
		}

		public GearAnimation gearAnimation { get; private set; }

		public EventListener onPlayEnd { get; private set; }

		public GMovieClip()
		{
			this.gearAnimation = new GearAnimation( this );

			this.onPlayEnd = new EventListener( this, EventType.PlayEnd );
		}

		protected override void CreateDisplayObject()
		{
			this.displayObject = this._content = new MovieClip( this );
		}

		internal override void HandleControllerChanged( Controller c )
		{
			base.HandleControllerChanged( c );

			if ( this.gearAnimation.controller == c )
				this.gearAnimation.Apply();
		}

		internal override void ConstructFromResource( PackageItem pkgItem )
		{
			this.packageItem = pkgItem;
			this.packageItem.Load();

			this.sourceWidth = this.packageItem.width;
			this.sourceHeight = this.packageItem.height;
			this.initWidth = this.sourceWidth;
			this.initHeight = this.sourceHeight;

			this._content.SetData( this.packageItem );

			this.size = new Vector2( this.sourceWidth, this.sourceHeight );
		}

		internal override void SetupBeforeAdd( XML xml )
		{
			base.SetupBeforeAdd( xml );

			string str = xml.GetAttribute( "frame" );
			if ( str != null )
				this._content.currentFrame = int.Parse( str );
			this._content.playing = xml.GetAttributeBool( "playing", true );

			str = xml.GetAttribute( "color" );
			if ( str != null )
				this.color = ToolSet.ConvertFromHtmlColor( str );

			str = xml.GetAttribute( "flip" );
			if ( str != null )
				this._content.flipType = FieldTypes.ParseFlipType( str );
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