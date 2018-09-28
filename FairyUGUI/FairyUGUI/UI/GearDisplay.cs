using System.Collections.Generic;

namespace FairyUGUI.UI
{
	/// <summary>
	/// Gear is a connection between object and controller.
	/// </summary>
	public class GearDisplay : GearBase
	{
		/// <summary>
		/// Pages involed in this gear.
		/// </summary>
		public List<string> pages { get; private set; }

		public GearDisplay( GObject owner )
			: base( owner )
		{
		}

		protected override void AddStatus( string pageId, string value )
		{
		}

		protected override void Init()
		{
			if ( this.pages != null )
				this.pages.Clear();
			else
				this.pages = new List<string>();
		}

		public override void Apply()
		{
			if ( this._controller == null || this.pages == null || this.pages.Count == 0 ||
				this.pages.Contains( this._controller.selectedPageId ) )
				this._owner.gearVisible = true;
			else
				this._owner.gearVisible = false;
		}

		public override void UpdateState()
		{
		}
	}
}
