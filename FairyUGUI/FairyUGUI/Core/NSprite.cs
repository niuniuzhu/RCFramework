using UnityEngine;

namespace FairyUGUI.Core
{
	public class NSprite
	{
		public string name => this.sprite.name;

		public NTexture nTexture { get; private set; }

		public Sprite sprite { get; private set; }

		public bool packed => this.sprite.packed;

		public Vector4 border => this.sprite.border;

		public Bounds bounds => this.sprite.bounds;

		public Vector2 pivot => this.sprite.pivot;

		public float pixelsPerUnit => this.sprite.pixelsPerUnit;

		public Rect rect => this.sprite.rect;

		public Rect textureRect => this.sprite.textureRect;

		public Vector2 textureRectOffset => this.sprite.textureRectOffset;

		public ushort[] triangles => this.sprite.triangles;

		public Vector2[] uv => this.sprite.uv;

		public Vector2[] vertices => this.sprite.vertices;

		public Texture2D associatedAlphaSplitTexture => this.nTexture.associatedAlphaSplitTexture;

		public NSprite( NTexture nTexture, Sprite sprite )
		{
			this.nTexture = nTexture;
			this.sprite = sprite;
		}
	}
}