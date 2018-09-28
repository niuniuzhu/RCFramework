using FairyUGUI.Event;
using FairyUGUI.UI;
using Game.Task;
using UnityEngine;
using UnityEngine.UI;
using EventType = FairyUGUI.Event.EventType;
using Logger = Core.Misc.Logger;
using Object = UnityEngine.Object;

namespace FairyUGUI.Core
{
	public sealed class Stage : Container
	{
		public const string LAYER_NAME = "UI";

		static Stage _inst;
		public static Stage inst => _inst;

		public static void Instantiate()
		{
			if ( _inst == null ) { _inst = new Stage( null ); }
		}

		public Canvas canvas { get; private set; }

		private CanvasScaler _canvasScaler;

		public Camera eventCamera => ( this.canvas.renderMode == RenderMode.ScreenSpaceOverlay ||
									   ( this.canvas.renderMode == RenderMode.ScreenSpaceCamera && this.canvas.worldCamera == null ) )
										 ? null
										 : this.canvas.worldCamera ?? Camera.main;

		public float planeDistance => this.canvas.planeDistance;

		public bool pixelPerfect
		{
			get => this.canvas.pixelPerfect;
			set => this.canvas.pixelPerfect = value;
		}

		public float soundVolume { get; set; }

		public float contentScale { get; private set; }

		public Vector2 referenceResolution
		{
			get => this._canvasScaler.referenceResolution;
			set => this._canvasScaler.referenceResolution = value;
		}

		public float scaleFactor => this.canvas.scaleFactor;

		public float referencePixelsPerUnit
		{
			get => this._canvasScaler.referencePixelsPerUnit;
			set => this._canvasScaler.referencePixelsPerUnit = value;
		}

		public float fallbackScreenDPI
		{
			get => this._canvasScaler.fallbackScreenDPI;
			set => this._canvasScaler.fallbackScreenDPI = value;
		}

		public float dynamicPixelsPerUnit
		{
			get => this._canvasScaler.dynamicPixelsPerUnit;
			set => this._canvasScaler.dynamicPixelsPerUnit = value;
		}

		public float defaultSpriteDPI
		{
			get => this._canvasScaler.defaultSpriteDPI;
			set => this._canvasScaler.defaultSpriteDPI = value;
		}

		public float matchWidthOrHeight
		{
			get => this._canvasScaler.matchWidthOrHeight;
			set => this._canvasScaler.matchWidthOrHeight = value;
		}

		public CanvasScaler.ScreenMatchMode screenMatchMode
		{
			get => this._canvasScaler.screenMatchMode;
			set => this._canvasScaler.screenMatchMode = value;
		}

		public CanvasScaler.ScaleMode uiScaleMode
		{
			get => this._canvasScaler.uiScaleMode;
			set => this._canvasScaler.uiScaleMode = value;
		}

		private AudioSource _audio;

		private readonly UpdateContext _updateContext = new UpdateContext();
		private Vector2 _lastSize = new Vector2( -1, -1 );

		private bool _updateManually;
		public bool updateManually
		{
			get => this._updateManually;
			set
			{
				if ( this._updateManually == value )
					return;
				this._updateManually = value;
				this.UpdateUpdateManaged();
			}
		}

		public EventListener onKeyDown { get; private set; }
		public EventListener onKeyUp { get; private set; }

		private int _uv1UsingCount;

		private Stage( GObject owner )
			: base( owner )
		{
			_inst = this;
			this.stage = this;
			this.name = "Stage";
			this.UnregisterAllEventTriggerTypes();

			DisplayObjectRegister.RegisterDisplayObject( this );

			GRoot gRoot = new GRoot();
			this.AddChild( gRoot.displayObject );
			gRoot.size = this.size;

			TaskManager.instance.RegisterLateUpdateMethod( this.OnUpdate );
			Canvas.willRenderCanvases += this.OnWillRenderCanvases;
			this.soundVolume = 1;
			this.EnableSound();

			this.onKeyDown = new EventListener( this, EventType.Keydown );
			this.onKeyUp = new EventListener( this, EventType.Keyup );
		}

