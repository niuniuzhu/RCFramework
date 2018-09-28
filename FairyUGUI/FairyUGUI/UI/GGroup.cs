using FairyUGUI.Event;
using UnityEngine;

namespace FairyUGUI.UI
{
	public class GGroup : GObject
	{
		private Vector2 _deltaPos;
		public override Vector2 position
		{
			set
			{
				if ( this._underConstruct )
					base.position = value;
				else
				{
					Vector2 oldPos = base.position;
					base.position = value;
					this.MoveChildren( value - oldPos );
				}
			}
		}

		protected internal override void HandleVisibleChanged()
		{
			base.HandleVisibleChanged();
			this.UpdateVisible();
		}

		protected internal override void HandleColorChanged()
		{
			base.HandleColorChanged();
			this.UpdateAlpha();
		}

		private void MoveChildren( Vector2 deltaPos )
		{
			if ( this.parent == null )
			{
				this._deltaPos += deltaPos;
				this.onAddedToStage.Add( this.UpdatePositionAfterAddedToStage );
				return;
			}
			int cnt = this.parent.numChildren;
			for ( int i = 0; i < cnt; i++ )
			{
				GObject child = this.parent.GetChildAt( i );
				if ( child.group == this )
					child.position += deltaPos;
			}
		}

		private void UpdateAlpha()
		{
			if ( this.parent == null )
			{
				this.onAddedToStage.Add( this.UpdateAlphaAfterAddedToStage );
				return;
			}
			int cnt = this.parent.numChildren;
			for ( int i = 0; i < cnt; i++ )
			{
				GObject child = this.parent.GetChildAt( i );
				if ( child.group == this )
					child.HandleColorChanged();
			}
		}

		private void UpdateVisible()
		{
			if ( this.parent == null )
			{
				this.onAddedToStage.Add( this.UpdateVisibleAfterAddedToStage );
				return;
			}
			int cnt = this.parent.numChildren;
			for ( int i = 0; i < cnt; i++ )
			{
				GObject child = this.parent.GetChildAt( i );
				if ( child.group == this )
					child.HandleVisibleChanged();
			}
		}

		private void UpdatePositionAfterAddedToStage( EventContext context )
		{
			this.onAddedToStage.Remove( this.UpdatePositionAfterAddedToStage );
			this.MoveChildren( this._deltaPos );
			this._deltaPos = Vector2.zero;
		}

		private void UpdateAlphaAfterAddedToStage( EventContext context )
		{
			this.onAddedToStage.Remove( this.UpdateAlphaAfterAddedToStage );
			this.UpdateAlpha();
		}

		private void UpdateVisibleAfterAddedToStage( EventContext context )
		{
			this.onAddedToStage.Remove( this.UpdateVisibleAfterAddedToStage );
			this.UpdateVisible();
		}

		protected override void InternalDispose()
		{
		}
	}
}
