using System.Collections.Generic;

namespace FairyUGUI.UI
{
	public class GearIcon : GearBase
	{
		private Dictionary<string, string> _storage;

		private string _default;

		public GearIcon( GObject owner ) : base( owner )
		{
		}

		protected override void Init()
		{
			this._default = this._owner.icon;
			this._storage = new Dictionary<string, string>();
		}

		protected override void AddStatus( string pageId, string value )
		{
			if ( pageId == null )
			{
				this._default = value;
				return;
			}
			this._storage[pageId] = value;
		}

		public override void Apply()
		{
			this._owner.SetGearState( GObject.GearState.Icon, true );
			string cv;
			if ( !this._storage.TryGetValue( this._controller.selectedPageId, out cv ) )
			{
				cv = this._default;
			}
			this._owner.icon = cv;
			this._owner.SetGearState( GObject.GearState.Icon, false );
		}

		public override void UpdateState()
		{
			this._storage[this._controller.selectedPageId] = this._owner.icon;
		}
	}
}