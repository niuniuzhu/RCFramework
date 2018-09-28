using System;
using System.IO;
using System.Text;
using Core.Crypto;
using UnityEngine;

namespace Game.Loader
{
	public static class LuaLoader
	{
		public static readonly byte[] AES_KEY = Convert.FromBase64String( "bZdEWqA/z5mfkeUYsRfiEA==" );
		public static readonly byte[] AES_IV = Convert.FromBase64String( "+qe0mOBjiaSU8YJwteeQ3Q==" );

		public static byte[] Load( ref string path, ref string name, bool binary )
		{
			byte[] fileBytes;
			name = name.Replace( '.', '/' );
			if ( binary )
			{
				string fullPath = Path.Combine( path, name + ".bytes" );
				TextAsset asset = AssetsManager.LoadAsset<TextAsset>( "lua", fullPath );
				if ( asset == null )
					throw new FileNotFoundException( $"Lua file:{fullPath} not exist." );
				byte[] bytes = AesUtil.AesDecrypt( asset.bytes, AES_KEY, AES_IV );
				fileBytes = bytes;
			}
			else
			{
				string fullPath = Path.Combine( path, name + ".lua" );
				fileBytes = Encoding.UTF8.GetBytes( File.ReadAllText( fullPath ) );
			}
			return fileBytes;
		}
	}
}