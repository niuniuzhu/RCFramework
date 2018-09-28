using FairyUGUI.Core;
using FairyUGUI.Event;
using UnityEngine;

namespace FairyUGUI.UI
{
	public class DragDropManager
	{
		private static DragDropManager _instance;
		public static DragDropManager instance => _instance ?? ( _instance = new DragDropManager() );

		public GLoader agent { get; private set; }

		public object sourceData { get; private set; }

		public bool dragging => this.agent != null;

		private Vector2 _lastDragPointerPosition;
		private GObject _source;

		private DragDropManager()
		{
		}

		public void StartDrag( GObject source, string icon, PointerEventData e, object sourceData )
		{
			if ( this.dragging )
				return;

			this.agent = ( GLoader )UIObjectFactory.NewObject( "loader" );
			GRoot.inst.AddChild( this.agent );

			this.agent.size = new Vector2( 100, 100 );
			this.agent.touchable = false;
			this.agent.align = AlignType.Center;
			this.agent.verticalAlign = VertAlignType.Middle;
			this.agent.sortingOrder = int.MaxValue;
			this.sourceData = sourceData;
			this.agent.url = icon;

			this._source = source;
			this._source.onDrag.Add( this.OnDrag );
			this._source.onEndDrag.Add( this.OnEndDrag );

			Vector3 worldPos = this._source.LocalToWorld( Vector3.zero );
			Vector3 localPos = GRoot.inst.WorldToLocal( worldPos );
			this.agent.position = localPos;

			RectTransformUtility.ScreenPointToLocalPointInRectangle( this._source.displayObject.rectTransform, e.position, Stage.inst.eventCamera, out this._lastDragPointerPosition );
		}

		private void OnDrag( EventContext context )
		{
			PointerEventData e = ( PointerEventData )context.eventData;

			Vector2 currPointerPosition;
			RectTransformUtility.ScreenPointToLocalPointInRectangle( this._source.displayObject.rectTransform, e.position, Stage.inst.eventCamera, out currPointerPosition );

			Vector2 delta = currPointerPosition - this._lastDragPointerPosition;
			delta.y = -delta.y;
			this.agent.position += delta;

			this._lastDragPointerPosition = currPointerPosition;

			e.StopPropagation();
		}

		private void OnEndDrag( EventContext context )
		{
			if ( !this.dragging )
				return;

			this.Cancel();
		}

		public void Cancel()
		{
			if ( !this.dragging )
				return;

			this.agent.Dispose();
			this.agent = null;
			this.sourceData = null;
			this._source.onDrag.Remove( this.OnDrag );
			this._source.onEndDrag.Remove( this.OnEndDrag );
			this._source = null;
		}
	}
}