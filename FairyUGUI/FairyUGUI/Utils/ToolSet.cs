using FairyUGUI.UI;
using UnityEngine;

namespace FairyUGUI.Utils
{
	public static class ToolSet
	{
		internal static T GetOrAddComponent<T>( GameObject go ) where T : Component
		{
			T comp = go.GetComponent<T>();
			if ( !comp )
				comp = go.AddComponent<T>();
			return comp;
		}

		public static Sprite CreateSpriteFromTexture( Texture2D texture )
		{
			Sprite sprite = Sprite.Create( texture, new Rect( 0, 0, texture.width, texture.height ), new Vector2( 0.5f, 0.5f ) );
			sprite.name = texture.name;
			return sprite;
		}

		public static void SetAnchor( RectTransform rt, AnchorType type )
		{
			switch ( type )
			{
				case AnchorType.Top_Left:
					rt.anchorMin = new Vector2( 0, 1 );
					rt.anchorMax = new Vector2( 0, 1 );
					break;

				case AnchorType.Top_Center:
					rt.anchorMin = new Vector2( 0.5f, 1 );
					rt.anchorMax = new Vector2( 0.5f, 1 );
					break;

				case AnchorType.Top_Right:
					rt.anchorMin = new Vector2( 1, 1 );
					rt.anchorMax = new Vector2( 1, 1 );
					break;

				case AnchorType.Top_Stretch:
					rt.anchorMin = new Vector2( 0, 1 );
					rt.anchorMax = Vector2.one;
					break;

				case AnchorType.Middle_Left:
					rt.anchorMin = new Vector2( 0, 0.5f );
					rt.anchorMax = new Vector2( 0, 0.5f );
					break;

				case AnchorType.Middle_Center:
					rt.anchorMin = new Vector2( 0.5f, 0.5f );
					rt.anchorMax = new Vector2( 0.5f, 0.5f );
					break;

				case AnchorType.Middle_Right:
					rt.anchorMin = new Vector2( 1, 0.5f );
					rt.anchorMax = new Vector2( 1, 0.5f );
					break;

				case AnchorType.Middle_Stretch:
					rt.anchorMin = new Vector2( 0, 0.5f );
					rt.anchorMax = new Vector2( 1, 0.5f );
					break;

				case AnchorType.Bottom_Left:
					rt.anchorMin = Vector2.zero;
					rt.anchorMax = Vector2.zero;
					break;

				case AnchorType.Bottom_Center:
					rt.anchorMin = new Vector2( 0.5f, 0 );
					rt.anchorMax = new Vector2( 0.5f, 0 );
					break;

				case AnchorType.Bottom_Right:
					rt.anchorMin = new Vector2( 1, 0 );
					rt.anchorMax = new Vector2( 1, 0 );
					break;

				case AnchorType.Bottom_Stretch:
					rt.anchorMin = new Vector2( 0, 0 );
					rt.anchorMax = new Vector2( 1, 0 );
					break;

				case AnchorType.Stretch_Left:
					rt.anchorMin = Vector2.zero;
					rt.anchorMax = new Vector2( 0, 1 );
					break;

				case AnchorType.Stretch_Center:
					rt.anchorMin = new Vector2( 0.5f, 0 );
					rt.anchorMax = new Vector2( 0.5f, 1 );
					break;
				case AnchorType.Stretch_Right:
					rt.anchorMin = new Vector2( 1, 0 );
					rt.anchorMax = new Vector2( 1, 1 );
					break;

				case AnchorType.Stretch_Stretch:
					rt.anchorMin = Vector2.zero;
					rt.anchorMax = Vector2.one;
					break;
			}
		}

		public static void SetParent( Transform t, Transform parent )
		{
			t.SetParent( parent, false );
		}

		public static Color ConvertFromHtmlColor( string str )
		{
			if ( string.IsNullOrEmpty( str ) || str.Length < 7 || str[0] != '#' )
				return Color.black;

			if ( str.Length == 9 )
			{
				//optimize:avoid using Convert.ToByte and Substring
				//return new Color32(Convert.ToByte(str.Substring(3, 2), 16), Convert.ToByte(str.Substring(5, 2), 16),
				//  Convert.ToByte(str.Substring(7, 2), 16), Convert.ToByte(str.Substring(1, 2), 16));
				return new Color32( ( byte )( CharToHex( str[3] ) * 16 + CharToHex( str[4] ) ),
					( byte )( CharToHex( str[5] ) * 16 + CharToHex( str[6] ) ),
					( byte )( CharToHex( str[7] ) * 16 + CharToHex( str[8] ) ),
					( byte )( CharToHex( str[1] ) * 16 + CharToHex( str[2] ) ) );
			}
			
			//return new Color32(Convert.ToByte(str.Substring(1, 2), 16), Convert.ToByte(str.Substring(3, 2), 16),
			//Convert.ToByte(str.Substring(5, 2), 16), 255);
			return new Color32( ( byte )( CharToHex( str[1] ) * 16 + CharToHex( str[2] ) ),
				( byte )( CharToHex( str[3] ) * 16 + CharToHex( str[4] ) ),
				( byte )( CharToHex( str[5] ) * 16 + CharToHex( str[6] ) ),
				255 );
		}

		public static Color ColorFromRGB( int value )
		{
			return new Color( ( ( value >> 16 ) & 0xFF ) / 255f, ( ( value >> 8 ) & 0xFF ) / 255f, ( value & 0xFF ) / 255f, 1 );
		}

		public static Color ColorFromRGBA( int value )
		{
			return new Color( ( ( value >> 16 ) & 0xFF ) / 255f, ( ( value >> 8 ) & 0xFF ) / 255f, ( value & 0xFF ) / 255f, ( ( value >> 24 ) & 0xFF ) / 255f );
		}

		public static int CharToHex( char c )
		{
			if ( c >= '0' && c <= '9' )
				return ( int )c - 48;
			if ( c >= 'A' && c <= 'F' )
				return 10 + ( int )c - 65;
			else if ( c >= 'a' && c <= 'f' )
				return 10 + ( int )c - 97;
			else
				return 0;
		}

		public static void FlipRect( ref Rect rect, FlipType flip )
		{
			if ( flip == FlipType.Horizontal || flip == FlipType.Both )
			{
				float tmp = rect.xMin;
				rect.xMin = rect.xMax;
				rect.xMax = tmp;
			}
			if ( flip == FlipType.Vertical || flip == FlipType.Both )
			{
				float tmp = rect.yMin;
				rect.yMin = rect.yMax;
				rect.yMax = tmp;
			}
		}

		public static void FlipBorder( ref Vector4 border, FlipType flip )
		{
			if ( flip == FlipType.Horizontal || flip == FlipType.Both )
			{
				float tmp = border.x;
				border.x = border.z;
				border.z = tmp;
			}
			if ( flip == FlipType.Vertical || flip == FlipType.Both )
			{
				float tmp = border.y;
				border.y = border.w;
				border.w = tmp;
			}
		}
	}
}