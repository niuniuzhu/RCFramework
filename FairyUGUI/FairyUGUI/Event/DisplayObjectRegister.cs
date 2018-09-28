using System.Collections.Generic;
using FairyUGUI.Core;

namespace FairyUGUI.Event
{
	public static class DisplayObjectRegister
	{
		private static readonly List<DisplayObject> DISPLAYOBJECTS = new List<DisplayObject>();
		private static readonly HashSet<DisplayObject> MAP = new HashSet<DisplayObject>();

		public static List<DisplayObject> GetDisplayObjects()
		{
			return DISPLAYOBJECTS;
		}

		public static void RegisterDisplayObject( DisplayObject displayObject )
		{
			if ( MAP.Contains( displayObject ) )
				return;
			MAP.Add( displayObject );
			DISPLAYOBJECTS.Add( displayObject );
		}

		public static bool UnregisterDisplayObject( DisplayObject displayObject )
		{
			return MAP.Remove( displayObject ) && DISPLAYOBJECTS.Remove( displayObject );
		}
	}
}