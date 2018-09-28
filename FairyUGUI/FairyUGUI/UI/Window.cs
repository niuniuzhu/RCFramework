using System.Collections.Generic;
using FairyUGUI.Core;
using FairyUGUI.Event;
using UnityEngine;

namespace FairyUGUI.UI
{
	public class Window : GComponent
	{
		public enum ModalType
		{
			Normal,
			Modal,
			Popup
		}

		public delegate void InitHandler();
		public delegate void ShownHandler();
		public delegate void HideHandler();

		public event InitHandler OnInit;
		public event ShownHandler OnShown;
		public event HideHandler OnHide;

		public override Vector2 position
		{
			get => base.position;
			set
			{
				if ( this._modalPane != null )
				{
					Vector2 deltaPos = this.position - value;
					this._modalPane.position += deltaPos;
				}
				base.position = value;
			}
		}

		private GComponent _contentPane;
		public GComponent contentPane
		{
			set
			{
				if ( this._contentPane == value )
					return;

				if ( this._contentPane != null )
					this.RemoveChild( this._contentPane );

				this._contentPane = value;

				if ( this._contentPane != null )
				{
					this.AddChild( this._contentPane );
					this.size = this._contentPane.size;
					this._contentPane.AddRelation( this, RelationType.Size );
					this._frame = this._contentPane.GetChild( "frame" ) as GComponent;
					if ( this._frame != null )
					{
						this.closeButton = this._frame.GetChild( "closeButton" );
						this.dragArea = this._frame.GetChild( "dragArea" );
						this.contentArea = this._frame.GetChild( "contentArea" );
					}
				}
				else
					this._frame = null;
			}
			get => this._contentPane;
		}

		private GComponent _frame;
		public GComponent frame => this._frame;

		private GObject _closeButton;
		public GObject closeButton
		{
			get => this._closeButton;
			set
			{
				this._closeButton?.onClick.Remove( this.OnCloseBtnClick );
				this._closeButton = value;
				this._closeButton?.onClick.Add( this.OnCloseBtnClick );
			}
		}

		private GObject _dragArea;
		public GObject dragArea
		{
			get => this._dragArea;
			set
			{
				if ( this._dragArea == value )
					return;

				if ( this._dragArea != null )
				{
					this._dragArea.displayObject.UnregisterEventTriggerType( EventTriggerType.BeginDrag );
					this._dragArea.displayObject.UnregisterEventTriggerType( EventTriggerType.Drag );
					this._dragArea.onBeginDrag.Remove( this.OnBeginDrag );
					this._dragArea.onDrag.Remove( this.OnDrag );
				}

				this._dragArea = value;
				if ( this._dragArea != null )
				{
					this._dragArea.displayObject.RegisterEventTriggerType( EventTriggerType.BeginDrag );
					this._dragArea.displayObject.RegisterEventTriggerType( EventTriggerType.Drag );
					this._dragArea.onBeginDrag.Add( this.OnBeginDrag );
					this._dragArea.onDrag.Add( this.OnDrag );
				}
			}
		}

		private GObject _contentArea;
		public GObject contentArea
		{
			get => this._contentArea;
			set => this._contentArea = value;
		}

		private GObject _modalWaitPane;
		public GObject modalWaitingPane => this._modalWaitPane;

		public bool modalWaiting => ( this._modalWaitPane != null ) && this._modalWaitPane.inContainer;

		public bool isShowing => this.parent != null;

		public bool isTop => this.parent != null && this.parent.GetChildIndex( this ) == this.parent.numChildren - 1;

		private ModalType _modalType;
		public ModalType modalType
		{
			get => this._modalType;
			set
			{
				if ( this._modalType == value )
					return;
				this._modalType = value;
				this.UpdateModalType();
			}
		}

		public delegate void AnimationCompleteHandler();

		private IWindowAnimation _showAnimation;
		public IWindowAnimation showAnimation
		{
			get => this._showAnimation;
			set
			{
				this._showAnimation = value;
				this._showAnimation.window = this;
			}
		}

		private IWindowAnimation _hideAnimation;
		public IWindowAnimation hideAnimation
		{
			get => this._hideAnimation;
			set
			{
				this._hideAnimation = value;
				this._hideAnimation.window = this;
			}
		}

		public bool bringToFontOnClick { get; set; }

