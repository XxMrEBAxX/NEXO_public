using System;
using AssetKits.ParticleImage;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BirdCase
{
    public class HPBarUI : MonoBehaviour
    {
        [SerializeField]
        private Vector2 hpHealScale = new Vector2(1.5f, 1.5f);
        [SerializeField]
        private AnimationCurve hpScaleCurve;
        
        private UIHologram hpObject;
        
        private SlicedFilledImage shieldSlider;
        
        private UIHologram[] hpObjects;
        private ParticleImage[] hpParticles;
        private Vector2[] hpOriginScale;
        private int currentHP;
        private int maxHP;

        private Color hpColor;
        private Color emptyColor;

        private void Awake()
        {
            hpObject = transform.GetChild(1).GetChild(0).GetComponent<UIHologram>();
            shieldSlider = transform.GetChild(0).GetComponent<SlicedFilledImage>();
        }

        public void InitHP(int currentHP, int maxHP)
        {
            if (hpObject.transform.parent.childCount > 1)
            {
                for (int i = 1; i < hpObject.transform.parent.childCount; i++)
                {
                    Destroy(hpObject.transform.parent.GetChild(i).gameObject);
                }
            }

            this.maxHP = maxHP;
            this.currentHP = maxHP;
            hpObjects = new UIHologram[maxHP];
            hpParticles = new ParticleImage[maxHP];
            hpOriginScale = new Vector2[maxHP];
            hpObjects[0] = hpObject;
            hpParticles[0] = hpObject.GetComponentInChildren<ParticleImage>();
            hpOriginScale[0] = hpObjects[0].rectTransform.localScale;
            for(int i = 1; i < maxHP; i++)
            {
                hpObjects[i] = Instantiate(hpObject, hpObject.transform.parent);
                hpParticles[i] = hpObjects[i].GetComponentInChildren<ParticleImage>();
                hpOriginScale[i] = hpObjects[i].rectTransform.localScale;
            }

            hpColor = hpObject.TextureColor;
            emptyColor = shieldSlider.transform.parent.GetComponent<UIHologram>().TextureColor;
        }

        public void SetHP(int currentHP, int maxHP)
        {
            if (this.currentHP <= currentHP)
                return;
            
            int cur = currentHP;
            int max = this.currentHP;
            
            for(int i = cur; i < max; i++)
            {
                hpObjects[i].TextureColor = emptyColor;
                hpParticles[i].Play();
            }
            this.currentHP = currentHP;
        }

        public void Heal(float time)
        {
            HealHP(time).Forget();
        }
        
        private async UniTaskVoid HealHP(float time)
        {
            float duration = time / hpObjects.Length;
            for (int i = 0; i < hpObjects.Length; i++)
            {
                ChangeScale(hpObjects[i].rectTransform, hpOriginScale[i], hpHealScale, duration).Forget();
                hpObjects[i].TextureColor = hpColor;
                
                await UniTask.Delay(TimeSpan.FromSeconds(duration));
            }
            currentHP = maxHP;
        }

        private async UniTaskVoid ChangeScale(RectTransform rect, Vector2 oriScale, Vector2 scale, float duration)
        {
            float time = 0;
            while (time < duration)
            {
                time += TimeManager.Instance.GetUnscaledDeltaTime();
                
                rect.localScale = Vector2.LerpUnclamped(oriScale, scale, hpScaleCurve.Evaluate((time / duration) * 2));
                
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken:this.GetCancellationTokenOnDestroy());;
            }

            rect.localScale = new Vector3(oriScale.x, oriScale.y, 1);
        }
        
        public void SetShield(float shield)
        {
            shieldSlider.fillAmount = 1 - shield;
        }
    }
}
