using System;
using System.Collections;
using System.Globalization;

namespace Core.Misc
{
	/// <summary>
	/// 一些类型判断和类别转换的方法
	/// </summary>
	public static class TypeUtil
	{

		/// <summary>
		/// 判断o是否Dictionary对象
		/// </summary>
		/// <param name="o"></param>
		/// <returns>是返回true</returns>
		public static bool IsMap( object o )
		{
			if ( o is IDictionary )
				return true;
			return false;
		}

		/// <summary>
		/// 判断o是否Array对象
		/// </summary>
		/// <param name="o"></param>
		/// <returns>是返回true</returns>
		public static bool IsArray( object o )
		{
			if ( o != null && o.GetType().IsArray )
				return true;
			return false;
		}

		/// <summary>
		/// 判断o是否List对象
		/// </summary>
		/// <param name="o"></param>
		/// <returns>是返回true</returns>
		public static bool IsList( object o )
		{
			if ( o is IList )
				return true;
			return false;
		}

		/// <summary>
		/// 判断o是否Collection对象
		/// </summary>
		/// <param name="o"></param>
		/// <returns>是返回true</returns>
		public static bool IsCollection( object o )
		{
			if ( o is ICollection )
				return true;
			return false;
		}

		/// <summary>
		/// 判断o是否String对象
		/// </summary>
		/// <param name="o"></param>
		/// <returns>是返回true</returns>
		public static bool IsString( object o )
		{
			if ( o is String )
				return true;
			return false;
		}

		/// <summary>
		/// 判断o是否Boolean对象
		/// </summary>
		/// <param name="o"></param>
		/// <returns>是返回true</returns>
		public static bool IsBool( object o )
		{
			if ( o is Boolean )
				return true;
			return false;
		}

		/// <summary>
		/// 判断是否数值对象，判断是否为Byte,Int16,Int32,Int64,Float,Double,Decimal,SByte,UInt16,UInt32,UInt64
		/// </summary>
		/// <param name="o"></param>
		/// <returns>是返回true</returns>
		public static bool IsNumber( object o )
		{
			return IsByte( o ) || IsShort( o ) || IsInt( o ) || IsLong( o ) || IsFloat( o ) || IsDouble( o ) || IsDecimal( o ) || IsSByte( o ) || IsUShort( o ) || IsUInt( o ) || IsULong( o );
		}

		/// <summary>
		/// 判断是否整数值对象，判断是否为Byte,Int16,Int32,Int64,SByte,UInt16,UInt32,UInt64
		/// </summary>
		/// <param name="o"></param>
		/// <returns>是返回true</returns>
		public static bool IsIntegral( object o )
		{
			return IsByte( o ) || IsShort( o ) || IsInt( o ) || IsLong( o ) || IsSByte( o ) || IsUShort( o ) || IsUInt( o ) || IsULong( o );
		}

		/// <summary>
		/// 判断是否浮点数值对象，判断是否为Float,Double
		/// </summary>
		/// <param name="o"></param>
		/// <returns>是返回true</returns>
		public static bool IsFloating( object o )
		{
			return IsFloat( o ) || IsDouble( o ) || IsDecimal( o );
		}

		/// <summary>
		/// 判断o是否Byte对象
		/// </summary>
		/// <param name="o"></param>
		/// <returns>是返回true</returns>
		public static bool IsByte( object o )
		{
			if ( o is Byte )
				return true;
			return false;
		}

		/// <summary>
		/// 判断o是否SByte对象
		/// </summary>
		/// <param name="o"></param>
		/// <returns>是返回true</returns>
		public static bool IsSByte( object o )
		{
			if ( o is SByte )
				return true;
			return false;
		}

		/// <summary>
		/// 判断o是否Int16对象
		/// </summary>
		/// <param name="o"></param>
		/// <returns>是返回true</returns>
		public static bool IsShort( object o )
		{
			if ( o is Int16 )
				return true;
			return false;
		}

		/// <summary>
		/// 判断o是否UInt16对象
		/// </summary>
		/// <param name="o"></param>
		/// <returns>是返回true</returns>
		public static bool IsUShort( object o )
		{
			if ( o is UInt16 )
				return true;
			return false;
		}

