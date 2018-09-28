using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Core.Crypto;

namespace Core.Misc
{
	/// <summary>
	/// 字典表替换器，配合Regex.Replace()方法使用
	/// </summary>
	public class MapMatchEvaluator
	{
		readonly IDictionary _map;

		public MapMatchEvaluator( IDictionary map )
		{
			this._map = map;
		}

		public string OnEvaluator( Match match )
		{
			return TypeUtil.GetString( this._map[match.Value] );
		}
	}

	/// <summary>
	/// 字符串操作相关便捷功能
	/// </summary>
	public static class StringsUtils
	{
		public static string Join( this object[] os, string sep )
		{
			return sep.Join( os );
		}

		public static string Join( this ICollection c, string sep )
		{
			return sep.Join( c );
		}

		//-------------------------------------------------------------------

		/// <summary>
		/// 头尾颠倒字符串的字符
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string Reverse( this string s )
		{
			StringBuilder sb = new StringBuilder( s.Length + 20 );
			for ( int i = s.Length - 1; i >= 0; i-- )
				sb.Append( s[i] );
			return sb.ToString();
		}

		/// <summary>
		/// 重复连接字符串
		/// </summary>
		/// <param name="s">指定子串</param>
		/// <param name="n">连接个数</param>
		/// <returns>返回n个s的连接结果，如：join("abc", 3) = "abcabcabc"</returns>
		public static string Join( this string s, int n )
		{
			StringBuilder sb = new StringBuilder();
			for ( int i = 0; i < n; i++ )
				sb.Append( s );
			return sb.ToString();
		}

		/// <summary>
		/// 列表中连接字符串
		/// </summary>
		/// <param name="sep">连接对象，可以是任意对象</param>
		/// <param name="c">对象列表，可以是任意对象列表</param>
		/// <returns>返回连接后的字符串</returns>
		public static string Join( this string sep, ICollection c )
		{
			return Join( sep, c.ToArray() );
		}

		/// <summary>
		/// 列表中连接字符串
		/// </summary>
		/// <param name="sep">连接对象，可以是任意对象</param>
		/// <param name="os">对象列表，可以是任意对象列表</param>
		/// <param name="fromIndex">列表的起始位置</param>
		/// <returns>返回连接后的字符串</returns>
		public static string Join( this string sep, object[] os, int fromIndex = 0 )
		{
			return Join( sep, os, fromIndex, os.Length );
		}

		/// <summary>
		/// 列表中连接字符串
		/// </summary>
		/// <param name="sep">连接对象，可以是任意对象</param>
		/// <param name="os">对象列表，可以是任意对象列表</param>
		/// <param name="fromIndex">列表的起始位置</param>
		/// <param name="toIndex">列表的结束位置</param>
		/// <returns>返回连接后的字符串</returns>
		public static string Join( this string sep, object[] os, int fromIndex, int toIndex )
		{
			if ( fromIndex >= 0 && toIndex <= os.Length && toIndex - fromIndex >= 0 )
			{
				StringBuilder sb = new StringBuilder();
				if ( toIndex - fromIndex > 0 )
				{
					sb.Append( os[fromIndex] );
					for ( int i = fromIndex + 1; i < toIndex; i++ )
						sb.Append( sep ).Append( os[i] );
				}
				return sb.ToString();
			}
			return "";
		}

		/// <summary>
		/// 分割字符串
		/// </summary>
		/// <param name="s">原字符串</param>
		/// <param name="sliceChars">每个子串的字符数</param>
		/// <returns>返回s中以sliceChars个字符为一组字符串的数组</returns>
		public static string[] Split( this string s, int sliceChars )
		{
			if ( s == null )
				return null;
			int len = s.Length;
			if ( sliceChars <= 0 || len <= sliceChars )
				return new[] { s };
			ArrayList ls = new ArrayList();
			for ( int i = 0; i < len / sliceChars; i++ )
				ls.Add( s.Substring( i * sliceChars, sliceChars ) );
			if ( len % sliceChars != 0 )
				ls.Add( s.Substring( len - len % sliceChars ) );
			string[] ss = new string[ls.Count];
			ls.CopyTo( ss );
			return ss;
		}

