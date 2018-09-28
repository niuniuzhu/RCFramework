using Core.Xml;
using UnityEngine;

namespace Game.Misc
{
	public static class XMLHelper
	{
		public static Color GetAttributeColor( this XML xml, string attrName, Color defValue )
		{
			string value = xml.GetAttribute( attrName );
			if ( string.IsNullOrEmpty( value ) )
				return defValue;

			return ColorUtils.ConvertFromHtmlColor( value );
		}

		public static Vector2 GetAttributeVector( this XML xml, string attrName )
		{
			string value = xml.GetAttribute( attrName );
			if ( value != null )
			{
				string[] arr = value.Split( ',' );
				return new Vector2( float.Parse( arr[0] ), float.Parse( arr[1] ) );
			}
			return Vector2.zero;
		}
	}
}