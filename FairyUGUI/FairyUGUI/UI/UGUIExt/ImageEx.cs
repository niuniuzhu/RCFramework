using Core.Math;
using FairyUGUI.Core;
using UnityEngine;
using UnityEngine.UI;
using Logger = Core.Misc.Logger;
using Rect = UnityEngine.Rect;

namespace FairyUGUI.UI.UGUIExt
{
	public class ImageEx : MaskableGraphic, ISerializationCallbackReceiver, ILayoutElement, ICanvasRaycastFilter
	{
		public enum Type
		{
			Simple,
			Sliced,
			Tiled,
			Filled
		}

		public enum FillMethod
		{
			None,
			Horizontal,
			Vertical,
			Radial90,
			Radial180,
			Radial360,
		}

		public Rect? customRect;

		[SerializeField]
		private NSprite _nSprite;
		public NSprite nSprite
		{
			get => this._nSprite;
			set
			{
				if ( this._nSprite == value )
					return;
				this._nSprite = value;

				this.SetAllDirty();
			}
		}

		private NSprite _maskSprite;
		internal NSprite maskSprite
		{
			get => this._maskSprite;
			set
			{
				if ( this._maskSprite == value )
					return;

				this._maskSprite = value;

				this.SetAllDirty();
			}
		}

		private Type _type;
		public Type type
		{
			get => this._type;
			set
			{
				if ( this._type == value )
					return;
				this._type = value;
				this.SetVerticesDirty();
			}
		}

		private bool _preserveAspect;

		public bool preserveAspect
		{
			get => this._preserveAspect;
			set
			{
				if ( this._preserveAspect == value )
					return;
				this._preserveAspect = value;
				this.SetVerticesDirty();
			}
		}

		private bool _fillCenter = true;

		public bool fillCenter
		{
			get => this._fillCenter;
			set
			{
				if ( this._fillCenter == value )
					return;
				this._fillCenter = value;
				this.SetVerticesDirty();
			}
		}

		private FillMethod _fillMethod;

		public FillMethod fillMethod
		{
			get => this._fillMethod;
			set
			{
				if ( this._fillMethod == value )
					return;
				this._fillMethod = value;
				this.CorrectFillOrigin();
				this.SetVerticesDirty();
			}
		}

		private float _fillAmount = 1.0f;
		public float fillAmount
		{
			get => this._fillAmount;
			set
			{
				value = Mathf.Clamp01( value );
				if ( this._fillAmount == value )
					return;
				this._fillAmount = value;
				this.SetVerticesDirty();
			}
		}

		private bool _fillClockwise = true;
		public bool fillClockwise
		{
			get => this._fillClockwise;
			set
			{
				if ( this._fillClockwise == value )
					return;
				this._fillClockwise = value;
				this.SetVerticesDirty();
			}
		}

		private int _fillOrigin0;
		private int _fillOrigin;
		public int fillOrigin
		{
			get => this._fillOrigin0;
			set
			{
				if ( this._fillOrigin0 == value )
					return;
				this._fillOrigin0 = value;
				this.CorrectFillOrigin();
				this.SetVerticesDirty();
			}
		}

		private FlipType _flipType;
		public FlipType flipType
		{
			get => this._flipType;
			set
			{
				if ( this._flipType == value )
					return;
				this._flipType = value;
				this.SetVerticesDirty();
			}
		}

		private float _alphaHitTestMinimumThreshold;
		public float alphaHitTestMinimumThreshold
		{
			get => this._alphaHitTestMinimumThreshold;
			set => this._alphaHitTestMinimumThreshold = value;
		}

		public override Texture mainTexture
		{
			get
			{
				if ( this.nSprite == null )
				{
					if ( this.material != null && this.material.mainTexture != null )
						return this.material.mainTexture;
					return s_WhiteTexture;
				}
				return this.nSprite.nTexture.texture;
			}
		}

		public bool hasBorder
		{
			get
			{
				if ( this.nSprite == null ) return false;
				Vector4 v = this.nSprite.border;
				return v.sqrMagnitude > 0f;
			}
		}

		public float pixelsPerUnit
		{
			get
			{
				float spritePixelsPerUnit = 100;
				if ( this.nSprite != null )
					spritePixelsPerUnit = this.nSprite.pixelsPerUnit;

				float referencePixelsPerUnit = 100;
				if ( this.canvas )
					referencePixelsPerUnit = this.canvas.referencePixelsPerUnit;

				return spritePixelsPerUnit / referencePixelsPerUnit;
			}
		}

		protected ImageEx()
		{
			this.useLegacyMeshGeneration = false;
		}

		private void CorrectFillOrigin()
		{
			switch ( this._fillMethod )
			{
				case FillMethod.Horizontal:
					this._fillOrigin0 = Mathf.Clamp( this._fillOrigin0, 0, 1 );
					break;

				case FillMethod.Vertical:
					this._fillOrigin0 = Mathf.Clamp( this._fillOrigin0, 0, 1 );
					this._fillOrigin = 1 - this._fillOrigin0;
					break;

				case FillMethod.Radial90:
					this._fillOrigin0 = Mathf.Clamp( this._fillOrigin0, 0, 3 );
					if ( this._fillOrigin0 == 0 )
						this._fillOrigin = 2;
					else if ( this._fillOrigin0 == 1 )
						this._fillOrigin = 0;
					else if ( this._fillOrigin0 == 2 )
						this._fillOrigin = 1;
					break;

				case FillMethod.Radial180:
					this._fillOrigin0 = Mathf.Clamp( this._fillOrigin0, 0, 3 );
					if ( this._fillOrigin0 == 0 )
						this._fillOrigin = 2;
					else if ( this._fillOrigin0 == 1 )
						this._fillOrigin = 0;
					else if ( this._fillOrigin0 == 2 )
						this._fillOrigin = 1;
					break;

				case FillMethod.Radial360:
					this._fillOrigin0 = Mathf.Clamp( this._fillOrigin0, 0, 3 );
					if ( this._fillOrigin0 == 0 )
						this._fillOrigin = 2;
					else if ( this._fillOrigin0 == 1 )
						this._fillOrigin = 0;
					else if ( this._fillOrigin0 == 2 )
						this._fillOrigin = 3;
					else
						this._fillOrigin = 1;
					break;
			}
		}

