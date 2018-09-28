using System.Collections.Generic;
using UnityEngine;

namespace FairyUGUI.UI
{
	class RelationDef
	{
		public bool affectBySelfSizeChanged;
		public bool percent;
		public RelationType type;

		public void CopyFrom( RelationDef source )
		{
			this.affectBySelfSizeChanged = source.affectBySelfSizeChanged;
			this.percent = source.percent;
			this.type = source.type;
		}
	}

	class RelationItem
	{
		private readonly GObject _owner;
		private GObject _target;
		private readonly List<RelationDef> _defs;
		private Vector4 _targetData;

		public GObject target
		{
			get => this._target;
			set
			{
				if ( this._target != value )
				{
					if ( this._target != null )
						this.ReleaseRefTarget( this._target );
					this._target = value;
					if ( this._target != null )
						this.AddRefTarget( this._target );
				}
			}
		}

		public RelationItem( GObject owner )
		{
			this._owner = owner;
			this._defs = new List<RelationDef>();
		}

		public void Add( RelationType relationType, bool usePercent )
		{
			if ( relationType == RelationType.Size )
			{
				this.Add( RelationType.Width, usePercent );
				this.Add( RelationType.Height, usePercent );
				return;
			}

			int count = this._defs.Count;
			for ( int i = 0; i < count; i++ )
			{
				RelationDef def = this._defs[i];
				if ( def.type == relationType )
					return;
			}

			RelationDef info = new RelationDef();
			info.affectBySelfSizeChanged = relationType >= RelationType.Center_Center && relationType <= RelationType.Right_Right
				|| relationType >= RelationType.Middle_Middle && relationType <= RelationType.Bottom_Bottom;
			info.percent = usePercent;
			info.type = relationType;
			this._defs.Add( info );
		}


		internal void QuickAdd( RelationType relationType, bool usePercent )
		{
			if ( relationType == RelationType.Size )
			{
				this.QuickAdd( RelationType.Width, usePercent );
				this.QuickAdd( RelationType.Height, usePercent );
				return;
			}
			RelationDef info = new RelationDef();
			info.affectBySelfSizeChanged = relationType >= RelationType.Center_Center && relationType <= RelationType.Right_Right
				|| relationType >= RelationType.Middle_Middle && relationType <= RelationType.Bottom_Bottom;
			info.percent = usePercent;
			info.type = relationType;
			this._defs.Add( info );
		}

		public void Remove( RelationType relationType )
		{
			if ( relationType == RelationType.Size )
			{
				this.Remove( RelationType.Width );
				this.Remove( RelationType.Height );
				return;
			}

			int dc = this._defs.Count;
			for ( int k = 0; k < dc; k++ )
			{
				if ( this._defs[k].type == relationType )
				{
					this._defs.RemoveAt( k );
					break;
				}
			}
		}

		public void CopyFrom( RelationItem source )
		{
			this.target = source.target;

			this._defs.Clear();
			foreach ( RelationDef info in source._defs )
			{
				RelationDef info2 = new RelationDef();
				info2.CopyFrom( info );
				this._defs.Add( info2 );
			}
		}

		public void Dispose()
		{
			if ( this._target != null )
			{
				this.ReleaseRefTarget( this._target );
				this._target = null;
			}
		}

		public bool isEmpty => this._defs.Count == 0;

		public void ApplyOnSelfSizeChanged( Vector2 deltaSize )
		{
			int cnt = this._defs.Count;
			if ( cnt == 0 )
				return;

			Vector2 pos = this._owner.position;
			Vector2 oldPos = pos;

			for ( int i = 0; i < cnt; i++ )
			{
				switch ( this._defs[i].type )
				{
					case RelationType.Center_Center:
					case RelationType.Right_Center:
						pos.x -= deltaSize.x * 0.5f;
						break;

					case RelationType.Right_Left:
					case RelationType.Right_Right:
						pos.x -= deltaSize.x;
						break;

					case RelationType.Middle_Middle:
					case RelationType.Bottom_Middle:
						pos.y -= deltaSize.y * 0.5f;
						break;
					case RelationType.Bottom_Top:
					case RelationType.Bottom_Bottom:
						pos.y -= deltaSize.y;
						break;
				}
			}

			if ( oldPos != pos )
			{
				oldPos = pos - oldPos;

				if ( this._owner.gearXY.controller != null )
					this._owner.gearXY.UpdateFromRelations( oldPos );

				if ( this._owner.parent != null )
				{
					int count = this._owner.parent.TransitionCount();
					for ( int i = 0; i < count; i++ )
						this._owner.parent.GetTransitionAt( i ).UpdateFromRelations( this._owner.id, oldPos );
				}
				this._owner.position = pos;
			}
		}

