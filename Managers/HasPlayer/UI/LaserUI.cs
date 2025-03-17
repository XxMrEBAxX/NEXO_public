using System;
using AssetKits.ParticleImage;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace BirdCase
{
    public class LaserUI : MonoBehaviour
    {
        private const int CHARGE_AMOUNT = 3;
        private const float CHARGE_AMOUNT_PER_ONE = 1.0f / CHARGE_AMOUNT;
        
        private Image laserBullet;
        private ParticleImage laserBulletParticle;
        private Image[] chargeBullet = new Image[CHARGE_AMOUNT];
        private ParticleImage chargeCompleteParticle;
        
        private ParticleImage reloadParticle;

        [SerializeField]
        private Color chargeCooldownColor = Color.red;
        [SerializeField]
        private float bulletReduceTime = 0.5f;

        private Color oriColor;
        
        private float bulletAmount;
        private bool isReduce;
        private bool isShot;
        private bool reduceCancel;

        private float laserBulletLeftX;
        private float laserBulletRightX;
        
        private void Awake()
        {
            laserBullet = transform.GetChild(0).GetChild(0).GetComponent<Image>();
            laserBulletParticle = laserBullet.transform.GetChild(0).GetComponent<ParticleImage>();
            reloadParticle = laserBullet.transform.GetChild(1).GetComponent<ParticleImage>();
            laserBulletLeftX = laserBullet.rectTransform.rect.xMin;
            laserBulletRightX = laserBullet.rectTransform.rect.xMax;
            for (int i = 0; i < chargeBullet.Length; i++)
            {
                chargeBullet[i] = transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<Image>();
                chargeBullet[i].fillAmount = 0;
            }
            chargeCompleteParticle = transform.GetChild(2).GetComponent<ParticleImage>();
            oriColor = chargeBullet[0].color;
            laserBulletParticle.Stop(true);
        }

        public void Shot(int maxBulletAmmo, int curBulletAmmo)
        {
            float curBulletAmount = (float)curBulletAmmo / maxBulletAmmo;
            if (curBulletAmount == 1)
            {
                laserBullet.fillAmount = curBulletAmount;
            }
            else
            {
                if (bulletAmount == 0)
                {
                    ReduceBulletAmount(curBulletAmount).Forget();
                }
                else
                {
                    bulletAmount = curBulletAmount;
                    isShot = true;
                }
            }
        }
        
        private async UniTaskVoid ReduceBulletAmount(float bulletAmount)
        {
            isReduce = true;
            laserBulletParticle.Play();
            this.bulletAmount = bulletAmount;
            float oriFillAmount = laserBullet.fillAmount;
            float reduceTime = bulletReduceTime;
            float elapsedTime = 0;
            while(elapsedTime < reduceTime)
            {
                elapsedTime += TimeManager.Instance.GetUnscaledDeltaTime();

                if (reduceCancel)
                {
                    reduceCancel = false;
                    break;
                }

                if (isShot)
                {
                    isShot = false;
                    reduceTime = reduceTime + bulletReduceTime - elapsedTime;
                    elapsedTime = 0;
                    oriFillAmount = laserBullet.fillAmount;
                }
                
                laserBullet.fillAmount = Mathf.Lerp(oriFillAmount, this.bulletAmount, elapsedTime / reduceTime);
                laserBulletParticle.transform.localPosition = 
                    new Vector3(Mathf.Lerp(laserBulletLeftX, laserBulletRightX, laserBullet.fillAmount), laserBulletParticle.transform.localPosition.y, 0);
                
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken:this.GetCancellationTokenOnDestroy());
            }
            laserBulletParticle.Stop();
            isReduce = false;
            this.bulletAmount = 0;
        }

        public void Charge(int chargeCompleteNum, float chargeAmount)
        {
            if(chargeCompleteNum >= CHARGE_AMOUNT)
                return;
            
            chargeBullet[chargeCompleteNum].fillAmount = Mathf.Min(chargeAmount, 1);
            if (chargeBullet[chargeCompleteNum].fillAmount >= 1)
            {
                chargeCompleteParticle.transform.position = chargeBullet[chargeCompleteNum].transform.position;
                chargeCompleteParticle.Play();
            }
        }
        
        public void ChargeCooldown(float cooldownAmount)
        {
            if (cooldownAmount == 1)
            {
                chargeBullet[0].color = chargeCooldownColor;
                chargeBullet[1].color = chargeCooldownColor;
                chargeBullet[2].color = chargeCooldownColor;
                chargeBullet[0].fillAmount = 1;
                chargeBullet[1].fillAmount = 1;
                chargeBullet[2].fillAmount = 1;
            }
            else if (cooldownAmount == 0)
            {
                chargeBullet[0].color = oriColor;
                chargeBullet[1].color = oriColor;
                chargeBullet[2].color = oriColor;
            }

            for (int i = CHARGE_AMOUNT - 1; i > cooldownAmount / CHARGE_AMOUNT_PER_ONE; --i)
            {
                chargeBullet[i].fillAmount = 0;
            }
            chargeBullet[(int)(cooldownAmount / CHARGE_AMOUNT_PER_ONE)].fillAmount = (cooldownAmount % CHARGE_AMOUNT_PER_ONE) * CHARGE_AMOUNT;
        }

        public void Reload(float reloadTime, bool isReload)
        {
            if (isReload)
            {
                reloadParticle.Play();
            }
            else
            {
                if (reloadTime == 0)
                {
                    reloadParticle.Stop(true);
                }
                else
                {
                    reloadParticle.Stop();
                    ReloadAnimation(reloadTime).Forget();
                }
            }
        }
        
        private async UniTaskVoid ReloadAnimation(float time)
        {
            float oriFillAmount = laserBullet.fillAmount;
            float elapsedTime = 0;
            laserBulletParticle.Play();
            while (elapsedTime < time)
            {
                elapsedTime += TimeManager.Instance.GetUnscaledDeltaTime();
                laserBullet.fillAmount = Mathf.Lerp(oriFillAmount, 1, elapsedTime / time);
                
                laserBulletParticle.transform.localPosition = 
                    new Vector3(Mathf.Lerp(laserBulletLeftX, laserBulletRightX, laserBullet.fillAmount), laserBulletParticle.transform.localPosition.y, 0);
                
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken:this.GetCancellationTokenOnDestroy());
            }
            laserBullet.fillAmount = 1;
            laserBulletParticle.Stop();
        }
    }
}
