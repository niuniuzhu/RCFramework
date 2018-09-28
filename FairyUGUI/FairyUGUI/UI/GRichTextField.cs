namespace FairyUGUI.UI
{
	public class GRichTextField : GTextField
	{
		public override bool supportRichText
		{
			get => base.supportRichText;
			set => base.supportRichText = true;
		}
	}
}