using UnityEngine;

namespace Game.Misc
{
	public static class ColorUtils
	{
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
			else
			{
				//return new Color32(Convert.ToByte(str.Substring(1, 2), 16), Convert.ToByte(str.Substring(3, 2), 16),
				//Convert.ToByte(str.Substring(5, 2), 16), 255);

				return new Color32( ( byte )( CharToHex( str[1] ) * 16 + CharToHex( str[2] ) ),
					( byte )( CharToHex( str[3] ) * 16 + CharToHex( str[4] ) ),
					( byte )( CharToHex( str[5] ) * 16 + CharToHex( str[6] ) ),
					255 );
			}
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
	}
}