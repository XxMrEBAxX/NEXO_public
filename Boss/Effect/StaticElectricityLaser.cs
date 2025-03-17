using UnityEngine;

namespace BirdCase
{
    public class StaticElectricityLaser : MonoBehaviour
    {
        private enum Direction
        {
            Left,
            Right,
            Up,
            Down
        }
        private ParticleSystem chargingEffect;
        [SerializeField] private Direction direction;
        [SerializeField] private ParticleSystem beamEffect;
        [SerializeField] private GameObject beamSizeObject;
        [SerializeField] private ParticleSystem beamHitEffect;
        public ParticleSystem BeamHitEffect => beamHitEffect;
        bool isActive = false;
        public bool IsActive => isActive;

        private void Awake()
        {
            chargingEffect= GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            if (isActive)
            {
                Vector3 dir = Vector3.zero;
                switch (direction)
                {
                    case Direction.Left:
                        dir = Vector3.left;
                        break;
                    case Direction.Right:
                        dir = Vector3.right;
                        break;
                    case Direction.Up:
                        dir = Vector3.up;
                        break;
                    case Direction.Down:
                        dir = Vector3.down;
                        break;
                }
                dir = Quaternion.Euler(0, 0, transform.parent.eulerAngles.z) * dir;
                Physics.Raycast(transform.position, dir, out RaycastHit hit, 100, LayerMask.GetMask("Ground"));
                beamSizeObject.transform.localScale = new Vector3(hit.distance > 0 ? hit.distance : 100, beamSizeObject.transform.localScale.y,  beamSizeObject.transform.localScale.z);
                if (!ReferenceEquals(hit.collider, null))
                {
                    beamHitEffect.transform.eulerAngles = new Vector3(0, 0, 90 + Vector3.Angle(Vector3.up, hit.normal));
                    beamHitEffect.transform.position = hit.point;
                }
                else
                    beamHitEffect.transform.position = new Vector3(0, -100, 0);
            }

            if (beamEffect.isStopped)
            {
                isActive = false;
            }
        }

        public void PlayLaser()
        {
            beamEffect.Play();
            beamHitEffect.Play();
            isActive = true;
        }

        public void PlayCharging()
        {
            chargingEffect.Play();
        }

        public void StopLaser()
        {
            chargingEffect.Stop();
            beamEffect.Stop();
            beamHitEffect.Stop();
            isActive = false;
        }
    }
}