		public virtual void OnBeforeSerialize()
		{
		}

		public virtual void OnAfterDeserialize()
		{
			if ( this._fillOrigin < 0 )
				this._fillOrigin = 0;
			else if ( this._fillMethod == FillMethod.Horizontal && this._fillOrigin > 1 )
				this._fillOrigin = 0;
			else if ( this._fillMethod == FillMethod.Vertical && this._fillOrigin > 1 )
				this._fillOrigin = 0;
			else if ( this._fillOrigin > 3 )
				this._fillOrigin = 0;

			this._fillAmount = Mathf.Clamp( this._fillAmount, 0f, 1f );
		}

		private Vector4 GetDrawingDimensions( bool shouldPreserveAspect )
		{
			var padding = this.nSprite == null ? Vector4.zero : UnityEngine.Sprites.DataUtility.GetPadding( this.nSprite.sprite );
			var size = this.nSprite == null ? Vector2.zero : new Vector2( this.nSprite.rect.width, this.nSprite.rect.height );

			Rect r = this.GetPixelAdjustedRect();

			int spriteW = Mathf.RoundToInt( size.x );
			int spriteH = Mathf.RoundToInt( size.y );

			var v = new Vector4(
					padding.x / spriteW,
					padding.y / spriteH,
					( spriteW - padding.z ) / spriteW,
					( spriteH - padding.w ) / spriteH );

			if ( shouldPreserveAspect && size.sqrMagnitude > 0.0f )
			{
				var spriteRatio = size.x / size.y;
				var rectRatio = r.width / r.height;

				if ( spriteRatio > rectRatio )
				{
					var oldHeight = r.height;
					r.height = r.width * ( 1.0f / spriteRatio );
					r.y += ( oldHeight - r.height ) * this.rectTransform.pivot.y;
				}
				else
				{
					var oldWidth = r.width;
					r.width = r.height * spriteRatio;
					r.x += ( oldWidth - r.width ) * this.rectTransform.pivot.x;
				}
			}

			v = new Vector4(
					r.x + r.width * v.x,
					r.y + r.height * v.y,
					r.x + r.width * v.z,
					r.y + r.height * v.w
					);

			return v;
		}

		public override void SetNativeSize()
		{
			if ( this.nSprite != null )
			{
				float w = this.nSprite.rect.width / this.pixelsPerUnit;
				float h = this.nSprite.rect.height / this.pixelsPerUnit;
				this.rectTransform.anchorMax = this.rectTransform.anchorMin;
				this.rectTransform.sizeDelta = new Vector2( w, h );
				this.SetAllDirty();
			}
		}

		protected override void OnPopulateMesh( VertexHelper toFill )
		{
			if ( this.nSprite == null )
			{
				base.OnPopulateMesh( toFill );
				return;
			}

			switch ( this.type )
			{
				case Type.Simple:
					this.GenerateSimpleSprite( toFill, this._preserveAspect );
					break;
				case Type.Sliced:
					this.GenerateSlicedSprite( toFill );
					break;
				case Type.Tiled:
					this.GenerateTiledSprite( toFill );
					break;
				case Type.Filled:
					this.GenerateFilledSprite( toFill, this._preserveAspect );
					break;
			}
		}

		protected override void UpdateMaterial()
		{
			base.UpdateMaterial();

			// check if this sprite has an associated alpha texture (generated when splitting RGBA = RGB + A as two textures without alpha)
			if ( this._nSprite == null )
			{
				this.canvasRenderer.SetAlphaTexture( null );
				return;
			}

			if ( this._nSprite.associatedAlphaSplitTexture != null )
				this.canvasRenderer.SetAlphaTexture( this._nSprite.associatedAlphaSplitTexture );

			if ( this._maskSprite != null )
				this.material.SetTexture( "_MaskTex", this._maskSprite.nTexture.texture );
		}

		#region Various fill functions

		static readonly Vector2[] S_VERT_SCRATCH = new Vector2[4];
		static readonly Vector2[] S_UV_SCRATCH = new Vector2[4];

		static readonly Vector3[] S_XY = new Vector3[4];
		static readonly Vector3[] S_UV = new Vector3[4];

