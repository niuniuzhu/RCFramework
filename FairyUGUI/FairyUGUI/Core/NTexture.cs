using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FairyUGUI.Core
{
	public class NTexture
	{
		private static NTexture _emptyTexture;
		public static NTexture emptyTexture
		{
			get
			{
				if ( _emptyTexture == null )
				{
					Sprite sprite;
					Texture2D tex = CreateEmptyTextureAndSprite( out sprite );
					_emptyTexture = new NTexture( tex, null, new[] { sprite } );
				}
				return _emptyTexture;
			}
		}

		public static NSprite emptySprite => emptyTexture.GetSprite( "empty" );

		static Texture2D CreateEmptyTextureAndSprite( out Sprite sprite )
		{
			Texture2D tex = new Texture2D( 1, 1, TextureFormat.RGB24, false );
			tex.hideFlags = DisplayOptions.hideFlags;
			tex.SetPixel( 0, 0, Color.white );
			tex.Apply();

			sprite = Sprite.Create( tex, new Rect( 0, 0, 1, 1 ), new Vector2( 0.5f, 0.5f ) );
			sprite.name = "empty";

			return tex;
		}

		public Texture2D texture { get; private set; }

		public Texture2D associatedAlphaSplitTexture { get; private set; }

		private readonly Dictionary<string, NSprite> _sprites = new Dictionary<string, NSprite>();

		public string name => this.texture.name;

		public int width => this.texture.width;

		public int height => this.texture.height;

		public Vector2 texelSize => this.texture.texelSize;

		public NTexture( Texture2D texture, Texture2D associatedAlphaSplitTexture, Sprite[] sprites )
		{
			this.texture = texture;
			this.associatedAlphaSplitTexture = associatedAlphaSplitTexture;

			if ( sprites != null )
			{
				int count = sprites.Length;
				for ( int i = 0; i < count; i++ )
				{
					Sprite sprite = sprites[i];
					this._sprites[sprite.name] = new NSprite( this, sprite );
				}
			}
		}

		public NSprite GetSprite( string name )
		{
			NSprite sprite;
			this._sprites.TryGetValue( name, out sprite );
			return sprite ?? emptySprite;
		}

		public void Dispose( bool allowDestroyingAssets )
		{
			if ( this != emptyTexture )
			{
				if ( allowDestroyingAssets )
					Object.DestroyImmediate( this.texture );
				this.texture = null;
				this._sprites.Clear();
			}
			if ( this.associatedAlphaSplitTexture != null )
			{
				if ( allowDestroyingAssets )
					Object.DestroyImmediate( this.associatedAlphaSplitTexture );
				this.associatedAlphaSplitTexture = null;
			}
		}

		public Color GetPixel( int x, int y )
		{
			return this.texture.GetPixel( x, y );
		}

		public Color GetPixelBilinear( float u, float v )
		{
			return this.texture.GetPixelBilinear( u, v );
		}

		public Color[] GetPixels()
		{
			return this.texture.GetPixels();
		}

		public Color[] GetPixels( int miplevel )
		{
			return this.texture.GetPixels( miplevel );
		}

		public Color[] GetPixels( int x, int y, int blockWidth, int blockHeight )
		{
			return this.texture.GetPixels( x, y, blockWidth, blockHeight );
		}

		public Color[] GetPixels( int x, int y, int blockWidth, int blockHeight, int miplevel )
		{
			return this.texture.GetPixels( x, y, blockWidth, blockHeight, miplevel );
		}

		public Color32[] GetPixels32()
		{
			return this.texture.GetPixels32();
		}

		public Color32[] GetPixels32( int miplevel )
		{
			return this.texture.GetPixels32( miplevel );
		}

		public void SetPixel( int x, int y, Color color )
		{
			this.texture.SetPixel( x, y, color );
		}

		public void SetPixels( Color[] colors )
		{
			this.texture.SetPixels( colors );
		}

		public void SetPixels( Color[] colors, int miplevel )
		{
			this.texture.SetPixels( colors, miplevel );
		}

		public void SetPixels( int x, int y, int blockWidth, int blockHeight, Color[] colors )
		{
			this.texture.SetPixels( x, y, blockWidth, blockHeight, colors );
		}

		public void SetPixels( int x, int y, int blockWidth, int blockHeight, Color[] colors, int miplevel )
		{
			this.texture.SetPixels( x, y, blockWidth, blockHeight, colors, miplevel );
		}

		public void SetPixels32( Color32[] colors )
		{
			this.texture.SetPixels32( colors );
		}

		public void SetPixels32( Color32[] colors, int miplevel )
		{
			this.texture.SetPixels32( colors, miplevel );
		}

		public void SetPixels32( int x, int y, int blockWidth, int blockHeight, Color32[] colors )
		{
			this.texture.SetPixels32( x, y, blockWidth, blockHeight, colors );
		}

		public void SetPixels32( int x, int y, int blockWidth, int blockHeight, Color32[] colors, int miplevel )
		{
			this.texture.SetPixels32( x, y, blockWidth, blockHeight, colors, miplevel );
		}
	}
}