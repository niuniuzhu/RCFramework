using FairyUGUI.Event;
using FairyUGUI.UI;
using FairyUGUI.UI.UGUIExt;
using UnityEngine;

namespace FairyUGUI.Core
{
	public class Shape : DisplayObject
	{
		private GraphGraphic _nGraphic;

		internal float lineSize { get => this._nGraphic.lineSize; set => this._nGraphic.lineSize = value; }

		internal Color lineColor { get => this._nGraphic.lineColor; set => this._nGraphic.lineColor = value; }

		internal override Color color { get => this._nGraphic.color; set => this._nGraphic.color = value; }

		internal bool enableDraw { get => this._nGraphic.enableDraw; set => this._nGraphic.enableDraw = value; }

		internal GraphGraphic.Type type { get => this._nGraphic.type; set => this._nGraphic.type = value; }

		public Shape( GObject owner )
			: base( owner )
		{
			this.RegisterEventTriggerType( EventTriggerType.PointerClick );
			this.RegisterEventTriggerType( EventTriggerType.PointerDown );
			this.RegisterEventTriggerType( EventTriggerType.PointerUp );
			this.RegisterEventTriggerType( EventTriggerType.PointerEnter );
			this.RegisterEventTriggerType( EventTriggerType.PointerExit );
		}

		protected override void OnGameObjectCreated()
		{
			this.graphic = this._nGraphic = this.AddComponent<GraphGraphic>();
		}
	}
}