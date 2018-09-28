using Core.Xml;
using DG.Tweening;
using FairyUGUI.Core;
using FairyUGUI.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyUGUI.UI
{

	public class Transition
	{
		public delegate void PlayCompleteCallback();

		public delegate void TransitionHook();

		private const float FRAME_RATE = 24;
		private const int OPTION_IGNORE_DISPLAY_CONTROLLER = 1;

		public string name { get; private set; }

		public bool playing => this._playing;

		public bool autoPlay;
		public int autoPlayRepeat;
		public float autoPlayDelay;

		private GComponent _owner;
		private readonly List<TransitionItem> _items = new List<TransitionItem>();
		private int _totalTimes;
		private int _totalTasks;
		private bool _playing;
		private int _options;
		private bool _reversed;
		private float _maxTime;
		private PlayCompleteCallback _onComplete;

		public Transition( GComponent owner )
		{
			this._owner = owner;
			this.autoPlayRepeat = 1;
		}

		public void Dispose()
		{
			this._owner = null;
			this._onComplete = null;
			this._items.Clear();
		}

		public void SetCompleteCallback( PlayCompleteCallback onComplete )
		{
			this._onComplete = onComplete;
		}

		public void Play( int times = 1, float delay = 0 )
		{
			this.InternalPlay( times, delay, false );
		}

		public void PlayReverse( int times = 1, float delay = 0 )
		{
			this.InternalPlay( times, delay, true );
		}

		private void InternalPlay( int times, float delay, bool reverse )
		{
			this.Stop();
			this._totalTimes = times == 0 ? 1 : ( times == -1 ? int.MaxValue : times );
			this._reversed = reverse;
			this.InternalPlay( delay );
		}

		private void InternalPlay( float delay )
		{
			this._totalTasks = 0;
			this._playing = true;

			int cnt = this._items.Count;
			for ( int i = 0; i < cnt; i++ )
			{
				TransitionItem item = this._items[i];
				item.target = item.targetId.Length > 0 ? this._owner.GetChildById( item.targetId ) : this._owner;
				if ( item.target == null || item.target.disposed )
				{
					this._items.RemoveAt( i );
					--i;
					--cnt;
					continue;
				}

				item.completed = false;
				this._totalTasks++;

				if ( ( this._options & OPTION_IGNORE_DISPLAY_CONTROLLER ) != 0 )
					++item.target.ignoreGearVisible;

				float startTime = delay;
				if ( this._reversed )
					startTime += ( this._maxTime - item.time - item.duration );
				else
					startTime += item.time;

				if ( item.tween )
					this.StartTween( item, delay );
				else
				{
					item.startValue.Copy( item.value );
					TransitionValue startValue = item.startValue;
					switch ( item.type )
					{
						case TransitionActionType.XY:
							if ( !startValue.b1 )
								startValue.f.x += item.target.position.x;
							if ( !startValue.b2 )
								startValue.f.y += item.target.position.y;
							break;

						case TransitionActionType.Size:
							if ( !startValue.b1 )
								startValue.f.x += item.target.size.x;
							if ( !startValue.b2 )
								startValue.f.y += item.target.size.y;
							break;

						case TransitionActionType.Animation:
							if ( !startValue.b1 )
								startValue.f.x += ( ( IAnimationGear )item.target ).frame;
							break;
					}
					item.tweener = DOVirtual.DelayedCall( 0, null ).OnComplete( () =>
					{
                        item.hook?.Invoke();
                        this.ApplyValue( item, startValue );
						this.Complete( item );
					} );

					if ( startTime > 0 )
						item.tweener.SetDelay( startTime );
					else
						this.ApplyValue( item, startValue );
				}
			}

			if ( this._totalTasks == 0 )
			{
				this._playing = false;

				if ( this._onComplete != null )
				{
					PlayCompleteCallback func = this._onComplete;
					this._onComplete = null;
					func();
				}
			}
		}

		private void StartTween( TransitionItem item, float delay )
		{
			Vector2 startValue = item.startValue.f;
			Vector2 endValue = item.endValue.f;

			switch ( item.type )
			{
				case TransitionActionType.XY:
					if ( !item.startValue.b1 )
						startValue.x += item.target.position.x;
					if ( !item.startValue.b2 )
						startValue.y += item.target.position.y;
					if ( !item.endValue.b1 )
						endValue.x += item.target.position.x;
					if ( !item.endValue.b2 )
						endValue.y += item.target.position.y;
					break;

				case TransitionActionType.Size:
					if ( !item.startValue.b1 )
						startValue.x += item.target.size.x;
					if ( !item.startValue.b2 )
						startValue.y += item.target.size.y;
					if ( !item.endValue.b1 )
						endValue.x += item.target.size.x;
					if ( !item.endValue.b2 )
						endValue.y += item.target.size.y;
					break;

				case TransitionActionType.Animation:
					if ( !item.startValue.b1 )
						startValue.x += ( ( IAnimationGear )item.target ).frame;
					if ( !item.endValue.b1 )
						endValue.x += ( ( IAnimationGear )item.target ).frame;
					break;
			}

			float startTime = delay;
			if ( this._reversed )
			{
				Vector2 tmp = startValue;
				startValue = endValue;
				endValue = tmp;
				startTime += ( this._maxTime - item.time - item.duration );
			}
			else
				startTime += item.time;

			switch ( item.type )
			{
				case TransitionActionType.Shake:
					{
						item.tweener = DOTween.Shake( () => item.target.position, v => item.endValue.f = v, item.value.f.y, item.value.f.x,
							10 /*频率*/, 90, true, true )
							.OnUpdate( () => this.ApplyValue( item, item.endValue ) );
					}
					break;

				case TransitionActionType.Color:
					{
						item.tweener = DOTween.To( () => item.startValue.c,
							v => { item.value.c = v; }, item.endValue.c, item.duration )
							.OnUpdate( () => this.ApplyValue( item, item.value ) );
					}
					break;

				default:
					{
						item.tweener = DOTween.To( () => startValue,
							v => { item.value.f = v; }, endValue, item.duration )
							.OnUpdate( () => this.ApplyValue( item, item.value ) );
					}
					break;
			}
			item.tweener
			.SetUpdate( true )
			.SetEase( item.easeType )
			.OnStart( () =>
			{
                item.hook?.Invoke();
            } )
			.OnComplete( () =>
			{
                item.hook2?.Invoke();
                this.Complete( item );
			} );

			if ( item.repeat != 0 )
				item.tweener.SetLoops( item.repeat == -1 ? int.MaxValue : ( item.repeat + 1 ), item.yoyo ? LoopType.Yoyo : LoopType.Restart );

			if ( startTime > 0 )
				item.tweener.SetDelay( startTime );
			else
			{
				item.value.f = startValue;
				this.ApplyValue( item, item.value );
			}
		}

		public void Stop( bool setToComplete = false )
		{
			if ( !this._playing )
				return;

			this._totalTimes = 0;

			int cnt = this._items.Count;
			for ( int i = 0; i < cnt; i++ )
			{
				TransitionItem item = this._items[i];
				if ( item.target == null )
					continue;
				this.StopItem( item, setToComplete );
			}
		}

		private void StopItem( TransitionItem item, bool setToComplete )
		{
			if ( item.completed )
				return;

			item.tweener.Kill( setToComplete );
			if ( !setToComplete )//当setToComplete为true时，tweener会自动执行，否则需要手动调用
				this.Complete( item );
		}

		private void Complete( TransitionItem item )
		{
			if ( ( this._options & OPTION_IGNORE_DISPLAY_CONTROLLER ) != 0 )
				--item.target.ignoreGearVisible;
			item.tweener = null;
			item.completed = true;
			this._totalTasks--;

			this.CheckAllComplete();
		}

		private void CheckAllComplete()
		{
			if ( !this._playing || this._totalTasks != 0 )
				return;

			if ( this._totalTimes < 0 )
				this.InternalPlay( 0 );
			else
			{
				this._totalTimes--;
				if ( this._totalTimes > 0 )
					this.InternalPlay( 0 );
				else
				{
					this._playing = false;

					if ( this._onComplete != null )
					{
						PlayCompleteCallback func = this._onComplete;
						this._onComplete = null;
						func();
					}
				}
			}
		}

		private void ApplyValue( TransitionItem item, TransitionValue value )
		{

			switch ( item.type )
			{
				case TransitionActionType.XY:
				case TransitionActionType.Shake:
					item.target.SetGearState( GObject.GearState.Position, true );
					item.target.position = value.f;
					item.target.SetGearState( GObject.GearState.Position, false );
					break;

				case TransitionActionType.Size:
					item.target.SetGearState( GObject.GearState.Size, true );
					item.target.size = value.f;
					item.target.SetGearState( GObject.GearState.Size, false );
					break;

				case TransitionActionType.Pivot:
					item.target.pivot = value.f;
					break;

				case TransitionActionType.Alpha:
					item.target.SetGearState( GObject.GearState.Look, true );
					item.target.alpha = value.f.x;
					item.target.SetGearState( GObject.GearState.Look, false );
					break;

				case TransitionActionType.Rotation:
					item.target.SetGearState( GObject.GearState.Look, true );
					item.target.rotation = value.f.x;
					item.target.SetGearState( GObject.GearState.Look, false );
					break;

				case TransitionActionType.Scale:
					item.target.SetGearState( GObject.GearState.Size, true );
					item.target.scale = value.f;
					item.target.SetGearState( GObject.GearState.Size, false );
					break;

				case TransitionActionType.Color:
					item.target.SetGearState( GObject.GearState.Color, true );
					item.target.color = value.c;
					item.target.SetGearState( GObject.GearState.Color, false );
					break;

				case TransitionActionType.Animation:
					item.target.SetGearState( GObject.GearState.Animation, true );
					( ( IAnimationGear )item.target ).frame = value.i;
					( ( IAnimationGear )item.target ).playing = value.b;
					item.target.SetGearState( GObject.GearState.Animation, false );
					break;

				case TransitionActionType.Visible:
					item.target.visible = value.b;
					break;

				case TransitionActionType.Controller:
					string[] arr = value.s.Split( ',' );
					foreach ( string str in arr )
					{
						string[] arr2 = str.Split( '=' );
						Controller cc = ( ( GComponent )item.target ).GetController( arr2[0] );
						if ( cc != null )
						{
							string str2 = arr2[1];
							if ( str2[0] == '$' )
							{
								str2 = str.Substring( 1 );
								cc.selectedPage = str2;
							}
							else
								cc.selectedIndex = int.Parse( str2 );
						}
					}
					break;

				case TransitionActionType.Transition:
					Transition trans = ( ( GComponent )item.target ).GetTransition( value.s );
					if ( trans != null )
					{
						if ( value.i == 0 )
							trans.Stop();
						else if ( trans.playing )
							trans._totalTimes = value.i == -1 ? int.MaxValue : value.i;
						else
						{
							if ( this._reversed )
								trans.PlayReverse( value.i );
							else
								trans.Play( value.i );
						}
					}
					break;

				case TransitionActionType.Sound:
					AudioClip sound = UIPackage.GetItemAssetByURL( value.s ) as AudioClip;
					if ( sound != null )
						Stage.inst.PlayOneShotSound( sound, value.f.x );
					break;
			}
		}

		public void SetValue( string label, params object[] aParams )
		{
			int cnt = this._items.Count;
			for ( int i = 0; i < cnt; i++ )
			{
				TransitionItem item = this._items[i];
				if ( item.label == null && item.label2 == null )
					continue;

				TransitionValue value;
				if ( item.label == label )
					value = item.tween ? item.startValue : item.value;
				else if ( item.label2 == label )
					value = item.endValue;
				else
					continue;

				switch ( item.type )
				{
					case TransitionActionType.XY:
					case TransitionActionType.Size:
					case TransitionActionType.Pivot:
					case TransitionActionType.Scale:
						value.b1 = true;
						value.b2 = true;
						value.f.x = Convert.ToSingle( aParams[0] );
						value.f.y = Convert.ToSingle( aParams[1] );
						break;

					case TransitionActionType.Alpha:
						value.f.x = Convert.ToSingle( aParams[0] );
						break;

					case TransitionActionType.Rotation:
						value.i = Convert.ToInt32( aParams[0] );
						break;

					case TransitionActionType.Color:
						value.c = ( Color )aParams[0];
						break;

					case TransitionActionType.Animation:
						value.i = Convert.ToInt32( aParams[0] );
						if ( aParams.Length > 1 )
							value.b = Convert.ToBoolean( aParams[1] );
						break;

					case TransitionActionType.Visible:
						value.b = Convert.ToBoolean( aParams[0] );
						break;

					case TransitionActionType.Controller:
						value.s = ( string )aParams[0];
						break;

					case TransitionActionType.Sound:
						value.s = ( string )aParams[0];
						if ( aParams.Length > 1 )
							value.f.x = Convert.ToSingle( aParams[1] );
						break;

					case TransitionActionType.Transition:
						value.s = ( string )aParams[0];
						if ( aParams.Length > 1 )
							value.i = Convert.ToInt32( aParams[1] );
						break;

					case TransitionActionType.Shake:
						value.f.x = Convert.ToSingle( aParams[0] );
						if ( aParams.Length > 1 )
							value.f.y = Convert.ToSingle( aParams[1] );
						break;
				}
			}
		}

		public void SetHook( string label, TransitionHook callback )
		{
			int cnt = this._items.Count;
			for ( int i = 0; i < cnt; i++ )
			{
				TransitionItem item = this._items[i];
				if ( item.label == null && item.label2 == null )
					continue;

				if ( item.label == label )
					item.hook = callback;
				else if ( item.label2 == label )
					item.hook2 = callback;
			}
		}

		public void ClearHooks()
		{
			int cnt = this._items.Count;
			for ( int i = 0; i < cnt; i++ )
			{
				TransitionItem item = this._items[i];
				item.hook = null;
				item.hook2 = null;
			}
		}

		public void SetTarget( string label, GObject newTarget )
		{
			int cnt = this._items.Count;
			for ( int i = 0; i < cnt; i++ )
			{
				TransitionItem item = this._items[i];
				if ( item.label == null && item.label2 == null )
					continue;

				item.targetId = newTarget.id;
			}
		}

		public void Copy( Transition source )
		{
			this.Stop();
			this._items.Clear();
			int cnt = source._items.Count;
			for ( int i = 0; i < cnt; i++ )
				this._items.Add( source._items[i].Clone() );
		}

		internal void UpdateFromRelations( string targetId, Vector2 delta )
		{
			int cnt = this._items.Count;
			if ( cnt == 0 )
				return;

			for ( int i = 0; i < cnt; i++ )
			{
				TransitionItem item = this._items[i];
				if ( item.type == TransitionActionType.XY && item.targetId == targetId )
				{
					if ( item.tween )
					{
						item.startValue.f += delta;
						item.endValue.f += delta;
					}
					else
						item.value.f += delta;
				}
			}
		}

		public void Setup( XML xml )
		{
			this.name = xml.GetAttribute( "name" );
			this._options = xml.GetAttributeInt( "options" );
			this.autoPlay = xml.GetAttributeBool( "autoPlay" );
			if ( this.autoPlay )
			{
				this.autoPlayRepeat = xml.GetAttributeInt( "autoPlayRepeat", 1 );
				this.autoPlayDelay = xml.GetAttributeFloat( "autoPlayDelay" );
			}
			XMLList col = xml.Elements( "item" );

			foreach ( XML cxml in col )
			{
				TransitionItem item = new TransitionItem();
				this._items.Add( item );

				item.time = cxml.GetAttributeInt( "time" ) / FRAME_RATE;
				item.targetId = cxml.GetAttribute( "target", string.Empty );
				item.type = FieldTypes.ParseTransitionActionType( cxml.GetAttribute( "type" ) );
				item.tween = cxml.GetAttributeBool( "tween" );
				item.label = cxml.GetAttribute( "label" );
				if ( item.tween )
				{
					item.duration = cxml.GetAttributeInt( "duration" ) / FRAME_RATE;
					if ( item.time + item.duration > this._maxTime )
						this._maxTime = item.time + item.duration;

					string ease = cxml.GetAttribute( "ease" );
					if ( ease != null )
						item.easeType = FieldTypes.ParseEaseType( ease );

					item.repeat = cxml.GetAttributeInt( "repeat" );
					item.yoyo = cxml.GetAttributeBool( "yoyo" );
					item.label2 = cxml.GetAttribute( "label2" );

					string v = cxml.GetAttribute( "endValue" );
					if ( v != null )
					{
						this.DecodeValue( item.type, cxml.GetAttribute( "startValue", string.Empty ), item.startValue );
						this.DecodeValue( item.type, v, item.endValue );
					}
					else
					{
						item.tween = false;
						this.DecodeValue( item.type, cxml.GetAttribute( "startValue", string.Empty ), item.value );
					}
				}
				else
				{
					if ( item.time > this._maxTime )
						this._maxTime = item.time;
					this.DecodeValue( item.type, cxml.GetAttribute( "value", string.Empty ), item.value );
					if ( item.type == TransitionActionType.Shake )
						item.tween = true;
				}
			}
		}

		private void DecodeValue( TransitionActionType type, string str, TransitionValue value )
		{
			string[] arr;
			switch ( type )
			{
				case TransitionActionType.XY:
				case TransitionActionType.Size:
					arr = str.Split( ',' );
					if ( arr[0] == "-" )
						value.b1 = false;
					else
					{
						value.f.x = float.Parse( arr[0] );
						value.b1 = true;
					}
					if ( arr[1] == "-" )
						value.b2 = false;
					else
					{
						value.f.y = float.Parse( arr[1] );
						value.b2 = true;
					}
					break;

				case TransitionActionType.Alpha:
				case TransitionActionType.Rotation:
					value.f.x = float.Parse( str );
					break;

				case TransitionActionType.Scale:
					arr = str.Split( ',' );
					value.f.x = float.Parse( arr[0] );
					value.f.y = float.Parse( arr[1] );
					break;

				case TransitionActionType.Pivot:
					arr = str.Split( ',' );
					value.f.x = float.Parse( arr[0] );
					value.f.y = 1 - float.Parse( arr[1] );
					break;

				case TransitionActionType.Color:
					value.c = ToolSet.ConvertFromHtmlColor( str );
					value.c.a = 1f;
					break;

				case TransitionActionType.Animation:
					arr = str.Split( ',' );
					if ( arr[0] == "-" )
						value.b1 = false;
					else
					{
						value.i = int.Parse( arr[0] );
						value.b1 = true;
					}
					value.b = arr[1] == "p";
					break;

				case TransitionActionType.Visible:
					value.b = str == "true";
					break;

				case TransitionActionType.Controller:
					value.s = str;
					break;

				case TransitionActionType.Sound:
					arr = str.Split( ',' );
					value.s = arr[0];
					if ( arr.Length > 1 )
					{
						int intv = int.Parse( arr[1] );
						if ( intv == 100 || intv == 0 )
							value.f.x = 1;
						else
							value.f.x = intv / 100f;
					}
					else
						value.f.x = 1;
					break;

				case TransitionActionType.Transition:
					arr = str.Split( ',' );
					value.s = arr[0];
					value.i = arr.Length > 1 ? int.Parse( arr[1] ) : 1;
					break;

				case TransitionActionType.Shake:
					arr = str.Split( ',' );
					value.f.x = float.Parse( arr[0] );
					value.f.y = float.Parse( arr[1] );
					break;
			}
		}
	}

	class TransitionItem
	{
		internal float time;
		internal string targetId;
		internal TransitionActionType type;
		internal float duration;
		internal readonly TransitionValue value;
		internal readonly TransitionValue startValue;
		internal readonly TransitionValue endValue;
		internal Ease easeType;
		internal int repeat;
		internal bool yoyo;
		internal bool tween;
		internal string label;
		internal string label2;

		//hooks
		internal Transition.TransitionHook hook;
		internal Transition.TransitionHook hook2;

		//running properties
		internal Tween tweener;
		internal bool completed;
		internal GObject target;

		internal TransitionItem()
		{
			this.easeType = Ease.OutQuad;
			this.value = new TransitionValue();
			this.startValue = new TransitionValue();
			this.endValue = new TransitionValue();
		}

		internal TransitionItem Clone()
		{
			TransitionItem item = new TransitionItem();
			item.time = this.time;
			item.targetId = this.targetId;
			item.type = this.type;
			item.duration = this.duration;
			item.value.Copy( this.value );
			item.startValue.Copy( this.startValue );
			item.endValue.Copy( this.endValue );
			item.easeType = this.easeType;
			item.repeat = this.repeat;
			item.yoyo = this.yoyo;
			item.tween = this.tween;
			item.label = this.label;
			item.label2 = this.label2;
			return item;
		}
	}

	class TransitionValue
	{
		internal Vector2 f;
		internal float f3;
		internal int i;//rotation,frame
		internal Color c;//color
		internal bool b;//playing
		internal string s;//sound,transName

		internal bool b1;
		internal bool b2;

		internal TransitionValue()
		{
			this.b1 = true;
			this.b2 = true;
		}

		internal void Copy( TransitionValue source )
		{
			this.f.x = source.f.x;
			this.f.y = source.f.y;
			this.f3 = source.f3;
			this.i = source.i;
			this.c = source.c;
			this.b = source.b;
			this.s = source.s;
			this.b1 = source.b1;
			this.b2 = source.b2;
		}
	}
}
