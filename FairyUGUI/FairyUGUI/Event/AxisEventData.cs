using UnityEngine;

namespace FairyUGUI.Event
{
	public enum MoveDirection
	{
		Left,
		Up,
		Right,
		Down,
		None
	}

	public class AxisEventData:BaseEventData
	{
		public Vector2 moveVector { get; set; }

		public MoveDirection moveDir { get; set; }

		public AxisEventData()
		{
			this.moveVector = Vector2.zero;
			this.moveDir = MoveDirection.None;
		}
	}
}