		private void GenerateSimpleSprite( VertexHelper vh, bool lPreserveAspect )
		{
			Vector4 v = this.GetDrawingDimensions( lPreserveAspect );

			if ( this.customRect != null )
			{
				Rect rect = this.customRect.Value;
				v.x += rect.xMin;
				v.z = v.x + rect.width;
				v.w -= rect.yMin;
				v.y = v.w - rect.height;
			}

			Vector4 uv = ( this.nSprite != null ) ? UnityEngine.Sprites.DataUtility.GetOuterUV( this.nSprite.sprite ) : Vector4.zero;

			Vector4 uvmaskTransform = Vector4.zero;
			if ( this.maskSprite != null )
			{
				Vector4 uvmask = UnityEngine.Sprites.DataUtility.GetOuterUV( this.maskSprite.sprite );
				uvmaskTransform = new Vector4( uvmask.x, uvmask.y,
					( uvmask.z - uvmask.x ) / ( uv.z - uv.x ), ( uvmask.w - uvmask.y ) / ( uv.w - uv.y ) );
			}

			if ( this._flipType == FlipType.Both || this._flipType == FlipType.Horizontal )
				FlipHorizontal( ref uv );
			if ( this._flipType == FlipType.Both || this._flipType == FlipType.Vertical )
				FlipVertical( ref uv );

			Color color32 = this.color;
			vh.Clear();
			vh.AddVert( new Vector3( v.x, v.y ), color32, new Vector2( uv.x, uv.y ) );
			vh.AddVert( new Vector3( v.x, v.w ), color32, new Vector2( uv.x, uv.w ) );
			vh.AddVert( new Vector3( v.z, v.w ), color32, new Vector2( uv.z, uv.w ) );
			vh.AddVert( new Vector3( v.z, v.y ), color32, new Vector2( uv.z, uv.y ) );

			vh.AddTriangle( 0, 1, 2 );
			vh.AddTriangle( 2, 3, 0 );

			if ( this.maskSprite != null )
				this.AddMaskUVs( vh, uvmaskTransform );
		}

		private void GenerateSlicedSprite( VertexHelper toFill )
		{
			if ( !this.hasBorder )
			{
				this.GenerateSimpleSprite( toFill, false );
				return;
			}

			Vector4 outer, inner, padding, border;
			if ( this.nSprite != null )
			{
				outer = UnityEngine.Sprites.DataUtility.GetOuterUV( this.nSprite.sprite );
				inner = UnityEngine.Sprites.DataUtility.GetInnerUV( this.nSprite.sprite );
				padding = UnityEngine.Sprites.DataUtility.GetPadding( this.nSprite.sprite );
				border = this.nSprite.border;
			}
			else
			{
				outer = Vector4.zero;
				inner = Vector4.zero;
				padding = Vector4.zero;
				border = Vector4.zero;
			}

			Vector4 uvmaskTransform = Vector4.zero;
			if ( this.maskSprite != null )
			{
				Vector4 uvmask = UnityEngine.Sprites.DataUtility.GetOuterUV( this.maskSprite.sprite );
				uvmaskTransform = new Vector4( uvmask.x, uvmask.y,
					( uvmask.z - uvmask.x ) / ( outer.z - outer.x ), ( uvmask.w - uvmask.y ) / ( outer.w - outer.y ) );
			}

			if ( this._flipType == FlipType.Both || this._flipType == FlipType.Horizontal )
			{
				FlipHorizontal( ref outer );
				FlipHorizontal( ref inner );
				FlipHorizontal( ref padding );
				FlipHorizontal( ref border );
			}
			if ( this._flipType == FlipType.Both || this._flipType == FlipType.Vertical )
			{
				FlipVertical( ref outer );
				FlipVertical( ref inner );
				FlipVertical( ref padding );
				FlipVertical( ref border );
			}

			Rect rect = this.GetPixelAdjustedRect();
			Vector4 adjustedBorders = this.GetAdjustedBorders( border / this.pixelsPerUnit, rect );
			padding = padding / this.pixelsPerUnit;

			S_VERT_SCRATCH[0] = new Vector2( padding.x, padding.y );
			S_VERT_SCRATCH[3] = new Vector2( rect.width - padding.z, rect.height - padding.w );

			S_VERT_SCRATCH[1].x = adjustedBorders.x;
			S_VERT_SCRATCH[1].y = adjustedBorders.y;

			S_VERT_SCRATCH[2].x = rect.width - adjustedBorders.z;
			S_VERT_SCRATCH[2].y = rect.height - adjustedBorders.w;

			for ( int i = 0; i < 4; ++i )
			{
				S_VERT_SCRATCH[i].x += rect.x;
				S_VERT_SCRATCH[i].y += rect.y;
			}

			S_UV_SCRATCH[0] = new Vector2( outer.x, outer.y );
			S_UV_SCRATCH[1] = new Vector2( inner.x, inner.y );
			S_UV_SCRATCH[2] = new Vector2( inner.z, inner.w );
			S_UV_SCRATCH[3] = new Vector2( outer.z, outer.w );

			toFill.Clear();

			for ( int x = 0; x < 3; ++x )
			{
				int x2 = x + 1;

				for ( int y = 0; y < 3; ++y )
				{
					if ( !this._fillCenter && x == 1 && y == 1 )
						continue;

					int y2 = y + 1;

					AddQuad( toFill,
						new Vector2( S_VERT_SCRATCH[x].x, S_VERT_SCRATCH[y].y ),
						new Vector2( S_VERT_SCRATCH[x2].x, S_VERT_SCRATCH[y2].y ),
						this.color,
						new Vector2( S_UV_SCRATCH[x].x, S_UV_SCRATCH[y].y ),
						new Vector2( S_UV_SCRATCH[x2].x, S_UV_SCRATCH[y2].y ) );
				}
			}

			if ( this.maskSprite != null )
				this.AddMaskUVs( toFill, uvmaskTransform );
		}

