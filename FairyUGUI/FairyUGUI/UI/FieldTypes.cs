using System.Collections.Generic;
using DG.Tweening;
using FairyUGUI.Core;
using FairyUGUI.UI.UGUIExt;

namespace FairyUGUI.UI
{
	public enum AnchorType
	{
		Top_Left,
		Top_Center,
		Top_Right,
		Top_Stretch,
		Middle_Left,
		Middle_Center,
		Middle_Right,
		Middle_Stretch,
		Bottom_Left,
		Bottom_Center,
		Bottom_Right,
		Bottom_Stretch,
		Stretch_Left,
		Stretch_Center,
		Stretch_Right,
		Stretch_Stretch
	}

	public enum PackageItemType
	{
		Image,
		MovieClip,
		Sound,
		Component,
		Atlas,
		Font,
		Misc
	}
	public enum FlipType
	{
		None,
		Horizontal,
		Vertical,
		Both
	}

	public enum ImageScaleMode
	{
		Simple,
		Grid9,
		Tile,
		Filled,
	}

	public enum AlignType
	{
		Left,
		Center,
		Right
	}

	public enum VertAlignType
	{
		Top,
		Middle,
		Bottom
	}

	public enum OverflowType
	{
		Visible,
		Hidden,
		Scroll
	}

	public enum FillType
	{
		None,
		Scale,
		ScaleFree
	}

	public enum AutoSizeType
	{
		None,
		Both,
		Height
	}

	public enum ScrollType
	{
		Horizontal,
		Vertical,
		Both
	}

	public enum ScrollBarDisplayType
	{
		Default,
		Visible,
		Auto,
		Hidden
	}

	public enum RelationType
	{
		Left_Left,
		Left_Center,
		Left_Right,
		Center_Center,
		Right_Left,
		Right_Center,
		Right_Right,

		Top_Top,
		Top_Middle,
		Top_Bottom,
		Middle_Middle,
		Bottom_Top,
		Bottom_Middle,
		Bottom_Bottom,

		Width,
		Height,

		LeftExt_Left,
		LeftExt_Right,
		RightExt_Left,
		RightExt_Right,
		TopExt_Top,
		TopExt_Bottom,
		BottomExt_Top,
		BottomExt_Bottom,

		Size
	}

	public enum LayoutType
	{
		SingleColumn,
		SingleRow,
		FlowHorizontal,
		FlowVertical
	}

	public enum ListSelectionMode
	{
		Single,
		Multiple,
		Multiple_SingleClick,
		None
	}

	public enum ProgressTitleType
	{
		Percent,
		ValueAndMax,
		Value,
		Max
	}

	public enum ButtonMode
	{
		Common,
		Check,
		Radio
	}

	public enum TransitionActionType
	{
		XY,
		Size,
		Scale,
		Pivot,
		Alpha,
		Rotation,
		Color,
		Animation,
		Visible,
		Controller,
		Sound,
		Transition,
		Shake,
		Unknown
	}

	public enum OriginHorizontal
	{
		Left,
		Right,
	}

	public enum OriginVertical
	{
		Top,
		Bottom
	}

	public struct Margin
	{
		public int left;
		public int right;
		public int top;
		public int bottom;
	}

	static class FieldTypes
	{
		public static PackageItemType ParsePackageItemType( string value )
		{
			switch ( value )
			{
				case "image":
					return PackageItemType.Image;
				case "movieclip":
					return PackageItemType.MovieClip;
				case "component":
					return PackageItemType.Component;
				case "atlas":
					return PackageItemType.Atlas;
				case "sound":
					return PackageItemType.Sound;
				case "font":
					return PackageItemType.Font;
				case "misc":
					return PackageItemType.Misc;
				default:
					return PackageItemType.Misc;
			}
		}

		public static AlignType ParseAlign( string value )
		{
			switch ( value )
			{
				case "left":
					return AlignType.Left;
				case "center":
					return AlignType.Center;
				case "right":
					return AlignType.Right;
				default:
					return AlignType.Left;
			}
		}

