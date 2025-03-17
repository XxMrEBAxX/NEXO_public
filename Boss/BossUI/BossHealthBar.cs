using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TMPro;
using DG.Tweening;

namespace BirdCase
{
    public class BossHealthBar : MonoBehaviour
    {
        public BossBase Owner;
        public TextMeshProUGUI HealthLineCountText;
        public SlicedFilledImage HealthBarBackground;
        public SlicedFilledImage HealthDamaged;
        public SlicedFilledImage HealthBarFill;
        public GameObject HealthBarTextBox;

        // 그라데이션 색 처리 해줍니다.
        [SerializeField] private Color[] greenGradientColor = new Color[2];
        [SerializeField] private Color[] orangeGradientColor = new Color[2];
        [SerializeField] private Color[] redGradientColor = new Color[2];
        [SerializeField] private Color[] grayGradientColor = new Color[2];

        [SerializeField] private Color[] barColor1;
        [SerializeField] private Color[] barColor2;

        private UIGradient backgroundGradient;
        private UIGradient fillGradient;
        private RectTransform healthBarRectTransform;
        private Vector2 oriRectTransform;
        private int curBarColorIndex = 0;
        private int prevLineCount; // 이전 줄 수
        private int curLineCount; // 현재 줄 수
        private int curLineHealth; // 현재 줄의 체력
        private readonly float epsilon = 0.0001f; // 허용 오차

        private void Awake()
        {
            Owner = FindFirstObjectByType<BossBase>();
            healthBarRectTransform = GetComponent<RectTransform>();
            backgroundGradient = HealthBarBackground.GetComponent<UIGradient>();
            fillGradient = HealthBarFill.GetComponent<UIGradient>();

            oriRectTransform = healthBarRectTransform.anchoredPosition;
            backgroundGradient.SetColor(orangeGradientColor[0], orangeGradientColor[1]);
            fillGradient.SetColor(greenGradientColor[0], greenGradientColor[1]);
        }

        private void Start()
        {
            updateOpacityVoid = new UniTaskVoid();
            opacityFillSpeed = 1f / opacityFillTime;
            fillHealthBarSpeed = 1f / fillHealthBarTime;

            curLineCount = Owner.BossData.BossLineHealthCount;
            prevLineCount = curLineCount;
            curLineHealth = Owner.BossData.BossLineHealth;
            SetMultipleText(curLineCount);
        }

        /// <summary>
        /// 데미지를 들어온 이후 체력바를 설정합니다.
        /// </summary>
        /// <param name="curHealth">데미지를 받은 후 체력을 가져옵니다.</param>
        public void SetLineHealth(int curHealth, float punchYPosition = -1, int shakeStrength = 1)
        {
            if (punchYPosition == -1)
            {
                punchYPosition = Owner.BossData.HealthBarPunchYPosition;
            }

            if (shakeStrength == 1)
            {
                shakeStrength = Owner.BossData.HealthBarShakeStrength;
            }

            curLineCount = Mathf.CeilToInt((float)curHealth / Owner.BossData.BossLineHealth);
            curLineHealth = curHealth % Owner.BossData.BossLineHealth;

            if (curLineHealth == 0 && curLineCount > 0)
            {
                curLineHealth = Owner.BossData.BossLineHealth;
            }

            SetHealthBar();
            SetMultipleText(curLineCount);
            ShakeHealthBar(punchYPosition, shakeStrength);
        }