		private void GenerateTiledSprite( VertexHelper toFill )
		{
			Vector4 outer, inner, border;
			Vector2 spriteSize;

			if ( this.nSprite != null )
			{
				outer = UnityEngine.Sprites.DataUtility.GetOuterUV( this.nSprite.sprite );
				inner = UnityEngine.Sprites.DataUtility.GetInnerUV( this.nSprite.sprite );
				border = this.nSprite.border;
				spriteSize = this.nSprite.rect.size;
			}
			else
			{
				outer = Vector4.zero;
				inner = Vector4.zero;
				border = Vector4.zero;
				spriteSize = Vector2.one * 100;
			}

			Vector4 uvmaskTransform = Vector4.zero;
			if ( this.maskSprite != null )
			{
				Vector4 uvmask = UnityEngine.Sprites.DataUtility.GetOuterUV( this.maskSprite.sprite );
				uvmaskTransform = new Vector4( uvmask.x, uvmask.y,
					( uvmask.z - uvmask.x ) / ( outer.z - outer.x ), ( uvmask.w - uvmask.y ) / ( outer.w - outer.y ) );
			}

			if ( this._flipType == FlipType.Both || this._flipType == FlipType.Horizontal )
			{
				FlipHorizontal( ref outer );
				FlipHorizontal( ref inner );
				FlipHorizontal( ref border );
			}
			if ( this._flipType == FlipType.Both || this._flipType == FlipType.Vertical )
			{
				FlipVertical( ref outer );
				FlipVertical( ref inner );
				FlipVertical( ref border );
			}

			Rect rect = this.GetPixelAdjustedRect();
			float tileWidth = ( spriteSize.x - border.x - border.z ) / this.pixelsPerUnit;
			float tileHeight = ( spriteSize.y - border.y - border.w ) / this.pixelsPerUnit;
			border = this.GetAdjustedBorders( border / this.pixelsPerUnit, rect );

			Vector2 uvMin = new Vector2( inner.x, inner.y );
			Vector2 uvMax = new Vector2( inner.z, inner.w );

			// Min to max max range for tiled region in coordinates relative to lower left corner.
			float xMin = border.x;
			float xMax = rect.width - border.z;
			float yMin = border.y;
			float yMax = rect.height - border.w;

			toFill.Clear();
			float clippedX;
			float clippedY = uvMin.y;

			// if either with is zero we cant tile so just assume it was the full width.
			if ( tileWidth <= 0 )
				tileWidth = xMax - xMin;

			if ( tileHeight <= 0 )
				tileHeight = yMax - yMin;

			if ( this._nSprite != null && ( this.hasBorder || this._nSprite.packed || this.mainTexture.wrapMode != TextureWrapMode.Repeat ) )
			{
				// Sprite has border, or is not in repeat mode, or cannot be repeated because of packing.
				// We cannot use texture tiling so we will generate a mesh of quads to tile the texture.

				// Evaluate how many vertices we will generate. Limit this number to something sane,
				// especially since meshes can not have more than 65000 vertices.

				int nTilesW;
				int nTilesH;
				if ( this._fillCenter )
				{
					nTilesW = MathUtils.CeilToInt( ( xMax - xMin ) / tileWidth );
					nTilesH = MathUtils.CeilToInt( ( yMax - yMin ) / tileHeight );

					int nVertices;
					if ( this.hasBorder )
					{
						nVertices = ( nTilesW + 2 ) * ( nTilesH + 2 ) * 4; // 4 vertices per tile
					}
					else
					{
						nVertices = nTilesW * nTilesH * 4; // 4 vertices per tile
					}

					if ( nVertices > 65000 )
					{
						Logger.Error( "Too many sprite tiles on Image \"" + this.name + "\". The tile size will be increased. To remove the limit on the number of tiles, convert the Sprite to an Advanced texture, remove the borders, clear the Packing tag and set the Wrap mode to Repeat." );

						const float maxTiles = 65000.0f / 4.0f; // Max number of vertices is 65000; 4 vertices per tile.
						float imageRatio;
						if ( this.hasBorder )
							imageRatio = ( nTilesW + 2.0f ) / ( nTilesH + 2.0f );
						else
							imageRatio = ( float )nTilesW / nTilesH;

						float targetTilesW = MathUtils.Sqrt( maxTiles / imageRatio );
						float targetTilesH = targetTilesW * imageRatio;
						if ( this.hasBorder )
						{
							targetTilesW -= 2;
							targetTilesH -= 2;
						}

						nTilesW = MathUtils.FloorToInt( targetTilesW );
						nTilesH = MathUtils.FloorToInt( targetTilesH );
						tileWidth = ( xMax - xMin ) / nTilesW;
						tileHeight = ( yMax - yMin ) / nTilesH;
					}
				}
				else
				{
					if ( this.hasBorder )
					{
						// Texture on the border is repeated only in one direction.
						nTilesW = ( int )MathUtils.Ceiling( ( xMax - xMin ) / tileWidth );
						nTilesH = ( int )MathUtils.Ceiling( ( yMax - yMin ) / tileHeight );
						int nVertices = ( nTilesH + nTilesW + 2 /*corners*/) * 2 /*sides*/ * 4 /*vertices per tile*/;
						if ( nVertices > 65000 )
						{
							Logger.Error( "Too many sprite tiles on Image \"" + this.name + "\". The tile size will be increased. To remove the limit on the number of tiles, convert the Sprite to an Advanced texture, remove the borders, clear the Packing tag and set the Wrap mode to Repeat." );

							const float maxTiles = 65000.0f / 4.0f; // Max number of vertices is 65000; 4 vertices per tile.
							float imageRatio = ( float )nTilesW / nTilesH;
							float targetTilesW = ( maxTiles - 4 /*corners*/) / ( 2f * ( 1.0f + imageRatio ) );
							float targetTilesH = targetTilesW * imageRatio;

							nTilesW = MathUtils.FloorToInt( targetTilesW );
							nTilesH = MathUtils.FloorToInt( targetTilesH );
							tileWidth = ( xMax - xMin ) / nTilesW;
							tileHeight = ( yMax - yMin ) / nTilesH;
						}
					}
				}

				if ( this._fillCenter )
				{
					for ( float y1 = yMax; y1 > yMin; y1 -= tileHeight )
					{
						float y2 = y1 - tileHeight;
						if ( y2 < yMin )
						{
							clippedY = uvMax.y - ( uvMax.y - uvMin.y ) * ( y1 - yMin ) / ( y1 - y2 );
							y2 = yMin;
						}

						clippedX = uvMax.x;
						for ( float x1 = xMin; x1 < xMax; x1 += tileWidth )
						{
							float x2 = x1 + tileWidth;
							if ( x2 > xMax )
							{
								clippedX = uvMin.x + ( uvMax.x - uvMin.x ) * ( xMax - x1 ) / ( x2 - x1 );
								x2 = xMax;
							}
							AddQuad( toFill, new Vector2( x1, y2 ) + rect.position,
								new Vector2( x2, y1 ) + rect.position,
								this.color, new Vector2( uvMin.x, clippedY ), new Vector2( clippedX, uvMax.y ) );
						}
					}
				}

				if ( this.hasBorder )
				{
					clippedX = uvMax.x;
					clippedY = uvMin.y;
					for ( float y1 = yMax; y1 > yMin; y1 -= tileHeight )
					{
						float y2 = y1 - tileHeight;
						if ( y2 < yMin )
						{
							clippedY = uvMax.y - ( uvMax.y - uvMin.y ) * ( y1 - yMin ) / ( y1 - y2 );
							y2 = yMin;
						}
						AddQuad( toFill,
							new Vector2( 0, y1 ) + rect.position,
							new Vector2( xMin, y2 ) + rect.position,
							this.color,
							new Vector2( outer.x, uvMin.y ),
							new Vector2( uvMin.x, clippedY ) );
						AddQuad( toFill,
							new Vector2( xMax, y1 ) + rect.position,
							new Vector2( rect.width, y2 ) + rect.position,
							this.color,
							new Vector2( uvMax.x, uvMin.y ),
							new Vector2( outer.z, clippedY ) );
					}

					// Bottom and top tiled border
					for ( float x1 = xMin; x1 < xMax; x1 += tileWidth )
					{
						float x2 = x1 + tileWidth;
						if ( x2 > xMax )
						{
							clippedX = uvMin.x + ( uvMax.x - uvMin.x ) * ( xMax - x1 ) / ( x2 - x1 );
							x2 = xMax;
						}
						AddQuad( toFill,
							new Vector2( x1, 0 ) + rect.position,
							new Vector2( x2, yMin ) + rect.position,
							this.color,
							new Vector2( uvMin.x, outer.y ),
							new Vector2( clippedX, uvMin.y ) );
						AddQuad( toFill,
							new Vector2( x1, yMax ) + rect.position,
							new Vector2( x2, rect.height ) + rect.position,
							this.color,
							new Vector2( uvMin.x, uvMax.y ),
							new Vector2( clippedX, outer.w ) );
					}

					// Corners
					AddQuad( toFill,
						new Vector2( 0, 0 ) + rect.position,
						new Vector2( xMin, yMin ) + rect.position,
						this.color,
						new Vector2( outer.x, outer.y ),
						new Vector2( uvMin.x, uvMin.y ) );
					AddQuad( toFill,
						new Vector2( xMax, 0 ) + rect.position,
						new Vector2( rect.width, yMin ) + rect.position,
						this.color,
						new Vector2( uvMax.x, outer.y ),
						new Vector2( outer.z, uvMin.y ) );
					AddQuad( toFill,
						new Vector2( 0, yMax ) + rect.position,
						new Vector2( xMin, rect.height ) + rect.position,
						this.color,
						new Vector2( outer.x, uvMax.y ),
						new Vector2( uvMin.x, outer.w ) );
					AddQuad( toFill,
						new Vector2( xMax, yMax ) + rect.position,
						new Vector2( rect.width, rect.height ) + rect.position,
						this.color,
						new Vector2( uvMax.x, uvMax.y ),
						new Vector2( outer.z, outer.w ) );
				}
			}
			else
			{
				// Texture has no border, is in repeat mode and not packed. Use texture tiling.
				Vector2 uvScale = new Vector2( ( xMax - xMin ) / tileWidth, ( yMax - yMin ) / tileHeight );

				if ( this._fillCenter )
				{
					AddQuad( toFill, new Vector2( xMin, yMin ) + rect.position, new Vector2( xMax, yMax ) + rect.position, this.color, Vector2.Scale( uvMin, uvScale ), Vector2.Scale( uvMax, uvScale ) );
				}
			}

			if ( this.maskSprite != null )
				this.AddMaskUVs( toFill, uvmaskTransform );
		}

