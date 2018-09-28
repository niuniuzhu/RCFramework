using UnityEngine;

namespace FairyUGUI.Core
{
	public struct ColorFilter
	{
		const float LUMA_R = 0.299f;
		const float LUMA_G = 0.587f;
		const float LUMA_B = 0.114f;

		public readonly float brightness;
		public readonly float saturation;
		public readonly float contrast;
		public readonly float hue;

		internal Matrix4x4 colorMatrix;
		internal Vector4 colorOffset;

		internal Matrix4x4 worldColorMatrix;
		internal Vector4 worldColorOffset;

		public bool isIdentity => this.worldColorMatrix.isIdentity && this.worldColorOffset == Vector4.zero;

		public ColorFilter( ColorFilter colorFilter )
		{
			this.brightness = colorFilter.brightness;
			this.contrast = colorFilter.contrast;
			this.saturation = colorFilter.saturation;
			this.hue = colorFilter.hue;
			this.colorMatrix = colorFilter.colorMatrix;
			this.colorOffset = colorFilter.colorOffset;
			this.worldColorMatrix = colorFilter.worldColorMatrix;
			this.worldColorOffset = colorFilter.worldColorOffset;
		}

		public ColorFilter( float brightness = 0f, float contrast = 0f, float saturation = 0f, float hue = 0f )
		{
			this.brightness = brightness;
			this.contrast = contrast;
			this.saturation = saturation;
			this.hue = hue;
			this.colorMatrix = this.worldColorMatrix = Matrix4x4.identity;
			this.colorOffset = this.worldColorOffset = Vector4.zero;
			this.SetValues( brightness, contrast, saturation, hue );
		}

		private void SetValues( float brightness = 0f, float contrast = 0f, float saturation = 0f, float hue = 0f )
		{
			this.Reset();
			this.AdjustBrightness( brightness );
			this.AdjustSaturation( saturation );
			this.AdjustContrast( contrast );
			this.AdjustHue( hue );
		}

		/// <summary>
		/// Changes the filter matrix back to the identity matrix
		/// </summary>
		internal void Reset()
		{
			this.worldColorMatrix = this.colorMatrix = Matrix4x4.identity;
			this.worldColorOffset = this.colorOffset = Vector4.zero;
		}

		private void Invert()
		{
			this.colorMatrix = this.colorMatrix.inverse;
			this.worldColorMatrix = this.colorMatrix;
		}

		private void Transpose()
		{
			this.colorMatrix = this.colorMatrix.transpose;
			this.worldColorMatrix = this.colorMatrix;
		}

		/// <summary>
		/// Changes the brightness. Typical values are in the range (-1, 1).
		/// Values above zero will make the image brighter, values below zero will make it darker.
		/// </summary>
		/// <param name="value"></param>
		private void AdjustBrightness( float value )
		{
			this.colorOffset += new Vector4( value, value, value, 0 );
			this.worldColorOffset = this.colorOffset;
		}

		/// <summary>
		/// Changes the saturation. Typical values are in the range (-1, 1).
		/// Values above zero will raise, values below zero will reduce the saturation.
		/// '-1' will produce a grayscale image. 
		/// </summary>
		/// <param name="sat"></param>
		private void AdjustSaturation( float sat )
		{
			sat += 1;

			float invSat = 1 - sat;
			float invLumR = invSat * LUMA_R;
			float invLumG = invSat * LUMA_G;
			float invLumB = invSat * LUMA_B;

			Matrix4x4 m = Matrix4x4.identity;
			m[0, 0] = invLumR + sat;
			m[0, 1] = invLumG;
			m[0, 2] = invLumB;
			m[1, 0] = invLumR;
			m[1, 1] = invLumG + sat;
			m[1, 2] = invLumB;
			m[2, 0] = invLumR;
			m[2, 1] = invLumG;
			m[2, 2] = invLumB + sat;
			this.colorMatrix = m * this.colorMatrix;
			this.worldColorMatrix = this.colorMatrix;
		}

		/// <summary>
		/// Changes the contrast. Typical values are in the range (-1, 1).
		/// Values above zero will raise, values below zero will reduce the contrast.
		/// </summary>
		/// <param name="value"></param>
		private void AdjustContrast( float value )
		{
			float s = value + 1;
			float o = 128f / 255 * ( 1 - s );

			Matrix4x4 m = Matrix4x4.identity;
			m[0, 0] = s;
			m[1, 1] = s;
			m[2, 2] = s;
			this.colorMatrix = m * this.colorMatrix;
			this.colorOffset += new Vector4( o, o, o, 0 );
			this.worldColorMatrix = this.colorMatrix;
			this.worldColorOffset = this.colorOffset;
		}

