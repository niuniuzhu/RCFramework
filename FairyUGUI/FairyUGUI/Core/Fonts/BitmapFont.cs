using System.Collections.Generic;
using FairyUGUI.UI;
using UnityEngine;

namespace FairyUGUI.Core.Fonts
{
	public class BitmapFont : BaseFont
	{
		public int lineHeight { get; private set; }

		public BitmapFont( PackageItem item )
		{
			this.packageItem = item;
			this.name = UIPackage.URL_PREFIX + this.packageItem.owner.id + this.packageItem.id;
		}

		public void MakeFont( NTexture texture, List<CharacterInfo> cis, int lineHeight )
		{
			this.lineHeight = lineHeight;

			this.font = new Font( this.packageItem.name );
			this.font.characterInfo = cis.ToArray();
			this.mainTexture = texture;
		}

		public override void Dispose()
		{
			if ( this.font != null )
			{
				Object.DestroyImmediate( this.font.material );
				Object.DestroyImmediate( this.font );
				this.font = null;
			}
		}
	}
}
