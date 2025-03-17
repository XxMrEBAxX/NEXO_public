using UnityEngine;

namespace BirdCase
{
    public class EffectTimingToolManager : MonoBehaviour
    {
        [SerializeField] private EffectTimingTool[] effectTimingTools;

        /// <summary>
        /// Awake 단계와 이전 단계에서 호출하지 마십쇼
        /// <!summary>
        public void SetFixedTime(float time)
        {
            foreach (var effectTimingTool in effectTimingTools)
            {
                effectTimingTool.FixedTime = time;
                effectTimingTool.ChangeFixedTime();
            }
        }
    }
}
