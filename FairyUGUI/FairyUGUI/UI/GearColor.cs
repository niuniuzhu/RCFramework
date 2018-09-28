using System;
using System.Collections.Generic;
using DG.Tweening;
using FairyUGUI.Utils;
using UnityEngine;

namespace FairyUGUI.UI
{
	class GearColorValue
	{
		public Color color;
		public Color strokeColor;

		public GearColorValue()
		{
			this.strokeColor = Color.clear;
		}

		public GearColorValue( Color color, Color strokeColor )
		{
			this.color = color;
			this.strokeColor = strokeColor;
		}
	}

	public class GearColor : GearBase
	{
		private Dictionary<string, GearColorValue> _storage;
		private GearColorValue _default;
		private GearColorValue _tweenTarget;
		private Tweener _tweener;

		public GearColor( GObject owner )
			: base( owner )
		{
		}

		protected override void Init()
		{
			this._default = new GearColorValue();
			this._default.color = this._owner.color;
			ITextColorGear textColorGear = this._owner as ITextColorGear;
			if ( textColorGear != null )
				this._default.strokeColor = textColorGear.strokeColor;
			this._storage = new Dictionary<string, GearColorValue>();
		}

		protected override void AddStatus( string pageId, string value )
		{
			if ( value == "-" )
			{
				return;
			}
			int pos = value.IndexOf( ",", StringComparison.Ordinal );
			Color col;
			Color col2;
			if ( pos == -1 )
			{
				col = ToolSet.ConvertFromHtmlColor( value );
				col2 = Color.clear;
			}
			else
			{
				col = ToolSet.ConvertFromHtmlColor( value.Substring( 0, pos ) );
				col2 = ToolSet.ConvertFromHtmlColor( value.Substring( pos + 1 ) );
			}
			if ( pageId == null )
			{
				this._default.color = col;
				this._default.strokeColor = col2;
				return;
			}
			this._storage[pageId] = new GearColorValue( col, col2 );
		}

		public override void Apply()
		{
			GearColorValue cv;
			if ( !this._storage.TryGetValue( this._controller.selectedPageId, out cv ) )
				cv = this._default;

			ITextColorGear textColorGear = this._owner as ITextColorGear;
			if ( this.tween &&
				 UIPackage.constructing == 0 &&
				 !disableAllTweenEffect )
			{
				if ( this._tweener != null )
				{
					if ( this._tweenTarget.color == cv.color )
						return;
					this._tweener.Kill( true );
					this._tweener = null;
				}
				if ( this._owner.color != cv.color )
				{
					this._tweenTarget = cv;
					this._tweener = DOTween.To( () => this._owner.color, delegate ( Color v )
					{
						this._owner.SetGearState( GObject.GearState.Color, true );
						this._owner.color = v;
						this._owner.SetGearState( GObject.GearState.Color, false );
					}, cv.color, this.tweenTime ).SetEase( this.easeType ).SetUpdate( true ).OnComplete( delegate
					{
						this._tweener = null;
					} );
					if ( this.delay > 0f )
						this._tweener.SetDelay( this.delay );
					return;
				}
			}
			this._owner.SetGearState( GObject.GearState.Color, true );
			if ( textColorGear != null )
			{
				textColorGear.textColor = cv.color;
				textColorGear.strokeColor = cv.strokeColor;
			}
			else
				this._owner.color = cv.color;
			this._owner.SetGearState( GObject.GearState.Color, false );
		}

		public override void UpdateState()
		{
			if ( this._owner.TestGearState( GObject.GearState.Color ) )
				return;

			GearColorValue cv;
			if ( !this._storage.TryGetValue( this._controller.selectedPageId, out cv ) )
				this._storage[this._controller.selectedPageId] = new GearColorValue();
			else
			{
				cv.color = this._owner.color;
				ITextColorGear textColorGear = this._owner as ITextColorGear;
				if ( textColorGear != null )
					cv.strokeColor = textColorGear.strokeColor;
			}
		}
	}
}