		/// <summary>
		/// 判断o是否Char对象
		/// </summary>
		/// <param name="o"></param>
		/// <returns>是返回true</returns>
		public static bool IsChar( object o )
		{
			if ( o is Char )
				return true;
			return false;
		}

		/// <summary>
		/// 判断o是否Int32对象
		/// </summary>
		/// <param name="o"></param>
		/// <returns>是返回true</returns>

		public static bool IsInt( object o )
		{
			if ( o is Int32 )
				return true;
			return false;
		}

		/// <summary>
		/// 判断o是否UInt32对象
		/// </summary>
		/// <param name="o"></param>
		/// <returns>是返回true</returns>
		public static bool IsUInt( object o )
		{
			if ( o is UInt32 )
				return true;
			return false;
		}

		/// <summary>
		/// 判断o是否Int64对象
		/// </summary>
		/// <param name="o"></param>
		/// <returns>是返回true</returns>
		public static bool IsLong( object o )
		{
			if ( o is Int64 )
				return true;
			return false;
		}

		/// <summary>
		/// 判断o是否UInt64对象
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public static bool IsULong( object o )
		{
			if ( o is UInt64 )
				return true;
			return false;
		}

		/// <summary>
		/// 判断o是否Float对象
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public static bool IsFloat( object o )
		{
			if ( o is Single )
				return true;
			return false;
		}

		/// <summary>
		/// 判断o是否Double对象
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public static bool IsDouble( object o )
		{
			if ( o is Double )
				return true;
			return false;
		}

		/// <summary>
		/// 判断o是否Decimal对象
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public static bool IsDecimal( object o )
		{
			if ( o is Decimal )
				return true;
			return false;
		}

		/// <summary>
		/// 判断o是否Date对象
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public static bool IsDateTime( object o )
		{
			if ( o is DateTime )
				return true;
			return false;
		}

		/// <summary>
		/// 判断o是否Type对象
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public static bool IsType( object o )
		{
			if ( o is Type )
				return true;
			return false;
		}

		/// <summary>
		/// 获得o对象的逻辑值
		/// </summary>
		/// <param name="o"></param>
		/// <param name="dv"></param>
		/// <returns></returns>
		public static bool GetBool( object o, bool dv = false )
		{
			if ( o == null )
				return dv;
			if ( IsBool( o ) )
				return ( bool )o;
			if ( IsNumber( o ) )
				return ( GetDouble( o, 0 ) != 0 );
			if ( IsString( o ) )
			{
				string s = ( String )o;
				if ( "true".Equals( s.ToLower() ) )
					return true;
				else if ( "false".Equals( s.ToLower() ) )
					return false;
				else
				{
					try
					{
						return double.Parse( s ) != 0;
					}
					catch ( Exception )
					{
					}
				}
			}
			return dv;
		}

		/// <summary>
		/// 获得o的byte值
		/// </summary>
		/// <param name="o"></param>
		/// <param name="dv"></param>
		/// <returns>返回o的byte值</returns>
		public static byte GetByte( object o, byte dv = (byte)0 )
		{
			if ( o == null )
				return dv;
			if ( IsBool( o ) )
				return ( ( bool )o ) ? ( byte )1 : ( byte )0;
			if ( IsByte( o ) )
				return ( byte )o;
			if ( IsShort( o ) )
				return Convert.ToByte( ( short )o );
			if ( IsInt( o ) )
				return Convert.ToByte( ( int )o );
			if ( IsLong( o ) )
				return Convert.ToByte( ( long )o );
			if ( IsFloat( o ) )
				return Convert.ToByte( ( float )o );
			if ( IsDouble( o ) )
				return Convert.ToByte( ( double )o );
			if ( IsDecimal( o ) )
				return Convert.ToByte( ( decimal )o );
			if ( IsString( o ) )
			{
				try
				{
					return byte.Parse( ( String )o );
				}
				catch ( Exception )
				{
				}
			}
			return dv;
		}

