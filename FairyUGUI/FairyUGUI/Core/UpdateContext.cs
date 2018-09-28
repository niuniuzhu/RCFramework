namespace FairyUGUI.Core
{
	public class UpdateContext
	{
		internal int sortingOrder;

		public void Begin()
		{
			this.sortingOrder = 0;
		}

		public void End()
		{
			
		}
	}
}