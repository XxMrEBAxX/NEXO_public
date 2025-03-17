using Unity.Netcode;
using UnityEngine;

namespace BirdCase
{
    public class DamageArea : MonoBehaviour
    {
        [SerializeField] private int damage = 1;
        [SerializeField] private bool isStun = false;
        public bool IsAttack = false;
        public bool IsRayCast = false;

        private void OnTriggerStay(Collider other)
        {
            if (!IsAttack) return;

            if (IsRayCast)
            {
                Physics.Linecast(transform.position, other.transform.position + Vector3.up, out RaycastHit hit, LayerMask.GetMask("Ground"));
                if (hit.collider != null)
                {
                    return;
                }
            }

            if (other.TryGetComponent(out PlayerBase playerBase))
            {
                if (isStun)
                {
                    playerBase.ShockClientRPC(2);
                }
                playerBase.TakeDamage(damage);
            }
        }
    }
}
