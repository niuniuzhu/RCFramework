using UnityEngine;

namespace FairyUGUI.Core
{
	public interface ILayoutItem
	{
		bool ignoreLayout { get; }

		Vector2 size { get; set; }

		Vector2 actualSize { get; set; }

		Vector2 position { get; set; }

		bool visible { get; }
	}
}