        private void ShakeHealthBar(float punchYpos, int strength)
        {
            healthBarRectTransform.DOKill();
            healthBarRectTransform.anchoredPosition = oriRectTransform;
            healthBarRectTransform.anchoredPosition = new Vector3(healthBarRectTransform.anchoredPosition.x,
                healthBarRectTransform.anchoredPosition.y, 0);

            // healthBarRectTransform
            //     .DOPunchPosition(new Vector3(0, Owner.BossData.HealthBarPunchYPosition, 0),
            //         Owner.BossData.HealthBarShakeDuration, Owner.BossData.HealthBarShakeStrength)
            //     .SetEase(Owner.BossData.HealthBarShakeEase).OnComplete(() =>
            //     {
            //         healthBarRectTransform.anchoredPosition = oriRectTransform;
            //         healthBarRectTransform.anchoredPosition = new Vector3(healthBarRectTransform.anchoredPosition.x,
            //             healthBarRectTransform.anchoredPosition.y, 0);
            //     });

            healthBarRectTransform
                .DOPunchPosition(new Vector3(0, punchYpos, 0), Owner.BossData.HealthBarShakeDuration, strength)
                .SetEase(Owner.BossData.HealthBarShakeEase).OnComplete(() =>
                {
                    healthBarRectTransform.anchoredPosition = oriRectTransform;
                    healthBarRectTransform.anchoredPosition = new Vector3(healthBarRectTransform.anchoredPosition.x,
                        healthBarRectTransform.anchoredPosition.y, 0);
                });
        }

        private void SetHealthBar()
        {
            float curFillAmount = (float)curLineHealth / Owner.BossData.BossLineHealth;
            if (curLineCount != prevLineCount)
            {
                if (Mathf.Approximately(curFillAmount, 1.0f))
                {
                    HealthBarFill.fillAmount = 0;
                    SetDamagedHealthBar(0);
                    return;
                }
                ChangeHealthBarColor();

                HealthDamaged.fillAmount = 1;
            }
            else
            {
                if (Mathf.Approximately(HealthBarFill.fillAmount, 0))
                {
                    ChangeHealthBarColor();
                }
            }

            HealthBarFill.fillAmount = curFillAmount;
            SetDamagedHealthBar(curFillAmount);
        }

        /// <summary>
        /// 잃은 체력을 하얀색으로 표시합니다.
        /// </summary>
        private void SetDamagedHealthBar(float healthBarFillAmount)
        {
            float curFillAmount = HealthDamaged.fillAmount;
            float offset = Mathf.Clamp(curFillAmount - healthBarFillAmount, 0.2f, 1);
            DOTween.To(() => curFillAmount, x => curFillAmount = x, healthBarFillAmount,
                    Owner.BossData.HealthBarDamagedDecreaseDuration * offset * TimeManager.Instance.TimeScale)
                .OnUpdate(() => { HealthDamaged.fillAmount = curFillAmount; });
        }

        private void SetMultipleText(int lineCount)
        {
            HealthLineCountText.text = "<color=white>X " + lineCount + "</color>";
        }

        #region 투명도 처리

        [SerializeField] private float opacityFillTime = 0.5f;
        [SerializeField] private float fillHealthBarTime = 1;
        [SerializeField] private float lineIncreaseTextTime = 0.5f;

        private UniTaskVoid updateOpacityVoid;
        private float opacityFillSpeed;
        private float curOpacityValue;
        private float fillHealthBarSpeed;
        private float curFillHealthBarValue;
        private float lineIncreaseTextSpeed;
        private int curLineTextCount;
        private float accumulatedIncrease;

        /// <summary>
        /// 보스의 체력을 다시 회복시키기 위해 체력바를 투명하게 만들어줍니다.
        /// </summary>
        public void DecreaseOpacity(int curHealth)
        {
            updateOpacityVoid = UpdateOpacity(curHealth);
            updateOpacityVoid.Forget();
        }

        private void InitOpacity()
        {
            curOpacityValue = 0f;
            curFillHealthBarValue = 0f;
            curLineTextCount = 0;
            accumulatedIncrease = 0f;

            backgroundGradient.SetColor(orangeGradientColor[0], orangeGradientColor[1]);
            fillGradient.SetColor(greenGradientColor[0], greenGradientColor[1]);

            HealthBarFill.fillAmount = 0;

            HealthDamaged.gameObject.SetActive(false);
            HealthBarTextBox.SetActive(true);
        }

