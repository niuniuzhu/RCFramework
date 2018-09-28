using FairyUGUI.UI;
using FairyUGUI.UI.UGUIExt;
using UnityEngine;

namespace FairyUGUI.Core
{
	public class Image : DisplayObject
	{
		private ImageEx _nImage;
		private ImageScaleMode _scaleMode;
		private ImageEx.Type _type;

		internal Image( GObject owner )
			: base( owner )
		{
		}

		internal override Vector2 size
		{
			set
			{
				base.size = value;
				this.SetActualType();
			}
		}

		internal ImageScaleMode scaleMode
		{
			get => this._scaleMode;
			set
			{
				this._scaleMode = value;
				switch ( this._scaleMode )
				{
					case ImageScaleMode.Grid9:
						this.type = ImageEx.Type.Sliced;
						break;

					case ImageScaleMode.Tile:
						this.type = ImageEx.Type.Tiled;
						break;

					case ImageScaleMode.Filled:
						this.type = ImageEx.Type.Filled;
						break;

					default:
						this.type = ImageEx.Type.Simple;
						break;
				}
			}
		}

		internal Rect? customRect
		{
			get => this._nImage.customRect;
			set => this._nImage.customRect = value;
		}

		internal NSprite nSprite
		{
			get => this._nImage.nSprite;
			set
			{
				if ( this._nImage.nSprite == value )
					return;

				this._nImage.nSprite = value;

				if ( this.material != null )
				{
					this.material = this._nImage.nSprite.associatedAlphaSplitTexture != null
										? MaterialManager.EnableAlphaTexture( this.material )
										: MaterialManager.DisableAlphaTexture( this.material );
				}

				this.SetActualType();
			}
		}

		internal NSprite maskSprite
		{
			get => this._nImage.maskSprite;
			set
			{
				if ( this._nImage.maskSprite == value )
					return;

				this._nImage.maskSprite = value;

				if ( this._nImage.maskSprite != null )
					Stage.inst.UseUV1();
				else
					Stage.inst.UnuseUV1();

				if ( this.material != null )
				{
					this.material = this._nImage.maskSprite != null
										? MaterialManager.EnableMaskTexture( this.material )
										: MaterialManager.DisableMaskTexture( this.material );
				}
			}
		}

		private ImageEx.Type type
		{
			set
			{
				if ( this._type == value )
					return;
				this._type = value;
				this.SetActualType();
			}
		}

		internal FlipType flipType
		{
			get => this._nImage.flipType;
			set => this._nImage.flipType = value;
		}

		internal ImageEx.FillMethod fillMethod
		{
			get => this._nImage.fillMethod;
			set => this._nImage.fillMethod = value;
		}

		internal int fillOrigin
		{
			get => this._nImage.fillOrigin;
			set => this._nImage.fillOrigin = value;
		}

		internal bool fillClockwise
		{
			get => this._nImage.fillClockwise;
			set => this._nImage.fillClockwise = value;
		}

		internal float fillAmount
		{
			get => this._nImage.fillAmount;
			set => this._nImage.fillAmount = value;
		}

		internal float alphaHitTestMinimumThreshold
		{
			get => this._nImage.alphaHitTestMinimumThreshold;
			set => this._nImage.alphaHitTestMinimumThreshold = value;
		}

		protected override void OnGameObjectCreated()
		{
			this.graphic = this._nImage = this.AddComponent<ImageEx>();
		}

		protected override void InternalDispose()
		{
			this._nImage = null;

			base.InternalDispose();
		}

		private void SetActualType()
		{
			if ( this._type == ImageEx.Type.Sliced && this.nSprite != null && this.size == this.nSprite.rect.size )
				this._nImage.type = ImageEx.Type.Simple;
			else
				this._nImage.type = this._type;
		}

		internal void SetNativeSize()
		{
			this._nImage.SetNativeSize();
		}

		internal override bool Raycast( Vector2 sp, Camera eventCamera )
		{
			if ( !base.Raycast( sp, eventCamera ) )
				return false;

			if ( this.eventGraphicOnly )
				return this._nImage.IsRaycastLocationValid( sp, eventCamera );

			return true;
		}

		protected internal override void HandleRemoveFromStage()
		{
			if ( this.visible && this.maskSprite != null )
				Stage.inst.UnuseUV1();

			base.HandleRemoveFromStage();
		}

		protected override void HandleVisibleChanged()
		{
			base.HandleVisibleChanged();

			if ( this.maskSprite == null )
				return;

			if ( this.visible )
				Stage.inst.UseUV1();
			else
				Stage.inst.UnuseUV1();
		}
	}
}