		/// <summary>
		/// 分割字符串
		/// </summary>
		/// <param name="s">原字符串</param>
		/// <param name="seps">指定分隔符</param>
		/// <returns>返回s中以sep分隔的字符串的数组</returns>
		public static string[] Split( this string s, params string[] seps )
		{
			return s.Split( seps, StringSplitOptions.None );
		}

		/// <summary>
		/// 将对象用左右字符串括起来
		/// </summary>
		/// <param name="s">目标对象</param>
		/// <param name="left">左边字符串</param>
		/// <param name="right">右边字符串</param>
		/// <returns>返回括起来后的字符串</returns>
		public static string Quote( this string s, string left, string right )
		{
			return left + s + right;
		}

		/// <summary>
		/// 将对象数组的每一项成员用左右字符串括起来
		/// </summary>
		/// <param name="ss">目标对象数组</param>
		/// <param name="left">左边字符串</param>
		/// <param name="right">右边字符串</param>
		/// <returns>返回括起来后的字符串数组</returns>
		public static string[] Quote( this object[] ss, string left, string right )
		{
			string[] ar = new string[ss.Length];
			for ( int i = 0; i < ar.Length; i++ )
				ar[i] = Quote( TypeUtil.GetString( ss[i] ), left, right );
			return ar;
		}

		/// <summary>
		/// 将对象列表的每一项成员用左右字符串括起来
		/// </summary>
		/// <param name="c">目标对象列表</param>
		/// <param name="left">左边字符串</param>
		/// <param name="right">右边字符串</param>
		/// <returns>返回括起来后的字符串列表</returns>
		public static IList Quote( this ICollection c, string left, string right )
		{
			ArrayList ls = new ArrayList();
			foreach ( object o in c )
				ls.Add( Quote( TypeUtil.GetString( o ), left, right ) );
			return ls;
		}

		/// <summary>
		/// 判断是否为数字
		/// </summary>
		/// <param name="s"></param>
		/// <param name="ignoreChars">忽略的字符，如".-"，则忽略'.','-'字符</param>
		/// <returns></returns>
		public static bool IsNumber( this string s, string ignoreChars = null )
		{
			int len = s.Length;
			bool noIgnore = string.IsNullOrEmpty( ignoreChars );

			for ( int n = 0; n < len; n++ )
			{
				char eachChar = s[n];
				if ( eachChar < '0' || eachChar > '9' )
				{
					if ( noIgnore )
						return false;
					if ( ignoreChars.IndexOf( eachChar ) == -1 )
						return false;
				}
			}
			return true;
		}

		/// <summary>
		/// 将十六进制字符串转为字节数组
		/// </summary>
		/// <param name="s">源十六进制字符串</param>
		/// <param name="sep">每个字节之间的分隔符</param>
		/// <returns>成功返回转换后的byte[]，失败返回null</returns>
		public static byte[] HexToBytes( string s, string sep = "" )
		{
			try
			{
				string[] sl;
				if ( sep == null || sep.Equals( "" ) )
					sl = s.Split( 2 );
				else
					sl = s.Split( sep );

				byte[] bf = new byte[sl.Length];
				for ( int i = 0; i < sl.Length; i++ )
				{
					string a = sl[i].Trim();
					if ( a.Length > 0 )
						bf[i] = ( byte )int.Parse( a, NumberStyles.AllowHexSpecifier );
					else
						bf[i] = 0;
				}
				return bf;
			}
			catch ( Exception )
			{
				return null;
			}
		}