		/// <summary>
		/// 获得o的short值
		/// </summary>
		/// <param name="o"></param>
		/// <param name="dv"></param>
		/// <returns>返回o的short值</returns>
		public static short GetShort( object o, short dv = (short)0 )
		{
			if ( o == null )
				return dv;
			if ( IsBool( o ) )
				return ( ( bool )o ) ? ( short )1 : ( short )0;
			if ( IsByte( o ) )
				return Convert.ToInt16( ( byte )o );
			if ( IsShort( o ) )
				return Convert.ToInt16( ( short )o );
			if ( IsInt( o ) )
				return Convert.ToInt16( ( int )o );
			if ( IsLong( o ) )
				return Convert.ToInt16( ( long )o );
			if ( IsFloat( o ) )
				return Convert.ToInt16( ( float )o );
			if ( IsDouble( o ) )
				return Convert.ToInt16( ( double )o );
			if ( IsDecimal( o ) )
				return Convert.ToInt16( ( decimal )o );
			if ( IsString( o ) )
			{
				try
				{
					return short.Parse( ( String )o );
				}
				catch ( Exception )
				{
				}
			}
			return dv;
		}

		/// <summary>
		/// 获得o的short值
		/// </summary>
		/// <param name="o"></param>
		/// <param name="dv"></param>
		/// <returns>返回o的short值</returns>
		public static ushort GetUShort( object o, ushort dv = (ushort)0 )
		{
			if ( o == null )
				return dv;
			if ( IsBool( o ) )
				return ( ( bool )o ) ? ( ushort )1 : ( ushort )0;
			if ( IsByte( o ) )
				return Convert.ToUInt16( ( byte )o );
			if ( IsShort( o ) )
				return Convert.ToUInt16( ( short )o );
			if ( IsInt( o ) )
				return Convert.ToUInt16( ( int )o );
			if ( IsLong( o ) )
				return Convert.ToUInt16( ( long )o );
			if ( IsFloat( o ) )
				return Convert.ToUInt16( ( float )o );
			if ( IsDouble( o ) )
				return Convert.ToUInt16( ( double )o );
			if ( IsDecimal( o ) )
				return Convert.ToUInt16( ( decimal )o );
			if ( IsString( o ) )
			{
				try
				{
					return ushort.Parse( ( String )o );
				}
				catch ( Exception )
				{
				}
			}
			return dv;
		}

		/// <summary>
		/// 获得o的char值
		/// </summary>
		/// <param name="o"></param>
		/// <param name="dv"></param>
		/// <returns>返回o的byte值</returns>
		public static char GetChar( object o, char dv = (char)0 )
		{
			if ( o == null )
				return dv;
			if ( IsChar( o ) )
				return ( char )o;
			if ( IsByte( o ) )
				return Convert.ToChar( ( byte )o );
			if ( IsShort( o ) )
				return Convert.ToChar( ( short )o );
			if ( IsInt( o ) )
				return Convert.ToChar( ( int )o );
			if ( IsLong( o ) )
				return Convert.ToChar( ( long )o );
			if ( IsFloat( o ) )
				return Convert.ToChar( ( float )o );
			if ( IsDouble( o ) )
				return Convert.ToChar( ( double )o );
			if ( IsDecimal( o ) )
				return Convert.ToChar( ( decimal )o );
			if ( IsString( o ) )
			{
				String s = ( String )o;
				if ( s.Length >= 1 )
					return s[0];
			}
			return dv;
		}

		/// <summary>
		/// 获得o的int值
		/// </summary>
		/// <param name="o"></param>
		/// <param name="dv"></param>
		/// <returns>返回o的int值</returns>
		public static int GetInt( object o, int dv = 0 )
		{
			if ( o == null )
				return dv;
			if ( IsBool( o ) )
				return ( ( bool )o ) ? 1 : 0;
			if ( IsByte( o ) )
				return Convert.ToInt32( ( byte )o );
			if ( IsShort( o ) )
				return Convert.ToInt32( ( short )o );
			if ( IsInt( o ) )
				return Convert.ToInt32( ( int )o );
			if ( IsLong( o ) )
				return Convert.ToInt32( ( long )o );
			if ( IsFloat( o ) )
				return Convert.ToInt32( ( float )o );
			if ( IsDouble( o ) )
				return Convert.ToInt32( ( double )o );
			if ( IsDecimal( o ) )
				return Convert.ToInt32( ( decimal )o );
			if ( IsString( o ) )
			{
				try
				{
					return int.Parse( ( String )o );
				}
				catch ( Exception )
				{
				}
			}
			return dv;
		}

