using System;
using System.Collections.Generic;
using FairyUGUI.UI;
using UnityEngine;

namespace FairyUGUI.Core.Fonts
{
	public class DynamicFont : BaseFont
	{
		private readonly Dictionary<int, int> _cachedBaseline = new Dictionary<int, int>();

		public DynamicFont( string name )
			: base( name )
		{
			this.isDynamic = true;

			this.LoadFont();
		}

		private void LoadFont()
		{
			this.font = Resources.Load<Font>( this.name ) ??
						 Resources.Load<Font>( "Fonts/" + this.name );

			if ( this.font == null )
			{
				if ( this.name.IndexOf( ",", StringComparison.Ordinal ) != -1 )
				{
					string[] arr = this.name.Split( ',' );
					int cnt = arr.Length;
					for ( int i = 0; i < cnt; i++ )
						arr[i] = arr[i].Trim();
					this.font = Font.CreateDynamicFontFromOSFont( arr, 32 );
				}
				else
					this.font = Font.CreateDynamicFontFromOSFont( this.name, 32 );
			}
			if ( this.font == null )
			{
				if ( this.name != UIConfig.defaultFont )
				{
					DynamicFont bf = FontManager.GetFont( UIConfig.defaultFont ) as DynamicFont;
					if ( bf != null )
						this.font = bf.font;
				}

				if ( this.font == null )
					this.font = ( Font )Resources.GetBuiltinResource( typeof( Font ), "Arial.ttf" );
			}

			if ( this.font == null )
				throw new Exception( "Cant load font '" + this.name + "'" );

			//this.font.material.mainTexture.filterMode = FilterMode.Point;
			this.mainTexture = new NTexture( ( Texture2D )this.font.material.mainTexture, null, null );
		}

		public override void Dispose()
		{
			this._cachedBaseline.Clear();
			this.font = null;
		}

		internal int GetBaseLine( int size )
		{
			int result;

			if ( this._cachedBaseline.TryGetValue( size, out result ) )
				return result;

			CharacterInfo charInfo;
			this.font.RequestCharactersInTexture( "f|体_j", size, FontStyle.Normal );

			float y0 = float.MinValue;
			if ( this.font.GetCharacterInfo( 'f', out charInfo, size, FontStyle.Normal ) )
				y0 = Mathf.Max( y0, charInfo.maxY );
			if ( this.font.GetCharacterInfo( '|', out charInfo, size, FontStyle.Normal ) )
				y0 = Mathf.Max( y0, charInfo.maxY );
			if ( this.font.GetCharacterInfo( '体', out charInfo, size, FontStyle.Normal ) )
				y0 = Mathf.Max( y0, charInfo.maxY );

			//find the most bottom position
			float y1 = float.MaxValue;
			if ( this.font.GetCharacterInfo( '_', out charInfo, size, FontStyle.Normal ) )
				y1 = Mathf.Min( y1, charInfo.minY );
			if ( this.font.GetCharacterInfo( '|', out charInfo, size, FontStyle.Normal ) )
				y1 = Mathf.Min( y1, charInfo.minY );
			if ( this.font.GetCharacterInfo( 'j', out charInfo, size, FontStyle.Normal ) )
				y1 = Mathf.Min( y1, charInfo.minY );

			result = ( int )( y0 + ( y0 - y1 - size ) * 0.5f );
			this._cachedBaseline.Add( size, result );

			return result;
		}
	}
}