		/// <summary>
		/// 将byte序列转换成十六进制字符串序列，以sep分隔每一个byte
		/// </summary>
		/// <param name="b">序列</param>
		/// <param name="sep">分隔符</param>
		/// <returns>十六进制字符串</returns>
		public static string ToHexString( this byte[] b, string sep = "" )
		{
			int len = b.Length;
			StringBuilder sb = new StringBuilder( len * ( 2 + sep.Length ) + 10 );
			if ( len > 0 )
			{
				sb.Append( ToHexString( b[0] ) );
				for ( int i = 1; i < len; i++ )
					sb.Append( sep + ToHexString( b[i] ) );
			}
			return sb.ToString();
		}

		/// <summary>
		/// 将char序列转换成十六进制字符串序列，以sep分隔每一个char
		/// </summary>
		/// <param name="s">字符串</param>
		/// <param name="sep">分隔符</param>
		/// <returns>返回十六进制字符串</returns>
		public static string ToHexString( this string s, string sep = "" )
		{
			int len = s.Length;
			StringBuilder sb = new StringBuilder( len * ( 6 + sep.Length ) );
			if ( len > 0 )
			{
				sb.Append( ToHexString( ( short )s[0] ) );
				for ( int i = 1; i < len; i++ )
					sb.Append( sep + ToHexString( ( short )s[i] ) );
			}
			return sb.ToString();
		}

		/// <summary>
		/// 将数值转换成十六进制字符串
		/// </summary>
		/// <param name="n"></param>
		/// <returns>返回十六进制字符串</returns>
		public static string ToHexString( this long n )
		{
			return n.ToString( "X16" );
		}

		/// <summary>
		/// 将数值转换成十六进制字符串
		/// </summary>
		/// <param name="n"></param>
		/// <returns>返回十六进制字符串</returns>
		public static string ToHexString( this ulong n )
		{
			return n.ToString( "X16" );
		}

		/// <summary>
		/// 将数值转换成十六进制字符串
		/// </summary>
		/// <param name="n"></param>
		/// <returns>返回十六进制字符串</returns>
		public static string ToHexString( this int n )
		{
			return n.ToString( "X8" );
		}

		/// <summary>
		/// 将数值转换成十六进制字符串
		/// </summary>
		/// <param name="n"></param>
		/// <returns>返回十六进制字符串</returns>
		public static string ToHexString( this uint n )
		{
			return n.ToString( "X8" );
		}

		/// <summary>
		/// 将数值转换成十六进制字符串
		/// </summary>
		/// <param name="n"></param>
		/// <returns>返回十六进制字符串</returns>
		public static string ToHexString( this short n )
		{
			return n.ToString( "X4" );
		}

		/// <summary>
		/// 将数值转换成十六进制字符串
		/// </summary>
		/// <param name="n"></param>
		/// <returns>返回十六进制字符串</returns>
		public static string ToHexString( this ushort n )
		{
			return n.ToString( "X4" );
		}

		/// <summary>
		/// 将数值转换成十六进制字符串
		/// </summary>
		/// <param name="n"></param>
		/// <returns>返回十六进制字符串</returns>
		public static string ToHexString( this byte n )
		{
			return n.ToString( "X2" );
		}

		/// <summary>
		/// 将数值转换成十六进制字符串
		/// </summary>
		/// <param name="n"></param>
		/// <returns>返回十六进制字符串</returns>
		public static string ToHexString( this sbyte n )
		{
			return n.ToString( "X2" );
		}

		/// <summary>
		/// 获得字符串开头的数字
		/// </summary>
		/// <param name="src">字符串</param>
		/// <returns>返回开头的数字字符串</returns>
		public static string GetStartDigit( this string src )
		{
			src = src.Trim();
			StringBuilder sb = new StringBuilder( src.Length );
			for ( int i = 0; i < src.Length; i++ )
			{
				char c = src[i];
				if ( char.IsDigit( c ) )
					sb.Append( c );
				else
					break;
			}
			return sb.ToString();
		}

		/// <summary>
		/// 获得字符串结尾的数字
		/// </summary>
		/// <param name="src">字符串</param>
		/// <returns>返回结尾的数字字符串</returns>
		public static string GetEndDigit( this string src )
		{
			src = src.Trim();
			StringBuilder sb = new StringBuilder( src.Length );
			for ( int i = src.Length - 1; i >= 0; i-- )
			{
				char c = src[i];
				if ( char.IsDigit( c ) )
					sb.Insert( 0, c );
				else
					break;
			}
			return sb.ToString();
		}

