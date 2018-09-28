using FairyUGUI.Event;
using FairyUGUI.UI;
using Game.Task;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using EventType = FairyUGUI.Event.EventType;
using Logger = Core.Misc.Logger;
using Object = UnityEngine.Object;

namespace FairyUGUI.Core
{
	public sealed class TextInput : TextField, ICanvasElement, ILayoutElement
	{
		public delegate char OnValidateInput( string text, int charIndex, char addedChar );

		public enum CharacterValidation
		{
			None = 0,
			Integer = 1,
			Decimal = 2,
			Alphanumeric = 3,
			Name = 4,
			EmailAddress = 5
		}

		public enum ContentType
		{
			Standard = 0,
			Autocorrected = 1,
			IntegerNumber = 2,
			DecimalNumber = 3,
			Alphanumeric = 4,
			Name = 5,
			EmailAddress = 6,
			Password = 7,
			Pin = 8,
			Custom = 9
		}

		public enum InputType
		{
			Standard = 0,
			AutoCorrect = 1,
			Password = 2
		}

		public enum LineType
		{
			SingleLine = 0,
			MultiLineSubmit = 1,
			MultiLineNewline = 2
		}

		private const string EMAIL_SPECIAL_CHARACTERS = "!#$%&'*+-/=?^_`{|}~";
		private static readonly char[] SEPARATORS = { ' ', '.', ',', '\t', '\r', '\n' };
		private readonly UnityEngine.Event _processingEvent = new UnityEngine.Event();
		private bool _allowInput;
		private char _asteriskChar = '*';
		private CanvasRenderer _cachedInputRenderer;
		private float _caretBlinkRate = 1.35f;
		private Color _caretColor = new Color( 50f / 255f, 50f / 255f, 50f / 255f, 1f );
		private int _caretPosition;
		private RectTransform _caretRectTrans;
		private int _caretSelectPosition;
		private bool _caretVisible;
		private int _caretWidth = 1;
		private int _characterLimit;
		private CharacterValidation _characterValidation;
		private ContentType _contentType = ContentType.Standard;
		private UIVertex[] _cursorVerts;
		private bool _dragPositionOutOfBounds;
		private int _drawEnd;
		private int _drawStart;
		private TextGenerator _inputTextCache;
		private InputType _inputType = InputType.Standard;
		private TouchScreenKeyboard _keyboard;
		private TouchScreenKeyboardType _keyboardType;
		private LineType _lineType;
		private Mesh _mesh;
		private string _originalText;
		private Text _placeholder;
		private PointerEventData _pointerDragEventData;
		private bool _preventFontCallback;
		private string _promptText;
		private Color _selectionColor = new Color( 168f / 255f, 206f / 255f, 255f / 255f, 192f / 255f );
		private bool _shouldHideMobileInput;
		private string _text = string.Empty;
		private bool _updateDrag;
		private bool _wasCanceled;
		internal bool enableSubmit;
		public OnValidateInput onValidateInput;
		public bool readOnly;
		private GameObject _inputCaret;
		private float _blinkStartTime;

		internal override bool supportRichText
		{
			get => this._supportRichText;
			set
			{
				if ( this._supportRichText == value )
					return;
				this._supportRichText = value;
				Logger.Warn( "TextInput do not support rich text" );
			}
		}

		internal override Color color
		{
			set
			{
				base.color = value;
				if ( this._placeholder != null )
				{
					Color c = this.color;
					c.a = 0.5f;
					this._placeholder.color = c;
				}
			}
		}

		internal override bool ubbEnabled
		{
			get => this._ubbEnabled;
			set
			{
				if ( this._ubbEnabled == value )
					return;
				this._ubbEnabled = value;
				Logger.Warn( "TextInput do not support ubb format" );
			}
		}

		internal int caretWidth
		{
			get => this._caretWidth;
			set
			{
				if ( this._caretWidth == value )
					return;
				this._caretWidth = value;

				this.MarkGeometryAsDirty();
			}
		}

		internal float caretBlinkRate
		{
			get => this._caretBlinkRate;
			set
			{
				if ( this._caretBlinkRate == value )
					return;
				this._caretBlinkRate = value;
				if ( this._allowInput )
					this.SetCaretActive();
			}
		}

		internal Color selectionColor
		{
			get => this._selectionColor;
			set
			{
				if ( this._selectionColor != value )
				{
					this._selectionColor = value;
					this.MarkGeometryAsDirty();
				}
			}
		}

		internal Color caretColor
		{
			get => this._caretColor;
			set
			{
				if ( this._caretColor != value )
				{
					this._caretColor = value;
					this.MarkGeometryAsDirty();
				}
			}
		}

		private int caretPositionInternal
		{
			get => this._caretPosition + Input.compositionString.Length;
			set
			{
				this._caretPosition = value;
				this.ClampPos( ref this._caretPosition );
			}
		}

		private int caretSelectPositionInternal
		{
			get => this._caretSelectPosition + Input.compositionString.Length;
			set
			{
				this._caretSelectPosition = value;
				this.ClampPos( ref this._caretSelectPosition );
			}
		}

		private bool hasSelection => this.caretPositionInternal != this.caretSelectPositionInternal;

		private TextGenerator cachedInputTextGenerator => this._inputTextCache ?? ( this._inputTextCache = new TextGenerator() );

		/// <summary>
		/// Get: Returns the focus position as thats the position that moves around even during selection.
		/// Set: Set both the anchor and focus position such that a selection doesn't happen
		/// </summary>

		public int caretPosition
		{
			get => this._caretSelectPosition + Input.compositionString.Length;
			set
			{
				this.selectionAnchorPosition = value;
				this.selectionFocusPosition = value;
			}
		}

		/// <summary>
		/// Get: Returns the fixed position of selection
		/// Set: If Input.compositionString is 0 set the fixed position
		/// </summary>

		public int selectionAnchorPosition
		{
			get => this._caretPosition + Input.compositionString.Length;
			set
			{
				if ( Input.compositionString.Length != 0 )
					return;

				this._caretPosition = value;
				this.ClampPos( ref this._caretPosition );
			}
		}

		/// <summary>
		/// Get: Returns the variable position of selection
		/// Set: If Input.compositionString is 0 set the variable position
		/// </summary>

		public int selectionFocusPosition
		{
			get => this._caretSelectPosition + Input.compositionString.Length;
			set
			{
				if ( Input.compositionString.Length != 0 )
					return;

				this._caretSelectPosition = value;
				this.ClampPos( ref this._caretSelectPosition );
			}
		}

		internal string promptText
		{
			get => this._promptText;
			set
			{
				if ( this._promptText == value )
					return;
				this._promptText = value;
				if ( string.IsNullOrEmpty( this._promptText ) )
					this.DestroyPlaceHolder();
				else
				{
					this.CreatePlaceHolder();
					this._placeholder.text = this._promptText;
				}
			}
		}

		internal bool shouldHideMobileInput
		{
			get
			{
				switch ( Application.platform )
				{
					case RuntimePlatform.Android:
					case RuntimePlatform.IPhonePlayer:
					case RuntimePlatform.TizenPlayer:
					case RuntimePlatform.tvOS:
						return this._shouldHideMobileInput;
				}

				return true;
			}
			set => this._shouldHideMobileInput = value;
		}

		bool shouldActivateOnSelect => Application.platform != RuntimePlatform.tvOS;

		internal InputType inputType
		{
			get => this._inputType;
			set
			{
				if ( this._inputType == value )
					return;
				this._inputType = value;
				this.contentType = ContentType.Custom;
			}
		}

		internal ContentType contentType
		{
			get => this._contentType;
			set
			{
				if ( this._contentType == value )
					return;
				this._contentType = value;
				this.EnforceContentType();
			}
		}

		internal TouchScreenKeyboardType keyboardType
		{
			get => this._keyboardType;
			set
			{
				if ( this._keyboardType == value )
					return;
				this._keyboardType = value;
				this.contentType = ContentType.Custom;
			}
		}

		public CharacterValidation characterValidation
		{
			get => this._characterValidation;
			set
			{
				if ( this._characterValidation == value )
					return;
				this._characterValidation = value;
				this.contentType = ContentType.Custom;
			}
		}

		internal int characterLimit
		{
			get => this._characterLimit;
			set
			{
				value = Mathf.Max( 0, value );
				if ( this._characterLimit == value )
					return;
				this._characterLimit = value;
				this.UpdateLabel();
			}
		}

