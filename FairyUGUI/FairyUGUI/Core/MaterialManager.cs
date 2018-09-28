using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyUGUI.Core
{
	internal static class MaterialManager
	{
		private static readonly Dictionary<KeywordFlag, MaterialPool> CACHED_MATERIALS = new Dictionary<KeywordFlag, MaterialPool>();

		[Flags]
		internal enum KeywordFlag
		{
			AlphaTex = 1 << 0,
			MaskTex = 1 << 1,
			Grayed = 1 << 2,
			ColorFilter = 1 << 3,
			BlurFilter = 1 << 4,
			OverlayNormal = 1 << 5,
			OverlayNone = 1 << 6,
			OverlayAdd = 1 << 7,
			OverlayMultiply = 1 << 8,
			OverlayScreen = 1 << 9,
			OverlayErase = 1 << 10,
			OverlayMask = 1 << 11,
			OverlayBelow = 1 << 12
		};

		private static readonly Dictionary<string, KeywordFlag> KEYWORD_TO_FLAGS = new Dictionary<string, KeywordFlag>
		{
			{"__", KeywordFlag.OverlayNormal},
			{"ALPHA_TEX", KeywordFlag.AlphaTex},
			{"MASK_TEX", KeywordFlag.MaskTex},
			{"GRAYED", KeywordFlag.Grayed},
			{"COLOR_FILTER", KeywordFlag.ColorFilter},
			{"BLUR_FILTER", KeywordFlag.BlurFilter}
		};

		private static readonly Dictionary<BlendMode, KeywordFlag> BLENDMODE_TO_FLAGS = new Dictionary<BlendMode, KeywordFlag>
		{
			{BlendMode.Normal, KeywordFlag.OverlayNormal},
			{BlendMode.None, KeywordFlag.OverlayNone},
			{BlendMode.Add, KeywordFlag.OverlayAdd},
			{BlendMode.Multiply, KeywordFlag.OverlayMultiply},
			{BlendMode.Screen, KeywordFlag.OverlayScreen},
			{BlendMode.Erase, KeywordFlag.OverlayErase},
			{BlendMode.Mask, KeywordFlag.OverlayMask},
			{BlendMode.Below, KeywordFlag.OverlayBelow},
		};

		internal static NMaterial EnableAlphaTexture( NMaterial currentMaterial )
		{
			return GetMaterial( currentMaterial, "ALPHA_TEX", true );
		}

		internal static NMaterial DisableAlphaTexture( NMaterial currentMaterial )
		{
			return GetMaterial( currentMaterial, "ALPHA_TEX", false );
		}

		internal static NMaterial EnableMaskTexture( NMaterial currentMaterial )
		{
			return GetMaterial( currentMaterial, "MASK_TEX", true );
		}

		internal static NMaterial DisableMaskTexture( NMaterial currentMaterial )
		{
			return GetMaterial( currentMaterial, "MASK_TEX", false );
		}

		internal static NMaterial EnableGrayed( NMaterial currentMaterial )
		{
			return GetMaterial( currentMaterial, "GRAYED", true );
		}

		internal static NMaterial DisableGrayed( NMaterial currentMaterial )
		{
			return GetMaterial( currentMaterial, "GRAYED", false );
		}

		internal static NMaterial EnableColorFilter( NMaterial currentMaterial )
		{
			return GetMaterial( currentMaterial, "COLOR_FILTER", true );
		}

		internal static NMaterial DisableColorFilter( NMaterial currentMaterial )
		{
			return GetMaterial( currentMaterial, "COLOR_FILTER", false );
		}

		internal static NMaterial EnableBlurFilter( NMaterial currentMaterial )
		{
			return GetMaterial( currentMaterial, "BLUR_FILTER", true );
		}

		internal static NMaterial DsiableBlurFilter( NMaterial currentMaterial )
		{
			return GetMaterial( currentMaterial, "BLUR_FILTER", false );
		}

		internal static bool IsShared( KeywordFlag keywordFlag )
		{
			return ( keywordFlag & KeywordFlag.ColorFilter ) == 0;
		}

		internal static NMaterial GetDefaultMaterial()
		{
			NMaterial material;
			MaterialPool pool;
			const KeywordFlag keywordFlag = KeywordFlag.OverlayNormal;
			CACHED_MATERIALS.TryGetValue( keywordFlag, out pool );
			if ( pool == null )
			{
				pool = new MaterialPool( Shader.Find( "UI/Default UI" ), null, keywordFlag );
				material = pool.Get();
				material.ApplyBlendMode( BlendMode.Normal );
				pool.Release( material );
				CACHED_MATERIALS[keywordFlag] = pool;
			}
			material = pool.Get();
			return material;
		}

		internal static NMaterial SetBlendMode( NMaterial currentMaterial, BlendMode blendMode )
		{
			KeywordFlag keywordFlag = currentMaterial.keywordFlag;
			if ( ( keywordFlag & BLENDMODE_TO_FLAGS[blendMode] ) > 0 )
				return currentMaterial;

			currentMaterial.pool.Release( currentMaterial );

			int flag = ( int )keywordFlag;
			flag &= ~0x1fe0;

			keywordFlag = ( KeywordFlag )flag;
			keywordFlag |= BLENDMODE_TO_FLAGS[blendMode];

			MaterialPool pool;
			CACHED_MATERIALS.TryGetValue( keywordFlag, out pool );
			if ( pool == null )
			{
				pool = new MaterialPool( Shader.Find( "UI/Default UI" ), currentMaterial.shaderKeywords, keywordFlag );
				CACHED_MATERIALS[keywordFlag] = pool;
			}
			NMaterial material = pool.Get();
			CopyProperties( currentMaterial, material );
			return material;
		}

		private static NMaterial GetMaterial( NMaterial currentMaterial, string keyword, bool enable )
		{
			KeywordFlag keywordFlag = currentMaterial.keywordFlag;
			if ( enable )
			{
				if ( ( keywordFlag & KEYWORD_TO_FLAGS[keyword] ) > 0 )
					return currentMaterial;
				keywordFlag |= KEYWORD_TO_FLAGS[keyword];
			}
			else
			{
				if ( ( keywordFlag & KEYWORD_TO_FLAGS[keyword] ) == 0 )
					return currentMaterial;
				keywordFlag &= ~KEYWORD_TO_FLAGS[keyword];
			}

			currentMaterial.pool.Release( currentMaterial );

			MaterialPool pool;
			CACHED_MATERIALS.TryGetValue( keywordFlag, out pool );
			if ( pool == null )
			{
				string[] keywords = currentMaterial.shaderKeywords;
				string[] newKeywords = new string[keywords.Length + 1];
				Array.Copy( keywords, newKeywords, keywords.Length );
				newKeywords[keywords.Length] = keyword;
				pool = new MaterialPool( Shader.Find( "UI/Default UI" ), newKeywords, keywordFlag );
				CACHED_MATERIALS[keywordFlag] = pool;
			}
			NMaterial material = pool.Get();
			CopyProperties( currentMaterial, material );
			return material;
		}

		private static void CopyProperties( NMaterial source, NMaterial destination )
		{
			if ( source.HasProperty( "_ClipRect" ) )
				destination.SetVector( "_ClipRect", source.GetVector( "_ClipRect" ) );

			if ( source.HasProperty( "_Color" ) )
				destination.SetColor( "_Color", source.GetColor( "_Color" ) );
			if ( source.HasProperty( "_ColorMask" ) )
				destination.SetFloat( "_ColorMask", source.GetFloat( "_ColorMask" ) );

			if ( source.HasProperty( "_BlendSrcFactor" ) )
				destination.SetFloat( "_BlendSrcFactor", source.GetFloat( "_BlendSrcFactor" ) );
			if ( source.HasProperty( "_BlendDstFactor" ) )
				destination.SetFloat( "_BlendDstFactor", source.GetFloat( "_BlendDstFactor" ) );

			if ( source.HasProperty( "_UseAlphaTexture" ) )
				destination.SetFloat( "_UseAlphaTexture", source.GetFloat( "_UseAlphaTexture" ) );

			if ( source.HasProperty( "_UseUIAlphaClip" ) )
				destination.SetFloat( "_UseUIAlphaClip", source.GetFloat( "_UseUIAlphaClip" ) );

			if ( source.HasProperty( "_UseGrayed" ) )
				destination.SetFloat( "_UseGrayed", source.GetFloat( "_UseGrayed" ) );

			if ( source.HasProperty( "_UseColorFilter" ) )
				destination.SetFloat( "_UseColorFilter", source.GetFloat( "_UseColorFilter" ) );
			if ( source.HasProperty( "_ColorOffset" ) )
				destination.SetVector( "_ColorOffset", source.GetVector( "_ColorOffset" ) );
			if ( source.HasProperty( "_ColorMatrix" ) )
				destination.SetMatrix( "_ColorMatrix", source.GetMatrix( "_ColorMatrix" ) );

			if ( source.HasProperty( "_UseBlurFilter" ) )
				destination.SetFloat( "_UseBlurFilter", source.GetFloat( "_UseBlurFilter" ) );
			if ( source.HasProperty( "_BlurSize" ) )
				destination.SetFloat( "_BlurSize", source.GetFloat( "_BlurSize" ) );
		}
	}

	class MaterialPool
	{
		private readonly Stack<NMaterial> _materials = new Stack<NMaterial>();
		private readonly Shader _shader;
		private readonly bool _shared;
		private readonly MaterialManager.KeywordFlag _keywordFlag;
		private readonly string[] _keywords;

		internal MaterialPool( Shader shader, string[] keywords, MaterialManager.KeywordFlag keywordFlag )
		{
			this._shader = shader;
			this._keywords = keywords;
			this._keywordFlag = keywordFlag;

			NMaterial material = this.Get();
			this.Release( material );

			this._shared = MaterialManager.IsShared( this._keywordFlag );
		}

		internal NMaterial Get()
		{
			NMaterial material;
			if ( this._shared )
				material = this._materials.Peek();
			else
			{
				if ( this._materials.Count == 0 )
				{
					material = new NMaterial( this._shader ) { keywordFlag = this._keywordFlag, pool = this };
					if ( this._keywords != null )
						material.shaderKeywords = this._keywords;
				}
				else
					material = this._materials.Pop();
			}
			return material;
		}

		internal void Release( NMaterial material )
		{
			if ( this._shared || material.pool != this )
				return;

			this._materials.Push( material );
		}
	}
}
