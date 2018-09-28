using FairyUGUI.Core;
using FairyUGUI.Event;
using UnityEngine;

namespace FairyUGUI.UI
{
	public class DragManager
	{
		private static DragManager _instance;
		public static DragManager instance => _instance ?? ( _instance = new DragManager() );

		private GObject _source;
		private Vector2 _startDragPointerPosition;
		private Vector2 _startDragPosition;
		private Vector3[] _restrictWorldCorners;

		private DragManager()
		{
		}

		public void StartDrag( GObject source, PointerEventData e, Vector3[] restrictWorldCorners = null )
		{
			this._source = source;
			this._source.onDrag.Add( this.OnDrag );
			this._source.onEndDrag.Add( this.OnEndDrag );
			this._restrictWorldCorners = restrictWorldCorners;

			this._startDragPosition = this._source.position;

			RectTransformUtility.ScreenPointToLocalPointInRectangle( this._source.displayObject.rectTransform, e.position, Stage.inst.eventCamera, out this._startDragPointerPosition );
		}

		private void OnDrag( EventContext context )
		{
			PointerEventData e = ( PointerEventData )context.eventData;

			this._source.position = this._startDragPosition;

			Vector2 currPointerPosition;
			RectTransformUtility.ScreenPointToLocalPointInRectangle( this._source.displayObject.rectTransform, e.position, Stage.inst.eventCamera, out currPointerPosition );

			Vector2 delta = currPointerPosition - this._startDragPointerPosition;
			delta.y = -delta.y;
			this._source.position += delta;

			if ( this._restrictWorldCorners != null )
			{
				Vector3 min = this._restrictWorldCorners[0];
				Vector3 max = this._restrictWorldCorners[2];
				min = this._source.parent.displayObject.rectTransform.InverseTransformPoint( min );
				max = this._source.parent.displayObject.rectTransform.InverseTransformPoint( max );
				Vector2 size = this._source.displayObject.rect.size;
				Vector3 pos = this._source.displayObject.rectTransform.localPosition;
				pos.x = Mathf.Clamp( pos.x, min.x, max.x - size.x );
				pos.y = Mathf.Clamp( pos.y, min.y + size.y, max.y );
				this._source.displayObject.rectTransform.localPosition = pos;
			}

			e.StopPropagation();
		}

		private void OnEndDrag( EventContext context )
		{
			this.Cancel();
		}

		public void Cancel()
		{
			this._source.onDrag.Remove( this.OnDrag );
			this._source.onEndDrag.Remove( this.OnEndDrag );
			this._source = null;
			this._restrictWorldCorners = null;
		}
	}
}