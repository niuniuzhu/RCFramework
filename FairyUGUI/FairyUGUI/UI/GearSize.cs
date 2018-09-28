using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace FairyUGUI.UI
{
	class GearSizeValue
	{
		public Vector2 size;
		public Vector2 scale;

		public GearSizeValue( float width, float height, float scaleX, float scaleY )
		{
			this.size = new Vector2( width, height );
			this.scale = new Vector2( scaleX, scaleY );
		}

		public GearSizeValue( Vector2 size, Vector2 scale )
		{
			this.size = size;
			this.scale = scale;
		}
	}

	/// <summary>
	/// Gear is a connection between object and controller.
	/// </summary>
	public class GearSize : GearBase
	{
		private Dictionary<string, GearSizeValue> _storage;
		private GearSizeValue _default;
		private GearSizeValue _tweenTarget;
		private Tweener _tweener;

		public GearSize( GObject owner )
			: base( owner )
		{

		}

		protected override void Init()
		{
			this._default = new GearSizeValue( this._owner.size, this._owner.scale );
			this._storage = new Dictionary<string, GearSizeValue>();
		}

		protected override void AddStatus( string pageId, string value )
		{
			string[] arr = value.Split( ',' );
			GearSizeValue gv;
			if ( pageId == null )
				gv = this._default;
			else
			{
				gv = new GearSizeValue( 0, 0, 1, 1 );
				this._storage[pageId] = gv;
			}
			gv.size.x = int.Parse( arr[0] );
			gv.size.y = int.Parse( arr[1] );
			if ( arr.Length > 2 )
			{
				gv.scale.x = float.Parse( arr[2] );
				gv.scale.y = float.Parse( arr[3] );
			}
		}

		public override void Apply()
		{
			GearSizeValue gv;
			if ( !this._storage.TryGetValue( this._controller.selectedPageId, out gv ) )
				gv = this._default;

			if ( this.tween && UIPackage.constructing == 0 && !disableAllTweenEffect )
			{
				if ( this._tweener != null )
				{
					if ( this._tweenTarget.size != gv.size || this._tweenTarget.scale != gv.scale )
					{
						this._tweener.Kill( true );
						this._tweener = null;
					}
					else
						return;
				}

				bool a = gv.size != this._owner.size;
				bool b = gv.scale != this._owner.scale;
				if ( a || b )
				{
					this._tweenTarget = gv;
					this._tweener = DOTween.To( () => new Vector4( this._owner.size.x, this._owner.size.y, this._owner.scale.x, this._owner.scale.y ), v =>
					{
						this._owner.SetGearState( GObject.GearState.Size, true );
						if ( a )
							this._owner.size = new Vector2( v.x, v.y );
						if ( b )
							this._owner.scale = new Vector2( v.z, v.w );
						this._owner.SetGearState( GObject.GearState.Size, false );
					}, new Vector4( gv.size.x, gv.size.y, gv.scale.x, gv.scale.y ), this.tweenTime )
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
				this._owner.SetGearState( GObject.GearState.Size, true );
				this._owner.size = gv.size;
				this._owner.scale = gv.scale;
				this._owner.SetGearState( GObject.GearState.Size, false );
			}
		}

		public override void UpdateState()
		{
			if ( this._owner.TestGearState( GObject.GearState.Size ) )
				return;

			GearSizeValue gv;
			if ( !this._storage.TryGetValue( this._controller.selectedPageId, out gv ) )
				this._storage[this._controller.selectedPageId] = new GearSizeValue( this._owner.size, this._owner.scale );
			else
			{
				gv.size = this._owner.size;
				gv.scale = this._owner.scale;
			}
		}

		internal void UpdateFromRelations( Vector2 delta )
		{
			if ( this._storage != null )
			{
				foreach ( GearSizeValue gv in this._storage.Values )
					gv.size += delta;

				this._default.size += delta;

				this.UpdateState();
			}
		}
	}
}