		internal LineType lineType
		{
			get => this._lineType;
			set
			{
				if ( this._lineType == value )
					return;
				this._lineType = value;
				this.SetToCustomIfContentTypeIsNot( ContentType.Standard, ContentType.Autocorrected );
				this.EnforceTextHOverflow();
			}
		}

		internal bool multiLine => this._lineType == LineType.MultiLineNewline || this.lineType == LineType.MultiLineSubmit;

		internal char asteriskChar
		{
			get => this._asteriskChar;
			set
			{
				if ( this._asteriskChar == value )
					return;
				this._asteriskChar = value;
				this.UpdateLabel();
			}
		}

		internal override string text
		{
			get => this._text;
			set
			{
				if ( this.text == value )
					return;

				if ( value == null )
					value = string.Empty;

				value = value.Replace( "\0", string.Empty );
				if ( this._lineType == LineType.SingleLine )
					value = value.Replace( "\n", "" ).Replace( "\t", "" );

				// If we have an input validator, validate the input and apply the character limit at the same time.
				if ( this.onValidateInput != null || this.characterValidation != CharacterValidation.None )
				{
					this._text = string.Empty;
					OnValidateInput validatorMethod = this.onValidateInput ?? this.Validate;
					this._caretPosition = this._caretSelectPosition = value.Length;
					int charactersToCheck = this.characterLimit > 0 ? Mathf.Min( this.characterLimit, value.Length ) : value.Length;
					for ( int i = 0; i < charactersToCheck; ++i )
					{
						char c = validatorMethod( this._text, this._text.Length, value[i] );
						if ( c != 0 )
							this._text += c;
					}
				}
				else
					this._text = this.characterLimit > 0 && value.Length > this.characterLimit
									 ? value.Substring( 0, this.characterLimit )
									 : value;

				if ( this._keyboard != null )
					this._keyboard.text = this._text;

				if ( this._caretPosition > this._text.Length )
					this._caretPosition = this._caretSelectPosition = this._text.Length;

				else if ( this._caretSelectPosition > this._text.Length )
					this._caretSelectPosition = this._text.Length;

				this.SendOnValueChangedAndUpdateLabel();
			}
		}

		private Mesh mesh => this._mesh ?? ( this._mesh = new Mesh() );

		private static string clipboard
		{
			get => GUIUtility.systemCopyBuffer;
			set => GUIUtility.systemCopyBuffer = value;
		}

		internal EventListener onChanged { get; private set; }
		internal EventListener onEndEdit { get; private set; }
		internal EventListener onSubmit { get; private set; }

		public Transform transform => this.gameObject.transform;

		public TextInput( GObject owner )
			: base( owner )
		{
			this.onChanged = new EventListener( this, EventType.Changed );
			this.onEndEdit = new EventListener( this, EventType.EndEdit );
			this.onSubmit = new EventListener( this, EventType.Submit );

			this.RegisterEventTriggerType( EventTriggerType.PointerClick );
			this.RegisterEventTriggerType( EventTriggerType.PointerDown );
			this.RegisterEventTriggerType( EventTriggerType.PointerUp );
			this.RegisterEventTriggerType( EventTriggerType.PointerEnter );
			this.RegisterEventTriggerType( EventTriggerType.PointerExit );
			this.RegisterEventTriggerType( EventTriggerType.BeginDrag );
			this.RegisterEventTriggerType( EventTriggerType.Drag );
			this.RegisterEventTriggerType( EventTriggerType.EndDrag );
			this.RegisterEventTriggerType( EventTriggerType.Select );
			this.RegisterEventTriggerType( EventTriggerType.Deselect );
			this.RegisterEventTriggerType( EventTriggerType.UpdateSelected );
		}

		public void LayoutComplete()
		{
		}

		public void GraphicUpdateComplete()
		{
		}

		public void Rebuild( CanvasUpdate update )
		{
			switch ( update )
			{
				case CanvasUpdate.LatePreRender:
					this.UpdateGeometry();
					break;
			}
		}

		public bool IsDestroyed()
		{
			return this.disposed;
		}

		protected override void OnGameObjectCreated()
		{
			base.OnGameObjectCreated();

			this.EnforceTextHOverflow();

			this._caretColor = this._nText.color;

			// If we have a cached renderer then we had OnDisable called so just restore the material.
			this._cachedInputRenderer?.SetMaterial( Graphic.defaultGraphicMaterial, Texture2D.whiteTexture );

			this._nText.RegisterDirtyVerticesCallback( this.MarkGeometryAsDirty );
			this._nText.RegisterDirtyVerticesCallback( this.UpdateLabel );
			this._nText.RegisterDirtyMaterialCallback( this.UpdateCaretMaterial );
			this.UpdateLabel();
		}

		protected override void InternalDispose()
		{
			TaskManager.instance.UnregisterTimer( this.OnCaretBlink );
			this.DeactivateInputField();
			CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild( this );
			this._nText.UnregisterDirtyVerticesCallback( this.MarkGeometryAsDirty );
			this._nText.UnregisterDirtyVerticesCallback( this.UpdateLabel );
			this._nText.UnregisterDirtyMaterialCallback( this.UpdateCaretMaterial );
			if ( this._cachedInputRenderer != null )
			{
				this._cachedInputRenderer.Clear();
				this._cachedInputRenderer = null;
			}
			if ( this._mesh != null )
			{
				Object.DestroyImmediate( this._mesh );
				this._mesh = null;
			}
			this._inputTextCache = null;
			this._keyboard = null;
			this._text = string.Empty;
			this._originalText = string.Empty;
			this._caretRectTrans = null;
			this._cursorVerts = null;
			this.onValidateInput = null;
			this.DestroyPlaceHolder();
			if ( this._inputCaret != null )
			{
				Object.DestroyImmediate( this._inputCaret );
				this._inputCaret = null;
			}

			base.InternalDispose();
		}

		protected override void OnPointerDown( BaseEventData eventData )
		{
			bool hadFocusBefore = this._allowInput;
			if ( !this.InPlaceEditing() )
			{
				if ( this._keyboard == null || !this._keyboard.active )
				{
					this.OnSelect( eventData );
					return;
				}
			}

			// Only set caret position if we didn't just get focus now.
			// Otherwise it will overwrite the select all on focus.
			if ( hadFocusBefore )
			{
				PointerEventData e = ( PointerEventData )eventData;
				Vector2 localMousePos;
				RectTransformUtility.ScreenPointToLocalPointInRectangle( this._nText.rectTransform, e.position,
																		 Stage.inst.eventCamera, out localMousePos );

				this.caretSelectPositionInternal =
					this.caretPositionInternal = this.GetCharacterIndexFromPosition( localMousePos ) + this._drawStart;
			}

			this.UpdateLabel();

			eventData.StopPropagation();
		}

		protected override void OnBeginDrag( BaseEventData eventData )
		{
			this._updateDrag = true;
		}

		protected override void OnDrag( BaseEventData eventData )
		{
			PointerEventData e = ( PointerEventData )eventData;
			Vector2 localMousePos;
			RectTransformUtility.ScreenPointToLocalPointInRectangle( this._nText.rectTransform, e.position,
																	 Stage.inst.eventCamera, out localMousePos );
			this.caretSelectPositionInternal = this.GetCharacterIndexFromPosition( localMousePos ) + this._drawStart;
			this.MarkGeometryAsDirty();

			this._dragPositionOutOfBounds =
				!RectTransformUtility.RectangleContainsScreenPoint( this._nText.rectTransform, e.position, Stage.inst.eventCamera );
			this._pointerDragEventData = e;

			eventData.StopPropagation();
		}

		protected override void OnEndDrag( BaseEventData eventData )
		{
			this._pointerDragEventData = null;
			this._updateDrag = false;
		}

		protected override void OnSelect( BaseEventData eventData )
		{
			if ( this.shouldActivateOnSelect )
				this.ActivateInputField();
		}

		protected override void OnDeselect( BaseEventData eventData )
		{
			this.DeactivateInputField();
		}