		static void AddQuad( VertexHelper vertexHelper, Vector3[] quadPositions, Color32 color, Vector3[] quadUVs )
		{
			int startIndex = vertexHelper.currentVertCount;

			for ( int i = 0; i < 4; ++i )
				vertexHelper.AddVert( quadPositions[i], color, quadUVs[i] );

			vertexHelper.AddTriangle( startIndex, startIndex + 1, startIndex + 2 );
			vertexHelper.AddTriangle( startIndex + 2, startIndex + 3, startIndex );
		}

		static void AddQuad( VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax )
		{
			int startIndex = vertexHelper.currentVertCount;

			vertexHelper.AddVert( new Vector3( posMin.x, posMin.y, 0 ), color, new Vector2( uvMin.x, uvMin.y ) );
			vertexHelper.AddVert( new Vector3( posMin.x, posMax.y, 0 ), color, new Vector2( uvMin.x, uvMax.y ) );
			vertexHelper.AddVert( new Vector3( posMax.x, posMax.y, 0 ), color, new Vector2( uvMax.x, uvMax.y ) );
			vertexHelper.AddVert( new Vector3( posMax.x, posMin.y, 0 ), color, new Vector2( uvMax.x, uvMin.y ) );

			vertexHelper.AddTriangle( startIndex, startIndex + 1, startIndex + 2 );
			vertexHelper.AddTriangle( startIndex + 2, startIndex + 3, startIndex );
		}

