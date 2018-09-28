using UnityEngine;

namespace FairyUGUI.Core
{
	public enum BlendMode
	{
		Normal,
		None,
		Add,
		Multiply,
		Screen,
		Erase,
		Mask,
		Below,
		Custom1,
		Custom2,
		Custom3
	}

	public class NMaterial
	{
		//Source指的是被计算的颜色，Destination是已经在屏幕上的颜色。
		//混合结果=Source * factor1 + Destination * factor2
		private static readonly float[] FACTORS =
		{
			//Normal
			(float)UnityEngine.Rendering.BlendMode.SrcAlpha,
			(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha,

			//None
			(float)UnityEngine.Rendering.BlendMode.One,
			(float)UnityEngine.Rendering.BlendMode.Zero,

			//Add
			(float)UnityEngine.Rendering.BlendMode.One,
			(float)UnityEngine.Rendering.BlendMode.One,

			//Multiply
			(float)UnityEngine.Rendering.BlendMode.DstColor,
			(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha,

			//Screen
			(float)UnityEngine.Rendering.BlendMode.One,
			(float)UnityEngine.Rendering.BlendMode.OneMinusSrcColor,

			//Erase
			(float)UnityEngine.Rendering.BlendMode.Zero,
			(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha,

			//Mask
			(float)UnityEngine.Rendering.BlendMode.Zero,
			(float)UnityEngine.Rendering.BlendMode.SrcAlpha,

			//Below
			(float)UnityEngine.Rendering.BlendMode.OneMinusDstAlpha,
			(float)UnityEngine.Rendering.BlendMode.DstAlpha
		};

		internal readonly Material material;

		internal MaterialManager.KeywordFlag keywordFlag;
		internal MaterialPool pool;

		public string[] shaderKeywords
		{
			set => this.material.shaderKeywords = value;
			get => this.material.shaderKeywords;
		}

		public NMaterial( Shader shader )
		{
			this.material = new Material( shader );
		}

		public void DisableKeyword( string keyword )
		{
			this.material.DisableKeyword( keyword );
		}

		public void EnableKeyword( string keyword )
		{
			this.material.EnableKeyword( keyword );
		}

		public Color GetColor( int nameID )
		{
			return this.material.GetColor( nameID );
		}

		public Color GetColor( string propertyName )
		{
			return this.material.GetColor( propertyName );
		}

		public float GetFloat( int nameID )
		{
			return this.material.GetFloat( nameID );
		}

		public float GetFloat( string propertyName )
		{
			return this.material.GetFloat( propertyName );
		}

		public int GetInt( int nameID )
		{
			return this.material.GetInt( nameID );
		}

		public int GetInt( string propertyName )
		{
			return this.material.GetInt( propertyName );
		}

		public Matrix4x4 GetMatrix( int nameID )
		{
			return this.material.GetMatrix( nameID );
		}

		public Matrix4x4 GetMatrix( string propertyName )
		{
			return this.material.GetMatrix( propertyName );
		}

		public Texture GetTexture( int nameID )
		{
			return this.material.GetTexture( nameID );
		}

		public Texture GetTexture( string propertyName )
		{
			return this.material.GetTexture( propertyName );
		}

		public Vector2 GetTextureOffset( string propertyName )
		{
			return this.material.GetTextureOffset( propertyName );
		}

		public Vector2 GetTextureScale( string propertyName )
		{
			return this.material.GetTextureScale( propertyName );
		}

		public Vector4 GetVector( int nameID )
		{
			return this.material.GetVector( nameID );
		}

		public Vector4 GetVector( string propertyName )
		{
			return this.material.GetVector( propertyName );
		}

		public bool HasProperty( int nameID )
		{
			return this.material.HasProperty( nameID );
		}

		public bool HasProperty( string propertyName )
		{
			return this.material.HasProperty( propertyName );
		}

		public bool IsKeywordEnabled( string keyword )
		{
			return this.material.IsKeywordEnabled( keyword );
		}

		public void Lerp( Material start, Material end, float t )
		{
			this.material.Lerp( start, end, t );
		}

		public void SetBuffer( string propertyName, ComputeBuffer buffer )
		{
			this.material.SetBuffer( propertyName, buffer );
		}

		public void SetColor( int nameID, Color color )
		{
			this.material.SetColor( nameID, color );
		}

		public void SetColor( string propertyName, Color color )
		{
			this.material.SetColor( propertyName, color );
		}

		public void SetColorArray( int nameID, Color[] values )
		{
			this.material.SetColorArray( nameID, values );
		}

		public void SetColorArray( string name, Color[] values )
		{
			this.material.SetColorArray( name, values );
		}

		public void SetFloat( int nameID, float value )
		{
			this.material.SetFloat( nameID, value );
		}

		public void SetFloat( string propertyName, float value )
		{
			this.material.SetFloat( propertyName, value );
		}

		public void SetFloatArray( int nameID, float[] values )
		{
			this.material.SetFloatArray( nameID, values );
		}

		public void SetFloatArray( string name, float[] values )
		{
			this.material.SetFloatArray( name, values );
		}

		public void SetInt( int nameID, int value )
		{
			this.material.SetInt( nameID, value );
		}

		public void SetInt( string propertyName, int value )
		{
			this.material.SetInt( propertyName, value );
		}

		public void SetMatrix( int nameID, Matrix4x4 matrix )
		{
			this.material.SetMatrix( nameID, matrix );
		}

		public void SetMatrix( string propertyName, Matrix4x4 matrix )
		{
			this.material.SetMatrix( propertyName, matrix );
		}

		public void SetMatrixArray( int nameID, Matrix4x4[] values )
		{
			this.material.SetMatrixArray( nameID, values );
		}

		public void SetMatrixArray( string name, Matrix4x4[] values )
		{
			this.material.SetMatrixArray( name, values );
		}

		public bool SetPass( int pass )
		{
			return this.material.SetPass( pass );
		}

		public void SetTexture( int nameID, Texture texture )
		{
			this.material.SetTexture( nameID, texture );
		}

		public void SetTexture( string propertyName, Texture texture )
		{
			this.material.SetTexture( propertyName, texture );
		}

		public void SetTextureOffset( string propertyName, Vector2 offset )
		{
			this.material.SetTextureOffset( propertyName, offset );
		}

		public void SetTextureScale( string propertyName, Vector2 scale )
		{
			this.material.SetTextureScale( propertyName, scale );
		}

		public void SetVector( int nameID, Vector4 vector )
		{
			this.material.SetVector( nameID, vector );
		}

		public void SetVector( string propertyName, Vector4 vector )
		{
			this.material.SetVector( propertyName, vector );
		}

		public void SetVectorArray( int nameID, Vector4[] values )
		{
			this.material.SetVectorArray( nameID, values );
		}

		public void SetVectorArray( string name, Vector4[] values )
		{
			this.material.SetVectorArray( name, values );
		}

		internal void ApplyBlendMode( BlendMode blendMode )
		{
			int index = ( int )blendMode * 2;
			this.material.SetFloat( "_BlendSrcFactor", FACTORS[index] );
			this.material.SetFloat( "_BlendDstFactor", FACTORS[index + 1] );
		}

		internal void ApplyColorFilter( ColorFilter colorFilter )
		{
			this.material.SetMatrix( "_ColorMatrix", colorFilter.worldColorMatrix );
			this.material.SetVector( "_ColorOffset", colorFilter.worldColorOffset );
		}

		internal void ApplyBlurFilter( BlurFilter blurFilter )
		{
			this.material.SetFloat( "_BlurSize", blurFilter.worldSize );
		}
	}
}