		protected override void OnUpdateSelected( BaseEventData eventData )
		{
			if ( !this._allowInput )
				return;

			bool consumedEvent = false;
			while ( UnityEngine.Event.PopEvent( this._processingEvent ) )
			{
				if ( this._processingEvent.rawType == UnityEngine.EventType.KeyDown )
				{
					consumedEvent = true;
					EditState shouldContinue = this.KeyPressed( this._processingEvent );
					if ( shouldContinue == EditState.Finish )
					{
						this.DeactivateInputField();
						break;
					}
				}

				switch ( this._processingEvent.type )
				{
					case UnityEngine.EventType.ValidateCommand:
					case UnityEngine.EventType.ExecuteCommand:
						switch ( this._processingEvent.commandName )
						{
							case "SelectAll":
								this.SelectAll();
								consumedEvent = true;
								break;
						}
						break;
				}
			}

			if ( consumedEvent )
				this.UpdateLabel();
		}

		private EditState KeyPressed( UnityEngine.Event evt )
		{
			EventModifiers currentEventModifiers = evt.modifiers;
			bool ctrl = SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX ? ( currentEventModifiers & EventModifiers.Command ) != 0 : ( currentEventModifiers & EventModifiers.Control ) != 0;
			bool shift = ( currentEventModifiers & EventModifiers.Shift ) != 0;
			bool alt = ( currentEventModifiers & EventModifiers.Alt ) != 0;
			bool ctrlOnly = ctrl && !alt && !shift;

			switch ( evt.keyCode )
			{
				case KeyCode.Backspace:
					{
						this.Backspace();
						return EditState.Continue;
					}

				case KeyCode.Delete:
					{
						this.ForwardSpace();
						return EditState.Continue;
					}

				case KeyCode.Home:
					{
						this.MoveTextStart( shift );
						return EditState.Continue;
					}

				case KeyCode.End:
					{
						this.MoveTextEnd( shift );
						return EditState.Continue;
					}

				case KeyCode.A:
					{
						if ( ctrlOnly )
						{
							this.SelectAll();
							return EditState.Continue;
						}
						break;
					}

				case KeyCode.C:
					{
						if ( ctrlOnly )
						{
							clipboard = this.inputType != InputType.Password ? this.GetSelectedString() : "";
							return EditState.Continue;
						}
						break;
					}

				case KeyCode.V:
					{
						if ( ctrlOnly )
						{
							this.Append( clipboard );
							return EditState.Continue;
						}
						break;
					}

				case KeyCode.X:
					{
						if ( ctrlOnly )
						{
							clipboard = this.inputType != InputType.Password ? this.GetSelectedString() : "";
							this.Delete();
							this.SendOnValueChangedAndUpdateLabel();
							return EditState.Continue;
						}
						break;
					}

				case KeyCode.LeftArrow:
					{
						this.MoveLeft( shift, ctrl );
						return EditState.Continue;
					}

				case KeyCode.RightArrow:
					{
						this.MoveRight( shift, ctrl );
						return EditState.Continue;
					}

				case KeyCode.UpArrow:
					{
						this.MoveUp( shift );
						return EditState.Continue;
					}

				case KeyCode.DownArrow:
					{
						this.MoveDown( shift );
						return EditState.Continue;
					}

				case KeyCode.Return:
				case KeyCode.KeypadEnter:
					{
						if ( this.lineType != LineType.MultiLineNewline )
						{
							return this.SendOnSubmitOrFinish();
						}
						break;
					}

				case KeyCode.Escape:
					{
						this._wasCanceled = true;
						return EditState.Finish;
					}
			}

			char c = evt.character;
			// Don't allow return chars or tabulator key to be entered into single line fields.
			if ( !this.multiLine && ( c == '\t' || c == '\r' || c == 10 ) )
				return EditState.Continue;

			// Convert carriage return and end-of-text characters to newline.
			if ( c == '\r' || c == 3 )
				c = '\n';

			if ( this.IsValidChar( c ) )
			{
				this.Append( c );
			}

			if ( c == 0 )
			{
				if ( Input.compositionString.Length > 0 )
				{
					this.UpdateLabel();
				}
			}
			return EditState.Continue;
		}

		public string GetSelectedString()
		{
			if ( !this.hasSelection )
				return string.Empty;

			int startPos = this.caretPositionInternal;
			int endPos = this.caretSelectPositionInternal;

			// Ensure pos is always less then selPos to make the code simpler
			if ( startPos > endPos )
			{
				int temp = startPos;
				startPos = endPos;
				endPos = temp;
			}

			return this.text.Substring( startPos, endPos - startPos );
		}

		private int FindtNextWordBegin()
		{
			if ( this.caretSelectPositionInternal + 1 >= this.text.Length )
				return this.text.Length;

			int spaceLoc = this.text.IndexOfAny( SEPARATORS, this.caretSelectPositionInternal + 1 );

			if ( spaceLoc == -1 )
				spaceLoc = this.text.Length;
			else
				spaceLoc++;

			return spaceLoc;
		}

		private void MoveRight( bool shift, bool ctrl )
		{
			if ( this.hasSelection && !shift )
			{
				// By convention, if we have a selection and move right without holding shift,
				// we just place the cursor at the end.
				this.caretPositionInternal =
					this.caretSelectPositionInternal = Mathf.Max( this.caretPositionInternal, this.caretSelectPositionInternal );
				return;
			}

			int pos;
			if ( ctrl )
				pos = this.FindtNextWordBegin();
			else
				pos = this.caretSelectPositionInternal + 1;

			if ( shift )
				this.caretSelectPositionInternal = pos;
			else
				this.caretSelectPositionInternal = this.caretPositionInternal = pos;
		}

		private int FindtPrevWordBegin()
		{
			if ( this.caretSelectPositionInternal - 2 < 0 )
				return 0;

			int spaceLoc = this.text.LastIndexOfAny( SEPARATORS, this.caretSelectPositionInternal - 2 );

			if ( spaceLoc == -1 )
				spaceLoc = 0;
			else
				spaceLoc++;

			return spaceLoc;
		}

		private void MoveLeft( bool shift, bool ctrl )
		{
			if ( this.hasSelection && !shift )
			{
				// By convention, if we have a selection and move left without holding shift,
				// we just place the cursor at the start.
				this.caretPositionInternal =
					this.caretSelectPositionInternal = Mathf.Min( this.caretPositionInternal, this.caretSelectPositionInternal );
				return;
			}

			int pos;
			if ( ctrl )
				pos = this.FindtPrevWordBegin();
			else
				pos = this.caretSelectPositionInternal - 1;

			if ( shift )
				this.caretSelectPositionInternal = pos;
			else
				this.caretSelectPositionInternal = this.caretPositionInternal = pos;
		}

		private void MoveDown( bool shift, bool goToLastChar = true )
		{
			if ( this.hasSelection && !shift )
			{
				// If we have a selection and press down without shift,
				// set caret position to end of selection before we move it down.
				this.caretPositionInternal =
					this.caretSelectPositionInternal = Mathf.Max( this.caretPositionInternal, this.caretSelectPositionInternal );
			}

			int pos = this.multiLine
						  ? this.LineDownCharacterPosition( this.caretSelectPositionInternal, goToLastChar )
						  : this.text.Length;

			if ( shift )
				this.caretSelectPositionInternal = pos;
			else
				this.caretPositionInternal = this.caretSelectPositionInternal = pos;
		}

		private void MoveUp( bool shift, bool goToFirstChar = true )
		{
			if ( this.hasSelection && !shift )
			{
				// If we have a selection and press up without shift,
				// set caret position to start of selection before we move it up.
				this.caretPositionInternal =
					this.caretSelectPositionInternal = Mathf.Min( this.caretPositionInternal, this.caretSelectPositionInternal );
			}

			int pos = this.multiLine ? this.LineUpCharacterPosition( this.caretSelectPositionInternal, goToFirstChar ) : 0;

			if ( shift )
				this.caretSelectPositionInternal = pos;
			else
				this.caretSelectPositionInternal = this.caretPositionInternal = pos;
		}

		private void Delete()
		{
			if ( this.readOnly )
				return;

			if ( this.caretPositionInternal == this.caretSelectPositionInternal )
				return;

			if ( this.caretPositionInternal < this.caretSelectPositionInternal )
			{
				this._text = this.text.Substring( 0, this.caretPositionInternal ) +
							 this.text.Substring( this.caretSelectPositionInternal,
												  this.text.Length - this.caretSelectPositionInternal );
				this.caretSelectPositionInternal = this.caretPositionInternal;
			}
			else
			{
				this._text = this.text.Substring( 0, this.caretSelectPositionInternal ) +
							 this.text.Substring( this.caretPositionInternal, this.text.Length - this.caretPositionInternal );
				this.caretPositionInternal = this.caretSelectPositionInternal;
			}
		}

