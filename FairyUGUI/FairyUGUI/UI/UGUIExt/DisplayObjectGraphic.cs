using UnityEngine.UI;

namespace FairyUGUI.UI.UGUIExt
{
	public class DisplayObjectGraphic : MaskableGraphic, ILayoutElement
	{
		public override void SetMaterialDirty()
		{
		}

		public override void SetVerticesDirty()
		{
		}

		protected override void UpdateGeometry()
		{
		}

		protected override void OnPopulateMesh( VertexHelper vh )
		{
			vh.Clear();
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