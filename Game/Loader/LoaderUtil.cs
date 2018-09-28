using UnityEngine;

namespace Game.Loader
{
	public static class LoaderUtil
	{
		public static string GetRelativePath()
		{
			string platform = GetAssetBundleOutputFolderName( Application.platform );
			return $"{AssetsManager.assetBundlePath}/{Application.productName}_{platform}/";
		}

		public static string GetAssetBundleOutputFolderName( RuntimePlatform platform )
		{
			switch ( platform )
			{
				case RuntimePlatform.WindowsEditor:
				case RuntimePlatform.WindowsPlayer:
					return "Windows";

				case RuntimePlatform.OSXPlayer:
				case RuntimePlatform.OSXEditor:
					return "OSX";

				case RuntimePlatform.Android:
					return Application.isEditor ? GetRuntimeOSFolderName( platform ) : "Android";

				case RuntimePlatform.IPhonePlayer:
					return Application.isEditor ? GetRuntimeOSFolderName( platform ) : "iOS";

				case RuntimePlatform.WebGLPlayer:
					return "WebGL";

				case RuntimePlatform.LinuxPlayer:
					return "Linux";

				default:
					return null;
			}
		}

		private static string GetRuntimeOSFolderName( RuntimePlatform platform )
		{
			switch ( platform )
			{
				case RuntimePlatform.WindowsEditor:
					return "Windows";

				case RuntimePlatform.OSXEditor:
					return "OSX";

				default:
					return null;
			}
		}
	}
}