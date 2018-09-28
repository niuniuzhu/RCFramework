#if !UNITY_IPHONE
using System.Collections;
using System.IO;
using Core.Net;

namespace Game.Loader
{
	/// <summary>
	/// WWW扩展实现，支持大数据的流式输出和文件输出
	/// </summary>
	public class WWWEx : WebClient
	{
		// Constructors

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="url">下载url</param>
		/// <param name="headers">请求header数据</param>
		public WWWEx( string url, IDictionary headers = null )
			: base( url, headers )
		{
		    this.AsyncGet();
		}

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="url">下载url</param>
		/// <param name="saveStream">输出流</param>
		/// <param name="headers">请求header数据</param>
		public WWWEx( string url, Stream saveStream, IDictionary headers = null )
			: base( url, headers )
		{
		    this.AsyncToStream( saveStream );
		}

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="url">下载url</param>
		/// <param name="saveFile">输出文件名</param>
		/// <param name="headers">请求header数据</param>
		public WWWEx( string url, string saveFile, IDictionary headers = null )
			: base( url, headers )
		{
		    this.AsyncToFile( saveFile );
		}

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="url">下载url</param>
		/// <param name="postData">提交数据</param>
		/// <param name="headers">请求header数据</param>
		public WWWEx( string url, byte[] postData, IDictionary headers = null )
			: base( url, headers )
		{
		    this.AsyncPost( postData );
		}

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="url">下载url</param>
		/// <param name="postData">提交数据</param>
		/// <param name="saveStream">输出流</param>
		/// <param name="headers">请求header数据</param>
		public WWWEx( string url, byte[] postData, Stream saveStream, IDictionary headers = null )
			: base( url, headers )
		{
		    this.AsyncPostAndToStream( postData, saveStream );
		}

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="url">下载url</param>
		/// <param name="postData">提交数据</param>
		/// <param name="saveFile">输出文件名</param>
		/// <param name="headers">请求header数据</param>
		public WWWEx( string url, byte[] postData, string saveFile, IDictionary headers = null )
			: base( url, headers )
		{
		    this.AsyncPostAndToFile( postData, saveFile );
		}
	}
}
#endif