using System.Collections.Generic;

namespace FairyUGUI.UI
{
	class GearAnimationValue
	{
		public bool playing;
		public int frame;

		public GearAnimationValue( bool playing, int frame )
		{
			this.playing = playing;
			this.frame = frame;
		}
	}

	/// <summary>
	/// Gear is a connection between object and controller.
	/// </summary>
	public class GearAnimation : GearBase
	{
		Dictionary<string, GearAnimationValue> _storage;
		GearAnimationValue _default;

		public GearAnimation( GObject owner )
			: base( owner )
		{
		}

		protected override void Init()
		{
			this._default = new GearAnimationValue( ( ( IAnimationGear )this._owner ).playing, ( ( IAnimationGear )this._owner ).frame );
			this._storage = new Dictionary<string, GearAnimationValue>();
		}

		protected override void AddStatus( string pageId, string value )
		{
			string[] arr = value.Split( ',' );
			int frame = int.Parse( arr[0] );
			bool playing = arr[1] == "p";
			if ( pageId == null )
			{
				this._default.playing = playing;
				this._default.frame = frame;
			}
			else
				this._storage[pageId] = new GearAnimationValue( playing, frame );
		}

		public override void Apply()
		{
			GearAnimationValue gv;
			if ( !this._storage.TryGetValue( this._controller.selectedPageId, out gv ) )
				gv = this._default;

			this._owner.SetGearState( GObject.GearState.Animation, true );
			IAnimationGear mc = ( IAnimationGear )this._owner;
			mc.frame = gv.frame;
			mc.playing = gv.playing;
			this._owner.SetGearState( GObject.GearState.Animation, false );
		}

		public override void UpdateState()
		{
			if ( this._owner.TestGearState( GObject.GearState.Animation ) )
				return;

			IAnimationGear mc = ( IAnimationGear )this._owner;
			GearAnimationValue gv;
			if ( !this._storage.TryGetValue( this._controller.selectedPageId, out gv ) )
				this._storage[this._controller.selectedPageId] = new GearAnimationValue( mc.playing, mc.frame );
			else
			{
				gv.playing = mc.playing;
				gv.frame = mc.frame;
			}
		}
	}
}