		private Vector4 GetAdjustedBorders( Vector4 border, Rect adjustedRect )
		{
			Rect originalRect = this.rectTransform.rect;

			for ( int axis = 0; axis <= 1; axis++ )
			{
				float borderScaleRatio;

				// The adjusted rect (adjusted for pixel correctness)
				// may be slightly larger than the original rect.
				// Adjust the border to match the adjustedRect to avoid
				// small gaps between borders (case 833201).
				if ( originalRect.size[axis] != 0 )
				{
					borderScaleRatio = adjustedRect.size[axis] / originalRect.size[axis];
					border[axis] *= borderScaleRatio;
					border[axis + 2] *= borderScaleRatio;
				}

				// If the rect is smaller than the combined borders, then there's not room for the borders at their normal size.
				// In order to avoid artefacts with overlapping borders, we scale the borders down to fit.
				float combinedBorders = border[axis] + border[axis + 2];
				if ( adjustedRect.size[axis] < combinedBorders && combinedBorders != 0 )
				{
					borderScaleRatio = adjustedRect.size[axis] / combinedBorders;
					border[axis] *= borderScaleRatio;
					border[axis + 2] *= borderScaleRatio;
				}
			}
			return border;
		}



		void GenerateFilledSprite( VertexHelper toFill, bool preserveAspect )
		{
			toFill.Clear();

			if ( this._fillAmount < 0.001f )
				return;

			Vector4 v = this.GetDrawingDimensions( preserveAspect );
			Vector4 outer = this.nSprite != null ? UnityEngine.Sprites.DataUtility.GetOuterUV( this.nSprite.sprite ) : Vector4.zero;

			float tx0 = outer.x;
			float ty0 = outer.y;
			float tx1 = outer.z;
			float ty1 = outer.w;

			// Horizontal and vertical filled sprites are simple -- just end the Image prematurely
			if ( this._fillMethod == FillMethod.Horizontal || this._fillMethod == FillMethod.Vertical )
			{
				if ( this.fillMethod == FillMethod.Horizontal )
				{
					float fill = ( tx1 - tx0 ) * this._fillAmount;

					if ( this._fillOrigin == 1 )
					{
						v.x = v.z - ( v.z - v.x ) * this._fillAmount;
						tx0 = tx1 - fill;
					}
					else
					{
						v.z = v.x + ( v.z - v.x ) * this._fillAmount;
						tx1 = tx0 + fill;
					}
				}
				else if ( this.fillMethod == FillMethod.Vertical )
				{
					float fill = ( ty1 - ty0 ) * this._fillAmount;

					if ( this._fillOrigin == 1 )
					{
						v.y = v.w - ( v.w - v.y ) * this._fillAmount;
						ty0 = ty1 - fill;
					}
					else
					{
						v.w = v.y + ( v.w - v.y ) * this._fillAmount;
						ty1 = ty0 + fill;
					}
				}
			}

			S_XY[0] = new Vector2( v.x, v.y );
			S_XY[1] = new Vector2( v.x, v.w );
			S_XY[2] = new Vector2( v.z, v.w );
			S_XY[3] = new Vector2( v.z, v.y );

			S_UV[0] = new Vector2( tx0, ty0 );
			S_UV[1] = new Vector2( tx0, ty1 );
			S_UV[2] = new Vector2( tx1, ty1 );
			S_UV[3] = new Vector2( tx1, ty0 );

			{
				if ( this._fillAmount < 1f && this._fillMethod != FillMethod.Horizontal && this._fillMethod != FillMethod.Vertical )
				{
					if ( this.fillMethod == FillMethod.Radial90 )
					{
						if ( RadialCut( S_XY, S_UV, this._fillAmount, this._fillClockwise, this._fillOrigin ) )
							AddQuad( toFill, S_XY, this.color, S_UV );
					}
					else if ( this.fillMethod == FillMethod.Radial180 )
					{
						for ( int side = 0; side < 2; ++side )
						{
							float fx0, fx1, fy0, fy1;
							int even = this._fillOrigin > 1 ? 1 : 0;

							if ( this._fillOrigin == 0 || this._fillOrigin == 2 )
							{
								fy0 = 0f;
								fy1 = 1f;
								if ( side == even )
								{
									fx0 = 0f;
									fx1 = 0.5f;
								}
								else
								{
									fx0 = 0.5f;
									fx1 = 1f;
								}
							}
							else
							{
								fx0 = 0f;
								fx1 = 1f;
								if ( side == even )
								{
									fy0 = 0.5f;
									fy1 = 1f;
								}
								else
								{
									fy0 = 0f;
									fy1 = 0.5f;
								}
							}

							S_XY[0].x = Mathf.Lerp( v.x, v.z, fx0 );
							S_XY[1].x = S_XY[0].x;
							S_XY[2].x = Mathf.Lerp( v.x, v.z, fx1 );
							S_XY[3].x = S_XY[2].x;

							S_XY[0].y = Mathf.Lerp( v.y, v.w, fy0 );
							S_XY[1].y = Mathf.Lerp( v.y, v.w, fy1 );
							S_XY[2].y = S_XY[1].y;
							S_XY[3].y = S_XY[0].y;

							S_UV[0].x = Mathf.Lerp( tx0, tx1, fx0 );
							S_UV[1].x = S_UV[0].x;
							S_UV[2].x = Mathf.Lerp( tx0, tx1, fx1 );
							S_UV[3].x = S_UV[2].x;

							S_UV[0].y = Mathf.Lerp( ty0, ty1, fy0 );
							S_UV[1].y = Mathf.Lerp( ty0, ty1, fy1 );
							S_UV[2].y = S_UV[1].y;
							S_UV[3].y = S_UV[0].y;

							float val = this._fillClockwise ? this.fillAmount * 2f - side : this._fillAmount * 2f - ( 1 - side );

							if ( RadialCut( S_XY, S_UV, Mathf.Clamp01( val ), this._fillClockwise, ( ( side + this._fillOrigin + 3 ) % 4 ) ) )
							{
								AddQuad( toFill, S_XY, this.color, S_UV );
							}
						}
					}
					else if ( this.fillMethod == FillMethod.Radial360 )
					{
						for ( int corner = 0; corner < 4; ++corner )
						{
							float fx0, fx1, fy0, fy1;

							if ( corner < 2 )
							{
								fx0 = 0f;
								fx1 = 0.5f;
							}
							else
							{
								fx0 = 0.5f;
								fx1 = 1f;
							}

							if ( corner == 0 || corner == 3 )
							{
								fy0 = 0f;
								fy1 = 0.5f;
							}
							else
							{
								fy0 = 0.5f;
								fy1 = 1f;
							}

							S_XY[0].x = Mathf.Lerp( v.x, v.z, fx0 );
							S_XY[1].x = S_XY[0].x;
							S_XY[2].x = Mathf.Lerp( v.x, v.z, fx1 );
							S_XY[3].x = S_XY[2].x;

							S_XY[0].y = Mathf.Lerp( v.y, v.w, fy0 );
							S_XY[1].y = Mathf.Lerp( v.y, v.w, fy1 );
							S_XY[2].y = S_XY[1].y;
							S_XY[3].y = S_XY[0].y;

							S_UV[0].x = Mathf.Lerp( tx0, tx1, fx0 );
							S_UV[1].x = S_UV[0].x;
							S_UV[2].x = Mathf.Lerp( tx0, tx1, fx1 );
							S_UV[3].x = S_UV[2].x;

							S_UV[0].y = Mathf.Lerp( ty0, ty1, fy0 );
							S_UV[1].y = Mathf.Lerp( ty0, ty1, fy1 );
							S_UV[2].y = S_UV[1].y;
							S_UV[3].y = S_UV[0].y;

							float val = this._fillClockwise ?
								this._fillAmount * 4f - ( ( corner + this._fillOrigin ) % 4 ) :
								this._fillAmount * 4f - ( 3 - ( ( corner + this._fillOrigin ) % 4 ) );

							if ( RadialCut( S_XY, S_UV, Mathf.Clamp01( val ), this._fillClockwise, ( ( corner + 2 ) % 4 ) ) )
								AddQuad( toFill, S_XY, this.color, S_UV );
						}
					}
				}
				else
				{
					AddQuad( toFill, S_XY, this.color, S_UV );
				}
			}
		}

