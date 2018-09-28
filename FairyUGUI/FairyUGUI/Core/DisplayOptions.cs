using UnityEngine;

namespace FairyUGUI.Core
{
	public static class DisplayOptions
	{
		public static RectTransform[] defaultRoot;//use only in edit mode. use array to avoid unity null reference checking
		public static HideFlags hideFlags = HideFlags.None;

		public static void SetEditModeHideFlags()
		{
			hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor;
		}
	}
}