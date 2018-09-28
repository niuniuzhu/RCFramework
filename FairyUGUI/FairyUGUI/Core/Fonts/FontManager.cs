using System.Collections.Generic;

namespace FairyUGUI.Core.Fonts
{
	public static class FontManager
	{
		private static readonly Dictionary<string, BaseFont> S_FONT_FACTORY = new Dictionary<string, BaseFont>();

		public static void RegisterFont( BaseFont font, string alias )
		{
			if ( !S_FONT_FACTORY.ContainsKey( font.name ) )
				S_FONT_FACTORY.Add( font.name, font );
			if ( alias != null )
			{
				if ( !S_FONT_FACTORY.ContainsKey( alias ) )
					S_FONT_FACTORY.Add( alias, font );
			}
		}

		public static void UnregisterFont( BaseFont font )
		{
			S_FONT_FACTORY.Remove( font.name );
		}

		public static BaseFont GetFont( string name )
		{
			BaseFont ret;
			if ( !S_FONT_FACTORY.TryGetValue( name, out ret ) )
			{
				ret = new DynamicFont( name );
				S_FONT_FACTORY.Add( name, ret );
			}

			if ( ret.packageItem != null && !ret.packageItem.decoded )
				ret.packageItem.Load();

			return ret;
		}

		public static void Clear()
		{
			S_FONT_FACTORY.Clear();
		}
	}
}