		public static VertAlignType ParseVerticalAlign( string value )
		{
			switch ( value )
			{
				case "top":
					return VertAlignType.Top;
				case "middle":
					return VertAlignType.Middle;
				case "bottom":
					return VertAlignType.Bottom;
				default:
					return VertAlignType.Top;
			}
		}

		public static ScrollType ParseScrollType( string value )
		{
			switch ( value )
			{
				case "horizontal":
					return ScrollType.Horizontal;
				case "vertical":
					return ScrollType.Vertical;
				case "both":
					return ScrollType.Both;
				default:
					return ScrollType.Horizontal;
			}
		}

		public static ScrollBarDisplayType ParseScrollBarDisplayType( string value )
		{
			switch ( value )
			{
				case "default":
					return ScrollBarDisplayType.Default;
				case "visible":
					return ScrollBarDisplayType.Visible;
				case "auto":
					return ScrollBarDisplayType.Auto;
				case "hidden":
					return ScrollBarDisplayType.Hidden;
				default:
					return ScrollBarDisplayType.Default;
			}
		}

		public static OverflowType ParseOverflowType( string value )
		{
			switch ( value )
			{
				case "visible":
					return OverflowType.Visible;
				case "hidden":
					return OverflowType.Hidden;
				case "scroll":
					return OverflowType.Scroll;
				default:
					return OverflowType.Visible;
			}
		}

		public static FillType ParseFillType( string value )
		{
			switch ( value )
			{
				case "none":
					return FillType.None;
				case "scale":
					return FillType.Scale;
				case "scaleFree":
					return FillType.ScaleFree;
				default:
					return FillType.None;
			}
		}

		public static AutoSizeType ParseAutoSizeType( string value )
		{
			switch ( value )
			{
				case "none":
					return AutoSizeType.None;
				case "both":
					return AutoSizeType.Both;
				case "height":
					return AutoSizeType.Height;
				default:
					return AutoSizeType.None;
			}
		}

		public static LayoutType ParseListLayoutType( string value )
		{
			switch ( value )
			{
				case "column":
					return LayoutType.SingleColumn;
				case "row":
					return LayoutType.SingleRow;
				case "flow_hz":
					return LayoutType.FlowHorizontal;
				case "flow_vt":
					return LayoutType.FlowVertical;
				default:
					return LayoutType.SingleColumn;
			}
		}

		public static ListSelectionMode ParseListSelectionMode( string value )
		{
			switch ( value )
			{
				case "single":
					return ListSelectionMode.Single;
				case "multiple":
					return ListSelectionMode.Multiple;
				case "multipleSingleClick":
					return ListSelectionMode.Multiple_SingleClick;
				case "none":
					return ListSelectionMode.None;
				default:
					return ListSelectionMode.Single;
			}
		}

		public static ProgressTitleType ParseProgressTitleType( string value )
		{
			switch ( value )
			{
				case "percent":
					return ProgressTitleType.Percent;
				case "valueAndmax":
					return ProgressTitleType.ValueAndMax;
				case "value":
					return ProgressTitleType.Value;
				case "max":
					return ProgressTitleType.Max;
				default:
					return ProgressTitleType.Percent;
			}
		}

		public static ButtonMode ParseButtonMode( string value )
		{
			switch ( value )
			{
				case "Common":
					return ButtonMode.Common;
				case "Check":
					return ButtonMode.Check;
				case "Radio":
					return ButtonMode.Radio;
				default:
					return ButtonMode.Common;
			}
		}

		public static FlipType ParseFlipType( string value )
		{
			switch ( value )
			{
				case "both":
					return FlipType.Both;
				case "hz":
					return FlipType.Horizontal;
				case "vt":
					return FlipType.Vertical;
				default:
					return FlipType.None;
			}
		}

		public static ImageEx.FillMethod ParseFillMethod( string value )
		{
			switch ( value )
			{
				case "hz":
					return ImageEx.FillMethod.Horizontal;
				case "vt":
					return ImageEx.FillMethod.Vertical;
				case "radial90":
					return ImageEx.FillMethod.Radial90;
				case "radial180":
					return ImageEx.FillMethod.Radial180;
				case "radial360":
					return ImageEx.FillMethod.Radial360;
				default:
					return ImageEx.FillMethod.Horizontal;
			}
		}

