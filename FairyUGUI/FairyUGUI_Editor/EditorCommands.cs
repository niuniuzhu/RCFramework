using System.Text;
using UnityEditor;
using UnityEngine;

namespace FairyUGUI_Editor
{
	public class EditorCommands
	{
		[MenuItem( "Edit/Generate Atlas &g" )]
		private static void GenAtlas()
		{
			AtlasGenerator.GenAtlas();
		}

		[MenuItem( "Edit/GetOSInstalledFontNames" )]
		private static void GetOSInstalledFontNames()
		{
			StringBuilder sb = new StringBuilder();
			string[] fonts = Font.GetOSInstalledFontNames();
			foreach ( string font in fonts )
			{
				sb.Append( font + "," );
			}
			Debug.Log( sb.ToString() );
		}
	}
}