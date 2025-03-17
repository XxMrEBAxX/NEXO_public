using System;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using System.Collections;

namespace BirdCase
{
    public enum EElectricWireState
    {
        DROP,
        DESTRUCT
    }

    public class ElectricWireSummon : NetworkBehaviour, IBossGetDamage, IAffectByExplosion
    {
        public event Action DeadAction;
        [SerializeField] private ParticleSystem wireEffect;
        [SerializeField] private ParticleSystem wireDestructEffect;
        [SerializeField] private ParticleSystem warningEffect;
        private ElectricWireSummonManager summonManager;
        private SphereCollider sphereExplosionCollider;
        private int curSummonHealth;
        private int groundLayer;
        private NetworkTransform networkTransform;
        private GameObject crackEffect;

        private void Awake()
        {
            networkTransform = GetComponent<NetworkTransform>();
            summonManager = GetComponentInParent<ElectricWireSummonManager>();
            sphereExplosionCollider = wireDestructEffect.GetComponent<SphereCollider>();
            groundLayer = LayerMask.NameToLayer("Ground");
            crackEffect = wireDestructEffect.transform.Find("CrackEmissive").gameObject;
        }

        private void Update()
        {
            if (!IsServer)
                return;

            if (wireEffect.isPlaying)
            {
                DropElectricWireSummon();
            }
        }

        private void DropElectricWireSummon()
        {
            transform.position += Vector3.down * (summonManager.BossOne.SummonData.ElectricWireDropSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer)
            {
                return;
            }

            if (other.gameObject.CompareTag("Player") && other.gameObject.TryGetComponent(out PlayerBase playerBase))
            {
                SetActiveCrackEffectClientRPC(false);
                Destruct();
            }

            if (other.gameObject.layer == groundLayer)
            {
                SetActiveCrackEffectClientRPC(true);
                Destruct();
            }
        }

        private void Destruct()
        {
            PlayElectricWireDestructEffectClientRPC(transform.position);
            summonManager.BossOne.sound.PlaySummonExplosionClientRPC(transform.position);

            Collider[] hitColliders = Physics.OverlapSphere(wireDestructEffect.gameObject.transform.position, sphereExplosionCollider.radius);

            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (hitColliders[i].gameObject.CompareTag("Player"))
                {
                    Physics.Linecast(wireDestructEffect.gameObject.transform.position, hitColliders[i].transform.position + Vector3.up, out RaycastHit hit, 1 << groundLayer);
                    if (hit.collider != null)
                    {
                        continue;
                    }

                    if (hitColliders[i].TryGetComponent(out PlayerBase player))
                    {
                        if (summonManager.BossOne.IsLastPury)
                        {
                            if (player.IsShieldActive)
                            {
                                StartCoroutine(WaitReduceShieldCoroutine(player));
                            }
                        }
                        player.ShockClientRPC(summonManager.BossOne.SummonData.ElectricWireStunTime);
                        player.TakeDamage(summonManager.BossOne.SummonData.SelfDestructDamage);
                    }
                }
            }
            SetActiveAndPosWireClientRpc(false, new Vector3(0, -summonManager.BossOne.SummonData.ElectricWireSetCreateHeight, 0));
            DeadAction?.Invoke();
        }

        private IEnumerator WaitReduceShieldCoroutine(PlayerBase player)
        {
            yield return new WaitForSeconds(0.2f);
            player.ReduceShieldCooldownClientRPC(summonManager.BossOne.BossData.LastpuryShieldReduce);
        }

        #region ClientRpc

        [ClientRpc]
        private void PlayElectricWireDestructEffectClientRPC(Vector3 pos)
        {
            wireDestructEffect.gameObject.transform.position = pos;
            wireDestructEffect.Play();
            wireEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            CameraManager.Instance.ExplosionYCameraShake(Vector3.up, 0.1f);
        }

        [ClientRpc]
        private void SetActiveCrackEffectClientRPC(bool active)
        {
            crackEffect.SetActive(active);
        }

        [ClientRpc]
        public void SetActiveAndPosWireClientRpc(bool active, Vector3 pos)
        {
            if (active)
            {
                wireEffect.Play();
            }
            else
            {
                if (wireEffect.IsAlive(true))
                {
                    wireEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }

            networkTransform.enabled = active;
            transform.position = pos;
        }


        public void WarningEffectDownRayCasting(Vector3 warningPos)
        {
            warningPos.z = 0;
            Physics.Raycast(warningPos, Vector3.down, out RaycastHit hit, 100, LayerMask.GetMask("Ground"));
            PlayWarningEffectClientRPC(new Vector3(hit.point.x, hit.point.y, 0));
            summonManager.BossOne.sound.PlayForeshadowClientRPC(hit.point);
        }

        [ClientRpc]
        private void PlayWarningEffectClientRPC(Vector3 position)
        {
            warningEffect.transform.position = position;
            warningEffect.Play();
        }

        #endregion
        int IBossGetDamage.CurrentGetDamage { get; set; }

        public void GetDamage(int damage, PlayerName playerName)
        {
            if (curSummonHealth >= 0)
            {
                int damageValue = curSummonHealth - damage >= 0 ? damage : curSummonHealth;
                if (playerName == PlayerName.Ria)
                    DataSaveManager.Instance.CurPlayData.BossDamagedByRia += damageValue;
                else
                    DataSaveManager.Instance.CurPlayData.BossDamagedByNia += damageValue;
            }

            curSummonHealth = Mathf.Clamp(curSummonHealth - damage, 0, summonManager.BossOne.SummonData.ElectricWireSummonHealth);

            if (curSummonHealth == 0)
            {
                SetActiveCrackEffectClientRPC(false);
                Destruct();
            }
        }

        public void GetNeutralize(int neutralize, PlayerName playerName)
        {
            return;
        }

        public ObjectSize GetObjectSize()
        {
            return ObjectSize.SMALL;
        }
        public void AffectByExplosion(Vector3 explosionCenterPosition,
            LauncherBaseData.ExplosionData explosionData, int damage, int neutralizeValue, PlayerName playerName)
        {
            GetDamage(damage, playerName);
        }
    }
}
