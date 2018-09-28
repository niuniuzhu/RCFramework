using System;
using System.Collections.Generic;

namespace Game.Misc
{
	public struct Keyframe : IComparable
	{
		public float time;
		public float value;
		public float inTangent;
		public float outTangent;

		public Keyframe( float time, float value, float inTangent, float outTangent )
		{
			this.time = time;
			this.value = value;
			this.inTangent = inTangent;
			this.outTangent = outTangent;
		}

		public int CompareTo( object obj )
		{
			Keyframe b = ( Keyframe )obj;
			return this.time > b.time ? 1 : -1;
		}
	}

	public class AnimationCurve
	{
		private readonly List<Keyframe> _keyframes = new List<Keyframe>();
		private bool _dirty;

		public void AddKey( float time, float value, float inTangent, float outTangent )
		{
			Keyframe keyframe;
			keyframe.time = time;
			keyframe.value = value;
			keyframe.inTangent = inTangent;
			keyframe.outTangent = outTangent;
			this.AddKey( keyframe );
		}

		public void AddKey( Keyframe keyframe )
		{
			this._keyframes.Add( keyframe );
			this._dirty = true;
		}

		public float Evaluate( float t )
		{
			if ( this._dirty )
			{
				this._keyframes.Sort();
				this._dirty = false;
			}

			int count = this._keyframes.Count;

			if ( count == 0 )
				return 0;

			if ( count == 1 )
				return this._keyframes[0].value;

			if ( t < this._keyframes[0].time )
				return this._keyframes[0].value;

			if ( t >= this._keyframes[count - 1].time )
				return this._keyframes[count - 1].value;

			int i = 1;
			for ( ; i < count; i++ )
			{
				if ( t < this._keyframes[i - 1].time ||
					 t >= this._keyframes[i].time )
					continue;

				Keyframe k0 = this._keyframes[i - 1];
				Keyframe k1 = this._keyframes[i];
				return this.Evaluate( t, k0, k1 );
			}
			return this._keyframes[count - 1].value;
		}

		private float Evaluate( float t, Keyframe keyframe0, Keyframe keyframe1 )
		{
			float dt = keyframe1.time - keyframe0.time;

			float m0 = keyframe0.outTangent * dt;
			float m1 = keyframe1.inTangent * dt;

			float t2 = t * t;
			float t3 = t2 * t;

			float a = 2 * t3 - 3 * t2 + 1;
			float b = t3 - 2 * t2 + t;
			float c = t3 - t2;
			float d = -2 * t3 + 3 * t2;

			return a * keyframe0.value + b * m0 + c * m1 + d * keyframe1.value;
		}
	}
}