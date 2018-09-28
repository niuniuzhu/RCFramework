using System.Collections.Generic;

namespace FairyUGUI.UI
{
	/// <summary>
	/// GObjectPool is use for GObject pooling.
	/// </summary>
	public class GObjectPool
	{
		/// <summary>
		/// Callback function when a new object is creating.
		/// </summary>
		/// <param name="obj"></param>
		public delegate void InitCallbackDelegate( GObject obj );

		/// <summary>
		/// Callback function when a new object is creating.
		/// </summary>
		public InitCallbackDelegate initCallback;

		private readonly Dictionary<string, Queue<GObject>> _pool = new Dictionary<string, Queue<GObject>>();

		/// <summary>
		/// Dispose all objects in the pool.
		/// </summary>
		public void Clear()
		{
			foreach ( KeyValuePair<string, Queue<GObject>> kv in this._pool )
			{
				Queue<GObject> list = kv.Value;
				foreach ( GObject obj in list )
					obj.Dispose();
			}
			this._pool.Clear();
		}

		public int count => this._pool.Count;

		public GObject GetObject( string url )
		{
			Queue<GObject> arr;
			if ( !this._pool.TryGetValue( url, out arr ) )
			{
				arr = new Queue<GObject>();
				this._pool.Add( url, arr );
			}

			if ( arr.Count > 0 )
			{
				return arr.Dequeue();
			}

			GObject obj = UIPackage.CreateObjectFromURL( url );
			if ( obj != null )
			{
                this.initCallback?.Invoke(obj);
            }

			return obj;
		}

		public void ReturnObject( GObject obj )
		{
			string url = obj.resourceURL;
			Queue<GObject> arr;
			if ( this._pool.TryGetValue( url, out arr ) )
				arr.Enqueue( obj );
		}
	}
}
