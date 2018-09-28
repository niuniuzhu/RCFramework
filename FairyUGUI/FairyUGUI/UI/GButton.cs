using Core.Xml;
using FairyUGUI.Core;
using FairyUGUI.Event;
using FairyUGUI.Utils;
using UnityEngine;
using EventType = FairyUGUI.Event.EventType;

namespace FairyUGUI.UI
{
	public class GButton : GComponent
	{
		public EventListener onChanged { get; private set; }

		/// <summary>
		/// The button will be in down status in these pages.
		/// </summary>
		public PageOption pageOption { get; private set; }

		/// <summary>
		/// Play sound when button is clicked.
		/// </summary>
		public AudioClip sound;

		/// <summary>
		/// Volume of the click sound. (0-1)
		/// </summary>
		public float soundVolumeScale;

		/// <summary>
		/// For radio or checkbox. if false, the button will not change selected status on click. Default is true.
		/// 如果为true，对于单选和多选按钮，当玩家点击时，按钮会自动切换状态。设置为false，则不会。默认为true。
		/// </summary>
		internal bool changeStateOnClick;

		private GObject _titleObject;
		private GObject _iconObject;
		private Controller _relatedController;

		private ButtonMode _mode;

		private bool _selected;
		/// <summary>
		/// If the button is in selected status.
		/// </summary>
		public bool selected
		{
			get => this._selected;
			set
			{
				if ( this._mode == ButtonMode.Common )
					return;

				if ( this._selected != value )
				{
					this._selected = value;
					this.SetCurrentState();
					if ( this._selectedTitle != null && this._titleObject != null )
						this._titleObject.text = this._selected ? this._selectedTitle : this._text;
					if ( this._selectedIcon != null )
					{
						string str = this._selected ? this._selectedIcon : this._icon;
						GLoader loader = this._iconObject as GLoader;
						if ( loader != null )
							loader.url = str;
						else
						{
							GLabel label = this._iconObject as GLabel;
							if ( label != null )
								label.icon = str;
							else
							{
								GButton btn = this._iconObject as GButton;
								if ( btn != null )
									btn.icon = str;
							}
						}
					}
					if ( this._relatedController != null && this.parent != null )
					{
						if ( this._selected )
						{
							this._relatedController.selectedPageId = this.pageOption.id;
							if ( this._relatedController.autoRadioGroupDepth )
								this.parent.AdjustRadioGroupDepth( this, this._relatedController );
						}
						else if ( this._mode == ButtonMode.Check && this._relatedController.selectedPageId == this.pageOption.id )
							this._relatedController.oppositePageId = this.pageOption.id;
					}
				}
			}
		}

		private string _text;
		/// <summary>
		/// Title of the button
		/// </summary>
		public override string text
		{
			get => this._titleObject?.text ?? string.Empty;
			set
			{
				if ( this._titleObject == null || this._text == value )
					return;

				this._text = value;
				this._titleObject.text = ( this._selected && this._selectedTitle != null ) ? this._selectedTitle : this._text;

				if ( this.gearText.controller != null )
					this.gearText.UpdateState();
			}
		}

		private string _selectedTitle;
		/// <summary>
		/// Title value on selected status.
		/// </summary>
		public string selectedTitle
		{
			get => this._selectedTitle;
			set
			{
				this._selectedTitle = value;
				if ( this._titleObject != null )
					this._titleObject.text = ( this._selected && this._selectedTitle != null ) ? this._selectedTitle : this._text;
			}
		}