		/// <summary>
		/// 获得o的long值
		/// </summary>
		/// <param name="o"></param>
		/// <param name="dv"></param>
		/// <returns>返回o的long值</returns>
		public static long GetLong( object o, long dv = 0 )
		{
			if ( o == null )
				return dv;
			if ( IsBool( o ) )
				return ( ( bool )o ) ? 1 : 0;
			if ( IsByte( o ) )
				return Convert.ToInt64( ( Byte )o );
			if ( IsShort( o ) )
				return Convert.ToInt64( ( short )o );
			if ( IsInt( o ) )
				return Convert.ToInt64( ( int )o );
			if ( IsLong( o ) )
				return Convert.ToInt64( ( long )o );
			if ( IsFloat( o ) )
				return Convert.ToInt64( ( float )o );
			if ( IsDouble( o ) )
				return Convert.ToInt64( ( double )o );
			if ( IsDecimal( o ) )
				return Convert.ToInt64( ( decimal )o );
			if ( IsDateTime( o ) )
				return ( ( DateTime )o ).Ticks;
			if ( IsString( o ) )
			{
				try
				{
					return long.Parse( ( String )o );
				}
				catch ( Exception )
				{
				}
			}
			return dv;
		}

		/// <summary>
		/// 获得o的float值
		/// </summary>
		/// <param name="o"></param>
		/// <param name="dv"></param>
		/// <returns>返回o的float值</returns>
		public static float GetFloat( object o, float dv = 0 )
		{
			if ( o == null )
				return dv;
			if ( IsBool( o ) )
				return ( ( bool )o ) ? 1 : 0;
			if ( IsByte( o ) )
				return Convert.ToSingle( ( byte )o );
			if ( IsShort( o ) )
				return Convert.ToSingle( ( short )o );
			if ( IsInt( o ) )
				return Convert.ToSingle( ( int )o );
			if ( IsLong( o ) )
				return Convert.ToSingle( ( long )o );
			if ( IsFloat( o ) )
				return Convert.ToSingle( ( float )o );
			if ( IsDouble( o ) )
				return Convert.ToSingle( ( double )o );
			if ( IsDecimal( o ) )
				return Convert.ToSingle( ( decimal )o );
			if ( IsString( o ) )
			{
				try
				{
					return float.Parse( ( String )o );
				}
				catch ( Exception )
				{
				}
			}
			return dv;
		}

		/// <summary>
		/// 获得o的double值
		/// </summary>
		/// <param name="o"></param>
		/// <param name="dv"></param>
		/// <returns>返回o的double值</returns>
		public static double GetDouble( object o, double dv = 0 )
		{
			if ( o == null )
				return dv;
			if ( IsBool( o ) )
				return ( ( bool )o ) ? 1 : 0;
			if ( IsByte( o ) )
				return Convert.ToDouble( ( byte )o );
			if ( IsShort( o ) )
				return Convert.ToDouble( ( short )o );
			if ( IsInt( o ) )
				return Convert.ToDouble( ( int )o );
			if ( IsLong( o ) )
				return Convert.ToDouble( ( long )o );
			if ( IsFloat( o ) )
				return Convert.ToDouble( ( float )o );
			if ( IsDouble( o ) )
				return Convert.ToDouble( ( double )o );
			if ( IsDecimal( o ) )
				return Convert.ToDouble( ( decimal )o );
			if ( IsDateTime( o ) )
				return ( ( DateTime )o ).Ticks;
			if ( IsString( o ) )
			{
				try
				{
					return double.Parse( ( String )o );
				}
				catch ( Exception )
				{
				}
			}
			return dv;
		}

		/// <summary>
		/// 获得o的decimal值
		/// </summary>
		/// <param name="o"></param>
		/// <param name="dv"></param>
		/// <returns>返回o的double值</returns>
		public static decimal GetDecimal( object o, decimal dv = 0 )
		{
			if ( o == null )
				return dv;
			if ( IsBool( o ) )
				return ( ( bool )o ) ? 1 : 0;
			if ( IsByte( o ) )
				return Convert.ToDecimal( ( byte )o );
			if ( IsShort( o ) )
				return Convert.ToDecimal( ( short )o );
			if ( IsInt( o ) )
				return Convert.ToDecimal( ( int )o );
			if ( IsLong( o ) )
				return Convert.ToDecimal( ( long )o );
			if ( IsFloat( o ) )
				return Convert.ToDecimal( ( float )o );
			if ( IsDouble( o ) )
				return Convert.ToDecimal( ( double )o );
			if ( IsDecimal( o ) )
				return ( decimal )o;
			if ( IsDateTime( o ) )
				return ( ( DateTime )o ).Ticks;
			if ( IsString( o ) )
			{
				try
				{
					return decimal.Parse( ( String )o );
				}
				catch ( Exception )
				{
				}
			}
			return dv;
		}

