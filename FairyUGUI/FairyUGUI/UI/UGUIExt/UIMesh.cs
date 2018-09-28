using UnityEngine;
using UnityEngine.UI;

namespace FairyUGUI.UI.UGUIExt
{
	public class UIMesh : MaskableGraphic, ILayoutElement
	{
		private Mesh _mesh;
		public Mesh mesh
		{
			get => this._mesh;
			set
			{
				if ( this._mesh == value )
					return;
				this._mesh = value;
				this.SetVerticesDirty();
			}
		}

		public override Texture mainTexture => this.material.mainTexture;

		protected override void OnPopulateMesh( VertexHelper vh )
		{
			Vector3[] vertices = this._mesh.vertices;
			int[] triangles = this._mesh.triangles;
			Vector3[] normals = this._mesh.normals;
			Vector2[] uv = this._mesh.uv;

			Vector3 boundSize = this._mesh.bounds.size;
			float meshSize = Mathf.Max( boundSize.x, boundSize.y, boundSize.z );
			float rectSize = Mathf.Max( this.rectTransform.rect.size.x, this.rectTransform.rect.size.y );
			float scale = rectSize / meshSize;

			vh.Clear();
			int count = vertices.Length;
			for ( int i = 0; i < count; i++ )
				vh.AddVert( vertices[i] * scale, this.color, uv[i], Vector2.zero, normals[i], Vector4.zero );

			count = triangles.Length;
			for ( int i = 0; i < count; i += 3 )
				vh.AddTriangle( triangles[i], triangles[i + 1], triangles[i + 2] );
		}

		public void CalculateLayoutInputHorizontal()
		{
		}

		public void CalculateLayoutInputVertical()
		{
		}

		public float minWidth => 0;

		public float preferredWidth => this.rectTransform.sizeDelta.x;

		public float flexibleWidth => -1;

		public float minHeight => 0;

		public float preferredHeight => this.rectTransform.sizeDelta.y;

		public float flexibleHeight => -1;

		public int layoutPriority => 0;
	}
}