		/// <summary>
		/// Adjust the specified quad, making it be radially filled instead.
		/// </summary>

		static bool RadialCut( Vector3[] xy, Vector3[] uv, float fill, bool invert, int corner )
		{
			// Nothing to fill
			if ( fill < 0.001f ) return false;

			// Even corners invert the fill direction
			if ( ( corner & 1 ) == 1 ) invert = !invert;

			// Nothing to adjust
			if ( !invert && fill > 0.999f ) return true;

			// Convert 0-1 value into 0 to 90 degrees angle in radians
			float angle = Mathf.Clamp01( fill );
			if ( invert ) angle = 1f - angle;
			angle *= 90f * Mathf.Deg2Rad;

			// Calculate the effective X and Y factors
			float cos = Mathf.Cos( angle );
			float sin = Mathf.Sin( angle );

			RadialCut( xy, cos, sin, invert, corner );
			RadialCut( uv, cos, sin, invert, corner );
			return true;
		}

		/// <summary>
		/// Adjust the specified quad, making it be radially filled instead.
		/// </summary>

		static void RadialCut( Vector3[] xy, float cos, float sin, bool invert, int corner )
		{
			int i0 = corner;
			int i1 = ( ( corner + 1 ) % 4 );
			int i2 = ( ( corner + 2 ) % 4 );
			int i3 = ( ( corner + 3 ) % 4 );

			if ( ( corner & 1 ) == 1 )
			{
				if ( sin > cos )
				{
					cos /= sin;
					sin = 1f;

					if ( invert )
					{
						xy[i1].x = Mathf.Lerp( xy[i0].x, xy[i2].x, cos );
						xy[i2].x = xy[i1].x;
					}
				}
				else if ( cos > sin )
				{
					sin /= cos;
					cos = 1f;

					if ( !invert )
					{
						xy[i2].y = Mathf.Lerp( xy[i0].y, xy[i2].y, sin );
						xy[i3].y = xy[i2].y;
					}
				}
				else
				{
					cos = 1f;
					sin = 1f;
				}

				if ( !invert ) xy[i3].x = Mathf.Lerp( xy[i0].x, xy[i2].x, cos );
				else xy[i1].y = Mathf.Lerp( xy[i0].y, xy[i2].y, sin );
			}
			else
			{
				if ( cos > sin )
				{
					sin /= cos;
					cos = 1f;

					if ( !invert )
					{
						xy[i1].y = Mathf.Lerp( xy[i0].y, xy[i2].y, sin );
						xy[i2].y = xy[i1].y;
					}
				}
				else if ( sin > cos )
				{
					cos /= sin;
					sin = 1f;

					if ( invert )
					{
						xy[i2].x = Mathf.Lerp( xy[i0].x, xy[i2].x, cos );
						xy[i3].x = xy[i2].x;
					}
				}
				else
				{
					cos = 1f;
					sin = 1f;
				}

				if ( invert ) xy[i3].y = Mathf.Lerp( xy[i0].y, xy[i2].y, sin );
				else xy[i1].x = Mathf.Lerp( xy[i0].x, xy[i2].x, cos );
			}
		}