		/// <summary>
		/// 获得字符串开头的数字
		/// </summary>
		/// <param name="src">字符串</param>
		/// <param name="dv">默认值</param>
		/// <returns>返回开头数字的整数值，没有数字开头则返回默认值</returns>
		public static long GetStartDigit( this string src, long dv = 0 )
		{
			return TypeUtil.GetLong( GetStartDigit( src ), dv );
		}

		/// <summary>
		/// 获得字符串开头的数字
		/// </summary>
		/// <param name="src">字符串</param>
		/// <param name="dv">默认值</param>
		/// <returns>返回开头数字的整数值，没有数字开头则返回默认值</returns>
		public static int GetStartDigit( this string src, int dv = 0 )
		{
			return TypeUtil.GetInt( GetStartDigit( src ), dv );
		}

		/// <summary>
		/// 获得字符串结尾的数字
		/// </summary>
		/// <param name="src">字符串</param>
		/// <param name="dv">默认值</param>
		/// <returns>返回结尾数字的整数值，没有数字结尾则返回默认值</returns>
		public static long GetEndDigit( this string src, long dv = 0 )
		{
			return TypeUtil.GetLong( GetEndDigit( src ), dv );
		}

		/// <summary>
		/// 获得字符串结尾的数字
		/// </summary>
		/// <param name="src">字符串</param>
		/// <param name="dv">默认值</param>
		/// <returns>返回结尾数字的整数值，没有数字结尾则返回默认值</returns>
		public static int GetEndDigit( this string src, int dv = 0 )
		{
			return TypeUtil.GetInt( GetEndDigit( src ), dv );
		}

		private static string TranslateStringToMapEscapeChars( Match match )
		{
			string s = match.Value;
			if ( "%25" == s )
				return "%";
			if ( "%3A" == s )
				return ":";
			if ( "%2C" == s )
				return ",";
			return s;
		}

		/// <summary>
		/// 字符串转换为Map对象时转换特殊字符
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string UnescapeStringToMapSpecialChars( string s )
		{
			return Regex.Replace( s, "(%25)|(%3A)|(%2C)", TranslateStringToMapEscapeChars );
		}

		/// <summary>
		/// 字符串转为Map对象
		/// </summary>
		/// <param name="s">字符串，key或value中的“%”“,”“:”符号分别用“%25”“%2C”“%3A”表示</param>
		/// <param name="map">目标Map对象</param>
		/// <param name="trim">是否去掉key和value的两边空白字符</param>
		public static void StringToMap( this string s, IDictionary map, bool trim = false )
		{
			if ( s.Length == 0 )
				return;
			string[] ss = s.Split( ',' );
			for ( int i = 0; i < ss.Length; i++ )
			{
				int pos = ss[i].IndexOf( ':' );
				if ( pos != -1 )
				{
					string key = ss[i].Substring( 0, pos );
					key = UnescapeStringToMapSpecialChars( key );
					string value = ss[i].Substring( pos + 1 );
					value = UnescapeStringToMapSpecialChars( value );
					map[trim ? key.Trim() : key] = ( trim ? value.Trim() : value );
				}
				else
				{
					string key = ( trim ? ss[i].Trim() : ss[i] );
					if ( key.Length > 0 )
						map[key] = key;
				}
			}
		}

		/// <summary>
		/// 字符串转为Map对象
		/// </summary>
		/// <param name="s">字符串，key或value中的“%”“,”“:”符号分别用“%25”“%2C”“%3A”表示</param>
		/// <param name="trim">是否去掉key和value的两边空白字符</param>
		/// <returns>返回Map对象</returns>
		public static IDictionary StringToMap( this string s, bool trim = false )
		{
			IDictionary map = new Hashtable();
			StringToMap( s, map, trim );
			return map;
		}

