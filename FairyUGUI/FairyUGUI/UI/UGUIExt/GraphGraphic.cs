using UnityEngine;
using UnityEngine.UI;

namespace FairyUGUI.UI.UGUIExt
{
	public class GraphGraphic : MaskableGraphic, ILayoutElement
	{
		public enum Type
		{
			Rect,
			Ellipse
		}

		public float lineSize = 1f;
		public Color lineColor = Color.black;
		public bool enableDraw;

		private Type _type;
		public Type type
		{
			get => this._type;
			set
			{
				if ( this._type == value )
					return;
				this._type = value;
				this.SetVerticesDirty();
			}
		}

		public override Texture mainTexture => s_WhiteTexture;

		protected override void OnPopulateMesh( VertexHelper vh )
		{
			vh.Clear();

			if ( !this.enableDraw )
				return;

			switch ( this._type )
			{
				case Type.Rect:
					this.DrawRect( vh );
					break;

				case Type.Ellipse:
					this.DrawEllipse( vh );
					break;
			}
		}

		private void DrawEllipse( VertexHelper vh )
		{
			Rect r = this.GetPixelAdjustedRect();
			float radiusX = r.width * 0.5f;
			float radiusY = r.height * 0.5f;
			int numSides = Mathf.Max( 6, Mathf.CeilToInt( Mathf.PI * ( radiusX + radiusY ) / 12 ) );

			float angleDelta = 2 * Mathf.PI / numSides;
			float angle = 0;

			vh.AddVert( new UIVertex { position = new Vector3( radiusX, -radiusY ), color = this.color } );
			for ( int i = 1; i <= numSides; i++ )
			{
				vh.AddVert( new UIVertex
				{
					position = new Vector3( Mathf.Cos( angle ) * radiusX + radiusX, Mathf.Sin( angle ) * radiusY - radiusY, 0 ),
					color = this.color
				} );
				angle += angleDelta;
			}
			for ( int i = 1; i < numSides; i++ )
				vh.AddTriangle( i, i + 1, 0 );
			vh.AddTriangle( numSides, 1, 0 );
		}

		private void DrawRect( VertexHelper vh )
		{
			Rect r = this.GetPixelAdjustedRect();
			Vector4 vo = new Vector4( r.x, r.y, r.x + r.width, r.y + r.height );

			if ( this.lineSize == 0 )
			{
				vh.AddVert( new UIVertex { position = new Vector3( vo.x, vo.y ), color = this.color } );
				vh.AddVert( new UIVertex { position = new Vector3( vo.x, vo.w ), color = this.color } );
				vh.AddVert( new UIVertex { position = new Vector3( vo.z, vo.w ), color = this.color } );
				vh.AddVert( new UIVertex { position = new Vector3( vo.z, vo.y ), color = this.color } );

				vh.AddTriangle( 0, 1, 2 );
				vh.AddTriangle( 2, 3, 0 );
			}
			else
			{
				Vector4 vi = new Vector4( r.x + this.lineSize, r.y + this.lineSize, r.x + r.width - this.lineSize, r.y + r.height - this.lineSize );

				//outter
				vh.AddVert( new UIVertex { position = new Vector3( vo.x, vo.y ), color = this.lineColor } );
				vh.AddVert( new UIVertex { position = new Vector3( vo.x, vo.w ), color = this.lineColor } );
				vh.AddVert( new UIVertex { position = new Vector3( vo.z, vo.w ), color = this.lineColor } );
				vh.AddVert( new UIVertex { position = new Vector3( vo.z, vo.y ), color = this.lineColor } );
				vh.AddVert( new UIVertex { position = new Vector3( vi.x, vi.y ), color = this.lineColor } );
				vh.AddVert( new UIVertex { position = new Vector3( vi.x, vi.w ), color = this.lineColor } );
				vh.AddVert( new UIVertex { position = new Vector3( vi.z, vi.w ), color = this.lineColor } );
				vh.AddVert( new UIVertex { position = new Vector3( vi.z, vi.y ), color = this.lineColor } );

				//inner
				vh.AddVert( new UIVertex { position = new Vector3( vi.x, vi.y ), color = this.color } );
				vh.AddVert( new UIVertex { position = new Vector3( vi.x, vi.w ), color = this.color } );
				vh.AddVert( new UIVertex { position = new Vector3( vi.z, vi.w ), color = this.color } );
				vh.AddVert( new UIVertex { position = new Vector3( vi.z, vi.y ), color = this.color } );

				vh.AddTriangle( 8, 9, 10 );
				vh.AddTriangle( 10, 11, 8 );

				vh.AddTriangle( 5, 1, 2 );
				vh.AddTriangle( 2, 6, 5 );
				vh.AddTriangle( 6, 2, 3 );
				vh.AddTriangle( 3, 7, 6 );
				vh.AddTriangle( 0, 4, 7 );
				vh.AddTriangle( 7, 3, 0 );
				vh.AddTriangle( 1, 5, 4 );
				vh.AddTriangle( 4, 0, 1 );
			}
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