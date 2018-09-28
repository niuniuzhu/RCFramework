using System;
using System.Collections.Generic;
using FairyUGUI.Core;
using UnityEngine;

namespace FairyUGUI.UI
{
	/// <summary>
	/// Global configs. These options should be set before any UI construction.
	/// </summary>
	[AddComponentMenu( "FairyGUI/UI Config" )]
	public class UIConfig : MonoBehaviour
	{
		public static bool useCanvasSortingOrder;

		/// <summary>
		/// Dynamic Font Support. 
		/// 4.x: Put the xxx.ttf into /Resources or /Resources/Font, and set defaultFont="xxx".
		/// 5.x: set defaultFont to system font name(or names joint with comma). e.g. defaultFont="Microsoft YaHei, SimHei"
		/// </summary>
		public static string defaultFont = string.Empty;

		/// <summary>
		/// Resource using in Window.ShowModalWait for locking the window.
		/// </summary>
		public static string windowModalWaiting;

		/// <summary>
		/// Resource using in GRoot.ShowModalWait for locking the screen.
		/// </summary>
		public static String globalModalWaiting;

		/// <summary>
		/// When a modal window is in front, the background becomes dark.
		/// </summary>
		public static Color modalLayerColor = new Color( 0f, 0f, 0f, 0.4f );

		/// <summary>
		/// Default button click sound.
		/// </summary>
		public static AudioClip buttonSound;

		/// <summary>
		/// Default button click sound volume.
		/// </summary>
		public static float buttonSoundVolumeScale = 1f;

		/// <summary>
		/// Resource url of horizontal scrollbar
		/// </summary>
		public static string horizontalScrollBar;

		/// <summary>
		/// Resource url of vertical scrollbar
		/// </summary>
		public static string verticalScrollBar;

		public static ScrollView.MovementType defaultMovementType = ScrollView.MovementType.Elastic;

		/// <summary>
		/// Scrolling step in pixels
		/// </summary>
		public static float defaultScrollElasticity = 0.1f;

		public static bool defaultScrollInertia = true;

		public static float defaultScrollDecelerationRate = 0.035f;

		public static float defaultScrollSensitivity = 20.0f;

		/// <summary>
		/// Scrollbar display mode. Recommended 'Auto' for mobile and 'Visible' for web.
		/// </summary>
		public static ScrollBarDisplayType defaultScrollBarDisplay = ScrollBarDisplayType.Visible;

		/// <summary>
		/// The "rebound" effect in the scolling container.
		/// </summary> 
		public static bool defaultScrollBounceEffect = true;

		/// <summary>
		/// Resources url of PopupMenu.
		/// </summary>
		public static string popupMenu;

		/// <summary>
		/// Resource url of menu seperator.
		/// </summary>
		public static string popupMenuSeperator;

		/// <summary>
		/// In case of failure of loading content for GLoader, use this sign to indicate an error.
		/// </summary>
		public static string loaderErrorSign;

		/// <summary>
		/// Resource url of tooltips.
		/// </summary>
		public static string tooltipsWin;

		/// <summary>
		/// The number of visible items in ComboBox.
		/// </summary>
		public static int defaultComboBoxVisibleItemCount = 10;

		/// <summary>
		/// When click the window, brings to front automatically.
		/// </summary>
		public static bool bringWindowToFrontOnClick = true;

		/// <summary>
		/// Pixel drag threshold in seconds
		/// </summary>
		public static float pixelDragThreshold = 5f;

		public enum ConfigKey
		{
			UseCanvasSortingOrder,
			DefaultFont,
			ButtonSound,
			ButtonSoundVolumeScale,
			HorizontalScrollBar,
			VerticalScrollBar,
			DefaultMovementType,
			DefaultScrollBarDisplay,
			DefaultScrollElasticity,
			DefaultScrollDecelerationRate,
			DefaultScrollInertia,
			DefaultScrollSensitivity,
			DefaultScrollBounceEffect,
			WindowModalWaiting,
			GlobalModalWaiting,
			PopupMenu,
			PopupMenuSeperator,
			LoaderErrorSign,
			TooltipsWin,
			DefaultComboBoxVisibleItemCount,
			ModalLayerColor,
			PixelDragThreshold,

