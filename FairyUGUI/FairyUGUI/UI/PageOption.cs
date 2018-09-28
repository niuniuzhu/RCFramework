namespace FairyUGUI.UI
{
	public sealed class PageOption
	{
		private Controller _controller;
		private string _id;

		public Controller controller
		{
			set => this._controller = value;
		}

		public int index
		{
			set => this._id = this._controller.GetPageId( value );
			get
			{
				if ( this._id != null )
					return this._controller.GetPageIndexById( this._id );
				return -1;
			}
		}

		public string name
		{
			set => this._id = this._controller.GetPageIdByName( value );
			get => this._id != null ? this._controller.GetPageNameById( this._id ) : null;
		}

		public void Clear()
		{
			this._id = null;
		}

		public string id
		{
			set => this._id = value;
			get => this._id;
		}
	}
}
