using FairyUGUI.Core.Fonts;
using System.Collections.Generic;
using Game.Pool;
using UnityEngine;
using UnityEngine.UI;

namespace FairyUGUI.UI.UGUIExt
{
	public class TextEx : Text
	{
		internal BaseFont baseFont;

		readonly UIVertex[] _tempVerts = new UIVertex[4];

		public override Texture mainTexture
		{
			get
			{
				if ( this.baseFont != null )
					return this.baseFont.mainTexture.texture;

				return base.mainTexture;
			}
		}

		#region effect

		private bool _shadow;
		public bool shadow
		{
			get => this._shadow;
			set
			{
				if ( this._shadow == value )
					return;
				this._shadow = value;
				this.SetVerticesDirty();
			}
		}

		private Color _shadowColor = new Color( 0f, 0f, 0f, 0.5f );
		public Color shadowColor
		{
			get => this._shadowColor;
			set
			{
				if ( this._shadowColor == value )
					return;
				this._shadowColor = value;
				this.SetVerticesDirty();
			}
		}

		private const float K_MAX_EFFECT_DISTANCE = 600f;
		private Vector2 _shadowDistance = new Vector2( 1f, -1f );
		public Vector2 shadowDistance
		{
			get => this._shadowDistance;
			set
			{
				if ( value.x > K_MAX_EFFECT_DISTANCE )
					value.x = K_MAX_EFFECT_DISTANCE;
				if ( value.x < -K_MAX_EFFECT_DISTANCE )
					value.x = -K_MAX_EFFECT_DISTANCE;

				if ( value.y > K_MAX_EFFECT_DISTANCE )
					value.y = K_MAX_EFFECT_DISTANCE;
				if ( value.y < -K_MAX_EFFECT_DISTANCE )
					value.y = -K_MAX_EFFECT_DISTANCE;

				if ( this._shadowDistance == value )
					return;

				this._shadowDistance = value;
				this.SetVerticesDirty();
			}
		}

		private bool _outline;
		public bool outline
		{
			get => this._outline;
			set
			{
				if ( this._outline == value )
					return;
				this._outline = value;
				this.SetVerticesDirty();
			}
		}

		private Vector2 _outlineDistance = new Vector2( 1f, -1f );
		public Vector2 outlineDistance
		{
			get => this._outlineDistance;
			set
			{
				if ( value.x > K_MAX_EFFECT_DISTANCE )
					value.x = K_MAX_EFFECT_DISTANCE;
				if ( value.x < -K_MAX_EFFECT_DISTANCE )
					value.x = -K_MAX_EFFECT_DISTANCE;

				if ( value.y > K_MAX_EFFECT_DISTANCE )
					value.y = K_MAX_EFFECT_DISTANCE;
				if ( value.y < -K_MAX_EFFECT_DISTANCE )
					value.y = -K_MAX_EFFECT_DISTANCE;

				if ( this._outlineDistance == value )
					return;

				this._outlineDistance = value;
				this.SetVerticesDirty();
			}
		}

		private Color _outlineColor = new Color( 0f, 0f, 0f, 0.5f );
		public Color outlineColor
		{
			get => this._outlineColor;
			set
			{
				if ( this._outlineColor == value )
					return;
				this._outlineColor = value;
				this.SetVerticesDirty();
			}
		}

		private bool _useGraphicAlpha;

		public bool useGraphicAlpha
		{
			get => this._useGraphicAlpha;
			set
			{
				this._useGraphicAlpha = value;
				this.SetVerticesDirty();
			}
		}

		#endregion

		protected override void UpdateMaterial()
		{
			base.UpdateMaterial();

			// check if this sprite has an associated alpha texture (generated when splitting RGBA = RGB + A as two textures without alpha)
			if ( this.mainTexture == null )
			{
				this.canvasRenderer.SetAlphaTexture( null );
				return;
			}

			if ( this.baseFont.mainTexture.associatedAlphaSplitTexture != null )
				this.canvasRenderer.SetAlphaTexture( this.baseFont.mainTexture.associatedAlphaSplitTexture );
		}

