namespace org.critterai.u3d
{
	public static class VectorHelper
	{
		public static void ToUnityVector3( ref Vector3 v, ref UnityEngine.Vector3 o )
		{
			o.x = v.x;
			o.y = v.y;
			o.z = v.z;
		}


		public static UnityEngine.Vector3 ToUnityVector3( this Vector3 v )
		{
			return new UnityEngine.Vector3( v.x, v.y, v.z );
		}


		public static Vector3 ToVector3( this UnityEngine.Vector3 v )
		{
			return new Vector3( v.x, v.y, v.z );
		}

		public static UnityEngine.Vector3[] ToUnityVector3Array( ref Vector3[] v )
		{
			if ( v == null )
				return null;
			int count = v.Length;
			if ( count == 0 )
				return new UnityEngine.Vector3[0];
			UnityEngine.Vector3[] o = new UnityEngine.Vector3[count];
			unsafe
			{
				fixed ( Vector3* v0 = &v[0] )
				{
					Vector3* ps = v0;
					fixed ( UnityEngine.Vector3* v1 = &o[0] )
					{
						UnityEngine.Vector3* pt = v1;
						for ( int i = 0; i < count; i++ )
						{
							*pt = *( UnityEngine.Vector3* )ps;
							++ps;
							++pt;
						}
					}
				}
			}
			return o;
		}

		public static Vector3[] ToVector3Array( ref UnityEngine.Vector3[] v )
		{
			if ( v == null )
				return null;
			int count = v.Length;
			if ( count == 0 )
				return new Vector3[0];
			Vector3[] o = new Vector3[count];
			unsafe
			{
				fixed ( UnityEngine.Vector3* v0 = &v[0] )
				{
					UnityEngine.Vector3* ps = v0;
					fixed ( Vector3* v1 = &o[0] )
					{
						Vector3* pt = v1;
						for ( int i = 0; i < count; i++ )
						{
							*pt = *( Vector3* )ps;
							++ps;
							++pt;
						}
					}
				}
			}
			return o;
		}
	}
}