using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BirdCase
{
    public class AnimatedEntry : MonoBehaviour
    {
        [Space(10)]
        [Header("Bools")]
        [SerializeField]
        private bool animateOnStart = false;

        [SerializeField]
        private bool animateOnEnabled = false;

        [SerializeField]
        private bool offset = false;

        [SerializeField]
        private bool movedByRect = false;

        [Space(10)]
        [Header("Timing")]
        [SerializeField]
        private float delay = 0;

        [SerializeField]
        private float effectTime = 1;

        [SerializeField]
        private bool isLoop = false;

        [Space(10)]
        [Header("Scale")]
        [SerializeField]
        private Vector3 startScale;

        [SerializeField]
        private AnimationCurve scaleCurve;


        [Space(10)]
        [Header("Position")]
        [SerializeField]
        private Vector3 startPos;

        [SerializeField]
        private AnimationCurve posCurve;

        private RectTransform rectTransform;
        
        private Vector3 endScale;
        private Vector3 endPos;
        
        private bool isStop = false;

        private void Awake()
        {
            if(movedByRect)
                rectTransform = GetComponent<RectTransform>();
            if (animateOnEnabled)
                animateOnStart = false;
            SetupVariables();
        }

        private void Start()
        {
            if (animateOnStart)
            {
                if (isLoop)
                {
                    LoopAnim().Forget();
                }
                else
                {
                    Animation().Forget();
                }
            }
        }

        private void OnEnable()
        {
            if (animateOnEnabled)
            {
                if (isLoop)
                {
                    LoopAnim().Forget();
                }
                else
                {
                    Animation().Forget();
                }
            }
        }


        void SetupVariables()
        {
            endScale = transform.localScale;
            endPos = transform.localPosition;
            if (offset)
            {
                startPos += endPos;
            }
        }

        public void Play()
        {
            if (isLoop)
            {
                LoopAnim().Forget();
            }
            else
            {
                Animation().Forget();
            }
        }

        public void Stop()
        {
            isStop = false;
        }

        private async UniTaskVoid Animation()
        {
            transform.localPosition = startPos;
            transform.localScale = startScale;
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: this.GetCancellationTokenOnDestroy());
            float time = 0;
            float perc = 0;
            float lastTime = Time.realtimeSinceStartup;
            do
            {
                if (isStop)
                {
                    isStop = false;
                    break;
                }

                time += Time.realtimeSinceStartup - lastTime;
                lastTime = Time.realtimeSinceStartup;
                perc = Mathf.Clamp01(time / effectTime);
                Vector3 tempScale = Vector3.LerpUnclamped(startScale, endScale, scaleCurve.Evaluate(perc));
                Vector3 tempPos = Vector3.LerpUnclamped(startPos, endPos, posCurve.Evaluate(perc));
                if (!movedByRect)
                {
                    transform.localScale = tempScale;
                    transform.localPosition = tempPos;
                }
                else
                {
                    rectTransform.localScale = tempScale;
                    rectTransform.position = tempPos;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: this.GetCancellationTokenOnDestroy());
            } while (perc < 1);

            if (!movedByRect)
            {
                transform.localScale = endScale;
                transform.localPosition = endPos;
            }
            else
            {
                rectTransform.localScale = endScale;
                rectTransform.position = endPos;
            }
        }

        private async UniTaskVoid LoopAnim()
        {
            transform.localPosition = startPos;
            transform.localScale = startScale;
            float time = 0;
            float perc = 0;
            float lastTime = Time.realtimeSinceStartup;
            do
            {
                if (isStop)
                    break;
                
                time += Time.realtimeSinceStartup - lastTime;
                lastTime = Time.realtimeSinceStartup;
                perc = Mathf.Clamp01(time / effectTime);
                Vector3 tempScale = Vector3.LerpUnclamped(startScale, endScale, scaleCurve.Evaluate(perc));
                Vector3 tempPos = Vector3.LerpUnclamped(startPos, endPos, posCurve.Evaluate(perc));
                transform.localScale = tempScale;
                transform.localPosition = tempPos;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: this.GetCancellationTokenOnDestroy());
            } while (perc < 1);

            transform.localScale = endScale;
            transform.localPosition = endPos;
            time = 0;
            perc = 0;
            lastTime = Time.realtimeSinceStartup;

            if (isStop)
            {
                isStop = false;
                return;
            }

            do
            {
                if (isStop)
                {
                    isStop = false;
                    return;
                }

                time += Time.realtimeSinceStartup - lastTime;
                lastTime = Time.realtimeSinceStartup;
                perc = 1 - Mathf.Clamp01(time / effectTime);
                Vector3 tempScale = Vector3.LerpUnclamped(startScale, endScale, scaleCurve.Evaluate(perc));
                Vector3 tempPos = Vector3.LerpUnclamped(startPos, endPos, posCurve.Evaluate(perc));
                transform.localScale = tempScale;
                transform.localPosition = tempPos;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: this.GetCancellationTokenOnDestroy());
            } while (perc > 0);

            LoopAnim().Forget();
        }
    }
}