		private void ForwardSpace()
		{
			if ( this.readOnly )
				return;

			if ( this.hasSelection )
			{
				this.Delete();
				this.SendOnValueChangedAndUpdateLabel();
			}
			else
			{
				if ( this.caretPositionInternal < this.text.Length )
				{
					this._text = this.text.Remove( this.caretPositionInternal, 1 );
					this.SendOnValueChangedAndUpdateLabel();
				}
			}
		}

		private void Backspace()
		{
			if ( this.readOnly )
				return;

			if ( this.hasSelection )
			{
				this.Delete();
				this.SendOnValueChangedAndUpdateLabel();
			}
			else
			{
				if ( this.caretPositionInternal > 0 )
				{
					this._text = this.text.Remove( this.caretPositionInternal - 1, 1 );
					this.caretSelectPositionInternal = this.caretPositionInternal = this.caretPositionInternal - 1;
					this.SendOnValueChangedAndUpdateLabel();
				}
			}
		}

		// Insert the character and update the label.
		private void Insert( char c )
		{
			if ( this.readOnly )
				return;

			string replaceString = c.ToString( CultureInfo.InvariantCulture );
			this.Delete();

			// Can't go past the character limit
			if ( this.characterLimit > 0 && this.text.Length >= this.characterLimit )
				return;

			this._text = this.text.Insert( this._caretPosition, replaceString );
			this.caretSelectPositionInternal = this.caretPositionInternal += replaceString.Length;

			this.SendOnValueChanged();
		}

		internal void MoveTextEnd( bool shift )
		{
			int pos = this.text.Length;

			if ( shift )
			{
				this.caretSelectPositionInternal = pos;
			}
			else
			{
				this.caretPositionInternal = pos;
				this.caretSelectPositionInternal = this.caretPositionInternal;
			}
			this.UpdateLabel();
		}

		internal void MoveTextStart( bool shift )
		{
			const int pos = 0;

			if ( shift )
			{
				this.caretSelectPositionInternal = pos;
			}
			else
			{
				this.caretPositionInternal = pos;
				this.caretSelectPositionInternal = this.caretPositionInternal;
			}

			this.UpdateLabel();
		}

		internal void Append( string input )
		{
			if ( this.readOnly )
				return;

			if ( !this.InPlaceEditing() )
				return;

			for ( int i = 0, imax = input.Length; i < imax; ++i )
			{
				char c = input[i];

				if ( c >= ' ' || c == '\t' || c == '\r' || c == 10 || c == '\n' )
				{
					this.Append( c );
				}
			}
		}

		private const int k_MaxTextLength = UInt16.MaxValue / 4 - 1;
		private void Append( char input )
		{
			if ( this.readOnly || this._text.Length >= k_MaxTextLength )
				return;

			if ( !this.InPlaceEditing() )
				return;

			// If we have an input validator, validate the input first
			int insertionPoint = Mathf.Min( this.selectionFocusPosition, this.selectionAnchorPosition );
			if ( this.onValidateInput != null )
				input = this.onValidateInput( this.text, insertionPoint, input );
			else if ( this.characterValidation != CharacterValidation.None )
				input = this.Validate( this.text, insertionPoint, input );

			// If the input is invalid, skip it
			if ( input == 0 )
				return;

			// Append the character and update the label
			this.Insert( input );
		}

		private int DetermineCharacterLine( int charPos, TextGenerator generator )
		{
			for ( int i = 0; i < generator.lineCount - 1; ++i )
			{
				if ( generator.lines[i + 1].startCharIdx > charPos )
					return i;
			}
			return generator.lineCount - 1;
		}

		private int LineUpCharacterPosition( int originalPos, bool goToFirstChar )
		{
			if ( originalPos >= this.cachedInputTextGenerator.characters.Count )
				return 0;

			UICharInfo originChar = this.cachedInputTextGenerator.characters[originalPos];
			int originLine = this.DetermineCharacterLine( originalPos, this.cachedInputTextGenerator );

			// We are on the first line return first character
			if ( originLine <= 0 )
				return goToFirstChar ? 0 : originalPos;

			int endCharIdx = this.cachedInputTextGenerator.lines[originLine].startCharIdx - 1;

			for ( int i = this.cachedInputTextGenerator.lines[originLine - 1].startCharIdx; i < endCharIdx; ++i )
			{
				if ( this.cachedInputTextGenerator.characters[i].cursorPos.x >= originChar.cursorPos.x )
					return i;
			}
			return endCharIdx;
		}

		private int LineDownCharacterPosition( int originalPos, bool goToLastChar )
		{
			if ( originalPos >= this.cachedInputTextGenerator.characterCountVisible )
				return this.text.Length;

			UICharInfo originChar = this.cachedInputTextGenerator.characters[originalPos];
			int originLine = this.DetermineCharacterLine( originalPos, this.cachedInputTextGenerator );

			// We are on the last line return last character
			if ( originLine + 1 >= this.cachedInputTextGenerator.lineCount )
				return goToLastChar ? this.text.Length : originalPos;

			// Need to determine end line for next line.
			int endCharIdx = GetLineEndPosition( this.cachedInputTextGenerator, originLine + 1 );

			for ( int i = this.cachedInputTextGenerator.lines[originLine + 1].startCharIdx; i < endCharIdx; ++i )
			{
				if ( this.cachedInputTextGenerator.characters[i].cursorPos.x >= originChar.cursorPos.x )
					return i;
			}
			return endCharIdx;
		}

		private void ActivateInputFieldInternal()
		{
			if ( EventSystem.instance.currentSelectedGameObject != this )
				EventSystem.instance.SetSelectedGameObject( this );

			if ( TouchScreenKeyboard.isSupported )
			{
				if ( Input.touchSupported )
				{
					TouchScreenKeyboard.hideInput = this.shouldHideMobileInput;
				}

				this._keyboard = ( this.inputType == InputType.Password )
									 ? TouchScreenKeyboard.Open( this._text, this.keyboardType, false, this.multiLine, true )
									 : TouchScreenKeyboard.Open( this._text, this.keyboardType, this.inputType == InputType.AutoCorrect,
																 this.multiLine );

				// Mimics OnFocus but as mobile doesn't properly support select all
				// just set it to the end of the text (where it would move when typing starts)
				this.MoveTextEnd( false );
			}
			else
			{
				Input.imeCompositionMode = IMECompositionMode.On;
			}

			this._allowInput = true;
			this._originalText = this.text;
			this._wasCanceled = false;
			this.SetCaretVisible();
			this.UpdateLabel();
		}

		internal void ActivateInputField()
		{
			if ( this._nText.font == null )
				return;

			if ( this._allowInput )
			{
				if ( this._keyboard != null && !this._keyboard.active )
				{
					this._keyboard.active = true;
					this._keyboard.text = this._text;
				}
			}
			else
				this.ActivateInputFieldInternal();
		}