		public static BlendMode ParseBlendMode( string value )
		{
			switch ( value )
			{
				case "add":
					return BlendMode.Add;

				case "multiply":
					return BlendMode.Multiply;

				case "none":
					return BlendMode.None;

				case "screen":
					return BlendMode.Screen;

				case "erase":
					return BlendMode.Erase;

				default:
					return BlendMode.Normal;
			}
		}

		static readonly Dictionary<string, Ease> EASE_TYPE_MAP = new Dictionary<string, Ease>
			{
				{ "Linear", Ease.Linear },
				{ "Elastic.In", Ease.InElastic },
				{ "Elastic.Out", Ease.InOutElastic },
				{ "Elastic.InOut", Ease.InOutElastic },
				{ "Quad.In", Ease.InQuad },
				{ "Quad.Out", Ease.OutQuad },
				{ "Quad.InOut", Ease.InOutQuad },
				{ "Cube.In", Ease.InCubic },
				{ "Cube.Out", Ease.OutCubic },
				{ "Cube.InOut", Ease.InOutCubic },
				{ "Quart.In", Ease.InQuart },
				{ "Quart.Out", Ease.OutQuart },
				{ "Quart.InOut", Ease.InOutQuart },
				{ "Quint.In", Ease.InQuint },
				{ "Quint.Out", Ease.OutQuint },
				{ "Quint.InOut", Ease.InOutQuint },
				{ "Sine.In", Ease.InSine },
				{ "Sine.Out", Ease.OutSine },
				{ "Sine.InOut", Ease.InOutSine },
				{ "Bounce.In", Ease.InBounce },
				{ "Bounce.Out", Ease.OutBounce },
				{ "Bounce.InOut", Ease.InOutBounce },
				{ "Circ.In", Ease.InCirc },
				{ "Circ.Out", Ease.OutCirc },
				{ "Circ.InOut", Ease.InOutCirc },
				{ "Expo.In", Ease.InExpo },
				{ "Expo.Out", Ease.OutExpo },
				{ "Expo.InOut", Ease.InOutExpo },
				{ "Back.In", Ease.InBack },
				{ "Back.Out", Ease.OutBack },
				{ "Back.InOut", Ease.InOutBack }
			};
		public static Ease ParseEaseType( string value )
		{
			Ease type;
			if ( !EASE_TYPE_MAP.TryGetValue( value, out type ) )
				type = Ease.OutExpo;

			return type;
		}

		public static TransitionActionType ParseTransitionActionType( string value )
		{
			switch ( value )
			{
				case "XY":
					return TransitionActionType.XY;
				case "Size":
					return TransitionActionType.Size;
				case "Scale":
					return TransitionActionType.Scale;
				case "Pivot":
					return TransitionActionType.Pivot;
				case "Alpha":
					return TransitionActionType.Alpha;
				case "Rotation":
					return TransitionActionType.Rotation;
				case "Color":
					return TransitionActionType.Color;
				case "Animation":
					return TransitionActionType.Animation;
				case "Visible":
					return TransitionActionType.Visible;
				case "Controller":
					return TransitionActionType.Controller;
				case "Sound":
					return TransitionActionType.Sound;
				case "Transition":
					return TransitionActionType.Transition;
				case "Shake":
					return TransitionActionType.Shake;
				default:
					return TransitionActionType.Unknown;
			}
		}

		static readonly char[] SEP0 = { ',' };
		public static void ParseMargin( string str, ref Margin margin )
		{
			if ( str == null )
			{
				margin.left = margin.right = margin.top = margin.bottom = 0;
				return;
			}
			string[] arr = str.Split( SEP0 );
			if ( arr.Length <= 1 )
			{
				int k = int.Parse( arr[0] );
				margin.top = k;
				margin.bottom = k;
				margin.left = k;
				margin.right = k;
			}
			else
			{
				margin.top = int.Parse( arr[0] );
				margin.bottom = int.Parse( arr[1] );
				margin.left = int.Parse( arr[2] );
				margin.right = int.Parse( arr[3] );
			}
		}
	}
}