		static void FlipHorizontal( ref Vector4 rect )
		{
			float tmp = rect.x;
			rect.x = rect.z;
			rect.z = tmp;
		}

		static void FlipVertical( ref Vector4 rect )
		{
			float tmp = rect.y;
			rect.y = rect.w;
			rect.w = tmp;
		}

		private void AddMaskUVs( VertexHelper vh, Vector4 uvmaskTransform )
		{
			UIVertex vert = new UIVertex();
			int count = vh.currentVertCount;
			Vector2 min = new Vector2( float.MaxValue, float.MaxValue );
			for ( int i = 0; i < count; i++ )
			{
				vh.PopulateUIVertex( ref vert, i );
				min.x = vert.uv0.x < min.x ? vert.uv0.x : min.x;
				min.y = vert.uv0.y < min.y ? vert.uv0.y : min.y;
			}
			for ( int i = 0; i < count; i++ )
			{
				vh.PopulateUIVertex( ref vert, i );
				vert.uv1 = TransUVToMask( vert.uv0, min, uvmaskTransform );
				vh.SetUIVertex( vert, i );
			}
		}

		private static Vector2 TransUVToMask( Vector2 uv, Vector2 min, Vector4 uvmaskTransform )
		{
			return new Vector2( ( uv.x - min.x ) * uvmaskTransform.z + uvmaskTransform.x, ( uv.y - min.y ) * uvmaskTransform.w + uvmaskTransform.y );
		}

		#endregion

		public virtual void CalculateLayoutInputHorizontal() { }
		public virtual void CalculateLayoutInputVertical() { }

		public virtual float minWidth => 0;

		public virtual float preferredWidth => this.rectTransform.sizeDelta.x;

		public virtual float flexibleWidth => -1;

		public virtual float minHeight => 0;

		public virtual float preferredHeight => this.rectTransform.sizeDelta.y;

		public virtual float flexibleHeight => -1;

		public virtual int layoutPriority => 0;

		public virtual bool IsRaycastLocationValid( Vector2 screenPoint, Camera eventCamera )
		{
			if ( this.alphaHitTestMinimumThreshold <= 0 )
				return true;

			if ( this.alphaHitTestMinimumThreshold > 1 )
				return false;

			if ( this.nSprite == null )
				return true;

			Vector2 local;
			if ( !RectTransformUtility.ScreenPointToLocalPointInRectangle( this.rectTransform, screenPoint, eventCamera, out local ) )
				return false;

			Rect rect = this.GetPixelAdjustedRect();

			// Convert to have lower left corner as reference point.
			local.x += this.rectTransform.pivot.x * rect.width;
			local.y += this.rectTransform.pivot.y * rect.height;

			local = this.MapCoordinate( local, rect );

			// Normalize local coordinates.
			Rect spriteRect = this.nSprite.textureRect;
			Vector2 normalized = new Vector2( local.x / spriteRect.width, local.y / spriteRect.height );

			// Convert to texture space.
			float x = Mathf.Lerp( spriteRect.x, spriteRect.xMax, normalized.x ) / this.nSprite.nTexture.width;
			float y = Mathf.Lerp( spriteRect.y, spriteRect.yMax, normalized.y ) / this.nSprite.nTexture.height;

			try
			{
				return this._nSprite.nTexture.GetPixelBilinear( x, y ).a >= this._alphaHitTestMinimumThreshold;
			}
			catch ( UnityException e )
			{
				Logger.Error( "Using alphaHitTestMinimumThreshold greater than 0 on Image whose sprite texture cannot be read. " + e.Message + " Also make sure to disable sprite packing for this sprite." );
				return true;
			}
		}

		private Vector2 MapCoordinate( Vector2 local, Rect rect )
		{
			Rect spriteRect = this.nSprite.rect;
			if ( this.type == Type.Simple || this.type == Type.Filled )
				return new Vector2( local.x * spriteRect.width / rect.width, local.y * spriteRect.height / rect.height );

			Vector4 border = this.nSprite.border;
			Vector4 adjustedBorder = this.GetAdjustedBorders( border / this.pixelsPerUnit, rect );

			for ( int i = 0; i < 2; i++ )
			{
				if ( local[i] <= adjustedBorder[i] )
					continue;

				if ( rect.size[i] - local[i] <= adjustedBorder[i + 2] )
				{
					local[i] -= ( rect.size[i] - spriteRect.size[i] );
					continue;
				}

				if ( this.type == Type.Sliced )
				{
					float lerp = Mathf.InverseLerp( adjustedBorder[i], rect.size[i] - adjustedBorder[i + 2], local[i] );
					local[i] = Mathf.Lerp( border[i], spriteRect.size[i] - border[i + 2], lerp );
				}
				else
				{
					local[i] -= adjustedBorder[i];
					local[i] = Mathf.Repeat( local[i], spriteRect.size[i] - border[i] - border[i + 2] );
					local[i] += border[i];
				}
			}

			return local;
		}
	}
}