		private void EnforceContentType()
		{
			switch ( this.contentType )
			{
				case ContentType.Standard:
					{
						// Don't enforce line type for this content type.
						this._inputType = InputType.Standard;
						this._keyboardType = TouchScreenKeyboardType.Default;
						this._characterValidation = CharacterValidation.None;
						break;
					}
				case ContentType.Autocorrected:
					{
						// Don't enforce line type for this content type.
						this._inputType = InputType.AutoCorrect;
						this._keyboardType = TouchScreenKeyboardType.Default;
						this._characterValidation = CharacterValidation.None;
						break;
					}
				case ContentType.IntegerNumber:
					{
						this._lineType = LineType.SingleLine;
						this._inputType = InputType.Standard;
						this._keyboardType = TouchScreenKeyboardType.NumberPad;
						this._characterValidation = CharacterValidation.Integer;
						break;
					}
				case ContentType.DecimalNumber:
					{
						this._lineType = LineType.SingleLine;
						this._inputType = InputType.Standard;
						this._keyboardType = TouchScreenKeyboardType.NumbersAndPunctuation;
						this._characterValidation = CharacterValidation.Decimal;
						break;
					}
				case ContentType.Alphanumeric:
					{
						this._lineType = LineType.SingleLine;
						this._inputType = InputType.Standard;
						this._keyboardType = TouchScreenKeyboardType.ASCIICapable;
						this._characterValidation = CharacterValidation.Alphanumeric;
						break;
					}
				case ContentType.Name:
					{
						this._lineType = LineType.SingleLine;
						this._inputType = InputType.Standard;
						this._keyboardType = TouchScreenKeyboardType.NamePhonePad;
						this._characterValidation = CharacterValidation.Name;
						break;
					}
				case ContentType.EmailAddress:
					{
						this._lineType = LineType.SingleLine;
						this._inputType = InputType.Standard;
						this._keyboardType = TouchScreenKeyboardType.EmailAddress;
						this._characterValidation = CharacterValidation.EmailAddress;
						break;
					}
				case ContentType.Password:
					{
						this._lineType = LineType.SingleLine;
						this._inputType = InputType.Password;
						this._keyboardType = TouchScreenKeyboardType.Default;
						this._characterValidation = CharacterValidation.None;
						break;
					}
				case ContentType.Pin:
					{
						this._lineType = LineType.SingleLine;
						this._inputType = InputType.Password;
						this._keyboardType = TouchScreenKeyboardType.NumberPad;
						this._characterValidation = CharacterValidation.Integer;
						break;
					}
				default:
					{
						// Includes Custom type. Nothing should be enforced.
						return;
					}
			}

			this.EnforceTextHOverflow();
		}

		private void EnforceTextHOverflow()
		{
			this._nText.horizontalOverflow = this.multiLine ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
		}

		private void SetToCustomIfContentTypeIsNot( params ContentType[] allowedContentTypes )
		{
			if ( this.contentType == ContentType.Custom )
				return;

			int count = allowedContentTypes.Length;
			for ( int i = 0; i < count; i++ )
				if ( this.contentType == allowedContentTypes[i] )
					return;

			this.contentType = ContentType.Custom;
		}

		private void SetCaretVisible()
		{
			if ( !this._allowInput )
				return;

			this._caretVisible = true;
			this._blinkStartTime = Time.unscaledTime;
			this.SetCaretActive();
		}

		// SetCaretActive will not set the caret immediately visible - it will wait for the next time to blink.
		// However, it will handle things correctly if the blink speed changed from zero to non-zero or non-zero to zero.
		private void SetCaretActive()
		{
			if ( !this._allowInput )
				return;

			if ( this._caretBlinkRate > 0.0f )
				TaskManager.instance.RegisterTimer( 1f / this._caretBlinkRate, 0, true, this.OnCaretBlink, null );
			else
				this._caretVisible = true;
		}

		private void OnCaretBlink( int index, float dt, object o )
		{
			if ( !this._allowInput )
				return;

			this._caretVisible = !this._caretVisible;
			if ( !this.hasSelection )
				this.MarkGeometryAsDirty();
		}

		private void SelectAll()
		{
			this.caretPositionInternal = this.text.Length;
			this.caretSelectPositionInternal = 0;
		}

		private static int GetLineStartPosition( TextGenerator gen, int line )
		{
			line = Mathf.Clamp( line, 0, gen.lines.Count - 1 );
			return gen.lines[line].startCharIdx;
		}

		private static int GetLineEndPosition( TextGenerator gen, int line )
		{
			line = Mathf.Max( line, 0 );
			if ( line + 1 < gen.lines.Count )
				return gen.lines[line + 1].startCharIdx - 1;
			return gen.characterCountVisible;
		}

		private int GetCharacterIndexFromPosition( Vector2 pos )
		{
			TextGenerator gen = this._nText.cachedTextGenerator;

			if ( gen.lineCount == 0 )
				return 0;

			int line = this.GetUnclampedCharacterLineFromPosition( pos, gen );
			if ( line < 0 )
				return 0;
			if ( line >= gen.lineCount )
				return gen.characterCountVisible;

			int startCharIndex = gen.lines[line].startCharIdx;
			int endCharIndex = GetLineEndPosition( gen, line );

			for ( int i = startCharIndex; i < endCharIndex; i++ )
			{
				if ( i >= gen.characterCountVisible )
					break;

				UICharInfo charInfo = gen.characters[i];
				Vector2 charPos = charInfo.cursorPos / this._nText.pixelsPerUnit;

				float distToCharStart = pos.x - charPos.x;
				float distToCharEnd = charPos.x + ( charInfo.charWidth / this._nText.pixelsPerUnit ) - pos.x;
				if ( distToCharStart < distToCharEnd )
					return i;
			}

			return endCharIndex;
		}

		private int GetUnclampedCharacterLineFromPosition( Vector2 pos, TextGenerator generator )
		{
			if ( !this.multiLine )
				return 0;

			// transform y to local scale
			float y = pos.y * this._nText.pixelsPerUnit;
			float lastBottomY = 0.0f;

			for ( int i = 0; i < generator.lineCount; ++i )
			{
				float topY = generator.lines[i].topY;
				float bottomY = topY - generator.lines[i].height;

				// pos is somewhere in the leading above this line
				if ( y > topY )
				{
					// determine which line we're closer to
					float leading = topY - lastBottomY;
					if ( y > topY - 0.5f * leading )
						return i - 1;
					return i;
				}

				if ( y > bottomY )
					return i;

				lastBottomY = bottomY;
			}

			// Position is after last line.
			return generator.lineCount;
		}

		private void MarkGeometryAsDirty()
		{
			CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild( this );
		}

		private void UpdateGeometry()
		{
			// No need to draw a cursor on mobile as its handled by the devices keyboard.
			if ( !this.shouldHideMobileInput )
				return;

			if ( this._cachedInputRenderer == null )
			{
				this._inputCaret = new GameObject( this._nText.transform.parent.name + " Input Caret", typeof( RectTransform ), typeof( CanvasRenderer ) );
				Object.DontDestroyOnLoad( this._inputCaret );
				this._inputCaret.transform.SetParent( this._nText.transform.parent );
				this._inputCaret.transform.SetAsFirstSibling();
				this._inputCaret.layer = this.layer;

				this._caretRectTrans = this._inputCaret.GetComponent<RectTransform>();
				this._cachedInputRenderer = this._inputCaret.GetComponent<CanvasRenderer>();
				this._cachedInputRenderer.SetMaterial( this._nText.GetModifiedMaterial( Graphic.defaultGraphicMaterial ), Texture2D.whiteTexture );

				this.AssignPositioningIfNeeded();
			}

			if ( this._cachedInputRenderer == null )
				return;

			this.OnFillVBO( this.mesh );
			this._cachedInputRenderer.SetMesh( this.mesh );
		}

		private void OnFillVBO( Mesh vbo )
		{
			using ( VertexHelper helper = new VertexHelper() )
			{
				if ( !this._allowInput )
				{
					helper.FillMesh( vbo );
					return;
				}

				Vector2 roundingOffset = this._nText.PixelAdjustPoint( Vector2.zero );

				if ( !this.hasSelection )
					this.GenerateCaret( helper, roundingOffset );
				else
					this.GenerateHightlight( helper, roundingOffset );

				helper.FillMesh( vbo );
			}
		}

