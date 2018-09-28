using FairyUGUI.Core;
using System.Collections.Generic;
using UnityEngine;

namespace FairyUGUI.Event
{
	public class Raycaster
	{
		private readonly List<DisplayObject> _results = new List<DisplayObject>();

		public void Raycast( PointerEventData eventData, out RaycastResult result )
		{
			result = new RaycastResult();

			Vector2 pos;
			if ( Stage.inst.eventCamera == null )
			{
				float w = Screen.width;
				float h = Screen.height;
				pos = new Vector2( eventData.position.x / w, eventData.position.y / h );
			}
			else
				pos = Stage.inst.eventCamera.ScreenToViewportPoint( eventData.position );

			if ( pos.x < 0f || pos.x > 1f || pos.y < 0f || pos.y > 1f )
				return;

			List<DisplayObject> rts = DisplayObjectRegister.GetDisplayObjects();
			int count = rts.Count;
			if ( count == 0 )
				return;

			DisplayObject displayObject;
			for ( int i = 0; i < count; i++ )
			{
				displayObject = rts[i];
				if ( displayObject.IsTriggerTypesEmpty() ||
					displayObject.stage == null ||
					!displayObject.touchable ||
					!displayObject.visible )
					continue;
				bool hit = displayObject.Raycast( eventData.position, Stage.inst.eventCamera );
				if ( hit )
					this._results.Add( displayObject );
			}
			if ( this._results.Count == 0 )
				return;

			if ( this._results.Count > 1 )
				this._results.Sort( Compare );

			//RectTransform hit = this._results[0];
			//this.GetChain( hit );

			//只需要第一个
			displayObject = this._results[0];
			result.displayObject = displayObject;
			result.screenPosition = eventData.position;
			this._results.Clear();
		}

		private static int Compare( DisplayObject d1, DisplayObject d2 )
		{
			DisplayObject t2 = d2;
			while ( t2 != null )
			{
				if ( t2 == d1 )
					return 1;
				t2 = t2.parent;
			}
			DisplayObject t1 = d1;
			while ( t1 != null )
			{
				if ( t1 == d2 )
					return -1;
				t1 = t1.parent;
			}
			//处理不在一条干路上
			bool found = false;
			t1 = d1;
			t2 = d2;
			while ( t1.parent != null )
			{
				t2 = d2;
				while ( t2.parent != null )
				{
					if ( t1.parent == t2.parent )
					{
						found = true;
						break;
					}
					t2 = t2.parent;
				}
				if ( found )
					break;
				t1 = t1.parent;
			}
			return t2.siblingIndex.CompareTo( t1.siblingIndex );
		}

		//private DisplayObject GetRootRect( DisplayObject rt )
		//{
		//	DisplayObject p = rt;
		//	while ( p != null && p != GRoot.inst.rootContainer )
		//	{
		//		p = p.parent;
		//	}
		//	return p;
		//}

		//private void GetChain( DisplayObject rt )
		//{
		//	Transform p = rt;
		//	while ( p != null && p.GetComponent<Canvas>() == null )
		//	{
		//		if ( p is RectTransform )
		//			this._chain.Add( ( RectTransform )p );
		//		p = p.parent;
		//	}
		//}
	}
}