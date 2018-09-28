using System.Collections;
using System.Collections.Generic;
using System.Text;
using Game.Task;
using UnityEngine;

namespace Game.Loader
{
	public class HttpLoader
	{
		public enum Type
		{
			Get,
			PosT
		}

		private HttpCompleteHandler _completeInvoker;
		public event HttpCompleteHandler OnComplete
		{
			add => this._completeInvoker += value;
			remove => this._completeInvoker -= value;
		}

		private ErrorHandler _errorInvoker;
		public event ErrorHandler OnError
		{
			add => this._errorInvoker += value;
			remove => this._errorInvoker -= value;
		}

		private ProgressHandler _progressInvoker;
		public event ProgressHandler OnProgress
		{
			add => this._progressInvoker += value;
			remove => this._progressInvoker -= value;
		}

		public Type type { get; private set; }

		public string url { get; private set; }

		public object data;

		private bool _cancel;

		private Dictionary<string, string> _params;

		public HttpLoader( string url, Type type, object data = null )
		{
			this.url = url;
			this.type = type;
			this.data = data;
		}

		public virtual void Cancel()
		{
			this._cancel = true;
		}

		public void AddParam( string key, string value )
		{
			if ( this._params == null )
				this._params = new Dictionary<string, string>();
			this._params[key] = value;
		}

		public void Load()
		{
			SyncTask.Create( this.OnLoad() );
		}

		private IEnumerator OnLoad()
		{
#if USE_WEB_RESUEST
			WWWEx www = new WWWEx( this.url );
#else
			WWW www;
			string url = this.url;
			if ( this.type == Type.Get )
			{
				if ( this._params != null )
				{
					url += "?";
					foreach ( KeyValuePair<string, string> kv in this._params )
						url += kv.Key + "=" + kv.Value + "&";
					url = url.Substring( 0, url.Length - 1 );
				}
				www = new WWW( url );
			}
			else
			{
				if ( this._params != null )
				{
					WWWForm form = new WWWForm();
					foreach ( KeyValuePair<string, string> kv in this._params )
						form.AddField( kv.Key, kv.Value );
					www = new WWW( url, form );
				}
				else
					www = new WWW( url );
			}
#endif

			while ( !www.isDone )
			{
			    this._progressInvoker?.Invoke( this, www.progress );
			    yield return 1;
			}
		    this._progressInvoker?.Invoke( this, 1f );

		    if ( this._cancel )
			{
				www.Dispose();
				yield break;
			}

			if ( !string.IsNullOrEmpty( www.error ) )
			{
                this._errorInvoker?.Invoke(this, www.error);
            }
			else
			{
			    this._completeInvoker?.Invoke( this, www.bytes, www.text );
			}

		    www.Dispose();
		}

		public string DumpParams()
		{
			StringBuilder sb = new StringBuilder();
			foreach ( KeyValuePair<string, string> kv in this._params )
			{
				sb.Append( kv.Key + "=" + kv.Value + "&" );
			}
			string str = sb.ToString();
			str = str.Substring( 0, str.Length - 1 );
			return str;
		}

		public void AddCompleteListener( HttpCompleteHandler handler )
		{
			this.OnComplete += handler;
		}

		public void RemoveCompleteListener( HttpCompleteHandler handler )
		{
			this.OnComplete -= handler;
		}

		public void AddErrorListener( ErrorHandler handler )
		{
			this.OnError += handler;
		}

		public void RemoveErrorListener( ErrorHandler handler )
		{
			this.OnError -= handler;
		}

		public void AddProgressListener( ProgressHandler handler )
		{
			this.OnProgress += handler;
		}

		public void RemoveProgressListener( ProgressHandler handler )
		{
			this.OnProgress -= handler;
		}
	}
}