		private void GenerateCaret( VertexHelper vbo, Vector2 roundingOffset )
		{
			if ( !this._caretVisible )
				return;

			if ( this._cursorVerts == null )
				this.CreateCursorVerts();

			float width = this._caretWidth;
			int adjustedPos = Mathf.Max( 0, this.caretPositionInternal - this._drawStart );
			TextGenerator gen = this._nText.cachedTextGenerator;

			if ( gen == null )
				return;

			if ( gen.lineCount == 0 )
				return;

			Vector2 startPosition = Vector2.zero;

			// Calculate startPosition
			if ( adjustedPos < gen.characters.Count )
			{
				UICharInfo cursorChar = gen.characters[adjustedPos];
				startPosition.x = cursorChar.cursorPos.x;
			}
			startPosition.x /= this._nText.pixelsPerUnit;

			// TODO: Only clamp when Text uses horizontal word wrap.
			if ( startPosition.x > this._nText.rectTransform.rect.xMax )
				startPosition.x = this._nText.rectTransform.rect.xMax;

			int characterLine = this.DetermineCharacterLine( adjustedPos, gen );
			startPosition.y = gen.lines[characterLine].topY / this._nText.pixelsPerUnit;
			float height = gen.lines[characterLine].height / this._nText.pixelsPerUnit;

			int count = this._cursorVerts.Length;
			for ( int i = 0; i < count; i++ )
				this._cursorVerts[i].color = this.caretColor;

			this._cursorVerts[0].position = new Vector3( startPosition.x, startPosition.y - height, 0.0f );
			this._cursorVerts[1].position = new Vector3( startPosition.x + width, startPosition.y - height, 0.0f );
			this._cursorVerts[2].position = new Vector3( startPosition.x + width, startPosition.y, 0.0f );
			this._cursorVerts[3].position = new Vector3( startPosition.x, startPosition.y, 0.0f );

			if ( roundingOffset != Vector2.zero )
			{
				for ( int i = 0; i < count; i++ )
				{
					UIVertex uiv = this._cursorVerts[i];
					uiv.position.x += roundingOffset.x;
					uiv.position.y += roundingOffset.y;
				}
			}

			vbo.AddUIVertexQuad( this._cursorVerts );

			int screenHeight = Screen.height;
			int displayIndex = this._nText.canvas.targetDisplay;
			if ( displayIndex > 0 && displayIndex < Display.displays.Length )
				screenHeight = Display.displays[displayIndex].renderingHeight;


			startPosition.y = screenHeight - startPosition.y;
			Input.compositionCursorPos = startPosition;
		}

		private void CreateCursorVerts()
		{
			this._cursorVerts = new UIVertex[4];

			int count = this._cursorVerts.Length;
			for ( int i = 0; i < count; i++ )
			{
				this._cursorVerts[i] = UIVertex.simpleVert;
				this._cursorVerts[i].uv0 = Vector2.zero;
			}
		}

		private void GenerateHightlight( VertexHelper vbo, Vector2 roundingOffset )
		{
			TextGenerator gen = this._nText.cachedTextGenerator;

			if ( gen.lineCount <= 0 )
				return;

			int startChar = Mathf.Max( 0, this.caretPositionInternal - this._drawStart );
			int endChar = Mathf.Max( 0, this.caretSelectPositionInternal - this._drawStart );

			// Ensure pos is always less then selPos to make the code simpler
			if ( startChar > endChar )
			{
				int temp = startChar;
				startChar = endChar;
				endChar = temp;
			}

			endChar -= 1;

			int currentLineIndex = this.DetermineCharacterLine( startChar, gen );
			int lastCharInLineIndex = GetLineEndPosition( gen, currentLineIndex );

			UIVertex vert = UIVertex.simpleVert;
			vert.uv0 = Vector2.zero;
			vert.color = this.selectionColor;

			int currentChar = startChar;
			while ( currentChar <= endChar && currentChar < gen.characterCount )
			{
				if ( currentChar == lastCharInLineIndex || currentChar == endChar )
				{
					UICharInfo startCharInfo = gen.characters[startChar];
					UICharInfo endCharInfo = gen.characters[currentChar];
					Vector2 startPosition = new Vector2( startCharInfo.cursorPos.x / this._nText.pixelsPerUnit,
														 gen.lines[currentLineIndex].topY / this._nText.pixelsPerUnit );
					Vector2 endPosition = new Vector2(
						( endCharInfo.cursorPos.x + endCharInfo.charWidth ) / this._nText.pixelsPerUnit,
						startPosition.y - gen.lines[currentLineIndex].height / this._nText.pixelsPerUnit );

					// Checking xMin as well due to text generator not setting position if char is not rendered.
					if ( endPosition.x > this._nText.rectTransform.rect.xMax || endPosition.x < this._nText.rectTransform.rect.xMin )
						endPosition.x = this._nText.rectTransform.rect.xMax;

					int startIndex = vbo.currentVertCount;
					vert.position = new Vector3( startPosition.x, endPosition.y, 0.0f ) + ( Vector3 )roundingOffset;
					vbo.AddVert( vert );

					vert.position = new Vector3( endPosition.x, endPosition.y, 0.0f ) + ( Vector3 )roundingOffset;
					vbo.AddVert( vert );

					vert.position = new Vector3( endPosition.x, startPosition.y, 0.0f ) + ( Vector3 )roundingOffset;
					vbo.AddVert( vert );

					vert.position = new Vector3( startPosition.x, startPosition.y, 0.0f ) + ( Vector3 )roundingOffset;
					vbo.AddVert( vert );

					vbo.AddTriangle( startIndex, startIndex + 1, startIndex + 2 );
					vbo.AddTriangle( startIndex + 2, startIndex + 3, startIndex + 0 );

					startChar = currentChar + 1;
					currentLineIndex++;

					lastCharInLineIndex = GetLineEndPosition( gen, currentLineIndex );
				}
				currentChar++;
			}
		}

		private void SetDrawRangeToContainCaretPosition( int caretPos )
		{
			// We don't have any generated lines generation is not valid.
			if ( this.cachedInputTextGenerator.lineCount <= 0 )
				return;

			// the extents gets modified by the pixel density, so we need to use the generated extents since that will be in the same 'space' as
			// the values returned by the TextGenerator.lines[x].height for instance.
			Vector2 extents = this.cachedInputTextGenerator.rectExtents.size;
			if ( this.multiLine )
			{
				IList<UILineInfo> lines = this.cachedInputTextGenerator.lines;
				int caretLine = this.DetermineCharacterLine( caretPos, this.cachedInputTextGenerator );

				if ( caretPos > this._drawEnd )
				{
					// Caret comes after drawEnd, so we need to move drawEnd to the end of the line with the caret
					this._drawEnd = GetLineEndPosition( this.cachedInputTextGenerator, caretLine );
					float bottomY = lines[caretLine].topY - lines[caretLine].height;

					if ( caretLine == lines.Count - 1 )// Remove interline spacing on last line.
						bottomY += lines[caretLine].leading;

					int startLine = caretLine;
					while ( startLine > 0 )
					{
						float topY = lines[startLine - 1].topY;
						if ( topY - bottomY > extents.y )
							break;
						startLine--;
					}
					this._drawStart = GetLineStartPosition( this.cachedInputTextGenerator, startLine );
				}
				else
				{
					if ( caretPos < this._drawStart )
					{
						// Caret comes before drawStart, so we need to move drawStart to an earlier line start that comes before caret.
						this._drawStart = GetLineStartPosition( this.cachedInputTextGenerator, caretLine );
					}

					int startLine = this.DetermineCharacterLine( this._drawStart, this.cachedInputTextGenerator );
					int endLine = startLine;

					float topY = lines[startLine].topY;
					float bottomY = lines[endLine].topY - lines[endLine].height;

					if ( endLine == lines.Count - 1 )// Remove interline spacing on last line.
						bottomY += lines[endLine].leading;

					while ( endLine < lines.Count - 1 )
					{
						bottomY = lines[endLine + 1].topY - lines[endLine + 1].height;

						if ( endLine + 1 == lines.Count - 1 )// Remove interline spacing on last line.
							bottomY += lines[endLine + 1].leading;

						if ( topY - bottomY > extents.y )
							break;
						++endLine;
					}
					this._drawEnd = GetLineEndPosition( this.cachedInputTextGenerator, endLine );

					while ( startLine > 0 )
					{
						topY = lines[startLine - 1].topY;
						if ( topY - bottomY > extents.y )
							break;
						startLine--;
					}
					this._drawStart = GetLineStartPosition( this.cachedInputTextGenerator, startLine );
				}
			}
			else
			{
				IList<UICharInfo> characters = this.cachedInputTextGenerator.characters;
				if ( this._drawEnd > this.cachedInputTextGenerator.characterCountVisible )
					this._drawEnd = this.cachedInputTextGenerator.characterCountVisible;

				float width = 0.0f;
				if ( caretPos > this._drawEnd || ( caretPos == this._drawEnd && this._drawStart > 0 ) )
				{
					// fit characters from the caretPos leftward
					this._drawEnd = caretPos;
					for ( this._drawStart = this._drawEnd - 1; this._drawStart >= 0; --this._drawStart )
					{
						if ( width + characters[this._drawStart].charWidth > extents.x )
							break;

						width += characters[this._drawStart].charWidth;
					}
					++this._drawStart; // move right one to the last character we could fit on the left
				}
				else
				{
					if ( caretPos < this._drawStart )
						this._drawStart = caretPos;

					this._drawEnd = this._drawStart;
				}

				// fit characters rightward
				for ( ; this._drawEnd < this.cachedInputTextGenerator.characterCountVisible; ++this._drawEnd )
				{
					width += characters[this._drawEnd].charWidth;
					if ( width > extents.x )
						break;
				}
			}
		}