		private GGraph _modalPane;

		private readonly List<IUISource> _uiSources = new List<IUISource>();

		private Vector2 _startDragPosition;
		private Vector2 _startDragPointerPosition;
		private bool _inited;
		private bool _loading;
		private int _requestingCmd;

		public Window()
		{
			this.bringToFontOnClick = UIConfig.bringWindowToFrontOnClick;

			this.displayObject.onAddedToStage.Add( this.OnAddedToStage );
			this.displayObject.onRemovedFromStage.Add( this.OnRemoveFromStage );

			this.rootContainer.gameObject.name = "Window";
		}

		protected override void InternalDispose()
		{
			this._requestingCmd = 0;

			this.showAnimation?.Cancel( false );
			this.hideAnimation?.Cancel( false );

			this.CloseModalWait();

			if ( this._modalWaitPane != null )
			{
				this._modalWaitPane.Dispose();
				this._modalWaitPane = null;
			}
			if ( this._modalPane != null )
			{
				this._modalPane.Dispose();
				this._modalPane = null;
			}

			base.InternalDispose();
		}

		private void Init()
		{
			if ( this._inited || this._loading )
				return;

			if ( this._uiSources.Count > 0 )
			{
				this._loading = false;
				int cnt = this._uiSources.Count;
				for ( int i = 0; i < cnt; i++ )
				{
					IUISource lib = this._uiSources[i];
					if ( !lib.loaded )
					{
						lib.Load( this.OnUILoadComplete );
						this._loading = true;
					}
				}

				if ( !this._loading )
					this.InternalInit();
			}
			else
				this.InternalInit();
		}

		protected virtual void InternalOnInit()
		{
		}

		private void OnUILoadComplete()
		{
			int cnt = this._uiSources.Count;
			for ( int i = 0; i < cnt; i++ )
			{
				IUISource lib = this._uiSources[i];
				if ( !lib.loaded )
					return;
			}

			this._loading = false;
			this.InternalInit();
		}

		private void InternalInit()
		{
			this._inited = true;
			this.InternalOnInit();

			this.OnInit?.Invoke();

			if ( this.isShowing )
			{
				this.OnShown?.Invoke();

				this.InternalOnShown();

				this.DoShowAnimation();
			}
		}

		/// <summary>
		/// Set a UISource to this window. It must call before the window is shown. When the window is first time to show,
		/// UISource.Load is called. Only after all UISource is loaded, the window will continue to init.
		/// 为窗口添加一个源。这个方法建议在构造函数调用。当窗口第一次显示前，UISource的Load方法将被调用，然后只有所有的UISource
		/// 都ready后，窗口才会继续初始化和显示。
		/// </summary>
		/// <param name="source"></param>
		public void AddUISource( IUISource source )
		{
			this._uiSources.Add( source );
		}

		public void Show( GComponent parent )
		{
			if ( this.isShowing )
				return;

			parent.AddChild( this );

			this.LayoutModal();
		}

		protected virtual void InternalOnShown()
		{
		}

		private void DoShowAnimation()
		{
			if ( this.showAnimation != null )
				this.showAnimation.Play( this.OnShowAnimationComplete );
			else
				this.OnShowAnimationComplete();
		}

		private void OnShowAnimationComplete()
		{
		}

		public void Hide( bool immediately = false )
		{
			if ( !this.isShowing )
				return;

			if ( immediately )
				this.parent.RemoveChild( this );
			else
				this.DoHideAnimation();
		}

		protected virtual void InternalOnHide()
		{
		}

		private void DoHideAnimation()
		{
			if ( this.hideAnimation != null )
				this.hideAnimation.Play( this.OnHideAnimationComplete );
			else
				this.OnHideAnimationComplete();
		}

		private void OnHideAnimationComplete()
		{
			this.Hide( true );
		}

		public void BringToFront()
		{
			this.parent?.SetChildIndex( this, this.parent.numChildren - 1 );
		}

		public void ShowModalWait( int requestingCmd = 0 )
		{
			if ( this.modalWaiting )
				return;

			if ( requestingCmd != 0 )
				this._requestingCmd = requestingCmd;

			if ( UIConfig.windowModalWaiting != null )
			{
				if ( this._modalWaitPane == null )
					this._modalWaitPane = UIPackage.CreateObjectFromURL( UIConfig.windowModalWaiting );

				this.AddChild( this._modalWaitPane );

				this.LayoutModalWaitPane();
			}
		}

