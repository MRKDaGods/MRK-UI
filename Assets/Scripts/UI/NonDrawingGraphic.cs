using UnityEngine.UI;

namespace MRK.UI
{
    //credit: https://answers.unity.com/questions/1091618/ui-panel-without-image-component-as-raycast-target.html
    public class NonDrawingGraphic : Graphic
    {
        public override void SetMaterialDirty() 
        { 
        }

        public override void SetVerticesDirty() 
        { 
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}