		internal void DeactivateInputField()
		{
			// Not activated do nothing.
			if ( !this._allowInput )
				return;

			this._allowInput = false;

			if ( this._placeholder != null )
				this._placeholder.enabled = string.IsNullOrEmpty( this._text );

			if ( this._wasCanceled )
				this.text = this._originalText;

			if ( this._keyboard != null )
			{
				this._keyboard.active = false;
				this._keyboard = null;
			}

			//this._caretPosition = this._caretSelectPosition = 0;

			this.SendOnEndEdit();

			Input.imeCompositionMode = IMECompositionMode.Auto;

			this.MarkGeometryAsDirty();
		}

		private void ClampPos( ref int pos )
		{
			if ( pos < 0 )
				pos = 0;
			else if ( pos > this.text.Length )
				pos = this.text.Length;
		}

		private bool InPlaceEditing()
		{
			return !TouchScreenKeyboard.isSupported;
		}

		private bool IsValidChar( char c )
		{
			// Delete key on mac
			if ( c == 127 )
				return false;
			// Accept newline and tab
			if ( c == '\t' || c == '\n' )
				return true;

			return this._nText.font.HasCharacter( c );
		}

		private char Validate( string text, int pos, char ch )
		{
			// Validation is disabled
			if ( this.characterValidation == CharacterValidation.None )
				return ch;

			if ( this.characterValidation == CharacterValidation.Integer ||
				 this.characterValidation == CharacterValidation.Decimal )
			{
				// Integer and decimal
				bool cursorBeforeDash = ( pos == 0 && text.Length > 0 && text[0] == '-' );
				bool dashInSelection = text.Length > 0 && text[0] == '-' &&
				                       ( ( this.caretPositionInternal == 0 && this.caretSelectPositionInternal > 0 ) ||
				                         ( this.caretSelectPositionInternal == 0 && this.caretPositionInternal > 0 ) );
				bool selectionAtStart = this.caretPositionInternal == 0 || this.caretSelectPositionInternal == 0;
				if ( !cursorBeforeDash || dashInSelection )
				{
					if ( ch >= '0' && ch <= '9' ) return ch;
					if ( ch == '-' && ( pos == 0 || selectionAtStart ) ) return ch;
					if ( ch == '.' &&
						 this.characterValidation == CharacterValidation.Decimal && !text.Contains( "." ) ) return ch;
				}
			}
			else if ( this.characterValidation == CharacterValidation.Alphanumeric )
			{
				// All alphanumeric characters
				if ( ch >= 'A' && ch <= 'Z' ) return ch;
				if ( ch >= 'a' && ch <= 'z' ) return ch;
				if ( ch >= '0' && ch <= '9' ) return ch;
			}
			else if ( this.characterValidation == CharacterValidation.Name )
			{
				// FIXME: some actions still lead to invalid input:
				//        - Hitting delete in front of an uppercase letter
				//        - Selecting an uppercase letter and deleting it
				//        - Typing some text, hitting Home and typing more text (we then have an uppercase letter in the middle of a word)
				//        - Typing some text, hitting Home and typing a space (we then have a leading space)
				//        - Erasing a space between two words (we then have an uppercase letter in the middle of a word)
				//        - We accept a trailing space
				//        - We accept the insertion of a space between two lowercase letters.
				//        - Typing text in front of an existing uppercase letter
				//        - ... and certainly more
				//
				// The rule we try to implement are too complex for this kind of verification.

				if ( char.IsLetter( ch ) )
				{
					// Character following a space should be in uppercase.
					if ( char.IsLower( ch ) && ( ( pos == 0 ) || ( text[pos - 1] == ' ' ) ) )
					{
						return char.ToUpper( ch );
					}

					// Character not following a space or an apostrophe should be in lowercase.
					if ( char.IsUpper( ch ) && ( pos > 0 ) && ( text[pos - 1] != ' ' ) && ( text[pos - 1] != '\'' ) )
					{
						return char.ToLower( ch );
					}

					return ch;
				}

				if ( ch == '\'' )
				{
					// Don't allow more than one apostrophe
					if ( !text.Contains( "'" ) )
						// Don't allow consecutive spaces and apostrophes.
						if ( !( ( ( pos > 0 ) && ( ( text[pos - 1] == ' ' ) || ( text[pos - 1] == '\'' ) ) ) ||
							  ( ( pos < text.Length ) && ( ( text[pos] == ' ' ) || ( text[pos] == '\'' ) ) ) ) )
							return ch;
				}

				if ( ch == ' ' )
				{
					// Don't allow consecutive spaces and apostrophes.
					if ( !( ( ( pos > 0 ) && ( ( text[pos - 1] == ' ' ) || ( text[pos - 1] == '\'' ) ) ) ||
						  ( ( pos < text.Length ) && ( ( text[pos] == ' ' ) || ( text[pos] == '\'' ) ) ) ) )
						return ch;
				}
			}
			else if ( this.characterValidation == CharacterValidation.EmailAddress )
			{
				// From StackOverflow about allowed characters in email addresses:
				// Uppercase and lowercase English letters (a-z, A-Z)
				// Digits 0 to 9
				// Characters ! # $ % & ' * + - / = ? ^ _ ` { | } ~
				// Character . (dot, period, full stop) provided that it is not the first or last character,
				// and provided also that it does not appear two or more times consecutively.

				if ( ch >= 'A' && ch <= 'Z' ) return ch;
				if ( ch >= 'a' && ch <= 'z' ) return ch;
				if ( ch >= '0' && ch <= '9' ) return ch;
				if ( ch == '@' && text.IndexOf( '@' ) == -1 ) return ch;
				if ( EMAIL_SPECIAL_CHARACTERS.IndexOf( ch ) != -1 ) return ch;
				if ( ch == '.' )
				{
					char lastChar = ( text.Length > 0 ) ? text[Mathf.Clamp( pos, 0, text.Length - 1 )] : ' ';
					char nextChar = ( text.Length > 0 ) ? text[Mathf.Clamp( pos + 1, 0, text.Length - 1 )] : '\n';
					if ( lastChar != '.' && nextChar != '.' )
						return ch;
				}
			}
			return ( char )0;
		}

		private void AssignPositioningIfNeeded()
		{
			if ( this._caretRectTrans != null &&
				 ( this._caretRectTrans.localPosition != this._nText.rectTransform.localPosition ||
				   this._caretRectTrans.localRotation != this._nText.rectTransform.localRotation ||
				   this._caretRectTrans.localScale != this._nText.rectTransform.localScale ||
				   this._caretRectTrans.anchorMin != this._nText.rectTransform.anchorMin ||
				   this._caretRectTrans.anchorMax != this._nText.rectTransform.anchorMax ||
				   this._caretRectTrans.anchoredPosition != this._nText.rectTransform.anchoredPosition ||
				   this._caretRectTrans.sizeDelta != this._nText.rectTransform.sizeDelta ||
				   this._caretRectTrans.pivot != this._nText.rectTransform.pivot ) )
			{
				this._caretRectTrans.localPosition = this._nText.rectTransform.localPosition;
				this._caretRectTrans.localRotation = this._nText.rectTransform.localRotation;
				this._caretRectTrans.localScale = this._nText.rectTransform.localScale;
				this._caretRectTrans.anchorMin = this._nText.rectTransform.anchorMin;
				this._caretRectTrans.anchorMax = this._nText.rectTransform.anchorMax;
				this._caretRectTrans.anchoredPosition = this._nText.rectTransform.anchoredPosition;
				this._caretRectTrans.sizeDelta = this._nText.rectTransform.sizeDelta;
				this._caretRectTrans.pivot = this._nText.rectTransform.pivot;
			}
		}

		private void SendOnValueChangedAndUpdateLabel()
		{
			this.SendOnValueChanged();
			this.UpdateLabel();
		}

		private void SendOnValueChanged()
		{
			this.onChanged.Call( this.text );
		}