		public bool CloseModalWait( int requestingCmd = 0 )
		{
			if ( !this.modalWaiting )
				return false;

			if ( requestingCmd != 0 )
			{
				if ( this._requestingCmd != requestingCmd )
					return false;
			}
			this._requestingCmd = 0;

			if ( this._modalWaitPane?.parent != null )
				this.RemoveChild( this._modalWaitPane );

			return true;
		}

		protected virtual void LayoutModalWaitPane()
		{
			if ( this._contentArea != null )
			{
				Vector3 worldMin = this._contentArea.LocalToWorld( Vector3.zero );
				Vector3 worldMax = this._contentArea.LocalToWorld( this._contentArea.size );
				Vector3 localMin = this.WorldToLocal( worldMin );
				Vector3 localMax = this.WorldToLocal( worldMax );

				this._modalWaitPane.size = localMax - localMin;
				this._modalWaitPane.position = localMin;
			}
			else
				this._modalWaitPane.size = this.size;
		}

		private void UpdateModalType()
		{
			switch ( this._modalType )
			{
				case ModalType.Popup:
				case ModalType.Modal:
					if ( this._modalPane == null )
					{
						this._modalPane = new GGraph();
						this._modalPane.color = new Color( 0, 0, 0, 0.4f );
						this._modalPane.enableDraw = true;
						this.LayoutModal();
						this.AddChildAt( this._modalPane, 0 );
						if ( this._modalType == ModalType.Popup )
							this._modalPane.onClick.Add( this.OnModalClick );
					}
					break;

				default:
					if ( this._modalPane != null )
					{
						this._modalPane.Dispose();
						this._modalPane = null;
					}
					break;
			}
		}

		private void LayoutModal()
		{
			if ( !this.isShowing || this._modalPane == null )
				return;

			this._modalPane.position = -this.position;
			this._modalPane.size = this.parent.size;
			this._modalPane.AddRelation( this.parent, RelationType.Size );
		}

		private void OnModalClick( EventContext context )
		{
			this.Hide();
		}

		private void OnCloseBtnClick( EventContext context )
		{
			this.Hide();
		}

		private void OnAddedToStage( EventContext context )
		{
			if ( !this._inited )
				this.Init();
			else
			{
				this.OnShown?.Invoke();

				this.InternalOnShown();

				this.DoShowAnimation();
			}
		}

		private void OnRemoveFromStage( EventContext context )
		{
			this.OnHide?.Invoke();

			this.InternalOnHide();
		}

		protected override void OnPointerDown( BaseEventData eventData )
		{
			if ( this.isShowing && this.bringToFontOnClick )
				this.BringToFront();
		}

		private void OnBeginDrag( EventContext context )
		{
			PointerEventData e = ( PointerEventData ) context.eventData;

			this._startDragPosition = this.position;

			RectTransformUtility.ScreenPointToLocalPointInRectangle( this.displayObject.rectTransform, e.position, Stage.inst.eventCamera, out this._startDragPointerPosition );

			e.StopPropagation();
		}

		private void OnDrag( EventContext context )
		{
			PointerEventData e = ( PointerEventData ) context.eventData;

			this.position = this._startDragPosition;

			RectTransformUtility.ScreenPointToLocalPointInRectangle( this.displayObject.rectTransform, e.position,
			                                                         Stage.inst.eventCamera, out Vector2 currPointerPosition );

			Vector2 delta = currPointerPosition - this._startDragPointerPosition;
			delta.y = -delta.y;
			this.position += delta;

			e.StopPropagation();
		}

		public void AddInitHandler( InitHandler callback )
		{
			this.OnInit += callback;
		}

		public void RemoveInitHandler( InitHandler callback )
		{
			this.OnInit -= callback;
		}

		public void AddShownHandler( ShownHandler callback )
		{
			this.OnShown += callback;
		}

		public void RemoveShownHandler( ShownHandler callback )
		{
			this.OnShown -= callback;
		}

		public void AddHideHandler( HideHandler callback )
		{
			this.OnHide += callback;
		}

		public void RemoveHideHandler( HideHandler callback )
		{
			this.OnHide -= callback;
		}
	}
}