		private void ApplyOnPositionChanged( RelationDef info, float dx, float dy )
		{
			Vector3 pos = this._owner.position;
			Vector2 size = this._owner.size;
			float tmp;
			switch ( info.type )
			{
				case RelationType.Left_Left:
				case RelationType.Left_Center:
				case RelationType.Left_Right:
				case RelationType.Center_Center:
				case RelationType.Right_Left:
				case RelationType.Right_Center:
				case RelationType.Right_Right:
					pos.x += dx;
					break;

				case RelationType.Top_Top:
				case RelationType.Top_Middle:
				case RelationType.Top_Bottom:
				case RelationType.Middle_Middle:
				case RelationType.Bottom_Top:
				case RelationType.Bottom_Middle:
				case RelationType.Bottom_Bottom:
					pos.y += dy;
					break;

				case RelationType.Width:
				case RelationType.Height:
					break;

				case RelationType.LeftExt_Left:
				case RelationType.LeftExt_Right:
					tmp = this._owner.position.x;
					pos.x += dx;
					size.x = this._owner.size.x - ( pos.x - tmp );
					break;

				case RelationType.RightExt_Left:
				case RelationType.RightExt_Right:
					size.x = this._owner.size.x + dx;
					break;

				case RelationType.TopExt_Top:
				case RelationType.TopExt_Bottom:
					tmp = pos.y;
					pos.y += dy;
					size.y = this._owner.size.y - ( pos.y - tmp );
					break;

				case RelationType.BottomExt_Top:
				case RelationType.BottomExt_Bottom:
					size.y = this._owner.size.y + dy;
					break;
			}
			this._owner.position = pos;
			this._owner.size = size;
		}