		private void SendOnEndEdit()
		{
			this.onEndEdit.Call( this._text );
		}

		private EditState SendOnSubmitOrFinish()
		{
			if ( this.enableSubmit )
			{
				this.onSubmit.Call( this.text );
				this.text = string.Empty;
				return EditState.Continue;
			}
			return EditState.Finish;
		}

		private void CreatePlaceHolder()
		{
			GameObject go = new GameObject( this.name );
			Object.DontDestroyOnLoad( go );
			go.AddComponent<RectTransform>();
			go.transform.SetParent( this.rectTransform, false );

			this._placeholder = go.AddComponent<Text>();
			this._placeholder.font = this._nText.font;
			this._placeholder.fontSize = this._nText.fontSize;
			this._placeholder.fontStyle = FontStyle.Italic;
			this._placeholder.supportRichText = true;
			this._placeholder.alignment = this._nText.alignment;
			this._placeholder.raycastTarget = false;
			Color placeholderColor = this._nText.color;
			placeholderColor.a = 0.5f;
			this._placeholder.color = placeholderColor;

			RectTransform rt = this._placeholder.GetComponent<RectTransform>();
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.one;
			rt.pivot = new Vector2( 0, 1 );
			rt.sizeDelta = Vector2.zero;
			rt.SetParent( this.rectTransform, false );
		}

		private void DestroyPlaceHolder()
		{
			if ( this._placeholder == null )
				return;
			Object.DestroyImmediate( this._placeholder.gameObject );
			this._placeholder = null;
		}

		public void ForceLabelUpdate()
		{
			this.UpdateLabel();
		}

		private void UpdateLabel()
		{
			if ( this._nText.font == null || this._preventFontCallback )
				return;

			this._preventFontCallback = true;

			string fullText;
			if ( Input.compositionString.Length > 0 )
				fullText = this.text.Substring( 0, this._caretPosition ) + Input.compositionString +
						   this.text.Substring( this._caretPosition );
			else
				fullText = this.text;

			string processed = this.inputType == InputType.Password ? new string( this.asteriskChar, fullText.Length ) : fullText;
			this.FitSize( processed );

			bool isEmpty = string.IsNullOrEmpty( fullText );

			if ( this._placeholder != null )
				this._placeholder.enabled = isEmpty;

			// If not currently editing the text, set the visible range to the whole text.
			// The UpdateLabel method will then truncate it to the part that fits inside the Text area.
			// We can't do this when text is being edited since it would discard the current scroll,
			// which is defined by means of the _drawStart and _drawEnd indices.
			if ( !this._allowInput )
			{
				this._drawStart = 0;
				this._drawEnd = this._text.Length;
			}

			if ( !isEmpty )
			{
				// Determine what will actually fit into the given line
				TextGenerationSettings settings = this._nText.GetGenerationSettings( this._nText.rectTransform.rect.size );
				settings.generateOutOfBounds = true;

				this.cachedInputTextGenerator.PopulateWithErrors( processed, settings, this.gameObject );

				this.SetDrawRangeToContainCaretPosition( this.caretSelectPositionInternal );

				processed = processed.Substring( this._drawStart, Mathf.Min( this._drawEnd, processed.Length ) - this._drawStart );

				this.SetCaretVisible();
			}
			this._nText.text = processed;
			this.MarkGeometryAsDirty();
			this._preventFontCallback = false;
		}

		private void UpdateCaretMaterial()
		{
			this._cachedInputRenderer?.SetMaterial( this._nText.GetModifiedMaterial( Graphic.defaultGraphicMaterial ), Texture2D.whiteTexture );
		}

		protected internal override void Update( UpdateContext context )
		{
			base.Update( context );

			if ( this._updateDrag && this._dragPositionOutOfBounds )
			{
				Vector2 localMousePos;
				RectTransformUtility.ScreenPointToLocalPointInRectangle( this._nText.rectTransform,
																		 this._pointerDragEventData.position, Stage.inst.eventCamera,
																		 out localMousePos );

				Rect mRect = this._nText.rectTransform.rect;

				if ( this.multiLine )
				{
					if ( localMousePos.y > mRect.yMax )
						this.MoveUp( true );
					else if ( localMousePos.y < mRect.yMin )
						this.MoveDown( true );
				}
				else
				{
					if ( localMousePos.x < mRect.xMin )
						this.MoveLeft( true, false );
					else if ( localMousePos.x > mRect.xMax )
						this.MoveRight( true, false );
				}
				this.UpdateLabel();
			}

			if ( this.InPlaceEditing() || !this._allowInput )
				return;

			this.AssignPositioningIfNeeded();

			if ( this._keyboard == null || this._keyboard.done )
			{
				if ( this._keyboard != null )
				{
					if ( !this.readOnly )
						this.text = this._keyboard.text;

					if ( this._keyboard.wasCanceled )
						this._wasCanceled = true;
				}

				this.OnDeselect( null );
				return;
			}

			string val = this._keyboard.text;

			if ( this._text != val )
			{
				if ( this.readOnly )
					this._keyboard.text = this._text;
				else
				{
					this._text = string.Empty;

					int count = val.Length;
					for ( int i = 0; i < count; ++i )
					{
						char c = val[i];

						if ( c == '\r' || c == 3 )
							c = '\n';

						if ( this.onValidateInput != null )
							c = this.onValidateInput( this._text, this._text.Length, c );
						else if ( this.characterValidation != CharacterValidation.None )
							c = this.Validate( this._text, this._text.Length, c );

						if ( this.lineType == LineType.MultiLineSubmit && c == '\n' )
						{
							this._keyboard.text = this._text;

							this.OnDeselect( null );
							return;
						}

						if ( c != 0 )
							this._text += c;
					}

					if ( this.characterLimit > 0 && this._text.Length > this.characterLimit )
						this._text = this._text.Substring( 0, this.characterLimit );

					if ( this._keyboard.canGetSelection )
						this.UpdateCaretFromKeyboard();
					else
						this.caretPositionInternal = this.caretSelectPositionInternal = this._text.Length;

					// Set keyboard text before updating label, as we might have changed it with validation
					// and update label will take the old value from keyboard if we don't change it here
					if ( this._text != val )
						this._keyboard.text = this._text;

					this.SendOnValueChangedAndUpdateLabel();
				}
			}
			else if ( this._keyboard.canGetSelection )
			{
				this.UpdateCaretFromKeyboard();
			}


			if ( this._keyboard.done )
			{
				if ( this._keyboard.wasCanceled )
					this._wasCanceled = true;

				this.OnDeselect( null );
			}
		}

		private void UpdateCaretFromKeyboard()
		{
			var selectionRange = this._keyboard.selection;

			var selectionStart = selectionRange.start;
			var selectionEnd = selectionRange.end;

			var caretChanged = false;

			if ( this.caretPositionInternal != selectionStart )
			{
				caretChanged = true;
				this.caretPositionInternal = selectionStart;
			}

			if ( this.caretSelectPositionInternal != selectionEnd )
			{
				this.caretSelectPositionInternal = selectionEnd;
				caretChanged = true;
			}

			if ( caretChanged )
			{
				this._blinkStartTime = Time.unscaledTime;

				this.UpdateLabel();
			}
		}

		public void CalculateLayoutInputHorizontal() { }
		public void CalculateLayoutInputVertical() { }

		public float minWidth => 0;

		public float preferredWidth
		{
			get
			{
				if ( this._nText == null )
					return 0;
				TextGenerationSettings settings = this._nText.GetGenerationSettings( Vector2.zero );
				return this._nText.cachedTextGeneratorForLayout.GetPreferredWidth( this._text, settings ) / this._nText.pixelsPerUnit;
			}
		}
		public float flexibleWidth => -1;
		public float minHeight => 0;

		public float preferredHeight
		{
			get
			{
				if ( this._nText == null )
					return 0;
				TextGenerationSettings settings = this._nText.GetGenerationSettings( new Vector2( this._nText.rectTransform.rect.size.x, 0.0f ) );
				return this._nText.cachedTextGeneratorForLayout.GetPreferredHeight( this._text, settings ) / this._nText.pixelsPerUnit;
			}
		}

		public float flexibleHeight => -1;
		public int layoutPriority => 1;

		private enum EditState
		{
			Continue = 0,
			Finish = 1
		}
	}
}