using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace FairyUGUI.UI
{
	class GearXYValue
	{
		public Vector2 position;

		public GearXYValue( float x, float y )
		{
			this.position = new Vector2( x, y );
		}

		public GearXYValue( Vector2 position )
		{
			this.position = position;
		}
	}

	/// <summary>
	/// Gear is a connection between object and controller.
	/// </summary>
	public class GearXY : GearBase
	{
		private Dictionary<string, GearXYValue> _storage;
		private GearXYValue _default;
		private GearXYValue _tweenTarget;
		private Tweener _tweener;

		public GearXY( GObject owner )
			: base( owner )
		{
		}

		protected override void Init()
		{
			this._default = new GearXYValue( this._owner.position );
			this._storage = new Dictionary<string, GearXYValue>();
		}

		protected override void AddStatus( string pageId, string value )
		{
			string[] arr = value.Split( ',' );
			if ( pageId == null )
			{
				this._default.position.x = int.Parse( arr[0] );
				this._default.position.y = int.Parse( arr[1] );
			}
			else
				this._storage[pageId] = new GearXYValue( int.Parse( arr[0] ), int.Parse( arr[1] ) );
		}

		public override void Apply()
		{
			GearXYValue gv;
			if ( !this._storage.TryGetValue( this._controller.selectedPageId, out gv ) )
				gv = this._default;

			if ( this.tween && UIPackage.constructing == 0 && !disableAllTweenEffect )
			{
				if ( this._tweener != null )
				{
					if ( this._tweenTarget.position != gv.position )
					{
						this._tweener.Kill( true );
						this._tweener = null;
					}
					else
						return;
				}

				if ( this._owner.position != gv.position )
				{
					this._tweenTarget = gv;
					this._tweener = DOTween.To( () => this._owner.position, v =>
					{
						this._owner.SetGearState( GObject.GearState.Position, true );
						this._owner.position = v;
						this._owner.SetGearState( GObject.GearState.Position, false );
					}, gv.position, this.tweenTime )
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
				this._owner.SetGearState( GObject.GearState.Position, true );
				this._owner.position = gv.position;
				this._owner.SetGearState( GObject.GearState.Position, false );
			}
		}

		public override void UpdateState()
		{
			if ( this._owner.TestGearState( GObject.GearState.Position ) )
				return;

			GearXYValue gv;
			if ( !this._storage.TryGetValue( this._controller.selectedPageId, out gv ) )
				this._storage[this._controller.selectedPageId] = new GearXYValue( this._owner.position );
			else
				gv.position = this._owner.position;
		}

		internal void UpdateFromRelations( Vector2 delta )
		{
			if ( this._storage == null )
				return;

			foreach ( GearXYValue gv in this._storage.Values )
				gv.position += delta;

			this._default.position += delta;

			this.UpdateState();
		}
	}
}
