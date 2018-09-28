using FairyUGUI.Core;
using UnityEngine;

namespace FairyUGUI.Event
{
	public struct RaycastResult
	{
		public DisplayObject displayObject;
		public Vector2 screenPosition;

		public void Clear()
		{
			this.displayObject = null;
			this.screenPosition = Vector2.zero;
		}

		public override string ToString()
		{
			return "name: " + ( this.displayObject == null ? string.Empty : this.displayObject.name ) + "\n" +
				   "screenPosition: " + this.screenPosition;
		}
	}
}