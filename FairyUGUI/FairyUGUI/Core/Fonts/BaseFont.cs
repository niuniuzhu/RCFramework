using FairyUGUI.UI;
using UnityEngine;

namespace FairyUGUI.Core.Fonts
{
	public abstract class BaseFont
	{
		public string name { get; protected set; }

		public Font font { get; protected set; }

		public NTexture mainTexture { get; protected set; }

		public bool isDynamic { get; protected set; }

		public DynamicFont asDynamicFont => this as DynamicFont;

		public BitmapFont asBitmapFont => this as BitmapFont;

		public PackageItem packageItem;

		protected BaseFont()
		{
		}

		protected BaseFont( string name )
		{
			this.name = name;
		}

		public abstract void Dispose();
	}
}
