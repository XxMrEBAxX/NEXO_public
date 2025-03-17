using System;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace BirdCase
{
    public class CounterMonsterDummy : NetworkBehaviour, IBossGetDamage, IGetOffLauncher, IAffectByExplosion
    {
        private readonly Vector2 MATERIAL_DISSOLVE_RANGE = new Vector2(-0.8f, 2.0f);

        [SerializeField]
        private int counterSuccessDamage = 100;
        [SerializeField]
        private float counterDuration = 5.0f;
        [SerializeField]
        private float counterFailedCooldown = 5.0f;
        
        private Rigidbody rb;
        
        private HitFXHandler hitFxHandler;
        private HitFXHandler counterFxHandler;
        
        private MeshRenderer meshRenderer;
        private SkinnedMeshRenderer skinnedMeshRenderer;

        private float elapsedTime;
        private bool isDummyActive = false;
        private bool isCounterActivated = false;
        
        public event Action CounterSuccessEvent;
        
        private void Awake()
        {
            hitFxHandler = transform.GetChild(1).GetComponent<HitFXHandler>();
            counterFxHandler = transform.GetChild(2).GetComponent<HitFXHandler>();
            rb = GetComponent<Rigidbody>();
            meshRenderer = GetComponentInChildren<MeshRenderer>();
            skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        }

        private void Start()
        {
            if (IsServer && !NetworkObject.IsSpawned)
            {
                NetworkObject.Spawn();
            }
            
            counterFxHandler.Duration = counterDuration;
        }

        private void OnEnable()
        {
            SetMaterial(MATERIAL_DISSOLVE_RANGE.x);
        }

        private void Update()
        {
            if(!IsServer)
                return;
            
            if (isDummyActive)
            {
                elapsedTime += Time.unscaledDeltaTime;

                if (isCounterActivated)
                {
                    if (elapsedTime >= counterDuration)
                    {
                        isCounterActivated = false;
                        elapsedTime = 0;
                    }
                    
                    if (CurrentGetDamage >= counterSuccessDamage)
                    {
                        SuccessCounter();
                    }
                    
                }
                else if (elapsedTime >= counterFailedCooldown)
                {
                    CurrentGetDamage = 0;
                    isCounterActivated = true;
                    CounterStartClientRPC();
                    elapsedTime = 0;
                }

            }
        }

        private void SetMaterial(float amount)
        {
            meshRenderer.materials[0].SetFloat("_DissolveAmount", amount);
            skinnedMeshRenderer.materials[0].SetFloat("_DissolveAmount", amount);
        }
        
        public void StartCounter()
        {
            isCounterActivated = false;
            isDummyActive = true;
            elapsedTime = 0;
            CurrentGetDamage = 0;
            CounterStartClientRPC(false);
        }

        [ClientRpc]
        private void CounterStartClientRPC(bool isPlay = true)
        {
            if(isPlay)
            {
                counterFxHandler.Play();
            }
            else
            {
                counterFxHandler.Stop();
            }
        }

        public void GetDamage(int damage, PlayerName playerName)
        {
            if(isDummyActive)
                hitFxHandler.Play();
            
            if (isCounterActivated)
            {
                CurrentGetDamage += damage;
            }
        }

        public void GetNeutralize(int neutralize, PlayerName playerName)
        {
        }

        public int CurrentGetDamage { get; set; }
        public event Action<IGetOffLauncher> GetOffLauncher;
        
        public void AffectByExplosion(Vector3 explosionCenterPosition, LauncherBaseData.ExplosionData explosionData, int damage, int neutralizeValue, PlayerName playerName)
        {
            if(isDummyActive)
                hitFxHandler.Play();

            if (isCounterActivated)
            {
                CurrentGetDamage += damage;
            }
        }

        public ObjectSize GetObjectSize()
        {
            return ObjectSize.LARGE;
        }
        
        private void SuccessCounter()
        {
            isDummyActive = false;
            isCounterActivated = false;
            CounterSuccessEvent?.Invoke();
            GetOffLauncher?.Invoke(this);
            DisableDummyClientRPC();
        }
        
        [ClientRpc]
        private void DisableDummyClientRPC()
        {
            isCounterActivated = false;
            counterFxHandler.Stop();
            rb.useGravity = true;
        } 
        
        private async UniTaskVoid DisableDummy()
        {
            float t = 0;
            while(t < 1.0f)
            {
                t += Time.unscaledDeltaTime;
                SetMaterial(Mathf.Lerp(MATERIAL_DISSOLVE_RANGE.x, MATERIAL_DISSOLVE_RANGE.y, t));
                
                await UniTask.Yield();
            }
            
            gameObject.SetActive(false);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                DisableDummy().Forget();
            }
        }
    }
}
