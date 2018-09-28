using System;
using FairyUGUI.Core;
using FairyUGUI.Event;
using FairyUGUI.Utils;
using UnityEngine;

namespace FairyUGUI.UI
{
	public enum PopupDirection
	{
		TouchPosition,
		Left,
		Upward,
		Right,
		Downward
	}

	[Flags]
	public enum PopupConstraint
	{
		Left = 1 << 0,
		Top = 1 << 1,
		Right = 1 << 2,
		Bottom = 1 << 3,
		Any = int.MaxValue
	}

	public class GRoot : GComponent
	{
		public delegate void PopupHideCallback();

		private static GRoot _inst;
		public static GRoot inst
		{
			get
			{
				if ( _inst == null )
					throw new Exception( "GRoot not instantiate" );
				return _inst;
			}
		}

		private PopupHideCallback _popupHideCallback;
		private GObject _blocker;

		public GObject popup { get; private set; }

		internal GRoot()
		{
			_inst = this;

			this.name = "GRoot";
		}

		protected override void CreateDisplayObject()
		{
			base.CreateDisplayObject();

			this.displayObject.name = "GRoot";
			this.container.UnregisterAllEventTriggerTypes();
		}

		public void ShowPopup( GObject popup, GObject owner, PopupDirection direction, PopupConstraint constraint, PopupHideCallback hideCallback = null )
		{
			this._popupHideCallback = hideCallback;

			this._blocker = new GObject();
			this._blocker.onClick.Add( this.OnPopupHide );
			this.AddChild( this._blocker );

			DisplayObject displayObj = this._blocker.displayObject;
			ToolSet.SetAnchor( displayObj.rectTransform, AnchorType.Stretch_Stretch );
			displayObj.RegisterEventTriggerType( EventTriggerType.PointerClick );
			displayObj.name = "Blocker";
			displayObj.size = Vector2.zero;

			this.popup = popup;
			this.AddChild( this.popup );

			owner = owner ?? this;

			Vector3[] corners = new Vector3[4];
			owner.displayObject.rectTransform.GetWorldCorners( corners );
			Vector3 min = this.displayObject.rectTransform.InverseTransformPoint( corners[0] );
			Vector3 max = this.displayObject.rectTransform.InverseTransformPoint( corners[2] );

			PointerEventData pointerEventData = EventSystem.instance.pointerInput.GetLastPointerEventData( PointerInput.K_MOUSE_LEFT_ID );
			switch ( direction )
			{
				case PopupDirection.TouchPosition:
					this.popup.position = this.ScreenToLocal( pointerEventData.position );
					break;

				case PopupDirection.Left:
					this.popup.position = new Vector2( min.x - this.popup.actualSize.x, -max.y );
					break;

				case PopupDirection.Upward:
					this.popup.position = new Vector2( min.x, -max.y - this.popup.actualSize.y );
					break;

				case PopupDirection.Right:
					this.popup.position = new Vector2( max.x, -max.y );
					break;

				case PopupDirection.Downward:
					this.popup.position = new Vector2( min.x, -min.y );
					break;
			}

			if ( constraint > 0 )
			{
				corners = new Vector3[4];
				this.popup.displayObject.rectTransform.GetWorldCorners( corners );
				min = this.displayObject.rectTransform.InverseTransformPoint( corners[0] );
				max = this.displayObject.rectTransform.InverseTransformPoint( corners[2] );
				if ( ( constraint & PopupConstraint.Right ) > 0 &&
					 max.x > this.size.x )
					this.popup.position = new Vector2( this.size.x - this.popup.size.x, this.popup.position.y );
				if ( ( constraint & PopupConstraint.Left ) > 0 &&
					 min.x < 0 )
					this.popup.position = new Vector2( 0, this.popup.position.y );
				if ( ( constraint & PopupConstraint.Bottom ) > 0 &&
					 -min.y > this.size.y )
					this.popup.position = new Vector2( this.popup.position.x, this.size.y - this.popup.size.y );
				if ( ( constraint & PopupConstraint.Top ) > 0 &&
					 -max.y < 0 )
					this.popup.position = new Vector2( this.popup.position.x, 0 );
			}
		}

		private void OnPopupHide( EventContext context )
		{
			this.HidePopup();
		}

		public void HidePopup()
		{
			if ( this.popup == null )
				return;

			GObject mPopup = this.popup;
			this.popup = null;

			this._blocker.onClick.Remove( this.OnPopupHide );
			this._blocker.Dispose();
			this._blocker = null;

			if ( mPopup.parent == this )
				this.RemoveChild( mPopup );

			this._popupHideCallback?.Invoke();
		}

		protected override void HandleSizeChanged()
		{
			base.HandleSizeChanged();

			if ( this.displayObject.parent != null )
				this.position = ( this.displayObject.parent.size - this.size ) * 0.5f;
		}

		//protected internal override void SetBoundsChangedFlag()
		//{
		//	base.SetBoundsChangedFlag();

		//	this.size = this.contentSize;
		//}
	}
}