		private string _icon;
		public override string icon
		{
			get => this._icon;
			set
			{
				if ( this._icon == value )
					return;
				this._icon = value;
				value = ( this._selected && this._selectedIcon != null ) ? this._selectedIcon : this._icon;
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

		private string _selectedIcon;
		public string selectedIcon
		{
			get => this._selectedIcon;
			set
			{
				if ( this._selectedIcon == value )
					return;
				this._selectedIcon = value;
				value = ( this._selected && this._selectedIcon != null ) ? this._selectedIcon : this._icon;
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
			}
		}

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
		/// Title color.
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

		/// <summary>
		/// A controller is connected to this button, the activate page of this controller will change while the button status changed.
		/// 对应编辑器中的单选控制器。
		/// </summary>
		public Controller relatedController
		{
			get => this._relatedController;
			set
			{
				if ( value == this._relatedController )
					return;
				this._relatedController = value;
				this.pageOption.controller = value;
				this.pageOption.Clear();
			}
		}

		private Controller _buttonController;
		private int _downEffect;
		private float _downEffectValue;
		private bool _down;
		private bool _over;
		private Button _content;

		private const string UP = "up";
		private const string DOWN = "down";
		private const string OVER = "over";
		private const string SELECTED_OVER = "selectedOver";
		private const string DISABLED = "disabled";
		private const string SELECTED_DISABLED = "selectedDisabled";

		public GButton()
		{
			this.pageOption = new PageOption();

			this.sound = UIConfig.buttonSound;
			this.soundVolumeScale = UIConfig.buttonSoundVolumeScale;
			this.changeStateOnClick = true;
			this._downEffectValue = 0.8f;

			this.onChanged = new EventListener( this, EventType.Changed );
		}

		protected override void InternalDispose()
		{
			this.onRemovedFromStage.Remove( this.OnRemoveFromStage );

			this._content = null;

			base.InternalDispose();
		}

		protected override void CreateDisplayObject()
		{
			this.rootContainer = this._content = new Button( this );
			this.container = this.rootContainer;
			this.displayObject = this.rootContainer;
		}

		protected override void ConstructFromXML( XML cxml )
		{
			base.ConstructFromXML( cxml );

			XML xml = cxml.GetNode( "Button" );

			string str = xml.GetAttribute( "mode" );
			this._mode = str != null ? FieldTypes.ParseButtonMode( str ) : ButtonMode.Common;

			str = xml.GetAttribute( "sound" );
			if ( str != null )
				this.sound = UIPackage.GetItemAssetByURL( str ) as AudioClip;

			str = xml.GetAttribute( "volume" );
			if ( str != null )
				this.soundVolumeScale = float.Parse( str ) / 100f;

			str = xml.GetAttribute( "downEffect" );
			if ( str != null )
			{
				this._downEffect = str == "dark" ? 1 : ( str == "scale" ? 2 : 0 );
				this._downEffectValue = xml.GetAttributeFloat( "downEffectValue" );
			}

			this._buttonController = this.GetController( "button" );
			this._titleObject = this.GetChild( "title" );
			this._iconObject = this.GetChild( "icon" );

			if ( this._mode == ButtonMode.Common )
				this.SetState( UP );

			this.onRemovedFromStage.Add( this.OnRemoveFromStage );
		}

		internal override void SetupAfterAdd( XML cxml )
		{
			base.SetupAfterAdd( cxml );

			XML xml = cxml.GetNode( "Button" );
			if ( xml == null )
			{
				this.text = string.Empty;
				this.icon = null;
				return;
			}

			this.text = xml.GetAttribute( "title" );
			this.icon = xml.GetAttribute( "icon" );

			string str = xml.GetAttribute( "selectedTitle" );
			if ( str != null )
				this.selectedTitle = str;
			str = xml.GetAttribute( "selectedIcon" );
			if ( str != null )
				this.selectedIcon = str;

			str = xml.GetAttribute( "titleColor" );
			if ( str != null )
				this.titleColor = ToolSet.ConvertFromHtmlColor( str );

			str = xml.GetAttribute( "titleFontSize" );
			if ( str != null )
				this.titleFontSize = int.Parse( str );

			str = xml.GetAttribute( "controller" );
			if ( str != null )
				this._relatedController = this.parent.GetController( str );

			this.pageOption.id = xml.GetAttribute( "page" );

			this.selected = xml.GetAttributeBool( "checked" );

			str = xml.GetAttribute( "sound" );
			if ( str != null )
				this.sound = UIPackage.GetItemAssetByURL( str ) as AudioClip;

			str = xml.GetAttribute( "volume" );
			if ( str != null )
				this.soundVolumeScale = float.Parse( str ) / 100f;
		}

		private void SetCurrentState()
		{
			if ( this.grayed && this._buttonController != null && this._buttonController.HasPage( DISABLED ) )
				this.SetState( this._selected ? SELECTED_DISABLED : DISABLED );
			else
			{
				if ( this._selected )
					this.SetState( this._over ? SELECTED_OVER : DOWN );
				else
					this.SetState( this._over ? OVER : UP );
			}
		}


		private void SetState( string val )
		{
			if ( this._buttonController != null )
				this._buttonController.selectedPage = val;

			if ( this._downEffect == 1 )
			{
				int cnt = this.numChildren;
				if ( val == DOWN || val == SELECTED_OVER || val == SELECTED_DISABLED )
				{
					Color c = new Color( this._downEffectValue, this._downEffectValue, this._downEffectValue );
					for ( int i = 0; i < cnt; i++ )
					{
						GObject obj = this.GetChildAt( i );
						obj.color = c;
					}
				}
				else
				{
					for ( int i = 0; i < cnt; i++ )
					{
						GObject obj = this.GetChildAt( i );
						obj.color = Color.white;
					}
				}
			}
			else if ( this._downEffect == 2 )
			{
				if ( val == DOWN || val == SELECTED_OVER || val == SELECTED_DISABLED )
				{
					this.pivot = new Vector2( 0.5f, 0.5f );
					this.scale = new Vector2( this._downEffectValue, this._downEffectValue );
				}
				else
				{
					this.pivot = new Vector2( 0, 1 );
					this.scale = Vector2.one;
				}
			}
		}

		protected override void OnPointerClick( BaseEventData eventData )
		{
			if ( this.sound != null )
				Stage.inst.PlayOneShotSound( this.sound, this.soundVolumeScale );

			if ( !this.changeStateOnClick )
				return;

			if ( this._mode == ButtonMode.Check )
			{
				this.selected = !this._selected;
				this.onChanged.Call();
			}
			else if ( this._mode == ButtonMode.Radio )
			{
				if ( !this._selected )
				{
					this.selected = true;
					this.onChanged.Call();
				}
			}
		}

		protected override void OnPointerEnter( BaseEventData eventData )
		{
			if ( this._buttonController == null || !this._buttonController.HasPage( OVER ) )
				return;

			this._over = true;
			if ( this._down )
				return;

			if ( this.grayed && this._buttonController.HasPage( DISABLED ) )
				return;

			this.SetState( this._selected ? SELECTED_OVER : OVER );
		}

		protected override void OnPointerExit( BaseEventData eventData )
		{
			if ( this._buttonController == null || !this._buttonController.HasPage( OVER ) )
				return;

			this._over = false;
			if ( this._down )
				return;

			if ( this.grayed && this._buttonController.HasPage( DISABLED ) )
				return;

			this.SetState( this._selected ? DOWN : UP );
		}

		protected override void OnPointerDown( BaseEventData eventData )
		{
			this._down = true;

			if ( this._mode == ButtonMode.Common )
			{
				if ( this.grayed && this._buttonController != null && this._buttonController.HasPage( DISABLED ) )
					this.SetState( SELECTED_DISABLED );
				else
					this.SetState( DOWN );
			}

			//if ( linkedPopup != null )
			//{
			//	if ( linkedPopup is Window )
			//		( ( Window )linkedPopup ).ToggleStatus();
			//	else
			//		this.root.TogglePopup( linkedPopup, this );
			//}
		}

		protected override void OnPointerUp( BaseEventData eventData )
		{
			if ( this._down )
			{
				this._down = false;

				if ( this._mode == ButtonMode.Common )
				{
					if ( this.grayed && this._buttonController != null && this._buttonController.HasPage( DISABLED ) )
						this.SetState( DISABLED );
					else if ( this._over )
						this.SetState( OVER );
					else
						this.SetState( UP );
				}
				else
				{
					if ( !this._over
						&& this._buttonController != null
						&& ( this._buttonController.selectedPage == OVER || this._buttonController.selectedPage == SELECTED_OVER ) )
					{
						this.SetCurrentState();
					}
				}
			}
		}

		private void OnRemoveFromStage( EventContext context )
		{
			if ( this._over )
				this.OnPointerExit( context.eventData );
		}

		internal override void HandleControllerChanged( Controller c )
		{
			base.HandleControllerChanged( c );

			if ( this._relatedController == c )
				this.selected = this.pageOption.id == c.selectedPageId;
		}
	}
}