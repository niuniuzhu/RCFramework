using System.Collections.Generic;

namespace Core.Misc
{
	public static class UBBParser
	{
		static string _text;
		static int _readPos;

		public static readonly Dictionary<string, TagHandler> HANDLERS = new Dictionary<string, TagHandler>();

		public const int DEFAULT_IMG_WIDTH = 0;
		public const int DEFAULT_IMG_HEIGHT = 0;

		public delegate string TagHandler( string tagName, bool end, string attr );

		static UBBParser()
		{
			HANDLERS["url"] = onTag_URL;
			HANDLERS["img"] = onTag_IMG;
			HANDLERS["b"] = onTag_Simple;
			HANDLERS["i"] = onTag_Simple;
			HANDLERS["u"] = onTag_Simple;
			HANDLERS["sup"] = onTag_Simple;
			HANDLERS["sub"] = onTag_Simple;
			HANDLERS["color"] = onTag_COLOR;
			HANDLERS["font"] = onTag_FONT;
			HANDLERS["size"] = onTag_SIZE;
		}

		private static string onTag_URL( string tagName, bool end, string attr )
		{
			if ( !end )
			{
				if ( attr != null )
					return "<a href=\"" + attr + "\" target=\"_blank\">";
				string href = GetTagText( false );
				return "<a href=\"" + href + "\" target=\"_blank\">";
			}
			return "</a>";
		}

		private static string onTag_IMG( string tagName, bool end, string attr )
		{
			if ( !end )
			{
				string src = GetTagText( true );
				if ( string.IsNullOrEmpty( src ) )
					return null;

				//if ( DEFAULT_IMG_WIDTH != 0 )
				//	return "<img src=\"" + src + "\" width=\"" + DEFAULT_IMG_WIDTH + "\" height=\"" + DEFAULT_IMG_HEIGHT + "\"/>";
				return "<img src=\"" + src + "\"/>";
			}
			return null;
		}

		private static string onTag_Simple( string tagName, bool end, string attr )
		{
			return end ? ( "</" + tagName + ">" ) : ( "<" + tagName + ">" );
		}

		private static string onTag_COLOR( string tagName, bool end, string attr )
		{
			if ( !end )
				return "<color=\"" + attr + "\">";
			return "</color>";
		}

		private static string onTag_FONT( string tagName, bool end, string attr )
		{
			if ( !end )
				return "<face=\"" + attr + "\">";
			return "</face>";
		}

		private static string onTag_SIZE( string tagName, bool end, string attr )
		{
			if ( !end )
				return "<size=\"" + attr + "\">";
			return "</size>";
		}

		private static string GetTagText( bool remove )
		{
			int pos = _text.IndexOf( "[", _readPos, System.StringComparison.Ordinal );
			if ( pos == -1 )
				return null;

			string ret = _text.Substring( _readPos, pos - _readPos );
			if ( remove )
				_readPos = pos;
			return ret;
		}

		public static string Parse( string text )
		{
			_text = text;
			int pos1 = 0, pos2, pos3;
			bool end;
			string tag, attr;
			string repl;
			TagHandler func;
			while ( ( pos2 = _text.IndexOf( "[", pos1, System.StringComparison.Ordinal ) ) != -1 )
			{
				pos1 = pos2;
				pos2 = _text.IndexOf( "]", pos1, System.StringComparison.Ordinal );
				if ( pos2 == -1 )
					break;

				end = _text[pos1 + 1] == '/';
				pos3 = end ? pos1 + 2 : pos1 + 1;
				tag = _text.Substring( pos3, pos2 - pos3 );
				pos2++;
				_readPos = pos2;
				attr = null;
				pos3 = tag.IndexOf( "=", System.StringComparison.Ordinal );
				if ( pos3 != -1 )
				{
					attr = tag.Substring( pos3 + 1 );
					tag = tag.Substring( 0, pos3 );
				}
				tag = tag.ToLower();
				if ( HANDLERS.TryGetValue( tag, out func ) )
				{
					repl = func( tag, end, attr ) ?? string.Empty;
				}
				else
				{
					pos1 = pos2;
					continue;
				}
				_text = _text.Substring( 0, pos1 ) + repl + _text.Substring( _readPos );
			}
			return _text;
		}
	}
}