			PleaseSelect = 100
		}

		[Serializable]
		private class ConfigValue
		{
			public bool valid;
			public string s;
			public int i;
			public float f;
			public bool b;
			public Color c;

			public void Reset()
			{
				this.valid = false;
				this.s = null;
				this.i = 0;
				this.f = 0;
				this.b = false;
				this.c = Color.black;
			}
		}

		private readonly List<ConfigValue> _items = new List<ConfigValue>();
		private readonly List<string> _preloadPackages = new List<string>();

		void Awake()
		{
			if ( Application.isPlaying )
			{
				int count = this._preloadPackages.Count;
				for ( int i = 0; i < count; i++ )
				{
					string packagePath = this._preloadPackages[i];
					UIPackage.AddPackage( packagePath );
				}

				this.Load();
			}
		}

		private void Load()
		{
			int cnt = this._items.Count;
			for ( int i = 0; i < cnt; i++ )
			{
				ConfigValue value = this._items[i];
				if ( !value.valid )
					continue;

				switch ( ( ConfigKey )i )
				{
					case ConfigKey.UseCanvasSortingOrder:
						useCanvasSortingOrder = value.b;
						break;

					case ConfigKey.ButtonSound:
						if ( Application.isPlaying )
							buttonSound = UIPackage.GetItemAssetByURL( value.s ) as AudioClip;
						break;

					case ConfigKey.ButtonSoundVolumeScale:
						buttonSoundVolumeScale = value.f;
						break;

					case ConfigKey.DefaultComboBoxVisibleItemCount:
						defaultComboBoxVisibleItemCount = value.i;
						break;

					case ConfigKey.DefaultFont:
						defaultFont = value.s;
						break;

					case ConfigKey.DefaultMovementType:
						defaultMovementType = ( ScrollView.MovementType )value.i;
						break;

					case ConfigKey.DefaultScrollBarDisplay:
						defaultScrollBarDisplay = ( ScrollBarDisplayType )value.i;
						break;

					case ConfigKey.DefaultScrollBounceEffect:
						defaultScrollBounceEffect = value.b;
						break;

					case ConfigKey.DefaultScrollElasticity:
						defaultScrollElasticity = value.i;
						break;

					case ConfigKey.DefaultScrollDecelerationRate:
						defaultScrollDecelerationRate = value.i;
						break;

					case ConfigKey.DefaultScrollInertia:
						defaultScrollInertia = value.b;
						break;

					case ConfigKey.DefaultScrollSensitivity:
						defaultScrollSensitivity = value.i;
						break;

					case ConfigKey.GlobalModalWaiting:
						globalModalWaiting = value.s;
						break;

					case ConfigKey.HorizontalScrollBar:
						horizontalScrollBar = value.s;
						break;

					case ConfigKey.LoaderErrorSign:
						loaderErrorSign = value.s;
						break;

					case ConfigKey.ModalLayerColor:
						modalLayerColor = value.c;
						break;

					case ConfigKey.PopupMenu:
						popupMenu = value.s;
						break;

					case ConfigKey.PopupMenuSeperator:
						popupMenuSeperator = value.s;
						break;

					case ConfigKey.TooltipsWin:
						tooltipsWin = value.s;
						break;

					case ConfigKey.VerticalScrollBar:
						verticalScrollBar = value.s;
						break;

					case ConfigKey.WindowModalWaiting:
						windowModalWaiting = value.s;
						break;

					case ConfigKey.PixelDragThreshold:
						pixelDragThreshold = value.f;
						break;
				}
			}
		}

		public static void ClearResourceRefs()
		{
			defaultFont = "";
			buttonSound = null;
			globalModalWaiting = null;
			horizontalScrollBar = null;
			loaderErrorSign = null;
			popupMenu = null;
			popupMenuSeperator = null;
			tooltipsWin = null;
			verticalScrollBar = null;
			windowModalWaiting = null;
		}
	}
}
