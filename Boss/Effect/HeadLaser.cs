using UnityEngine;

namespace BirdCase
{
    public class HeadLaser : MonoBehaviour
    {
        private ParticleSystem laserEffect;
        private Transform followTransform = null;
        [SerializeField] private ParticleSystem laserChargingEffect;
        [SerializeField] private ParticleSystem laserEndEffect;
        public ParticleSystem LaserEndEffect => laserEndEffect;
        private bool isActive = false;
        private float offset = 0;

        private void Start()
        {
            laserEffect = GetComponent<ParticleSystem>();
        }

        private void FixedUpdate()
        {
            if (isActive)
            {
                Vector3 dir = transform.forward;
                Physics.Raycast(transform.position, dir.normalized, out RaycastHit hit, 100, LayerMask.GetMask("Ground"));
                float offset = 0.0189f; // 1 당 스케일 값
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, hit.distance > 0 ? hit.distance * offset : 100);
                if (!ReferenceEquals(hit.collider, null))
                    laserEndEffect.transform.position = hit.point + Vector3.up * 0.2f;
            }

            if (laserEffect.isStopped)
            {
                isActive = false;
            }

            if (followTransform != null)
            {
                laserChargingEffect.transform.position = followTransform.position + transform.forward * offset;
                transform.position = followTransform.position + transform.forward * offset;
            }
        }

        public void PlayLaser()
        {
            laserEffect.Play();
            laserEndEffect.Play();
            isActive = true;
        }

        public void PlayCharging()
        {
            laserChargingEffect.Play();
        }

        public void StopLaser()
        {
            laserEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            laserEndEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            laserChargingEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            isActive = false;
        }

        public void FollowTransform(Transform transform)
        {
            followTransform = transform;
        }

        public void FollowOffset(float amount)
        {
            offset = amount;
        }
    }
}