		protected override void OnPopulateMesh( VertexHelper vh )
		{
			if ( this.font == null )
				return;

			this.m_DisableFontTextureRebuiltCallback = true;

			Vector2 extents = this.rectTransform.rect.size;

			if ( !this.baseFont.isDynamic )
				extents.y -= this.baseFont.asBitmapFont.lineHeight;

			TextGenerationSettings settings = this.GetGenerationSettings( extents );

			if ( !this.cachedTextGenerator.Populate( this.text, settings ) )
				return;

			if ( this.cachedTextGenerator.vertexCount == 0 )
				return;

			Rect inputRect = this.rectTransform.rect;

			Vector2 textAnchorPivot = GetTextAnchorPivot( this.alignment );
			Vector2 refPoint = Vector2.zero;
			refPoint.x = Mathf.Lerp( inputRect.xMin, inputRect.xMax, textAnchorPivot.x );
			refPoint.y = Mathf.Lerp( inputRect.yMin, inputRect.yMax, textAnchorPivot.y );

			Vector2 roundingOffset = this.PixelAdjustPoint( refPoint ) - refPoint;

			IList<UIVertex> verts = this.cachedTextGenerator.verts;
			float unitsPerPixel = 1 / this.pixelsPerUnit;
			int vertCount = verts.Count - 4;

			vh.Clear();

			if ( roundingOffset != Vector2.zero )
			{
				for ( int i = 0; i < vertCount; ++i )
				{
					int tempVertsIndex = i & 3;
					this._tempVerts[tempVertsIndex] = verts[i];
					this._tempVerts[tempVertsIndex].position *= unitsPerPixel;
					this._tempVerts[tempVertsIndex].position.x += roundingOffset.x;
					this._tempVerts[tempVertsIndex].position.y += roundingOffset.y;
					if ( tempVertsIndex == 3 )
						vh.AddUIVertexQuad( this._tempVerts );
				}
			}
			else
			{
				for ( int i = 0; i < vertCount; ++i )
				{
					int tempVertsIndex = i & 3;
					this._tempVerts[tempVertsIndex] = verts[i];
					this._tempVerts[tempVertsIndex].position *= unitsPerPixel;
					if ( tempVertsIndex == 3 )
						vh.AddUIVertexQuad( this._tempVerts );
				}
			}
			this.m_DisableFontTextureRebuiltCallback = false;

			if ( this._shadow || this._outline )
			{
				List<UIVertex> output = ListPool<UIVertex>.Get();
				vh.GetUIVertexStream( output );
				if ( this._shadow )
					this.ApplyShadow( output, this.shadowColor, 0, output.Count, this.shadowDistance.x, -this.shadowDistance.y );
				if ( this._outline )
					this.ApplyOutline( output, this.outlineColor, 0, output.Count, this.outlineDistance.x, -this.outlineDistance.y );
				vh.Clear();
				vh.AddUIVertexTriangleStream( output );
				ListPool<UIVertex>.Release( output );
			}
		}

		private void ApplyOutline( List<UIVertex> verts, Color32 color, int start, int end, float x, float y )
		{
			int neededCpacity = verts.Count * 5;
			if ( verts.Capacity < neededCpacity )
				verts.Capacity = neededCpacity;

			this.ApplyShadowZeroAlloc( verts, color, start, end, x, y );

			start = end;
			end = verts.Count;
			this.ApplyShadowZeroAlloc( verts, color, start, end, x, -y );

			start = end;
			end = verts.Count;
			this.ApplyShadowZeroAlloc( verts, color, start, end, -x, y );

			start = end;
			end = verts.Count;
			this.ApplyShadowZeroAlloc( verts, color, start, end, -x, -y );
		}

		private void ApplyShadowZeroAlloc( List<UIVertex> verts, Color32 color, int start, int end, float x, float y )
		{
			int neededCapacity = verts.Count + end - start;
			if ( verts.Capacity < neededCapacity )
				verts.Capacity = neededCapacity;

			for ( int i = start; i < end; ++i )
			{
				UIVertex vt = verts[i];
				verts.Add( vt );

				Vector3 v = vt.position;
				v.x += x;
				v.y += y;
				vt.position = v;
				var newColor = color;
				if ( this._useGraphicAlpha )
					newColor.a = ( byte )( ( newColor.a * verts[i].color.a ) / 255 );
				vt.color = newColor;
				verts[i] = vt;
			}
		}

		private void ApplyShadow( List<UIVertex> verts, Color32 color, int start, int end, float x, float y )
		{
			this.ApplyShadowZeroAlloc( verts, color, start, end, x, y );
		}
	}
}
