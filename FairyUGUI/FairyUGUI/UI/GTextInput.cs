using Core.Xml;
using FairyUGUI.Core;
using FairyUGUI.Event;
using UnityEngine;
using EventType = FairyUGUI.Event.EventType;

namespace FairyUGUI.UI
{
	public class GTextInput : GTextField
	{
		private TextInput _input;

		public override bool touchable
		{
			get => this._touchable;
			set
			{
				if ( this._touchable == value )
					return;
				this._touchable = value;

				this.HandleTouchableChanged();
			}
		}

		public bool readOnly
		{
			get => this._input.readOnly;
			set => this._input.readOnly = value;
		}

		public override string text
		{
			get => this._input.text;
			set => this._input.text = value;
		}

		public string promptText
		{
			get => this._input.promptText;
			set => this._input.promptText = value;
		}

		public override bool supportRichText
		{
			get => base.supportRichText;
			set => base.supportRichText = false;
		}

		public bool enableSubmit
		{
			get => this._input.enableSubmit;
			set => this._input.enableSubmit = value;
		}

		public bool singleLine
		{
			get => !this._input.multiLine;
			set => this._input.lineType = value ? TextInput.LineType.SingleLine : TextInput.LineType.MultiLineNewline;
		}

		public TextInput.CharacterValidation characterValidation
		{
			get => this._input.characterValidation;
			set => this._input.characterValidation = value;
		}

		public int characterLimit
		{
			get => this._input.characterLimit;
			set => this._input.characterLimit = value;
		}

		public Color caretColor
		{
			get => this._input.caretColor;
			set => this._input.caretColor = value;
		}

		public Color selectionColor
		{
			get => this._input.selectionColor;
			set => this._input.selectionColor = value;
		}

		public float caretBlinkRate
		{
			get => this._input.caretBlinkRate;
			set => this._input.caretBlinkRate = value;
		}

		public int caretWidth
		{
			get => this._input.caretWidth;
			set => this._input.caretWidth = value;
		}

		public bool shouldHideMobileInput
		{
			get => this._input.shouldHideMobileInput;
			set => this._input.shouldHideMobileInput = value;
		}

		public TextInput.InputType inputType
		{
			get => this._input.inputType;
			set => this._input.inputType = value;
		}

		public TextInput.ContentType contentType
		{
			get => this._input.contentType;
			set => this._input.contentType = value;
		}

		public TouchScreenKeyboardType keyboardType
		{
			get => this._input.keyboardType;
			set => this._input.keyboardType = value;
		}

		public char asteriskChar
		{
			get => this._input.asteriskChar;
			set => this._input.asteriskChar = value;
		}

		public EventListener onChanged { get; private set; }

		public EventListener onEndEdit { get; private set; }

		public EventListener onSubmit { get; private set; }

		public GTextInput()
		{
			this.onChanged = new EventListener( this, EventType.Changed );
			this.onEndEdit = new EventListener( this, EventType.EndEdit );
			this.onSubmit = new EventListener( this, EventType.Submit );
		}

		protected override void CreateDisplayObject()
		{
			this.displayObject = this._content = this._input = new TextInput( this );
		}

		public void Append( string input )
		{
			this._input.Append( input );
		}

		public void MoveTextEnd( bool shift )
		{
			this._input.MoveTextEnd( shift );
		}

		public void MoveTextStart( bool shift )
		{
			this._input.MoveTextStart( shift );
		}

		public void ForceLabelUpdate()
		{
			this._input.ForceLabelUpdate();
		}

		public void ActivateInputField()
		{
			this._input.ActivateInputField();
		}

		public void DeactivateInputField()
		{
			this._input.DeactivateInputField();
		}

		internal override void SetupAfterAdd( XML xml )
		{
			base.SetupAfterAdd( xml );

			this.promptText = xml.GetAttribute( "prompt" );
			this.contentType = xml.GetAttributeBool( "password" ) ? TextInput.ContentType.Password : TextInput.ContentType.Standard;
			this.singleLine = xml.GetAttributeBool( "singleLine" );
			this.caretColor = this.color;
		}

		protected override void InternalDispose()
		{
			this._input = null;

			base.InternalDispose();
		}
	}
}