		protected override void InternalDispose()
		{
			TaskManager.instance.UnregisterLateUpdateMethod( this.OnUpdate );
			Canvas.willRenderCanvases -= this.OnWillRenderCanvases;

			this.RemoveChild( GRoot.inst.displayObject );
			GRoot.inst.Dispose();

			Object.DestroyImmediate( GameObject.Find( "UICamera" ) );
			Object.DestroyImmediate( this.gameObject );

			base.InternalDispose();

			_inst = null;
		}

		protected override void OnGameObjectCreated()
		{
			Object.DontDestroyOnLoad( this.gameObject );
			this.gameObject.layer = LayerMask.NameToLayer( LAYER_NAME );
			this.gameObject.hideFlags = HideFlags.None;
			this.gameObject.SetActive( true );

			Camera camera = new GameObject( "UICamera" ).AddComponent<Camera>();
			Object.DontDestroyOnLoad( camera.gameObject );
			camera.gameObject.layer = LayerMask.NameToLayer( LAYER_NAME );
			//camera.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
			camera.allowHDR = false;
			camera.allowMSAA = false;
			camera.useOcclusionCulling = false;
			camera.orthographic = true;
			camera.orthographicSize = 100;
			camera.nearClipPlane = 1f;
			camera.farClipPlane = 500f;
			camera.clearFlags = CameraClearFlags.Depth;
			camera.backgroundColor = new Color( 0, 0, 0, 0 );
			camera.cullingMask = 1 << LayerMask.NameToLayer( LAYER_NAME );
			camera.depth = 999;

			this.canvas = this.AddComponent<Canvas>();
			this.canvas.renderMode = RenderMode.ScreenSpaceCamera;
			this.canvas.worldCamera = camera;
			this.canvas.planeDistance = camera.farClipPlane / 2f;
			//this.canvas.pixelPerfect = true;

			this._canvasScaler = this.AddComponent<CanvasScaler>();
			this._canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			this._canvasScaler.referenceResolution = new Vector2( 1334, 750 );
			this._canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
			this._canvasScaler.matchWidthOrHeight = 1;
		}

		public void EnableSound()
		{
			if ( this._audio == null )
			{
				this._audio = this.gameObject.AddComponent<AudioSource>();
				this._audio.bypassEffects = true;
			}
		}

		public void DisableSound()
		{
			if ( this._audio != null )
			{
				Object.DestroyObject( this._audio );
				this._audio = null;
			}
		}

		public void PlayOneShotSound( AudioClip clip, float volumeScale )
		{
			if ( this._audio != null && this.soundVolume > 0 )
				this._audio.PlayOneShot( clip, volumeScale * this.soundVolume );
		}

		public void PlayOneShotSound( AudioClip clip )
		{
			if ( this._audio != null && this.soundVolume > 0 )
				this._audio.PlayOneShot( clip, this.soundVolume );
		}

		public void Update( float dt )
		{
			if ( !this._updateManually )
			{
				Logger.Log( "Update only can be called when updateManually was set to true." );
				return;
			}
			this.OnUpdate( dt );
		}

		private void OnUpdate( float dt )
		{
			if ( this._lastSize != this.size )
			{
				this.HandleSizeChanged();
				this._lastSize = this.size;
			}
			EventSystem.instance.Update();

			this._updateContext.Begin();
			this.Update( this._updateContext );
			this._updateContext.End();
		}

		private void OnWillRenderCanvases()
		{
			this.WillRenderCanvases();
		}

		private void UpdateUpdateManaged()
		{
			if ( !this._updateManually )
				TaskManager.instance.RegisterLateUpdateMethod( this.OnUpdate );
			else
				TaskManager.instance.UnregisterLateUpdateMethod( this.OnUpdate );
		}

		protected override void HandleSizeChanged()
		{
			base.HandleSizeChanged();
			GRoot.inst.size = this.size;
			//GRoot.inst.size = GRoot.inst.contentSize;
			//GRoot.inst.position = ( this.size - GRoot.inst.size ) * 0.5f;
		}

		internal void UseUV1()
		{
			++this._uv1UsingCount;
			if ( this._uv1UsingCount > 0 )
			{
				this.canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
			}
		}

		internal void UnuseUV1()
		{
			--this._uv1UsingCount;
			if ( this._uv1UsingCount <= 0 )
			{
				this.canvas.additionalShaderChannels &= ~AdditionalCanvasShaderChannels.TexCoord1;
			}
		}
	}
}