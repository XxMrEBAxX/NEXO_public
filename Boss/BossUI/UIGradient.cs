using UnityEngine;
using UnityEngine.UI;

namespace BirdCase
{
    [AddComponentMenu("UI/Effects/Gradient")]
    public class UIGradient : BaseMeshEffect
    {
        [SerializeField] private Color color1 = Color.white;
        [SerializeField] private Color color2 = Color.white;
        //[Range(-180f, 180f)]
        private float angle = -90f; 
        private bool ignoreRatio = true;

        public void SetColor(Color firstColor, Color secondColor)
        {
            color1 = firstColor;
            color2 = secondColor;
            
            if (graphic != null)
            {
                graphic.SetVerticesDirty();
            }
        }
        
        public Color GetFirstColor()
        {
            return color1;
        }
        
        public Color GetSecondColor()
        {
            return color2;
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (enabled)
            {
                Rect rect = graphic.rectTransform.rect;
                Vector2 dir = UIGradientUtils.RotationDir(angle);

                if (!ignoreRatio)
                    dir = UIGradientUtils.CompensateAspectRatio(rect, dir);

                UIGradientUtils.Matrix2x3 localPositionMatrix = UIGradientUtils.LocalPositionMatrix(rect, dir);

                UIVertex vertex = default(UIVertex);
                for (int i = 0; i < vh.currentVertCount; i++)
                {
                    vh.PopulateUIVertex(ref vertex, i);
                    Vector2 localPosition = localPositionMatrix * vertex.position;
                    vertex.color *= Color.Lerp(color2, color1, localPosition.y);
                    vh.SetUIVertex(vertex, i);
                }
            }
        }
    }
}
