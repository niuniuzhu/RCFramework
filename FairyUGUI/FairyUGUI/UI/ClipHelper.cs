using System.Collections.Generic;
using FairyUGUI.Core;
using UnityEngine;

namespace FairyUGUI.UI
{
	public static class ClipHelper
	{
		static readonly Vector3[] WORLD_CORNERS = new Vector3[4];
		static readonly Vector3[] CANVAS_CORNERS = new Vector3[4];

		public static Rect GetCanvasRect( DisplayObject clipper, DisplayObject root )
		{
			clipper.GetWorldCorners( WORLD_CORNERS );
			float xMin = float.MaxValue;
			float xMax = -float.MaxValue;
			float yMin = float.MaxValue;
			float yMax = -float.MaxValue;
			for ( int i = 0; i < 4; ++i )
			{
				CANVAS_CORNERS[i] = root.rectTransform.InverseTransformPoint( WORLD_CORNERS[i] );
				float xx = CANVAS_CORNERS[i].x;
				float yy = CANVAS_CORNERS[i].y;
				xMin = xx < xMin ? xx : xMin;
				xMax = xx > xMax ? xx : xMax;
				yMin = yy < yMin ? yy : yMin;
				yMax = yy > yMax ? yy : yMax;
			}

			return new Rect( xMin, yMin, xMax - xMin, yMax - yMin );
		}

		public static Rect FindCullAndClipWorldRect( Rect canvasRect, List<Rect> parentMaskRects, out DisplayObject.CullState cullsaState )
		{
			Rect compoundRect = canvasRect;
			int count = parentMaskRects.Count;
			for ( int i = count - 1; i >= 0; --i )
			{
				compoundRect = RectIntersect( compoundRect, parentMaskRects[i] );
				bool cull = compoundRect.width <= 0 || compoundRect.height <= 0;
				if ( cull )
				{
					cullsaState = DisplayObject.CullState.Culled;
					return compoundRect;
				}
			}
			cullsaState = DisplayObject.CullState.Overlaps;
			return compoundRect;
		}

		private static Rect RectIntersect( Rect a, Rect b )
		{
			float xMin = Mathf.Max( a.x, b.x );
			float xMax = Mathf.Min( a.x + a.width, b.x + b.width );
			float yMin = Mathf.Max( a.y, b.y );
			float yMax = Mathf.Min( a.y + a.height, b.y + b.height );
			if ( xMax >= xMin && yMax >= yMin )
				return new Rect( xMin, yMin, xMax - xMin, yMax - yMin );
			return new Rect( 0f, 0f, 0f, 0f );
		}
	}
}