		private static string TranslateMapToStringEscapeChars( Match match )
		{
			string s = match.Value;
			if ( "%" == s )
				return "%25";
			if ( ":" == s )
				return "%3A";
			if ( "," == s )
				return "%2C";
			return s;
		}

		/// <summary>
		/// Map对象转换为字符串时转换特殊字符
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string UnescapeMapToStringSpecialChars( string s )
		{
			return Regex.Replace( s, "(%)|(:)|(,)", TranslateMapToStringEscapeChars );
		}

		/// <summary>
		/// Map对象转为字符串
		/// </summary>
		/// <param name="map">Map对象</param>
		/// <returns>返回字符串，key或value中的“%”“,”“:”符号分别用“%25”“%2C”“%3A”表示</returns>
		public static string MapToString( this IDictionary map )
		{
			IList ls = new ArrayList();
			foreach ( DictionaryEntry de in map )
			{
				String key = de.Key + "";
				key = UnescapeMapToStringSpecialChars( key );
				String value = de.Value + "";
				value = UnescapeMapToStringSpecialChars( value );
				ls.Add( key + ":" + value );
			}
			return Join( ",", ls );
		}

		public static readonly Regex PATTERN_REGEX_SPECIAL_CHARS = new Regex( "(\\\\)|(\\^)|(\\$)|(\\.)|(\\*)|(\\+)|(\\-)|(\\?)|(\\|)|(\\{)|(\\})|(\\[)|(\\])|(\\()|(\\))" );

		static class RegexSpecialCharsTranslator
		{
			static readonly Hashtable REGEX_CHARS_MAP = new Hashtable();

			static RegexSpecialCharsTranslator()
			{
				REGEX_CHARS_MAP.Add( "\\", "\\\\" );
				REGEX_CHARS_MAP.Add( "^", "\\^" );
				REGEX_CHARS_MAP.Add( "$", "\\$" );
				REGEX_CHARS_MAP.Add( ".", "\\." );
				REGEX_CHARS_MAP.Add( "*", "\\*" );
				REGEX_CHARS_MAP.Add( "+", "\\+" );
				REGEX_CHARS_MAP.Add( "-", "\\-" );
				REGEX_CHARS_MAP.Add( "?", "\\?" );
				REGEX_CHARS_MAP.Add( "|", "\\|" );
				REGEX_CHARS_MAP.Add( "{", "\\{" );
				REGEX_CHARS_MAP.Add( "}", "\\}" );
				REGEX_CHARS_MAP.Add( "[", "\\[" );
				REGEX_CHARS_MAP.Add( "]", "\\]" );
				REGEX_CHARS_MAP.Add( "(", "\\(" );
				REGEX_CHARS_MAP.Add( ")", "\\)" );

			}

			public static string OnTranslate( Match match )
			{
				string group = match.Value;
				return TypeUtil.GetString( REGEX_CHARS_MAP[group], group );
			}
		}

		/// <summary>
		/// 转义正则表达式的特殊字符
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string EscapeRegexSpecialChars( this string s )
		{
			return PATTERN_REGEX_SPECIAL_CHARS.Replace( s, RegexSpecialCharsTranslator.OnTranslate );
		}

		/// <summary>
		/// 替换字符串
		/// </summary>
		/// <param name="s">原字符串</param>
		/// <param name="replaceMap">替换Map</param>
		/// <param name="options">匹配规则，指定位可以包含：RegexOptions.Multiline, RegexOptions.IgnoreCase等</param>
		/// <returns>返回替换后的字符串</returns>
		public static string Replace( this string s, IDictionary replaceMap, RegexOptions options = RegexOptions.Multiline )
		{
			IList sl = new ArrayList( replaceMap.Keys );
			for ( int i = 0; i < sl.Count; i++ )
				sl[i] = EscapeRegexSpecialChars( TypeUtil.GetString( sl[i] ) );
			String ps = "|".Join( Quote( sl, "(", ")" ) );
			return Regex.Replace( s, ps, new MapMatchEvaluator( replaceMap ).OnEvaluator, options );
		}

