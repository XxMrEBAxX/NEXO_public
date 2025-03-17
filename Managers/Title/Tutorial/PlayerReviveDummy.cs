using System;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace BirdCase
{
    public class PlayerReviveDummy : PlayerBase
    {
        private readonly Vector2 MATERIAL_DISSOLVE_RANGE = new Vector2(-0.8f, 2.0f);
        
        public override PlayerType PlayerType { get; protected set; }
        public override bool IsDead() => isDead;

        public event Action ReviveEvent;
        
        private Func<ulong, bool, GameObject> oriFunc;

        private GameObject riaDummy;
        private GameObject niaDummy;

        private Animator riaAnimator;
        private Animator niaAnimator;
        
        private bool isRia = false;

        private bool isDead = true;
        
        private SkinnedMeshRenderer[] skinnedMeshRenderers;
        
        protected override void Awake()
        {
            base.Awake();
            riaDummy = transform.GetChild(0).gameObject;
            niaDummy = transform.GetChild(1).gameObject;
            riaAnimator = riaDummy.GetComponent<Animator>();
            niaAnimator = niaDummy.GetComponent<Animator>();
            playerComp.IsDummy = true;
            skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        protected override void Start()
        {
            gameObject.SetActive(false);
        }

        protected override void Update()
        {
            
        }

        private void OnEnable()
        {
            DummyDead();
            
            SetMaterial(MATERIAL_DISSOLVE_RANGE.x);
            oriFunc = GetPlayerAction;
            GetPlayerAction -= oriFunc;
            GetPlayerAction += GetDummy;
            
            ServerRPC(false);
        }

        private void SetMaterial(float amount)
        {
            foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers)
            {
                renderer.material.SetFloat("_DissolveAmount", amount);
            }
        }
        
        private void EnableDummy(bool isRia)
        {
            this.isRia = isRia;
            if (isRia)
            {
                riaDummy.SetActive(true);
                niaDummy.SetActive(false);
                riaAnimator.SetBool("IsRia", true);
                riaAnimator.SetBool("IsDead", true);
            }
            else
            {
                riaDummy.SetActive(false);
                niaDummy.SetActive(true);
                niaAnimator.SetBool("IsNia", true);
                niaAnimator.SetBool("IsDead", true);
            }
        }

        private void OnDisable()
        {
            GetPlayerAction -= GetDummy;
            GetPlayerAction += oriFunc;
        }

        protected override void Attack()
        {
            
        }

        protected override void ChargeAttack()
        {
            
        }
        protected void DummyDead()
        {
            isDead = true;
            playerComp.IsDead = true;
            playerComp.CanAct = false;
        }

        private GameObject GetDummy(ulong owner, bool isGetOwner)
        {
            if (isRia)
            {
                return PlayManager.Instance.CurPlayerType == PlayerType.LAUNCHER ? gameObject : null;
            }
            else
            {
                return PlayManager.Instance.CurPlayerType == PlayerType.LASER ? gameObject : null;
            }
        }
        
        protected override void Revive(bool isHealSelf)
        {
            ReviveEvent?.Invoke();
            isDead = false;
            if (isRia)
            {
                DummyDeadServerRPC();
            }
            else
            {
                DummyDead();
                ServerRPC(true);
            }
        }
        
        private async UniTask DummyDeadAnim()
        {
            Animator animator = isRia ? riaAnimator : niaAnimator;
            animator.SetBool("IsDead", false);
            await UniTask.WaitUntil(() => !animator.GetCurrentAnimatorStateInfo(0).IsTag("Death"), 
                PlayerLoopTiming.Update, cancellationToken: this.GetCancellationTokenOnDestroy());
            
            float elapsedTime = 0;
            
            while(elapsedTime < 1.0f)
            {
                elapsedTime += Time.unscaledDeltaTime;
                SetMaterial(Mathf.Lerp(MATERIAL_DISSOLVE_RANGE.x, MATERIAL_DISSOLVE_RANGE.y, elapsedTime));
                
                await UniTask.Yield();
            }
            
            gameObject.SetActive(false);
        }

        [ServerRpc (RequireOwnership = false)]
        private void ServerRPC(bool isRia)
        {
            ClientRPC(isRia);
        }

        [ClientRpc]
        private void ClientRPC(bool isRia)
        {
            EnableDummy(isRia);
        }

        [ServerRpc (RequireOwnership = false)]
        private void DummyDeadServerRPC()
        {
           DummyDeadClientRPC(); 
        }

        [ClientRpc]
        private void DummyDeadClientRPC()
        {
            DummyDeadAnim().Forget();
        }
    }
}