		void ApplyOnSizeChanged( RelationDef info )
		{
			float targetX, targetY;
			if ( this._target != this._owner.parent )
			{
				targetX = this._target.position.x;
				targetY = this._target.position.y;
			}
			else
			{
				targetX = 0;
				targetY = 0;
			}
			float v, tmp;
			Vector2 pos = this._owner.position;
			Vector2 size = this._owner.size;

			switch ( info.type )
			{
				case RelationType.Left_Left:
					if ( info.percent && this._target == this._owner.parent )
					{
						v = pos.x - targetX;
						if ( info.percent )
							v = v / this._targetData.z * this._target.size.x;
						pos.x = targetX + v;
						return;
					}
					break;

				case RelationType.Left_Center:
					v = pos.x - ( targetX + this._targetData.z * 0.5f );
					if ( info.percent )
						v = v / this._targetData.z * this._target.size.x;
					pos.x = targetX + this._target.size.x * 0.5f + v;
					break;

				case RelationType.Left_Right:
					v = pos.x - ( targetX + this._targetData.z );
					if ( info.percent )
						v = v / this._targetData.z * this._target.size.x;
					pos.x = targetX + this._target.size.x + v;
					break;

				case RelationType.Center_Center:
					v = pos.x + this._owner.size.x * 0.5f - ( targetX + this._targetData.z * 0.5f );
					if ( info.percent )
						v = v / this._targetData.z * this._target.size.x;
					pos.x = targetX + this._target.size.x * 0.5f + v - this._owner.size.x * 0.5f;
					break;

				case RelationType.Right_Left:
					v = pos.x + this._owner.size.x - targetX;
					if ( info.percent )
						v = v / this._targetData.z * this._target.size.x;
					pos.x = targetX + v - this._owner.size.x;
					break;

				case RelationType.Right_Center:
					v = pos.x + this._owner.size.x - ( targetX + this._targetData.z * 0.5f );
					if ( info.percent )
						v = v / this._targetData.z * this._target.size.x;
					pos.x = targetX + this._target.size.x * 0.5f + v - this._owner.size.x;
					break;

				case RelationType.Right_Right:
					v = pos.x + this._owner.size.x - ( targetX + this._targetData.z );
					if ( info.percent )
						v = v / this._targetData.z * this._target.size.x;
					pos.x = targetX + this._target.size.x + v - this._owner.size.x;
					break;

				case RelationType.Top_Top:
					if ( info.percent && this._target == this._owner.parent )
					{
						v = pos.y - targetY;
						if ( info.percent )
							v = v / this._targetData.w * this._target.size.y;
						pos.y = targetY + v;
						return;
					}
					break;

				case RelationType.Top_Middle:
					v = pos.y - ( targetY + this._targetData.w * 0.5f );
					if ( info.percent )
						v = v / this._targetData.w * this._target.size.y;
					pos.y = targetY + this._target.size.y * 0.5f + v;
					break;

				case RelationType.Top_Bottom:
					v = pos.y - ( targetY + this._targetData.w );
					if ( info.percent )
						v = v / this._targetData.w * this._target.size.y;
					pos.y = targetY + this._target.size.y + v;
					break;

				case RelationType.Middle_Middle:
					v = pos.y + this._owner.size.y * 0.5f - ( targetY + this._targetData.w * 0.5f );
					if ( info.percent )
						v = v / this._targetData.w * this._target.size.y;
					pos.y = targetY + this._target.size.y * 0.5f + v - this._owner.size.y * 0.5f;
					break;

				case RelationType.Bottom_Top:
					v = pos.y + this._owner.size.y - targetY;
					if ( info.percent )
						v = v / this._targetData.w * this._target.size.y;
					pos.y = targetY + v - this._owner.size.y;
					break;

				case RelationType.Bottom_Middle:
					v = pos.y + this._owner.size.y - ( targetY + this._targetData.w * 0.5f );
					if ( info.percent )
						v = v / this._targetData.w * this._target.size.y;
					pos.y = targetY + this._target.size.y * 0.5f + v - this._owner.size.y;
					break;

				case RelationType.Bottom_Bottom:
					v = pos.y + this._owner.size.y - ( targetY + this._targetData.w );
					if ( info.percent )
						v = v / this._targetData.w * this._target.size.y;
					pos.y = targetY + this._target.size.y + v - this._owner.size.y;
					break;

				case RelationType.Width:
					if ( this._owner._underConstruct && this._owner == this._target.parent )
						v = this._owner.sourceWidth - this._target.size.x;
					else
						v = this._owner.size.x - this._targetData.z;
					if ( info.percent )
						v = v / this._targetData.z * this._target.size.x;
					if ( this._target == this._owner.parent )
					{
						size.x = this._target.size.x + v;
						size.y = this._owner.size.y;
					}
					else
						size.x = this._target.size.x + v;
					break;

				case RelationType.Height:
					if ( this._owner._underConstruct && this._owner == this._target.parent )
						v = this._owner.sourceHeight - this._target.size.y;
					else
						v = this._owner.size.y - this._targetData.w;
					if ( info.percent )
						v = v / this._targetData.w * this._target.size.y;
					if ( this._target == this._owner.parent )
					{
						size.x = this._owner.size.x;
						size.y = this._target.size.y + v;
					}
					else
						size.y = this._target.size.y + v;
					break;

				case RelationType.LeftExt_Left:
					break;

				case RelationType.LeftExt_Right:
					v = pos.x - ( targetX + this._targetData.z );
					if ( info.percent )
						v = v / this._targetData.z * this._target.size.x;
					tmp = pos.x;
					pos.x = targetX + this._target.size.x + v;
					size.x = this._owner.size.x - ( pos.x - tmp );
					break;

				case RelationType.RightExt_Left:
					break;

				case RelationType.RightExt_Right:
					if ( this._owner._underConstruct && this._owner == this._target.parent )
						v = this._owner.sourceWidth - ( targetX + this._target.size.x );
					else
						v = this._owner.size.x - ( targetX + this._targetData.z );
					if ( this._owner != this._target.parent )
						v += pos.x;
					if ( info.percent )
						v = v / this._targetData.z * this._target.size.x;
					if ( this._owner != this._target.parent )
						size.x = targetX + this._target.size.x + v - pos.x;
					else
						size.x = targetX + this._target.size.x + v;
					break;

				case RelationType.TopExt_Top:
					break;

				case RelationType.TopExt_Bottom:
					v = pos.y - ( targetY + this._targetData.w );
					if ( info.percent )
						v = v / this._targetData.w * this._target.size.y;
					tmp = pos.y;
					pos.y = targetY + this._target.size.y + v;
					size.y = this._owner.size.y - ( pos.y - tmp );
					break;

				case RelationType.BottomExt_Top:
					break;

				case RelationType.BottomExt_Bottom:
					if ( this._owner._underConstruct && this._owner == this._target.parent )
						v = this._owner.sourceHeight - ( targetY + this._target.size.y );
					else
						v = this._owner.size.y - ( targetY + this._targetData.w );
					if ( this._owner != this._target.parent )
						v += pos.y;
					if ( info.percent )
						v = v / this._targetData.w * this._target.size.y;
					if ( this._owner != this._target.parent )
						size.y = targetY + this._target.size.y + v - pos.y;
					else
						size.y = targetY + this._target.size.y + v;
					break;
			}
			this._owner.position = pos;
			this._owner.size = size;
		}

