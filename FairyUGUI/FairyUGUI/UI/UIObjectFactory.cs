using System.Collections.Generic;
using System.Reflection;
using Core.Xml;

namespace FairyUGUI.UI
{
	public static class UIObjectFactory
	{
		private static readonly Dictionary<string, ConstructorInfo> PACKAGE_ITEM_EXTENSIONS = new Dictionary<string, ConstructorInfo>();
		private static ConstructorInfo _loaderConstructor;

		public static void SetPackageItemExtension( string url, System.Type type )
		{
			PACKAGE_ITEM_EXTENSIONS[url.Substring( 5 )] = type.GetConstructor( System.Type.EmptyTypes );
		}

		public static void SetLoaderExtension( System.Type type )
		{
			_loaderConstructor = type.GetConstructor( System.Type.EmptyTypes );
		}

		public static GObject NewObject( PackageItem pi )
		{
			switch ( pi.type )
			{
				case PackageItemType.Image:
					return new GImage();

				case PackageItemType.MovieClip:
					return new GMovieClip();

				case PackageItemType.Component:
					{
						ConstructorInfo extentionConstructor;
						if ( PACKAGE_ITEM_EXTENSIONS.TryGetValue( pi.owner.id + pi.id, out extentionConstructor ) )
						{
							GComponent g = ( GComponent )extentionConstructor.Invoke( null );
							if ( g == null )
								throw new System.Exception( "Unable to create instance of '" + extentionConstructor.Name + "'" );

							return g;
						}

						pi.Load();
						XML xml = pi.componentData;
						string extention = xml.GetAttribute( "extention" );
						if ( extention != null )
						{
							switch ( extention )
							{
								case "Button":
									return new GButton();

								case "Label":
									return new GLabel();

								case "ProgressBar":
									return new GProgressBar();

								case "Slider":
									return new GSlider();

								case "ScrollBar":
									return new GScrollBar();

								case "ComboBox":
									return new GComboBox();

								default:
									return new GComponent();
							}
						}
						return new GComponent();
					}
			}
			return null;
		}

		public static GObject NewObject( string type, bool defValue = false )
		{
			switch ( type )
			{
				case "image":
					return new GImage();

				case "movieclip":
					return new GMovieClip();

				case "component":
					return new GComponent();

				case "text":
					if ( defValue )
						return new GTextInput();
					return new GTextField();

				case "richtext":
					return new GRichTextField();

				case "group":
					return new GGroup();

				case "list":
					return new GList();

				case "graph":
					return new GGraph();

				case "loader":
					if ( _loaderConstructor != null )
						return ( GLoader )_loaderConstructor.Invoke( null );
					return new GLoader();
			}
			return null;
		}
	}
}
