using Core.FMath;
using Core.Math;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest
{
	public class MathTest
	{
		private readonly ITestOutputHelper _output;

		public MathTest( ITestOutputHelper output )
		{
			this._output = output;
		}

		public void Log( object msg )
		{
			this._output.WriteLine( msg.ToString() );
		}

		[Fact]
		public void MTest()
		{
			var m = Mat4.FromTRS( new Vec3( 1, -2, 3 ), Quat.Euler( new Vec3( 90, 0, 0 ) ), new Vec3( 2, 3, 4 ) );
			this.Log( m.ToString() );
			m.Invert();
			this.Log( m );
			this.Log( m.TransformPoint( new Vec3( 1, 0, -1 ) ) );

			var m2 = Mat4.FromRotationAxis( -43, Vec3.Normalize( new Vec3( 3, 2, 4 ) ) );
			this.Log( m2 );
			var m3 = m2 * m;
			this.Log( m3 );


			//var m4 = Mat3.FromOuterProduct( new Vec3( 1, -2, 3 ), new Vec3( 93, 44, 32 ) );
			//var m5 = Mat3.FromCross( new Vec3( 2.5f, 3, 4 ) );
			//var m6 = m4 * m5;
			//m6 = m6.RotateAround( 33, new Vec3( 2, 3, 4 ) );
			//this.Log( m6 );
		}

		[Fact]
		public void Fix64Test()
		{
			Fix64 f = Fix64.Zero;
			float f2 = -2.89f;
			Stopwatch sw = new Stopwatch();
			sw.Start();
			for ( int i = 0; i < 10000000; i++ )
			{
				f = ( Fix64 )f2;
				f2 = ( float )f;
			}
			sw.Stop();
			this._output.WriteLine( sw.ElapsedMilliseconds.ToString() );
			this._output.WriteLine( f2.ToString() );
		}

		[Fact]
		public void Vector()
		{
			FVec3 fv = new FVec3( 12.5f, 9, 8 );
			FVec3 fv2 = new FVec3( 4, 6, 9 );
			Vec3 v = new Vec3( 12.5f, 9, 8 );
			Vec3 v2 = new Vec3( 4, 6, 9 );
			this._output.WriteLine( ( fv * fv2 ).ToString() );
			this._output.WriteLine( ( v * v2 ).ToString() );
			fv += fv2;
			v += v2;
			this._output.WriteLine( fv.ToString() );
			this._output.WriteLine( v.ToString() );
			this._output.WriteLine( ( fv - fv2 ).ToString() );
			this._output.WriteLine( ( v - v2 ).ToString() );
			this._output.WriteLine( fv.Dot( fv2 ).ToString() );
			this._output.WriteLine( v.Dot( v2 ).ToString() );
			this._output.WriteLine( fv.Cross( fv2 ).ToString() );
			this._output.WriteLine( v.Cross( v2 ).ToString() );
			this._output.WriteLine( FVec3.Normalize( fv ).ToString() );
			this._output.WriteLine( Vec3.Normalize( v ).ToString() );
			this._output.WriteLine( FVec3.Slerp( fv, fv2, ( Fix64 )0.45678 ).ToString() );
			this._output.WriteLine( Vec3.Slerp( v, v2, 0.45678f ).ToString() );
		}

		[Fact]
		public void Matrix3()
		{
			FMat3 fm = FMat3.FromQuaternion( FQuat.Euler( ( Fix64 )30, ( Fix64 )( -20 ), ( Fix64 )49.342f ) );
			Mat3 m = Mat3.FromQuaternion( Quat.Euler( 30, -20, 49.342f ) );
			FVec3 fv = new FVec3( 12.5f, 9, 8 );
			FVec3 fv2 = new FVec3( 4, 6, 9 );
			Vec3 v = new Vec3( 12.5f, 9, 8 );
			Vec3 v2 = new Vec3( 4, 6, 9 );
			fv = fm.TransformPoint( fv );
			fv2 = fm.TransformVector( fv2 );
			v = m.TransformPoint( v );
			v2 = m.TransformVector( v2 );
			this._output.WriteLine( fm.ToString() );
			this._output.WriteLine( m.ToString() );
			this._output.WriteLine( fv.ToString() );
			this._output.WriteLine( fv2.ToString() );
			this._output.WriteLine( v.ToString() );
			this._output.WriteLine( v2.ToString() );
			fm = FMat3.LookAt( fv, fv2 );
			m = Mat3.LookAt( v, v2 );
			this._output.WriteLine( fm.ToString() );
			this._output.WriteLine( m.ToString() );
			fv = fm.Euler();
			v = m.Euler();
			this._output.WriteLine( fv.ToString() );
			this._output.WriteLine( v.ToString() );
			fm = FMat3.FromEuler( fv );
			m = Mat3.FromEuler( v );
			this._output.WriteLine( fm.ToString() );
			this._output.WriteLine( m.ToString() );
			fm = FMat3.FromScale( fv );
			m = Mat3.FromScale( v );
			this._output.WriteLine( fm.ToString() );
			this._output.WriteLine( m.ToString() );
			fm = FMat3.FromCross( fv );
			m = Mat3.FromCross( v );
			this._output.WriteLine( fm.ToString() );
			this._output.WriteLine( m.ToString() );
			fm = FMat3.FromOuterProduct( fv, fv2 );
			m = Mat3.FromOuterProduct( v, v2 );
			this._output.WriteLine( fm.ToString() );
			this._output.WriteLine( m.ToString() );
			fm = FMat3.FromRotationAxis( ( Fix64 )35, fv );
			m = Mat3.FromRotationAxis( 35, v );
			this._output.WriteLine( fm.ToString() );
			this._output.WriteLine( m.ToString() );
			fm = FMat3.NonhomogeneousInverse( fm );
			m = Mat3.NonhomogeneousInvert( m );
			this._output.WriteLine( fm.ToString() );
			this._output.WriteLine( m.ToString() );
		}

		[Fact]
		public void Matrix4()
		{
			FMat4 fm = FMat4.FromQuaternion( FQuat.Euler( ( Fix64 )30, ( Fix64 )( -20 ), ( Fix64 )49.342f ) );
			Mat4 m = Mat4.FromQuaternion( Quat.Euler( 30, -20, 49.342f ) );
			FVec3 fv = new FVec3( 12.5f, 9, 8 );
			FVec3 fv2 = new FVec3( 4, 6, 9 );
			Vec3 v = new Vec3( 12.5f, 9, 8 );
			Vec3 v2 = new Vec3( 4, 6, 9 );
			fv = fm.TransformPoint( fv );
			fv2 = fm.TransformVector( fv2 );
			v = m.TransformPoint( v );
			v2 = m.TransformVector( v2 );
			this._output.WriteLine( fm.ToString() );
			this._output.WriteLine( m.ToString() );
			this._output.WriteLine( fv.ToString() );
			this._output.WriteLine( fv2.ToString() );
			this._output.WriteLine( v.ToString() );
			this._output.WriteLine( v2.ToString() );
			fm = FMat4.FromEuler( fv );
			m = Mat4.FromEuler( v );
			this._output.WriteLine( fm.ToString() );
			this._output.WriteLine( m.ToString() );
			fm = FMat4.FromScale( fv );
			m = Mat4.FromScale( v );
			this._output.WriteLine( fm.ToString() );
			this._output.WriteLine( m.ToString() );
			fm = FMat4.FromRotationAxis( ( Fix64 )35, fv );
			m = Mat4.FromRotationAxis( 35, v );
			this._output.WriteLine( fm.ToString() );
			this._output.WriteLine( m.ToString() );
			fm = FMat4.NonhomogeneousInverse( fm );
			m = Mat4.NonhomogeneousInvert( m );
			this._output.WriteLine( fm.ToString() );
			this._output.WriteLine( m.ToString() );
			fm = FMat4.FromTRS( new FVec3( 4, 5, 6 ), FQuat.identity, FVec3.one );
			m = Mat4.FromTRS( new Vec3( 4, 5, 6 ), Quat.identity, Vec3.one );
			this._output.WriteLine( fm.ToString() );
			this._output.WriteLine( m.ToString() );
			fm = FMat4.NonhomogeneousInverse( fm );
			m = Mat4.NonhomogeneousInvert( m );
			this._output.WriteLine( fm.ToString() );
			this._output.WriteLine( m.ToString() );
			fv = fm.TransformPoint( fv );
			v = m.TransformPoint( v );
			this._output.WriteLine( fv.ToString() );
			this._output.WriteLine( v.ToString() );
		}

		[Fact]
		public void Quat4()
		{
			FQuat fq = FQuat.Euler( ( Fix64 )45, ( Fix64 )( -23 ), ( Fix64 )( -48.88 ) );
			FQuat fq2 = FQuat.Euler( ( Fix64 )23, ( Fix64 )( -78 ), ( Fix64 )( -132.43f ) );
			Quat q = Quat.Euler( 45, -23, -48.88f );
			Quat q2 = Quat.Euler( 23, -78, -132.43f );
			FVec3 fv = new FVec3( 12.5f, 9, 8 );
			FVec3 fv2 = new FVec3( 1, 0, 0 );
			Vec3 v = new Vec3( 12.5f, 9, 8 );
			Vec3 v2 = new Vec3( 1, 0, 0 );
			this._output.WriteLine( fq.ToString() );
			this._output.WriteLine( q.ToString() );
			this._output.WriteLine( fq2.ToString() );
			this._output.WriteLine( q2.ToString() );
			Fix64 fa = FQuat.Angle( fq, fq2 );
			float a = Quat.Angle( q, q2 );
			this._output.WriteLine( fa.ToString() );
			this._output.WriteLine( a.ToString() );
			fq = FQuat.AngleAxis( ( Fix64 )( -123.324 ), fv );
			q = Quat.AngleAxis( -123.324f, v );
			this._output.WriteLine( fq.ToString() );
			this._output.WriteLine( q.ToString() );
			fa = FQuat.Dot( fq, fq2 );
			a = Quat.Dot( q, q2 );
			this._output.WriteLine( fa.ToString() );
			this._output.WriteLine( a.ToString() );
			fq = FQuat.FromToRotation( FVec3.Normalize( fv ), fv2 );
			q = Quat.FromToRotation( Vec3.Normalize( v ), v2 );
			this._output.WriteLine( fq.ToString() );
			this._output.WriteLine( q.ToString() );
			fq = FQuat.Lerp( fq, fq2, ( Fix64 )0.66 );
			q = Quat.Lerp( q, q2, 0.66f );
			this._output.WriteLine( fq.ToString() );
			this._output.WriteLine( q.ToString() );
			fq = FQuat.Normalize( fq );
			q.Normalize();
			this._output.WriteLine( fq.ToString() );
			this._output.WriteLine( q.ToString() );
			fq.Inverse();
			q = Quat.Inverse( q );
			this._output.WriteLine( fq.ToString() );
			this._output.WriteLine( q.ToString() );
			fv = FQuat.Orthogonal( fv );
			v = Quat.Orthogonal( v );
			this._output.WriteLine( fv.ToString() );
			this._output.WriteLine( v.ToString() );
			fq = FQuat.Slerp( fq, fq2, ( Fix64 )0.66 );
			q = Quat.Slerp( q, q2, 0.66f );
			this._output.WriteLine( fq.ToString() );
			this._output.WriteLine( q.ToString() );
			fq = FQuat.LookRotation( FVec3.Normalize( fv ), fv2 );
			q = Quat.LookRotation( Vec3.Normalize( v ), v2 );
			this._output.WriteLine( fq.ToString() );
			this._output.WriteLine( q.ToString() );
			fq.ToAngleAxis( out fa, out fv );
			q.ToAngleAxis( out a, out v );
			this._output.WriteLine( fa.ToString() );
			this._output.WriteLine( a.ToString() );
			this._output.WriteLine( fv.ToString() );
			this._output.WriteLine( v.ToString() );
			fq = fq.Conjugate();
			q = q.Conjugate();
			this._output.WriteLine( fq.ToString() );
			this._output.WriteLine( q.ToString() );
			fq.SetLookRotation( FVec3.Normalize( fv ), fv2 );
			q.SetLookRotation( Vec3.Normalize( v ), v2 );
			this._output.WriteLine( fq.ToString() );
			this._output.WriteLine( q.ToString() );
		}
	}
}
