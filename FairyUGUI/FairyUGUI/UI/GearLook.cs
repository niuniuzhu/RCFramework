using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace FairyUGUI.UI
{
	class GearLookValue
	{
		public float alpha;
		public float rotation;
		public bool grayed;

		public GearLookValue( float alpha, float rotation, bool grayed )
		{
			this.alpha = alpha;
			this.rotation = rotation;
			this.grayed = grayed;
		}
	}

	/// <summary>
	/// Gear is a connection between object and controller.
	/// </summary>
	public class GearLook : GearBase
	{
		private Dictionary<string, GearLookValue> _storage;
		private GearLookValue _default;
		private GearLookValue _tweenTarget;
		private Tweener _tweener;

		public GearLook( GObject owner )
			: base( owner )
		{
		}

		protected override void Init()
		{
			this._default = new GearLookValue( this._owner.alpha, this._owner.rotation, this._owner.grayed );
			this._storage = new Dictionary<string, GearLookValue>();
		}

		protected override void AddStatus( string pageId, string value )
		{
			string[] arr = value.Split( ',' );
			if ( pageId == null )
			{
				this._default.alpha = float.Parse( arr[0] );
				this._default.rotation = float.Parse( arr[1] );
				this._default.grayed = int.Parse( arr[2] ) == 1;
			}
			else
				this._storage[pageId] = new GearLookValue( float.Parse( arr[0] ), float.Parse( arr[1] ), int.Parse( arr[2] ) == 1 );
		}

		public override void Apply()
		{
			GearLookValue gv;
			if ( !this._storage.TryGetValue( this._controller.selectedPageId, out gv ) )
				gv = this._default;

			if ( this.tween && UIPackage.constructing == 0 && !disableAllTweenEffect )
			{
				this._owner.SetGearState( GObject.GearState.Look, true );
				this._owner.grayed = gv.grayed;
				this._owner.SetGearState( GObject.GearState.Look, false );

				if ( this._tweener != null )
				{
					if ( this._tweenTarget.alpha != gv.alpha || this._tweenTarget.rotation != gv.rotation )
					{
						this._tweener.Kill( true );
						this._tweener = null;
					}
					else
						return;
				}

				bool a = !Mathf.Approximately( gv.alpha, this._owner.alpha );
				bool b = !Mathf.Approximately( gv.rotation, this._owner.rotation );
				if ( a || b )
				{
					this._tweenTarget = gv;
					this._tweener = DOTween.To( () => new Vector2( this._owner.alpha, this._owner.rotation ), val =>
					{
						this._owner.SetGearState( GObject.GearState.Look, true );
						if ( a )
							this._owner.alpha = val.x;
						if ( b )
							this._owner.rotation = val.y;
						this._owner.SetGearState( GObject.GearState.Look, false );
					}, new Vector2( gv.alpha, gv.rotation ), this.tweenTime )
					.SetEase( this.easeType )
					.SetUpdate( true )
					.OnComplete( () =>
					{
						this._tweener = null;
					} );

					if ( this.delay > 0 )
						this._tweener.SetDelay( this.delay );
				}
			}
			else
			{
				this._owner.SetGearState( GObject.GearState.Look, true );
				this._owner.alpha = gv.alpha;
				this._owner.rotation = gv.rotation;
				this._owner.grayed = gv.grayed;
				this._owner.SetGearState( GObject.GearState.Look, false );
			}
		}

		public override void UpdateState()
		{
			if ( this._owner.TestGearState( GObject.GearState.Look ) )
				return;

			GearLookValue gv;
			if ( !this._storage.TryGetValue( this._controller.selectedPageId, out gv ) )
				this._storage[this._controller.selectedPageId] = new GearLookValue( this._owner.alpha, this._owner.rotation, this._owner.grayed );
			else
			{
				gv.alpha = this._owner.alpha;
				gv.rotation = this._owner.rotation;
				gv.grayed = this._owner.grayed;
			}
		}
	}
}