        /// <summary>
        /// 투명도를 시간 변수에 따라 증가 시킵니다.
        /// </summary>
        private async UniTaskVoid UpdateOpacity(int curHealth)
        {
            InitOpacity();

            int setLineCount = curHealth == Owner.BossData.BossHealth
                ? Owner.BossData.BossLineHealthCount
                : Mathf.CeilToInt((float)curHealth / Owner.BossData.BossLineHealth);

            lineIncreaseTextSpeed = setLineCount / lineIncreaseTextTime;
            prevLineCount = setLineCount;

            while (Math.Abs(curFillHealthBarValue - 1) > epsilon)
            {
                curOpacityValue += opacityFillSpeed * Time.deltaTime;
                curOpacityValue = Mathf.Clamp(curOpacityValue, 0, 1);

                curFillHealthBarValue += fillHealthBarSpeed * Time.deltaTime;
                curFillHealthBarValue = Mathf.Clamp(curFillHealthBarValue, 0, 1);

                accumulatedIncrease += lineIncreaseTextSpeed * Time.deltaTime;
                if (accumulatedIncrease >= 1)
                {
                    int increaseAmount = (int)accumulatedIncrease;
                    curLineTextCount += increaseAmount;
                    accumulatedIncrease -= increaseAmount;
                    curLineTextCount = Mathf.Clamp(curLineTextCount, 0, setLineCount);

                    SetMultipleText(curLineTextCount);
                }

                if (Math.Abs(curFillHealthBarValue - 1) < epsilon)
                {
                    HealthDamaged.gameObject.SetActive(true);
                }

                HealthBarBackground.color = new Color(
                    HealthBarBackground.color.r,
                    HealthBarBackground.color.g,
                    HealthBarBackground.color.b,
                    curOpacityValue);

                HealthBarFill.color = new Color(
                    HealthBarFill.color.r,
                    HealthBarFill.color.g,
                    HealthBarFill.color.b,
                    curOpacityValue);

                HealthBarFill.fillAmount = curFillHealthBarValue;

                HealthLineCountText.color = new Color(
                    HealthLineCountText.color.r,
                    HealthLineCountText.color.g,
                    HealthLineCountText.color.b,
                    curOpacityValue);

                await UniTask.Yield();
            }

            SetMultipleText(setLineCount); // 마지막에 정확한 값을 설정해줍니다.
        }

        #endregion

        private void ChangeHealthBarColor()
        {
            fillGradient.SetColor(backgroundGradient.GetFirstColor(), backgroundGradient.GetSecondColor());

            while (curLineCount < prevLineCount)
            {
                switch (prevLineCount)
                {
                    case 1:
                        backgroundGradient.SetColor(grayGradientColor[0], grayGradientColor[1]);
                        HealthBarTextBox.SetActive(false);
                        curBarColorIndex = 0;
                        break;
                    case 2:
                        backgroundGradient.SetColor(grayGradientColor[0], grayGradientColor[1]);
                        HealthBarTextBox.SetActive(false);
                        break;
                    case 3:
                        backgroundGradient.SetColor(redGradientColor[0], redGradientColor[1]);
                        break;
                    case 4:
                        backgroundGradient.SetColor(orangeGradientColor[0], orangeGradientColor[1]);
                        break;
                    default:
                        backgroundGradient.SetColor(barColor1[curBarColorIndex], barColor2[curBarColorIndex]);

                        if (curBarColorIndex >= barColor1.Length - 1)
                        {
                            curBarColorIndex = 0;
                        }
                        else
                        {
                            curBarColorIndex++;
                        }

                        break;
                }

                prevLineCount--;
            }

            prevLineCount = curLineCount;
        }

        #region SetArraySize

        private void OnValidate()
        {
            EnsureArraySize(ref greenGradientColor);
            EnsureArraySize(ref orangeGradientColor);
            EnsureArraySize(ref redGradientColor);
            EnsureArraySize(ref grayGradientColor);
        }

        private void EnsureArraySize(ref Color[] colorArray)
        {
            if (colorArray.Length != 2)
            {
                Array.Resize(ref colorArray, 2);
            }
        }

        #endregion
    }
}