		/// <summary>
		/// 替换字符串
		/// </summary>
		/// <param name="s">原字符串</param>
		/// <param name="replaceMap">替换Map，结构形式为mapToString()结果，key或value中的“%”“,”“:”符号分别用“%25”“%2C”“%3A”表示</param>
		/// <param name="options">匹配规则，指定位可以包含：RegexOptions.Multiline, RegexOptions.IgnoreCase等</param>
		/// <returns>返回替换后的字符串</returns>
		public static string Replace( this string s, string replaceMap, RegexOptions options = RegexOptions.Multiline )
		{
			return Replace( s, StringToMap( replaceMap ), options );
		}

		/// <summary>
		/// 字符编码
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public static string EscapeChar( char c )
		{
			return "&#" + ( ( int )c ) + ";";
		}

		public static string EscapeAll( string s )
		{
			StringBuilder sb = new StringBuilder();
			for ( int i = 0; i < s.Length; i++ )
				sb.Append( "&#" ).Append( ( int )s[i] ).Append( ";" );
			return sb.ToString();
		}

		public static string EscapeCharTranslate( Match match )
		{
			string s = match.Value;
			int c;
			if ( s.StartsWith( "&#x" ) )
				c = int.Parse( s.Substring( 3, s.Length - 4 ), NumberStyles.HexNumber );
			else if ( s.StartsWith( "&#" ) )
				c = int.Parse( s.Substring( 2, s.Length - 3 ) );
			else
				return UnescapeXML( s );
			return char.ToString( ( char )c );
		}

		public static readonly Regex PATTERN_CHAR = new Regex( "(\\&#\\d{1,5};)|(\\&#x[0-9a-fA-F]{1,4};)" );

		public static string Unescape( this string s )
		{
			return PATTERN_CHAR.Replace( s, EscapeCharTranslate );
		}

		public static readonly Regex PATTERN_XML = new Regex( "[&<>'\"]" );
		public static readonly MapMatchEvaluator XML_ESCAPE_TRANSLATOR = new MapMatchEvaluator(
			StringToMap( "&:&amp;,<:&lt;,>:&gt;,':&#39;,\":&quot;, :&nbsp;" ) );

		/// <summary>
		/// 转义XML特殊字符
		/// </summary>
		/// <param name="s">原字符</param>
		/// <returns>返回转换后的字符串</returns>
		public static string EscapeXML( this string s )
		{
			return PATTERN_XML.Replace( s, XML_ESCAPE_TRANSLATOR.OnEvaluator );
		}

		public static readonly Regex PATTERN_XML2 = new Regex( "(\\&amp;)|(&lt;)|(&gt;)|(&#39;)|(&quot;)|(\\$\\$)|(&nbsp;)" );
		public static readonly MapMatchEvaluator XML_UNESCAPE_TRANSLATOR = new MapMatchEvaluator(
			StringToMap( "&amp;:&,&lt;:<,&gt;:>,&#39;:',&quot;:\",&nbsp;: " ) );

		/// <summary>
		/// 反向转义XML特殊字符
		/// </summary>
		/// <param name="s">原字符</param>
		/// <returns>返回转换后的字符串</returns>
		public static string UnescapeXML( this string s )
		{
			return PATTERN_XML2.Replace( s, XML_UNESCAPE_TRANSLATOR.OnEvaluator );
		}

		public static readonly Regex PATTERN_HTML = new Regex( "[&<>'\" ]" );

		/// <summary>
		/// 转义HTML特殊字符
		/// </summary>
		/// <param name="s">原字符</param>
		/// <returns>返回转换后的字符串</returns>
		public static string EscapeHtml( this string s )
		{
			s = PATTERN_HTML.Replace( s, XML_ESCAPE_TRANSLATOR.OnEvaluator );
			s = s.Replace( "\n", "<br>\r\n" );
			return s;
		}

	}
}
