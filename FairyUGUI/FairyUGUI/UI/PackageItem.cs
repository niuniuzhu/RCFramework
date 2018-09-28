using Core.Xml;
using FairyUGUI.Core;
using FairyUGUI.Core.Fonts;
using UnityEngine;

namespace FairyUGUI.UI
{
	/// <summary>
	/// 
	/// </summary>
	public class PackageItem
	{
		public UIPackage owner;

		public PackageItemType type;
		public string id;
		public string name;
		public int width;
		public int height;
		public string file;
		public bool decoded;
		public bool exported;

		//image
		public NTexture texture;
		public NSprite sprite;
		public ImageScaleMode scaleMode;

		//movieclip
		public Vector2 pivot;
		public float interval;
		public float repeatDelay;
		public bool swing;
		public MovieClip.Frame[] frames;

		//componenet
		public XML componentData;

		//font
		public BitmapFont bitmapFont;

		//sound
		public AudioClip audioClip;

		//misc
		public byte[] binary;

		public object Load()
		{
			return this.owner.GetItemAsset( this );
		}
	}
}
