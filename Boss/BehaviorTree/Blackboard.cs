using UnityEngine;

namespace BirdCase
{
    [System.Serializable]
    public class Blackboard
    {
        public PlayerBase target;

        public bool isPatternProgress = false;
        public bool isSummonPatternProgress = false;
        public bool isSpecialPatternProgress = false;
        
        public int PatternCount = 0;
        public float PatternCoolTime = 0f;
        public float PatternCurrentTime { get; private set; } = float.MinValue;
        public void SetPatternCoolTime()
        {
            PatternCurrentTime = Time.time;
        }
    }
}