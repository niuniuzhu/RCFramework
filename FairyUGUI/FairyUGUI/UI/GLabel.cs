using Core.Xml;
using FairyUGUI.Utils;
using UnityEngine;

namespace FairyUGUI.UI
{
	public sealed class GLabel : GComponent, ITextColorGear
	{
		private GObject _titleObject;
		private GObject _iconObject;

		/// <summary>
		/// Icon of the label.
		/// </summary>
		public override string icon
		{
			get
			{
				GLoader loader = this._iconObject as GLoader;
				if ( loader != null )
					return loader.url;
				GLabel label = this._iconObject as GLabel;
				if ( label != null )
					return label.icon;
				GButton btn = this._iconObject as GButton;
				return btn?.icon;
			}
			set
			{
				if ( this.icon == value )
					return;

				GLoader loader = this._iconObject as GLoader;
				if ( loader != null )
					loader.url = value;
				else
				{
					GLabel label = this._iconObject as GLabel;
					if ( label != null )
						label.icon = value;
					else
					{
						GButton btn = this._iconObject as GButton;
						if ( btn != null )
							btn.icon = value;
					}
				}

				if ( this.gearIcon.controller != null )
					this.gearIcon.UpdateState();
			}
		}

		/// <summary>
		/// Title of the label.
		/// </summary>
		public override string text
		{
			get => this._titleObject?.text;
			set
			{
				if ( this._titleObject == null || this._titleObject.text == value )
					return;

				this._titleObject.text = value;

				if ( this.gearText.controller != null )
					this.gearText.UpdateState();
			}
		}

		public Color textColor { get => this.titleColor; set => this.titleColor = value; }

		public Color strokeColor
		{
			get
			{
				GTextField textField = this._titleObject as GTextField;
				if ( textField != null )
					return textField.strokeColor;
				GLabel label = this._titleObject as GLabel;
				if ( label != null )
					return label.strokeColor;
				GButton btn = this._titleObject as GButton;
				if ( btn != null )
					return btn.strokeColor;
				return Color.black;
			}
			set
			{
				GTextField textField = this._titleObject as GTextField;
				if ( textField != null )
					textField.strokeColor = value;
				else
				{
					GLabel label = this._titleObject as GLabel;
					if ( label != null )
						label.strokeColor = value;
					else
					{
						GButton btn = this._titleObject as GButton;
						if ( btn != null )
							btn.strokeColor = value;
					}
				}
			}
		}

		/// <summary>
		/// If title is readOnly.
		/// </summary>
		public bool readOnly
		{
			get
			{
				if ( this._titleObject is GTextInput )
					return this._titleObject.asTextInput.readOnly;
				return true;
			}
			set
			{
				if ( this._titleObject is GTextInput )
					this._titleObject.asTextInput.readOnly = value;
			}
		}

		/// <summary>
		/// Title color of the label
		/// </summary>
		public Color titleColor
		{
			get
			{
				GTextField textField = this._titleObject as GTextField;
				if ( textField != null )
					return textField.color;
				GLabel label = this._titleObject as GLabel;
				if ( label != null )
					return label.titleColor;
				GButton btn = this._titleObject as GButton;
				if ( btn != null )
					return btn.titleColor;
				return Color.black;
			}
			set
			{
				GTextField textField = this._titleObject as GTextField;
				if ( textField != null )
					textField.color = value;
				else
				{
					GLabel label = this._titleObject as GLabel;
					if ( label != null )
						label.titleColor = value;
					else
					{
						GButton btn = this._titleObject as GButton;
						if ( btn != null )
							btn.titleColor = value;
					}
				}
			}
		}

		public int titleFontSize
		{
			get
			{
				GTextField textField = this._titleObject as GTextField;
				if ( textField != null )
					return textField.fontSize;
				GLabel label = this._titleObject as GLabel;
				if ( label != null )
					return label.titleFontSize;
				GButton btn = this._titleObject as GButton;
				if ( btn != null )
					return btn.titleFontSize;
				return 0;
			}
			set
			{
				GTextField textField = this._titleObject as GTextField;
				if ( textField != null )
					textField.fontSize = value;
				else
				{
					GLabel label = this._titleObject as GLabel;
					if ( label != null )
						label.titleFontSize = value;
					else
					{
						GButton btn = this._titleObject as GButton;
						if ( btn != null )
							btn.titleFontSize = value;
					}
				}
			}
		}

		protected override void ConstructFromXML( XML cxml )
		{
			base.ConstructFromXML( cxml );

			this._titleObject = this.GetChild( "title" );
			this._iconObject = this.GetChild( "icon" );
		}

		internal override void SetupAfterAdd( XML cxml )
		{
			base.SetupAfterAdd( cxml );

			XML xml = cxml.GetNode( "Label" );
			if ( xml == null )
			{
				this.text = string.Empty;
				this.icon = null;
				return;
			}

			this.text = xml.GetAttribute( "title" );
			this.icon = xml.GetAttribute( "icon" );
			string str = xml.GetAttribute( "titleColor" );
			if ( str != null )
				this.titleColor = ToolSet.ConvertFromHtmlColor( str );

			str = xml.GetAttribute( "titleFontSize" );
			if ( str != null )
				this.titleFontSize = int.Parse( str );

			GTextInput textInput = this._titleObject as GTextInput;
			if ( textInput != null )
			{
				str = xml.GetAttribute( "promptText" );
				if ( str != null )
					textInput.promptText = str;
			}
		}
	}
}
