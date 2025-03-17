using UnityEngine;

namespace BirdCase
{
    public class EffectTimingTool : MonoBehaviour
    {
        [Tooltip("기존의 기준점이 되는 이펙트 시간")]
        [SerializeField] private float OriginTime;

        [Tooltip("수정하고 싶은 시간")]
        [Range(1, 10)] public float FixedTime = 1;
        private float previousFixedTime;

        [Tooltip("Duration 시간을 수정할 이펙트")]
        [SerializeField] private ParticleSystem[] durationEffect;

        [Tooltip("Start Delay 시간을 수정할 이펙트")]
        [SerializeField] private ParticleSystem[] delayEffect;

        [Tooltip("LifeTime 시간을 수정할 이펙트")]
        [SerializeField] private ParticleSystem[] lifeTimeEffect;

        private void Awake()
        {
            if (durationEffect.Length == 0 && delayEffect.Length == 0 && lifeTimeEffect.Length == 0)
            {
                Debug.LogError("수정할 이펙트가 없습니다.");
                return;
            }

            if (durationEffect.Length > 0)
            {
                foreach (var effect in durationEffect)
                {
                    var main = effect.main;
                    float offset = OriginTime - main.duration;
                    main.duration = FixedTime - offset;
                }
            }

            if (delayEffect.Length > 0)
            {
                foreach (var effect in delayEffect)
                {
                    var main = effect.main;
                    float offset = OriginTime - main.startDelay.constant;
                    main.startDelay = FixedTime - offset;
                }
            }

            if (lifeTimeEffect.Length > 0)
            {
                foreach (var effect in lifeTimeEffect)
                {
                    var main = effect.main;
                    float offset = OriginTime - main.startLifetime.constant;
                    main.startLifetime = FixedTime - offset;
                }
            }

            previousFixedTime = FixedTime;
        }

        public void ChangeFixedTime()
        {
            if (previousFixedTime == FixedTime)
                return;

            if (durationEffect.Length > 0)
            {
                foreach (var effect in durationEffect)
                {
                    var main = effect.main;
                    float offset = previousFixedTime - main.duration;
                    main.duration = FixedTime - offset;
                }
            }

            if (delayEffect.Length > 0)
            {
                foreach (var effect in delayEffect)
                {
                    var main = effect.main;
                    float offset = previousFixedTime - main.startDelay.constant;
                    main.startDelay = FixedTime - offset;
                }
            }

            if (lifeTimeEffect.Length > 0)
            {
                foreach (var effect in lifeTimeEffect)
                {
                    var main = effect.main;
                    float offset = previousFixedTime - main.startLifetime.constant;
                    main.startLifetime = FixedTime - offset;
                }
            }

            previousFixedTime = FixedTime;
        }
    }
}
