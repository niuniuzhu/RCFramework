using FairyUGUI.Core;
using Game.Task;
using UnityEngine;
using Logger = Core.Misc.Logger;

namespace FairyUGUI.Event
{
	public class EventSystem
	{
		private static EventSystem _instance;
		public static EventSystem instance => _instance ?? ( _instance = new EventSystem() );

		public PointerInput pointerInput { get; private set; }

		private readonly Raycaster _raycaster;

		private DisplayObject _currentSelected;
		public DisplayObject currentSelectedGameObject => this._currentSelected;

		private bool _selectionGuard;
		public bool alreadySelecting => this._selectionGuard;

		private BaseEventData _dummyData;
		private BaseEventData baseEventDataCache => this._dummyData ?? ( this._dummyData = new BaseEventData() );

		public bool debug
		{
			set
			{
				if ( value )
					TaskManager.instance.RegisterOnGUIMethod( this.OnGUI );
				else
					TaskManager.instance.UnregisterOnGUIMethod( this.OnGUI );
			}
		}

		private EventSystem()
		{
			this.pointerInput = new PointerInput();
			this._raycaster = new Raycaster();
		}

		public void SetSelectedGameObject( DisplayObject selected )
		{
			this.SetSelectedGameObject( selected, this.baseEventDataCache );
		}

		public void SetSelectedGameObject( DisplayObject selected, BaseEventData pointer )
		{
			if ( this._selectionGuard )
			{
				Logger.Error( "Attempting to select " + selected + "while already selecting an object." );
				return;
			}

			this._selectionGuard = true;
			if ( selected == this._currentSelected )
			{
				this._selectionGuard = false;
				return;
			}

			ExecuteEvents.Execute( this._currentSelected, pointer, EventTriggerType.Deselect );
			this._currentSelected = selected;
			ExecuteEvents.Execute( this._currentSelected, pointer, EventTriggerType.Select );
			this._selectionGuard = false;
		}

		public void Raycast( PointerEventData eventData, out RaycastResult result )
		{
			this._raycaster.Raycast( eventData, out result );
		}

		public bool IsPointerOverGameObject( int pointerId = PointerInput.K_MOUSE_LEFT_ID )
		{
			return this.pointerInput.IsPointerOverGameObject( pointerId );
		}

		public void Update()
		{
			this.pointerInput.Process();
		}

		private GUIStyle _guiStyle;
		private void OnGUI( float dt )
		{
			if ( this._guiStyle == null )
			{
				this._guiStyle = new GUIStyle();
				this._guiStyle.fontSize = 24;
				this._guiStyle.normal.textColor = Color.white;
			}
			GUI.Label( new Rect( Screen.width - 400, 0, 400, Screen.height ), this.pointerInput.ToString(), this._guiStyle );
		}
	}
}