		/// <summary>
		/// 获得o的DateTime值
		/// </summary>
		/// <param name="o">有效对象为Long，DateTime，String</param>
		/// <param name="pattern">日期时间匹配形式</param>
		/// <param name="dv">默认值</param>
		/// <returns>返回o的Date值</returns>
		public static DateTime GetDateTime( object o, String pattern, DateTime dv )
		{
			if ( o == null )
				return dv;
			else if ( IsLong( o ) )
				return new DateTime( ( long )o );
			else if ( IsDateTime( o ) )
				return ( DateTime )o;
			else if ( IsString( o ) )
			{
				try
				{
					return DateTime.ParseExact( ( String )o, pattern, DateTimeFormatInfo.InvariantInfo );
				}
				catch ( Exception )
				{
				}
			}
			return dv;
		}

		/// <summary>
		/// 获得o的DateTime值
		/// </summary>
		/// <param name="o">有效对象为Long，Date，String(yyyy-MM-dd HH:mm:ss)</param>
		/// <param name="dv"></param>
		/// <returns>返回o的Date值</returns>
		public static DateTime GetDateTime( object o, DateTime dv )
		{
			return GetDateTime( o, "yyyy-MM-dd HH:mm:ss", dv );
		}

		/// <summary>
		/// 获得o的DateTime值
		/// </summary>
		/// <param name="o">有效对象为Long，Date，String(yyyy-MM-dd)</param>
		/// <param name="dv"></param>
		/// <returns>返回o的Date值</returns>
		public static DateTime GetDate( object o, DateTime dv )
		{
			return GetDateTime( o, "yyyy-MM-dd", dv );
		}

		/// <summary>
		/// 获得o的DateTime值
		/// </summary>
		/// <param name="o">有效对象为Long，Date，String(HH:mm:ss)</param>
		/// <param name="dv"></param>
		/// <returns>返回o的Date值</returns>
		public static DateTime GetTime( object o, DateTime dv )
		{
			return GetDateTime( o, "HH:mm:ss", dv );
		}

		/// <summary>
		/// 获得o的string值
		/// </summary>
		/// <param name="o"></param>
		/// <param name="dv"></param>
		/// <returns>返回o.toString(), o为null时返回dv</returns>
		public static string GetString( object o, string dv = "" )
		{
			return o == null ? dv : o.ToString();
		}

		public static IList GetList( object o, IList dv )
		{
			if ( IsList( o ) )
				return ( IList )o;
			return dv;
		}

		public static IDictionary GetMap( object o, IDictionary dv = null )
		{
			if ( IsMap( o ) )
				return ( IDictionary )o;
			return dv;
		}

		/// <summary>
		/// 任意对象转为字符串
		/// </summary>
		/// <param name="o">对象</param>
		/// <returns>返回对象的字符串形式</returns>
		public static string ObjectToString( object o )
		{
			if ( o == null )
				return "null";
			else if ( IsString( o ) )
				return ( String )o;
			else if ( IsDateTime( o ) )
				return ( ( DateTime )o ).ToString( "yyyy-MM-dd HH:mm:ss" );
			else if ( IsChar( o ) )
				return ( ( char )o ).ToString();
			else if ( o is char[] )
				return new string( ( char[] )o );
			return o.ToString();
		}

		public static object[] ToArray( this ICollection c )
		{
			object[] os = new object[c.Count];
			int i = 0;
			foreach ( object o in c )
				os[i++] = o;
			return os;
		}

		public static ArrayList ToList( this object[] os )
		{
			ArrayList ls = new ArrayList();
			for ( int i = 0; i < os.Length; i++ )
				ls.Add( os[i] );
			return ls;
		}

	}
}
