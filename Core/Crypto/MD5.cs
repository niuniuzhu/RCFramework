using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Core.Misc;

namespace Core.Crypto
{
	/// <summary>
	/// MD5便捷方法
	/// </summary>
	public static class MD5Util
	{
		/// <summary>
		/// 获得摘要
		/// </summary>
		/// <param name="data"></param>
		/// <returns>返回摘要</returns>
		public static byte[] GetMd5Digest( byte[] data )
		{
			MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
			return md5.ComputeHash( data );
		}

		public static byte[] GetMd5Digest( string data )
		{
			MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
			return md5.ComputeHash( Encoding.UTF8.GetBytes( data ) );
		}

		public static byte[] GetMd5Digest( byte[] data, int offset, int count )
		{
			MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
			return md5.ComputeHash( data, offset, count );
		}

		public static byte[] GetMd5Digest( Stream i )
		{
			MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
			return md5.ComputeHash( i );
		}

		public static byte[] GetMd5Digest( FileInfo file )
		{
			return new MD5().GetDigest( file );
		}

		/// <summary>
		/// 获得十六进制摘要
		/// </summary>
		/// <param name="data"></param>
		/// <returns>返回摘要字符串</returns>
		public static string GetMd5HexDigest( byte[] data )
		{
			return GetMd5Digest( data ).ToHexString();
		}

		public static string GetMd5HexDigest( string data )
		{
			return GetMd5Digest( data ).ToHexString();
		}

		public static string GetMd5HexDigest( byte[] data, int offset, int count )
		{
			return GetMd5Digest( data, offset, count ).ToHexString();
		}

		public static string GetMd5HexDigest( Stream i )
		{
			return GetMd5Digest( i ).ToHexString();
		}

		public static string GetMd5HexDigest( FileInfo file )
		{
			return new MD5().GetHexDigest( file );
		}
	}

	/// <summary>
	/// MD5对象便捷封装
	/// </summary>
	public class MD5 : IDisposable
	{
		private readonly System.Security.Cryptography.MD5 _md5 = new MD5CryptoServiceProvider();

		public System.Security.Cryptography.MD5 md5
		{
			get { return this._md5; }
		}

		/// <summary>
		/// 获得摘要
		/// </summary>
		/// <param name="data"></param>
		/// <returns>返回摘要</returns>
		public byte[] GetDigest( byte[] data )
		{
			return this._md5.ComputeHash( data );
		}

		public byte[] GetDigest( string data )
		{
			return this._md5.ComputeHash( Encoding.UTF8.GetBytes( data ) );
		}

		public byte[] GetDigest( byte[] data, int offset, int count )
		{
			return this._md5.ComputeHash( data, offset, count );
		}

		public byte[] GetDigest( Stream i )
		{
			return this._md5.ComputeHash( i );
		}

		public byte[] GetDigest( FileInfo file )
		{
			FileStream fi = new FileStream( file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read );
			try
			{
				return this._md5.ComputeHash( fi );
			}
			finally
			{
				fi.Close();
			}
		}

		/// <summary>
		/// 获得十六进制摘要
		/// </summary>
		/// <param name="data"></param>
		/// <returns>返回摘要字符串</returns>
		public string GetHexDigest( byte[] data )
		{
			return this.GetDigest( data ).ToHexString();
		}

		public string GetHexDigest( string data )
		{
			return this.GetDigest( data ).ToHexString();
		}

		public string GetHexDigest( byte[] data, int offset, int count )
		{
			return this.GetDigest( data, offset, count ).ToHexString();
		}

		public string GetHexDigest( Stream i )
		{
			return this.GetDigest( i ).ToHexString();
		}

		public string GetHexDigest( FileInfo file )
		{
			return this.GetDigest( file ).ToHexString();
		}

		public void Dispose()
		{
			this._md5.Clear();
		}
	}
}
