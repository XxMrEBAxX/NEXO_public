using System;
using UnityEngine;
using AssetKits.ParticleImage;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine.UI;

namespace BirdCase
{
    public class LauncherUI : MonoBehaviour
    {
        private readonly Vector2 RELOAD_OFFSET = new Vector2(0, -200);
        private readonly Vector2 RELOAD_COMPLETE_OFFSET = new Vector2(0, 50);
        
        [SerializeField]
        private AnimationCurve reloadCurve;
        
        private Image specialBullet;
        private ParticleImage specialBulletParticle;
        private UIHologram bullet;
        
        private UIHologram[] bullets = null;
        private ParticleImage[] bulletParticles;
        private RectTransform[] bulletsRect;
        private Vector2[] bulletsOriginPos;
        
        private int currentBulletCount;
        private bool isReload = false;
        private int reloadIndex = 0;

        private Color oriColor;
        private Color emptyColor;

        private void Awake()
        {
            specialBullet = transform.GetChild(0).GetChild(0).GetComponent<Image>();
            specialBulletParticle = specialBullet.GetComponentInChildren<ParticleImage>();
            bullet = transform.GetChild(1).GetChild(0).GetComponent<UIHologram>();
        }

        public void Init(int maxBulletAmmo)
        {
            if(ReferenceEquals(bullets, null) || bullets.Length == 0)
            {
                bullets = new UIHologram[maxBulletAmmo];
                bulletParticles = new ParticleImage[maxBulletAmmo];
                bullets[0] = bullet;
                bulletParticles[0] = bullet.GetComponentInChildren<ParticleImage>();
                for (int i = 1; i < bullets.Length; i++)
                {
                    bullets[i] = Instantiate(bullet, bullet.transform.parent);
                    bulletParticles[i] = bullets[i].GetComponentInChildren<ParticleImage>();
                }
            }
            currentBulletCount = maxBulletAmmo;
            
            oriColor = bullet.TextureColor;
            emptyColor = specialBullet.transform.parent.GetComponent<UIHologram>().TextureColor;
        }
        
        public void Shot(int maxBulletAmmo, int curBulletAmmo)
        {
            bool isDecreased = currentBulletCount > curBulletAmmo;
            int cur = isDecreased ? curBulletAmmo : currentBulletCount;
            int max = isDecreased ? currentBulletCount : curBulletAmmo;
            
            for(int i = cur; i < max; i++)
            {
                bullets[i].TextureColor = isDecreased ? emptyColor : oriColor;
                if(isDecreased)
                    bulletParticles[i].Play();
            }
            currentBulletCount = curBulletAmmo;
        }
        
        public void SpecialShotCooldown(float cooldownAmount)
        {
            if(cooldownAmount == 0)
                specialBulletParticle.Play();
            specialBullet.fillAmount = cooldownAmount;
        }
        
        public void Reload(float reloadTime, bool isReload)
        {
            if (isReload)
            {
                ReloadStart(reloadTime).Forget();
            }
            else
            {
                if (reloadTime == 0)
                {
                    this.isReload = false;
                }
                else
                {
                    ReloadAnimation(reloadTime).Forget();
                }

                reloadIndex = 0;
            }
        }

        private async UniTaskVoid ReloadStart(float reloadTime)
        {
            isReload = true;
            float duration = reloadTime / bullets.Length;
            bool isOriginPosExist = !ReferenceEquals(bulletsOriginPos, null) && bulletsOriginPos.Length == bullets.Length;
            if (!isOriginPosExist)
            {
                bulletsOriginPos = new Vector2[bullets.Length];
                for(int i = 0; i < bullets.Length; i++)
                {
                    bulletsOriginPos[i] = bullets[i].rectTransform.anchoredPosition;
                }
            }
            
            for (int i = 0; i < bullets.Length; i++)
            {
                ChangePositionLerp(bullets[i].rectTransform, bullets[i].rectTransform.anchoredPosition + RELOAD_OFFSET, duration).Forget();
                reloadIndex = i;

                if (!isReload)
                {
                    ReloadFailAnimation(duration).Forget();
                    return;
                }
                
                await UniTask.Delay(TimeSpan.FromSeconds(duration / TimeManager.Instance.PlayerTimeScale), DelayType.UnscaledDeltaTime, cancellationToken:this.GetCancellationTokenOnDestroy());
            }

            Shot(bullets.Length, bullets.Length);
        }
        
        private async UniTaskVoid ReloadAnimation(float time)
        {
            float duration = time / (bullets.Length + 1);
            for (int i = 0; i < bullets.Length; i++)
            {
                ChangePositionLerp(bullets[i].rectTransform, bulletsOriginPos[i] + RELOAD_COMPLETE_OFFSET, duration).Forget();
                
                await UniTask.Delay(TimeSpan.FromSeconds(duration), DelayType.UnscaledDeltaTime, cancellationToken:this.GetCancellationTokenOnDestroy());
            }
            
            for (int i = 0; i < bullets.Length; i++)
            {
                ChangePositionLerp(bullets[i].rectTransform, bulletsOriginPos[i], duration).Forget();
                
                await UniTask.Yield();
            }
        }
        
        private async UniTaskVoid ReloadFailAnimation(float time)
        {
            float duration = time / bullets.Length;
            for (int i = reloadIndex; i >= 0; i--)
            {
                ChangePositionLerp(bullets[i].rectTransform, bulletsOriginPos[i], duration, false).Forget();
                await UniTask.Yield();
            }
        }

        private async UniTaskVoid ChangePositionLerp(RectTransform rect, Vector2 targetPos, float duration, bool canCancel = true)
        {
            float elapsedTime = 0;
            Vector2 startPos = rect.anchoredPosition;
            while (elapsedTime < duration)
            {
                elapsedTime += TimeManager.Instance.GetUnscaledDeltaTime() * TimeManager.Instance.PlayerTimeScale;
                
                rect.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, reloadCurve.Evaluate(elapsedTime / duration));

                if (canCancel && !isReload)
                {
                    return;
                }
                
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken:this.GetCancellationTokenOnDestroy());
            }
        }
    }
}
