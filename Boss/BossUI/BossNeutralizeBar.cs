using UnityEngine;
using UnityEngine.UI;

namespace BirdCase
{
    public class BossNeutralizeBar : MonoBehaviour
    {
        public Image NeutralizeBarFill;
        private Color originColor;
        
        private void Start()
        {
            originColor = NeutralizeBarFill.color;
        }

        public void SetNeutralizeBar(int curNeutralize, int maxNeutralize)
        {
            NeutralizeBarFill.fillAmount = (float)curNeutralize / maxNeutralize;
        }

        public void ChangeNeutralizeBarColorBlack()
        {
            NeutralizeBarFill.color = Color.black;
        }

        public void ChangeNeutralizeBarColorOrigin()
        {
            NeutralizeBarFill.color = originColor;
        }
    }
}