		private void AddRefTarget( GObject target )
		{
			if ( target != this._owner.parent )
				target.onPositionChanged.Add( this.OnTargetPositionChanged );
			target.onSizeChanged.Add( this.OnTargetSizeChanged );
			this._targetData.x = this._target.position.x;
			this._targetData.y = this._target.position.y;
			this._targetData.z = this._target.size.x;
			this._targetData.w = this._target.size.y;
		}

		private void ReleaseRefTarget( GObject target )
		{
			target.onPositionChanged.Remove( this.OnTargetPositionChanged );
			target.onSizeChanged.Remove( this.OnTargetSizeChanged );
		}

		private void OnTargetPositionChanged( object sender )
		{
			//if ( this._owner.relations.handling != null
			//	|| this._owner.group != null && this._owner.group._updating
			//	|| this._owner.onPositionChanged.isDispatching )
			//{
			//	this._targetData.x = this._target.x;
			//	this._targetData.y = this._target.y;
			//	return;
			//}

			//this._owner.relations.handling = ( GObject )sender;

			Vector2 oldPos = this._owner.position;

			float dx = this._target.position.x - this._targetData.x;
			float dy = this._target.position.y - this._targetData.y;

			foreach ( RelationDef info in this._defs )
				this.ApplyOnPositionChanged( info, dx, dy );

			this._targetData.x = this._target.position.x;
			this._targetData.y = this._target.position.y;

			if ( oldPos != this._owner.position )
			{
				oldPos = this._owner.position - oldPos;

				if ( this._owner.gearXY.controller != null )
					this._owner.gearXY.UpdateFromRelations( oldPos );

				if ( this._owner.parent != null )
				{
					int count = this._owner.parent.TransitionCount();
					for ( int i = 0; i < count; i++ )
						this._owner.parent.GetTransitionAt( i ).UpdateFromRelations( this._owner.id, oldPos );
				}
			}

			//this._owner.relations.handling = null;
		}

		private void OnTargetSizeChanged( object sender )
		{
			//if ( this._owner.relations.handling != null || this._owner.onSizeChanged.isDispatching )
			//{
			//	this._targetData.z = this._target.size.x;
			//	this._targetData.w = this._target.size.y;
			//	return;
			//}

			//this._owner.relations.handling = ( GObject )sender;

			Vector2 oldPos = this._owner.position;
			Vector2 oldSize = this._owner.size;

			foreach ( RelationDef info in this._defs )
				this.ApplyOnSizeChanged( info );

			this._targetData.z = this._target.size.x;
			this._targetData.w = this._target.size.y;

			if ( oldPos != this._owner.position )
			{
				oldPos = this._owner.position - oldPos;

				if ( this._owner.gearXY.controller != null )
					this._owner.gearXY.UpdateFromRelations( oldPos );

				if ( this._owner.parent != null )
				{
					int count = this._owner.parent.TransitionCount();
					for ( int i = 0; i < count; i++ )
						this._owner.parent.GetTransitionAt( i ).UpdateFromRelations( this._owner.id, oldPos );
				}
			}

			if ( oldSize != this._owner.size )
			{
				oldSize = this._owner.size - oldSize;

				if ( this._owner.gearSize.controller != null )
					this._owner.gearSize.UpdateFromRelations( oldSize );
			}

			//this._owner.relations.handling = null;
		}

		private void OnTargetSizeWillChange()
		{
		}
	}
}