		/// <summary>
		///Changes the hue of the image. Typical values are in the range (-1, 1).
		/// </summary>
		/// <param name="value"></param>
		private void AdjustHue( float value )
		{
			value *= Mathf.PI;

			float cos = Mathf.Cos( value );
			float sin = Mathf.Sin( value );

			Matrix4x4 m = Matrix4x4.identity;
			m[0, 0] = ( LUMA_R + ( cos * ( 1 - LUMA_R ) ) ) + ( sin * -( LUMA_R ) );
			m[0, 1] = ( LUMA_G + ( cos * -( LUMA_G ) ) ) + ( sin * -( LUMA_G ) );
			m[0, 2] = ( LUMA_B + ( cos * -( LUMA_B ) ) ) + ( sin * ( 1 - LUMA_B ) );
			m[1, 0] = ( LUMA_R + ( cos * -( LUMA_R ) ) ) + ( sin * 0.143f );
			m[1, 1] = ( LUMA_G + ( cos * ( 1 - LUMA_G ) ) ) + ( sin * 0.14f );
			m[1, 2] = ( LUMA_B + ( cos * -( LUMA_B ) ) ) + ( sin * -0.283f );
			m[2, 0] = ( LUMA_R + ( cos * -( LUMA_R ) ) ) + ( sin * -( ( 1 - LUMA_R ) ) );
			m[2, 1] = ( LUMA_G + ( cos * -( LUMA_G ) ) ) + ( sin * LUMA_G );
			m[2, 2] = ( LUMA_B + ( cos * ( 1 - LUMA_B ) ) ) + ( sin * LUMA_B );
			this.colorMatrix = m * this.colorMatrix;
			this.worldColorMatrix = this.colorMatrix;
		}

		/// <summary>
		/// Tints the image in a certain color, analog to what can be done in Adobe Animate.
		/// </summary>
		/// <param name="color">the RGB color with which the image should be tinted.</param>
		/// <param name="amount">the intensity with which tinting should be applied. Range (0, 1).</param>
		private void Tint( Color color, float amount = 1.0f )
		{
			float q = 1 - amount;

			float rA = amount * color.r;
			float gA = amount * color.g;
			float bA = amount * color.b;

			Matrix4x4 m = Matrix4x4.identity;
			m[0, 0] = q + rA * LUMA_R;
			m[0, 1] = rA * LUMA_G;
			m[0, 2] = rA * LUMA_B;
			m[1, 0] = gA * LUMA_R;
			m[1, 1] = q + gA * LUMA_G;
			m[1, 2] = gA * LUMA_B;
			m[2, 0] = bA * LUMA_R;
			m[2, 1] = bA * LUMA_G;
			m[2, 2] = q + bA * LUMA_B;
			this.colorMatrix = m * this.colorMatrix;
			this.worldColorMatrix = this.colorMatrix;
		}

		public static ColorFilter operator *( ColorFilter lhs, ColorFilter rhs )
		{
			lhs.worldColorMatrix = rhs.worldColorMatrix * lhs.colorMatrix;
			lhs.worldColorOffset = rhs.worldColorOffset + lhs.colorOffset;
			return lhs;
		}

		public static bool operator ==( ColorFilter lhs, ColorFilter rhs )
		{
			return lhs.colorMatrix == rhs.colorMatrix && lhs.colorOffset == rhs.colorOffset &&
				lhs.worldColorMatrix == rhs.worldColorMatrix && lhs.worldColorOffset == rhs.worldColorOffset;
		}

		public static bool operator !=( ColorFilter lhs, ColorFilter rhs )
		{
			return !( lhs == rhs );
		}

		public override bool Equals( object other )
		{
			if ( !( other is ColorFilter ) )
				return false;

			ColorFilter obj = ( ColorFilter )other;
			return this.colorMatrix == obj.colorMatrix && this.colorOffset == obj.colorOffset &&
				this.worldColorMatrix == obj.worldColorMatrix && this.worldColorOffset == obj.worldColorOffset;
		}

		public override int GetHashCode()
		{
			float calculation = this.brightness + this.saturation + this.saturation + this.hue;
			return calculation.GetHashCode();
		}
	}
}