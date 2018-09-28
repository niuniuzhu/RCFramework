using FairyUGUI.Utils;
using UnityEngine;

namespace FairyUGUI.Core
{
	/// <summary>
	/// GoWrapper is class for wrapping common gameobject into UI display list.
	/// </summary>
	public class GoWrapper : DisplayObject
	{
		private GameObject _go;
		private Renderer[] _renders;

		internal override int sortingOrder
		{
			set
			{
				base.sortingOrder = value;
				int cnt = this._renders.Length;
				for ( int i = 0; i < cnt; i++ )
				{
					Renderer r = this._renders[i];
					if ( r != null )
						this._renders[i].sortingOrder = value;
				}
			}
		}

		internal override int layer
		{
			set
			{
				base.layer = value;
				this.SetGoLayer( value );
			}
		}

		public GoWrapper( GameObject go )
			: base( null )
		{
			this._go = go;
			this.SetGoLayer( LayerMask.NameToLayer( Stage.LAYER_NAME ) );
			this.name = "GoWrapper";
			ToolSet.SetParent( this._go.transform, this.rectTransform );
			this.CacheRenderers();
		}

		private void SetGoLayer( int layer )
		{
			Transform[] transforms = this._go.GetComponentsInChildren<Transform>( true );
			int count = transforms.Length;
			for ( int i = 0; i < count; i++ )
			{
				Transform t = transforms[i];
				t.gameObject.layer = layer;
			}
		}

		/// <summary>
		/// GoWrapper will cache all renderers of your gameobject on constructor. 
		/// If your gameobject change laterly, call this function to update the cache.
		/// GoWrapper会在构造函数里查询你的gameobject所有的Renderer并保存。如果你的gameobject
		/// 后续发生了改变，调用这个函数通知GoWrapper重新查询和保存。
		/// </summary>
		private void CacheRenderers()
		{
			this._renders = this._go.GetComponentsInChildren<Renderer>( true );
			int cnt = this._renders.Length;
			for ( int i = 0; i < cnt; i++ )
				this._renders[i].material.renderQueue = 3000;
		}

		protected override void InternalDispose()
		{
			if ( this._go != null )
			{
				Object.DestroyImmediate( this._go );
				this._go = null;
			}

			base.InternalDispose();
		}
	}
}