using Unity.Netcode;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using Unity.Netcode.Components;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;

namespace BirdCase
{
    public class BossParts : NetworkBehaviour, IBossGetDamage, IAffectByExplosion
    {
        public EBossParts BossPart; // 현재 이 부위가 어디인지 지정합니다.
        public BossHandPositionManager handPositionManager { get; private set; }

        private BossOne bossOne;
        private Rigidbody rb;
        public NetworkAnimator networkAnimator { get; private set; }
        [SerializeField] private CapsuleCollider capsuleCollider;
        [SerializeField] private MeshCollider meshCollider;
        public int CurrentGetDamage { get; set; }
        public ObjectSize GetObjectSize() => ObjectSize.LARGE;
        private LayerMask playerLayerMask;

        private Vector3 originPos;
        public Vector3 OriginPos => originPos;
        private Vector3 targetPos;
        [SerializeField] private Transform handLaserPosition;
        public Transform HandLaserPosition => handLaserPosition;
        private float capsuleRadius = 0;
        /// <summary>
        /// 충돌로 인한 피해가 들어갈 때 true가 됩니다.
        /// </summary>
        private bool isAttacking = false;
        private bool isSuccessAttack = false;
        private bool isGrabAttack = false;
        private bool isGrabbing = false;
        private PlayerBase grabPlayer;
        public PlayerBase GrabPlayer => grabPlayer;

        public Ease easeType = Ease.Linear;
        Tween currentTween = null;

        [SerializeField] private Transform wavePosition;
        [SerializeField] private ParticleSystem waveEffect;
        [SerializeField] private ParticleSystem warningEffect;
        private EffectTimingToolManager warningEffectTimingToolManager;

        [SerializeField] private ParticleSystem neutralizeEffect;

        private HitFXHandler counterFXHandler;

        [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
        [SerializeField] private Material hologramMaterial;

        [SerializeField] private ParticleSystem warningSwingEffect;
        private EffectTimingToolManager warningSwingEffectTimingToolManager;

        private void Awake()
        {
            bossOne = GetComponentInParent<BossOne>();
            networkAnimator = GetComponentInChildren<NetworkAnimator>();
            handPositionManager = GetComponent<BossHandPositionManager>();
            rb = GetComponent<Rigidbody>();
            warningEffectTimingToolManager = warningEffect.GetComponent<EffectTimingToolManager>();
            warningSwingEffectTimingToolManager = warningSwingEffect.GetComponent<EffectTimingToolManager>();
            counterFXHandler = GetComponentInChildren<HitFXHandler>();

            originPos = handPositionManager.OriginPosition.position;
            transform.position = originPos;

            playerLayerMask = LayerMask.GetMask("PlayerCollider") + LayerMask.GetMask("PlayerOne") + LayerMask.GetMask("PlayerTwo");
            meshCollider.excludeLayers = 0;
        }

        private void Start()
        {
            rb.isKinematic = true;
            capsuleRadius = capsuleCollider.radius * 0.45f;
        }

        public void OnTriggerStay(Collider other)
        {
            if (isAttacking && other.gameObject.CompareTag("Player"))
            {
                if (other.gameObject.TryGetComponent(out PlayerBase playerBase))
                {
                    if (playerBase.IsDead() || playerBase.playerComp.Invincible || playerBase.playerComp.IsGrabbed)
                        return;

                    if (PredictClientPlayerPosition(playerBase))
                    {
                        return;
                        // 재판정
                    }

                    float difX = playerBase.transform.position.x - transform.position.x;
                    playerBase.AddForceClientRPC(new Vector3(0.7f * MathF.Sign(difX), 0.3f, 0) * 60, ForceMode.Impulse, 0.2f);

                    isSuccessAttack = true;
                    if (isGrabAttack)
                    {
                        playerBase.GrabClientRPC(true);
                        grabPlayer = playerBase;
                        GrabSetPosClientRPC(grabPlayer.OwnerClientId);
                        isGrabAttack = false;
                        isGrabbing = true;
                        PlayGrabSound();
                    }
                    else
                    {
                        playerBase.TakeDamage(bossOne.BossData.AttackDamage);
                    }
                }
            }

            if (isCounterAttack)
            {
                if (other.transform.parent != null && other.gameObject.activeInHierarchy)
                {
                    if (other.transform.parent.TryGetComponent<MeshDemolisherTool>(out var meshDemolisherTool))
                    {
                        DOVirtual.DelayedCall(0.1f, () =>
                        {
                            counterMeshDemolisherTools.Add(meshDemolisherTool);
                            MeshDemolisherDisappear(meshDemolisherTool, 3f, bossOne.BossData.CounterAttackPlatformRespawnTime);
                        });
                    }
                }
            }
        }

        private void Update()
        {
            if (!IsServer) return;

            if (grabPlayer != null)
            {
                GrabSetPosClientRPC(grabPlayer.OwnerClientId);
            }
        }

        private void SetAttacking(bool isAttacking)
        {
            this.isAttacking = isAttacking;
            isSuccessAttack = false;
            ColliderSettingClientRPC(isAttacking);
        }

        [ClientRpc]
        private void ColliderSettingClientRPC(bool isAttacking)
        {
            if (!bossOne.IsPhase2)
            {
                meshCollider.excludeLayers = isAttacking ? playerLayerMask : 0;
                meshCollider.isTrigger = isAttacking;
            }
            else
            {
                if (isCounterAttack)
                {
                    meshCollider.excludeLayers = isAttacking ? 0 : -1;
                    rb.excludeLayers = isAttacking ? 0 : -1;
                }
                else
                {
                    meshCollider.excludeLayers = isAttacking ? ~playerLayerMask : -1;
                    rb.excludeLayers = isAttacking ? ~playerLayerMask : -1;
                }
            }
            capsuleCollider.isTrigger = isAttacking;
        }

        public void HandLaser()
        {
            bossOne.SetTransformLaserEffectClientRPC(BossPart);

            targetPos = handPositionManager.HandLaserPosition.position;
            networkAnimator.SetTrigger("HandLaser");

            if (BossPart == EBossParts.RIGHT_ARM)
                bossOne.LeftBossParts.MovePosition(bossOne.LeftBossParts.handPositionManager.SwingAttackPosition.position, bossOne.BossData.SwingAttackSignalMoveTime);
            else
                bossOne.RightBossParts.MovePosition(bossOne.RightBossParts.handPositionManager.SwingAttackPosition.position, bossOne.BossData.SwingAttackSignalMoveTime);

            PlayHandMoveSound();
            currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), targetPos, bossOne.BossData.HeadLaserSignalTime).SetEase(bossOne.BossData.HeadLaserSignalEase).SetUpdate(UpdateType.Fixed)
            .OnComplete(() =>
            {
                StopHandMoveSound();
                //ANCHOR - y좌표가 땅 표면 위치여야함
                Vector3 startPosition = handPositionManager.LaserPosition.position;
                Vector3 endPosition = new Vector3(-startPosition.x, startPosition.y, startPosition.z);
                Transform tf = bossOne.LaserEffect.transform;
                tf.forward = (startPosition - tf.position).normalized;
                bossOne.PlayLaserChargingEffectClientRPC();
                currentTween = transform.DOLocalRotate(new Vector3(0, transform.localEulerAngles.y, startPosition.x < 0 ? 70 : -70), bossOne.BossData.HeadLaserChargeTime).SetUpdate(UpdateType.Fixed)
                .OnComplete(() =>
                {
                    bossOne.HeadLaserDamageArea.IsAttack = true;
                    bossOne.PlayLaserEffectClientRPC();
                    float startRotateZ = transform.localEulerAngles.z;
                    DOVirtual.DelayedCall(bossOne.BossData.HeadLaserAttackTime * 0.85f, () =>
                    {
                        bossOne.sound.StopLaserFireClientRPC();
                        bossOne.HeadLaserDamageArea.IsAttack = false;
                    });
                    currentTween = DOTween.To(() => startPosition, x =>
                    {
                        tf.forward = (x - tf.position).normalized;
                        // 플랫폼 위치 틀어질 경우에만 사용
                        // Physics.Raycast(tf.position, tf.forward, out RaycastHit hit, 100, LayerMask.GetMask("Ground"));
                        // if (Mathf.Abs(hit.point.z) > 0.3f)
                        // {
                        //     tf.forward = (new Vector3(x.x, x.y, -4.5f) - tf.position).normalized;
                        // }
                        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, Mathf.LerpAngle(startRotateZ, -startRotateZ, Mathf.InverseLerp(startPosition.x, endPosition.x, x.x)));
                    }, endPosition, bossOne.BossData.HeadLaserAttackTime).SetUpdate(UpdateType.Fixed)
                    .OnComplete(() =>
                    {
                        currentTween = transform.DOLocalRotate(new Vector3(0, transform.localEulerAngles.y, 0), bossOne.BossData.HeadLaserChargeTime).SetUpdate(UpdateType.Fixed)
                        .OnComplete(() =>
                        {
                            if (BossPart == EBossParts.RIGHT_ARM)
                                bossOne.LeftBossParts.MoveOriginPosition(bossOne.BossData.HeadLaserReturnTime);
                            else
                                bossOne.RightBossParts.MoveOriginPosition(bossOne.BossData.HeadLaserReturnTime);

                            PlayHandMoveSound();
                            currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), originPos, bossOne.BossData.HeadLaserReturnTime).SetEase(bossOne.BossData.HeadLaserSignalEase).SetUpdate(UpdateType.Fixed)
                            .OnComplete(() =>
                            {
                                StopHandMoveSound();
                                bossOne.PatternComplete();
                            });
                        });

                    });
                });

            });
        }

        public void SwingAttack(bool isLinked = false)
        {
            PlayWarningSwingEffectClientRPC(1.5f);
            networkAnimator.SetTrigger("HandSwing");
            targetPos = handPositionManager.SwingAttackPosition.position;

            float signalTime = isLinked ? bossOne.BossData.SwingAttackLinkedSignalMoveTime : bossOne.BossData.SwingAttackSignalMoveTime;

            if (BossPart == EBossParts.RIGHT_ARM)
                bossOne.LeftBossParts.MovePosition(bossOne.LeftBossParts.handPositionManager.SwingAttackPosition.position, bossOne.BossData.SwingAttackSignalMoveTime);
            else
                bossOne.RightBossParts.MovePosition(bossOne.RightBossParts.handPositionManager.SwingAttackPosition.position, bossOne.BossData.SwingAttackSignalMoveTime);

            PlayHandMoveSound();
            currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), targetPos, signalTime).SetEase(bossOne.BossData.SwingAttackSignalEase).SetUpdate(UpdateType.Fixed)
            .OnComplete(() =>
            {
                SetAttacking(true);
                ExplosionXShakeClientRPC(BossPart == EBossParts.RIGHT_ARM ? Vector3.right : Vector3.left, 0.6f);
                float amount = bossOne.BossData.SwingAttackSignalMoveAmount;
                targetPos = targetPos + new Vector3(BossPart == EBossParts.LEFT_ARM ? amount : -amount, 0, 0);
                StopHandMoveSound();
                PlayHandSwingSound();

                DOVirtual.DelayedCall(bossOne.BossData.SwingAttackMoveTime * 0.95f, () => SetAttacking(false));
                currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), targetPos, bossOne.BossData.SwingAttackMoveTime).SetEase(Ease.Linear).SetUpdate(UpdateType.Fixed)
                .OnComplete(() =>
                {
                    currentTween = DOVirtual.DelayedCall(bossOne.BossData.SwingAttackRestTime, () =>
                    {
                        if (BossPart == EBossParts.RIGHT_ARM)
                            bossOne.LeftBossParts.MoveOriginPosition(bossOne.BossData.SwingAttackSignalMoveTime);
                        else
                            bossOne.RightBossParts.MoveOriginPosition(bossOne.BossData.SwingAttackSignalMoveTime);

                        PlayHandMoveSound();
                        currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), originPos, bossOne.BossData.SwingAttackSignalMoveTime).SetEase(bossOne.BossData.SwingAttackSignalEase).SetUpdate(UpdateType.Fixed)
                        .OnComplete(() =>
                        {
                            StopHandMoveSound();
                            bossOne.PatternComplete();
                        });
                    });
                });
            });
        }

        public void DownAttack()
        {
            networkAnimator.SetTrigger("HandDown");
            // 손을 올릴 때
            targetPos = handPositionManager.GrabSuccessPosition.position;
            WarningEffectSetFixedTimeClientRPC(bossOne.BossData.DownAttackSignalMoveTime);
            WarningEffectDownRayCasting(targetPos);

            PlayHandMoveSound();

            currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), targetPos, bossOne.BossData.DownAttackSignalMoveTime).SetEase(bossOne.BossData.DownAttackSignalEase).SetUpdate(UpdateType.Fixed)
            .OnComplete(() =>
            {
                SetAttacking(true);
                Vector3 downPos = Vector3.zero;

                downPos = handPositionManager.GrabSuccessAttackNotPlatformPosition.position;

                Physics.Raycast(new Vector3(transform.position.x, transform.position.y, 0), Vector3.down, out RaycastHit downHit, 100, LayerMask.GetMask("Ground"));

                if (downHit.collider.transform.parent != null)
                {
                    if (downHit.collider.transform.parent.TryGetComponent<MeshDemolisherTool>(out var meshDemolisherTool))
                    {
                        downPos = handPositionManager.GrabSuccessAttackPosition.position;
                        DOVirtual.DelayedCall(bossOne.BossData.DownAttackMoveTime, () => MeshDemolisherDisappear(meshDemolisherTool, 3f, bossOne.BossData.DownPlatformRespawnTime));
                    }
                }

                DOVirtual.DelayedCall(bossOne.BossData.DownAttackMoveTime * 0.5f, () => PlayDownAttackSound());
                currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), downPos, bossOne.BossData.DownAttackMoveTime).SetEase(bossOne.BossData.DownAttackMoveEase).SetUpdate(UpdateType.Fixed)
                .OnComplete(() =>
                {
                    StopHandMoveSound();
                    ExplosionYShakeClientRPC(Vector3.down, 0.6f);

                    WaitForUpdateUniTask(() =>
                    {
                        PlayWaveEffectClientRPC(wavePosition.position);
                        if (downPos == handPositionManager.GrabSuccessAttackNotPlatformPosition.position)
                            bossOne.GroundEmissionAnimationClientRPC();

                        BoxCollider boxCollider = waveEffect.GetComponent<BoxCollider>();
                        foreach (var collision in Physics.OverlapBox(wavePosition.position + Vector3.up * boxCollider.center.z, boxCollider.bounds.size * 0.5f, Quaternion.identity))
                        {
                            if (collision.TryGetComponent(out PlayerBase playerBase))
                            {
                                playerBase.TakeDamage(bossOne.BossData.DownAttackWaveDamage);
                                isSuccessAttack = true;
                            }
                        }
                        if (isSuccessAttack)
                        {
                            SetAttacking(false);
                            SwingAttack();
                        }
                        else
                        {
                            currentTween = DOVirtual.DelayedCall(bossOne.BossData.DownAttackRestTime, () =>
                            {
                                PlayHandMoveSound();
                                currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), originPos, bossOne.BossData.DownAttackSignalMoveTime).SetEase(bossOne.BossData.DownAttackSignalEase).SetUpdate(UpdateType.Fixed)
                                .OnComplete(() =>
                                {
                                    StopHandMoveSound();
                                    bossOne.PatternComplete();
                                });
                            });
                        }
                        WaitForUpdateUniTask(() => SetAttacking(false)).Forget();
                    }).Forget();
                });
            });
        }

        Coroutine downTargetAttackCoroutine = null;
        public void DownTargetAttack(PlayerBase target)
        {
            networkAnimator.SetTrigger("HandDownTarget");
            // 손을 올릴 때
            targetPos = handPositionManager.DownTargetAttackPosition.position;
            targetPos.x = target.transform.position.x;

            PlayHandMoveSound();
            currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), targetPos, bossOne.BossData.DownTargetAttackSignalMoveTime).SetEase(bossOne.BossData.DownTargetAttackSignalEase).SetUpdate(UpdateType.Fixed)
            .OnComplete(() =>
            {
                downTargetAttackCoroutine = StartCoroutine(DownTargetAttackCoroutine(target));
            });
        }

        private IEnumerator DownTargetAttackCoroutine(PlayerBase target)
        {
            float currentTime = Time.time;
            float time = bossOne.BossData.DownTargetAttackFollowTime;
            float speed = bossOne.BossData.DownTargetAttackFollowSpeed;

            WarningEffectSetFixedTimeClientRPC(time);
            WarningEffectDownRayCasting(rb.position);

            while (currentTime + time > Time.time)
            {
                float targetX = target.transform.position.x;
                rb.MovePosition(new Vector3(Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * speed), transform.position.y, transform.position.z));
                WarningEffectDownRayCasting(rb.position, true);
                yield return new WaitForFixedUpdate();
            }

            Vector3 downPos = Vector3.zero;

            downPos = handPositionManager.GrabSuccessAttackNotPlatformPosition.position;

            Physics.Raycast(new Vector3(transform.position.x - capsuleRadius, transform.position.y, 0), Vector3.down, out RaycastHit leftHit, 100, LayerMask.GetMask("Ground"));
            Physics.Raycast(new Vector3(transform.position.x + capsuleRadius, transform.position.y, 0), Vector3.down, out RaycastHit rightHit, 100, LayerMask.GetMask("Ground"));

            if (leftHit.collider.transform.parent != null)
            {
                if (leftHit.collider.transform.parent.TryGetComponent<MeshDemolisherTool>(out var meshDemolisherTool))
                {
                    downPos = handPositionManager.GrabSuccessAttackPosition.position;
                    DOVirtual.DelayedCall(bossOne.BossData.DownTargetAttackMoveTime * 0.5f, () => MeshDemolisherDisappear(meshDemolisherTool, 3f, bossOne.BossData.DownTargetPlatformRespawnTime));
                }
            }
            if (rightHit.collider.transform.parent != null && downPos != handPositionManager.GrabSuccessAttackPosition.position)
            {
                if (rightHit.collider.transform.parent.TryGetComponent<MeshDemolisherTool>(out var meshDemolisherTool))
                {
                    downPos = handPositionManager.GrabSuccessAttackPosition.position;
                    DOVirtual.DelayedCall(bossOne.BossData.DownTargetAttackMoveTime * 0.5f, () => MeshDemolisherDisappear(meshDemolisherTool, 3f, bossOne.BossData.DownTargetPlatformRespawnTime));
                }
            }

            downPos.x = transform.position.x;
            SetAttacking(true);
            StopHandMoveSound();
            currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), downPos, bossOne.BossData.DownTargetAttackMoveTime).SetEase(easeType).SetUpdate(UpdateType.Fixed)
            .OnComplete(() =>
            {
                ExplosionYShakeClientRPC(Vector3.down, 0.7f);
                PlayDownAttackSound();

                WaitForUpdateUniTask(() =>
                {
                    PlayWaveEffectClientRPC(wavePosition.position);
                    if (downPos.y == handPositionManager.GrabSuccessAttackNotPlatformPosition.position.y)
                        bossOne.GroundEmissionAnimationClientRPC();
                    SetAttacking(false);
                    BoxCollider boxCollider = waveEffect.GetComponent<BoxCollider>();
                    foreach (var collision in Physics.OverlapBox(wavePosition.position + Vector3.up * boxCollider.center.z, boxCollider.bounds.size * 0.5f, Quaternion.identity))
                    {
                        if (collision.TryGetComponent(out PlayerBase playerBase))
                        {
                            playerBase.TakeDamage(bossOne.BossData.DownTargetAttackWaveDamage);
                        }
                    }
                }).Forget();

                currentTween = DOVirtual.DelayedCall(bossOne.BossData.DownTargetAttackRestTime, () =>
                {
                    PlayHandMoveSound();
                    currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), originPos, bossOne.BossData.DownTargetAttackSignalMoveTime).SetEase(bossOne.BossData.DownTargetAttackSignalEase).SetUpdate(UpdateType.Fixed)
                    .OnComplete(() =>
                    {
                        StopHandMoveSound();
                        bossOne.PatternComplete();
                    });
                });
            });
        }

        [SerializeField] private Transform grabPosition;
        [SerializeField] private GameObject grabDummyRia;
        [SerializeField] private GameObject grabDummyNia;

        public void GrabAttack()
        {
            PlayWarningSwingEffectClientRPC(0.8f);
            networkAnimator.SetTrigger("HandGrab");
            targetPos = handPositionManager.SwingAttackPosition.position;

            if (BossPart == EBossParts.RIGHT_ARM)
                bossOne.LeftBossParts.MovePosition(bossOne.LeftBossParts.handPositionManager.SwingAttackPosition.position, bossOne.BossData.GrabSignalMoveTime);
            else
                bossOne.RightBossParts.MovePosition(bossOne.RightBossParts.handPositionManager.SwingAttackPosition.position, bossOne.BossData.GrabSignalMoveTime);

            PlayHandMoveSound();
            currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), targetPos, bossOne.BossData.GrabSignalMoveTime).SetEase(bossOne.BossData.GrabSignalEase).SetUpdate(UpdateType.Fixed)
            .OnComplete(() =>
            {
                isGrabbing = false;
                isGrabAttack = true;
                SetAttacking(true);
                PlayHandSwingSound();

                DOVirtual.DelayedCall(bossOne.BossData.GrabMoveTime * 0.95f, () => 
                {
                    isGrabAttack = false;
                    SetAttacking(false);
                });

                float amount = bossOne.BossData.GrabSignalMoveAmount;
                targetPos = targetPos + (BossPart == EBossParts.RIGHT_ARM ? Vector3.left * amount : Vector3.right * amount);
                currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), targetPos, bossOne.BossData.GrabMoveTime).SetEase(bossOne.BossData.GrabMoveEase).SetUpdate(UpdateType.Fixed)
                .OnComplete(() =>
                {
                    if (isGrabbing)
                    {
                        networkAnimator.SetTrigger("HandGrabSuccess");
                        targetPos = handPositionManager.GrabSuccessPosition.position;
                        currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), targetPos, bossOne.BossData.GrabMoveUpTime).SetEase(bossOne.BossData.GrabMoveUpEase).SetUpdate(UpdateType.Fixed)
                        .OnComplete(() =>
                        {
                            Vector3 downPos = Vector3.zero;

                            downPos = handPositionManager.GrabSuccessAttackNotPlatformPosition.position;

                            Physics.Raycast(new Vector3(transform.position.x, transform.position.y, 0), Vector3.down, out RaycastHit downHit, 100, LayerMask.GetMask("Ground"));
                            if (downHit.collider.transform.parent != null)
                            {
                                if (downHit.collider.transform.parent.TryGetComponent<MeshDemolisherTool>(out var meshDemolisherTool))
                                {
                                    downPos = handPositionManager.GrabSuccessAttackPosition.position;
                                    DOVirtual.DelayedCall(bossOne.BossData.GrabDownAttackMoveTime * 0.5f, () => MeshDemolisherDisappear(meshDemolisherTool, 3f, bossOne.BossData.GrabPlatformRespawnTime));
                                }
                            }

                            currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), downPos, bossOne.BossData.GrabDownAttackMoveTime).SetEase(bossOne.BossData.GrabDownAttackMoveEase).SetUpdate(UpdateType.Fixed)
                            .OnComplete(() =>
                            {
                                PlayDownAttackSound();
                                StopHandMoveSound();
                                grabPlayer.GrabClientRPC(false);
                                grabPlayer.AddForceClientRPC(new Vector3(BossPart == EBossParts.RIGHT_ARM ? -0.3f : 0.3f, 0.7f, 0) * bossOne.BossData.GrabReboundForce, ForceMode.Impulse, 0.5f);
                                grabPlayer.TakeDamage(bossOne.BossData.GrabDownAttackDamage);
                                GrabSetPosClientRPC(grabPlayer.OwnerClientId, false);
                                grabPlayer = null;

                                WaitForUpdateUniTask(()
                                =>
                                {
                                    PlayWaveEffectClientRPC(wavePosition.position);
                                    bossOne.GroundEmissionAnimationClientRPC();
                                    BoxCollider boxCollider = waveEffect.GetComponent<BoxCollider>();
                                    foreach (var collision in Physics.OverlapBox(wavePosition.position + Vector3.up * boxCollider.center.z, boxCollider.bounds.size * 0.5f, Quaternion.identity))
                                    {
                                        if (collision.TryGetComponent(out PlayerBase playerBase))
                                        {
                                            playerBase.TakeDamage(1);
                                        }
                                    }
                                }).Forget();

                                isGrabbing = false;
                                ExplosionYShakeClientRPC(Vector3.down, 0.8f);

                                currentTween = DOVirtual.DelayedCall(bossOne.BossData.GrabRestTime, () =>
                                {
                                    if (BossPart == EBossParts.RIGHT_ARM)
                                        bossOne.LeftBossParts.MoveOriginPosition(bossOne.BossData.GrabSignalMoveTime);
                                    else
                                        bossOne.RightBossParts.MoveOriginPosition(bossOne.BossData.GrabSignalMoveTime);

                                    PlayHandMoveSound();

                                    currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), originPos, bossOne.BossData.GrabSignalMoveTime).SetEase(bossOne.BossData.GrabSignalEase).SetUpdate(UpdateType.Fixed)
                                    .OnComplete(() =>
                                    {
                                        StopHandMoveSound();
                                        bossOne.PatternComplete();
                                    });
                                });
                            });
                        });
                    }
                    // 그랩 실패
                    else
                    {
                        StopHandMoveSound();
                        currentTween = DOVirtual.DelayedCall(bossOne.BossData.GrabRestTime * 0.5f, () =>
                        {
                            if (BossPart == EBossParts.RIGHT_ARM)
                                bossOne.LeftBossParts.MoveOriginPosition(bossOne.BossData.GrabSignalMoveTime);
                            else
                                bossOne.RightBossParts.MoveOriginPosition(bossOne.BossData.GrabSignalMoveTime);

                            PlayHandMoveSound();
                            currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), originPos, bossOne.BossData.GrabSignalMoveTime).SetEase(bossOne.BossData.GrabSignalEase).SetUpdate(UpdateType.Fixed)
                            .OnComplete(() =>
                            {
                                StopHandMoveSound();
                                bossOne.PatternComplete();
                            });
                        });
                    }
                });
            });
        }

        private int totalCounterDamage = 0;
        private bool isCounterAttack = false;
        private List<MeshDemolisherTool> counterMeshDemolisherTools = new List<MeshDemolisherTool>();

        public void CounterAttack()
        {
            ShowHelpCounterAttackClientRPC();
            counterMeshDemolisherTools.Clear();
            isCounterAttack = true;
            targetPos = handPositionManager.CounterAttackPosition.position;
            networkAnimator.SetTrigger("HandCounter");
            bossOne.sound.PlayCounterStartClientRPC();

            if (bossOne.IsPhase2)
            {
                SetPhase2CounterColliderClientRPC(true);
            }

            currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), targetPos, bossOne.BossData.CounterAttackSignalMoveTime).SetEase(bossOne.BossData.CounterAttackSignalEase).SetUpdate(UpdateType.Fixed)
            .OnComplete(() =>
            {
                StartCounterVignetteClientRPC();
                currentTween = DOVirtual.DelayedCall(0.5f, () =>
                {
                    totalCounterDamage = 0;
                    SetAttacking(true);
                    SetCounterFXClientRPC(true);
                    bossOne.PlayHandPatternBodyAnimation("CounterAttack");
                    networkAnimator.SetTrigger("HandCounterAttack");

                    targetPos = handPositionManager.CounterAttackPosition.position;
                    targetPos.x = -handPositionManager.CounterAttackPosition.position.x;

                    float curTime = 0f;
                    Vector3 curPos = transform.position;
                    PlayCounterSwingSound();
                    currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), targetPos, bossOne.BossData.CounterAttackMoveTime)
                    .SetEase(bossOne.BossData.CounterAttackMoveEase).SetUpdate(UpdateType.Fixed)
                    .OnUpdate(() =>
                    {
                        curTime += Time.deltaTime;
                        float t = curTime / bossOne.BossData.CounterAttackMoveTime;
                        TimeManager.Instance.TimeScale = bossOne.BossData.CounterAttackBulletTimeCurve.Evaluate(t);
                        TimeManager.Instance.PlayerTimeScale = bossOne.BossData.CounterAttackBulletPlayerTimeCurve.Evaluate(t);

                        //  중간의 캔슬
                        if (totalCounterDamage > bossOne.BossData.RequiredCounterDamage)
                        {
                            ShowHelpCounterSuccessClientRPC();
                            if (bossOne.IsPhase2)
                            {
                                SetPhase2CounterColliderClientRPC(false);
                            }
                            bossOne.PlayHandPatternBodyAnimation("CounterSuccess");
                            networkAnimator.SetTrigger("HandCounterSuccess");
                            StopCounterVignetteClientRPC();
                            currentTween.Kill();
                            SetCounterFXClientRPC(false);
                            bossOne.sound.PlayNeutralizeClientRPC();
                            StopCounterSwingSound();
                            PlayNeutralizeEffectClientRPC();
                            SetAttacking(false);
                            isCounterAttack = false;
                            counterMeshDemolisherTools.ForEach(x => x.Appear(0));
                            DOTween.To(() => TimeManager.Instance.TimeScale, x => TimeManager.Instance.TimeScale = x, TimeManager.Instance.OriginTimeScale, 0.2f);
                            DOTween.To(() => TimeManager.Instance.PlayerTimeScale, x => TimeManager.Instance.PlayerTimeScale = x, TimeManager.Instance.OriginTimeScale, 0.2f);
                            targetPos = handPositionManager.GrabSuccessAttackNotPlatformPosition.position + Vector3.down * 1.5f;
                            targetPos.x = transform.position.x;
                            currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), targetPos, bossOne.BossData.CounterAttackMoveBackTime).SetEase(bossOne.BossData.CounterAttackMoveBackEase).SetUpdate(UpdateType.Fixed)
                            .OnComplete(() =>
                            {
                                PlayNeutralizeDown();
                                currentTween = DOVirtual.DelayedCall(bossOne.BossData.CounterAttackGroggyTime, () =>
                                {
                                    CompleteCounter();
                                });
                            });
                        }
                    })
                    .OnComplete(() =>
                    {
                        SetAttacking(false);
                        isCounterAttack = false;
                        TimeManager.Instance.SetOriginTimeScale();
                        TimeManager.Instance.SetOriginPlayerTimeScale();
                        StopCounterVignetteClientRPC();
                        CompleteCounter();
                    });
                });
            });
        }

        private void CompleteCounter()
        {
            if (bossOne.IsPhase2)
            {
                SetPhase2CounterColliderClientRPC(false);
            }
            currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), originPos, bossOne.BossData.CounterAttackSignalMoveTime).SetEase(bossOne.BossData.CounterAttackSignalEase).SetUpdate(UpdateType.Fixed)
            .OnComplete(() =>
            {
                bossOne.SetIsNotNeutralizeChangeClientRPC(false);
                bossOne.PatternComplete();
            });
        }

        public void MovePosition(Vector3 position, float time, Ease easeType = Ease.Linear)
        {
            PlayHandMoveSound();

            currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), position, time).SetEase(easeType).SetUpdate(UpdateType.Fixed)
            .OnComplete(() =>
            {
                StopHandMoveSound();
            });
        }

        public void MoveNeutralizePosition(float time, bool isPreview = false)
        {
            Vector3 vector = handPositionManager.NeutralPosition.position;
            if (isPreview)
                vector.y += 20;

            currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), vector, time).SetEase(
                isPreview ? Ease.OutSine : Ease.InSine).SetUpdate(UpdateType.Fixed)
            .OnComplete(() => { if (!isPreview) PlayNeutralizeDown(); });
        }

        public void MoveOriginPosition(float time)
        {
            PlayHandMoveSound();
            currentTween = DOTween.To(() => transform.position, x => rb.MovePosition(x), originPos, time).SetEase(easeType).SetUpdate(UpdateType.Fixed)
            .OnComplete(() =>
            {
                StopHandMoveSound();
            });
        }

        public void ClashDownAttack()
        {
            ExplosionYShakeClientRPC(Vector3.down, 1);

            WaitForUpdateUniTask(() =>
            {
                PlayWaveEffectClientRPC(wavePosition.position);
                bossOne.GroundEmissionAnimationClientRPC();
                BoxCollider boxCollider = waveEffect.GetComponent<BoxCollider>();
                foreach (var collision in Physics.OverlapBox(wavePosition.position + Vector3.up * boxCollider.center.z, boxCollider.bounds.size * 0.5f, Quaternion.identity))
                {
                    if (collision.TryGetComponent(out PlayerBase playerBase))
                    {
                        playerBase.TakeDamage(100);
                    }
                }
            }).Forget();
        }

        public void Neutralize()
        {
            SetAttacking(false);
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
            isCounterAttack = false;
            isGrabbing = false;
            isGrabAttack = false;
            currentTween.Kill();
            networkAnimator.SetTrigger("Neutralize");
            if (downTargetAttackCoroutine != null)
                StopCoroutine(downTargetAttackCoroutine);
            if (grabPlayer != null)
            {
                grabPlayer.GrabClientRPC(false);
                grabPlayer = null;
                grabDummyRia.SetActive(false);
                grabDummyNia.SetActive(false);
            }
        }

        public void DissolveEnd()
        {
            originPos = handPositionManager.OriginPhase2Position.position;
            transform.position = originPos + Vector3.down * 20;
            var list = skinnedMeshRenderer.materials.ToList();
            list[0] = hologramMaterial;
            skinnedMeshRenderer.SetMaterials(list);
        }

        #region ClientRpc

        [ClientRpc]
        public void GrabSetPosClientRPC(ulong clientId, bool isGrab = true)
        {
            if (!IsServer)
                grabPlayer = InGamePlayManager.Instance.GetPlayer(clientId);

            grabPlayer.transform.position = new Vector3(grabPosition.position.x, grabPosition.position.y, 0);
            if (grabPlayer.PlayerType == PlayerType.LASER)
            {
                grabDummyRia.SetActive(true);
            }
            else
            {
                grabDummyNia.SetActive(true);
            }

            if (!isGrab)
            {
                grabPlayer = null;
                grabDummyRia.SetActive(false);
                grabDummyNia.SetActive(false);
            }
        }

        [ClientRpc]
        private void PlayWarningEffectClientRPC(Vector3 position, bool onlyPosition = false)
        {
            warningEffect.transform.position = position;
            if (!onlyPosition)
            {
                warningEffect.Play();
                bossOne.sound.PlayForeshadowClientRPC(position);
            }
        }

        [ClientRpc]
        private void PlayWaveEffectClientRPC(Vector3 position)
        {
            waveEffect.transform.position = position;
            waveEffect.Play();
        }

        [ClientRpc]
        private void PlayWarningSwingEffectClientRPC(float time)
        {
            bossOne.sound.PlayForeshadowClientRPC(new Vector3(0, -4, 0));
            WarningSwingSetFixedTime(time);
            warningSwingEffect.Play();
        }

        private async UniTaskVoid WaitForUpdateUniTask(Action action)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
            action();
        }

        public void MeshDemolisherDisappear(MeshDemolisherTool meshDemolisherTool, float time, float appearTime = 0)
        {
            meshDemolisherTool.Disappear(time, appearTime);
            Vector3 explosionShakeDirection = meshDemolisherTool.transform.position - CameraManager.Instance.transform.position;
            explosionShakeDirection.z = 0;
            explosionShakeDirection.Normalize();
            ExplosionXShakeClientRPC(explosionShakeDirection, 0.5f);
        }

        [ClientRpc]
        private void ExplosionXShakeClientRPC(Vector3 dir, float power)
        {
            dir.z = 0;
            CameraManager.Instance.ExplosionXCameraShake(dir, power);
        }

        [ClientRpc]
        private void ExplosionYShakeClientRPC(Vector3 dir, float power)
        {
            dir.z = 0;
            CameraManager.Instance.ExplosionYCameraShake(dir, power);
        }

        [ClientRpc]
        private void StartCounterVignetteClientRPC()
        {
            CameraManager.Instance.StartCounterVignette(1.2f);
        }

        [ClientRpc]
        private void StopCounterVignetteClientRPC()
        {
            CameraManager.Instance.StopCounterVignette(2);
        }

        [ClientRpc]
        private void SetCounterFXClientRPC(bool isPlay)
        {
            if (isPlay)
                counterFXHandler.Play();
            else
                counterFXHandler.Stop();
        }

        [ClientRpc]
        public void PlayNeutralizeEffectClientRPC()
        {
            neutralizeEffect.transform.position = transform.position;
            neutralizeEffect.Play();
        }

        [ClientRpc]
        private void WarningEffectSetFixedTimeClientRPC(float time)
        {
            warningEffectTimingToolManager.SetFixedTime(time);
        }

        private void WarningSwingSetFixedTime(float time)
        {
            warningSwingEffectTimingToolManager.SetFixedTime(time);
        }

        public void GetDamage(int damage, PlayerName playerName)
        {
            bossOne.lastDamagedBossParts = BossPart;
            totalCounterDamage += damage;
            if (!bossOne.IsPhase2)
                bossOne.GetDamage(damage, playerName);
        }

        public void GetNeutralize(int neutralize, PlayerName playerName)
        {
            if (!bossOne.IsPhase2)
                bossOne.GetNeutralize(neutralize, playerName);
        }

        public void AffectByExplosion(Vector3 explosionCenterPosition,
            LauncherBaseData.ExplosionData explosionData, int damage, int neutralizeValue, PlayerName playerName)
        {
            GetDamage(damage, playerName);
            GetNeutralize(neutralizeValue, playerName);
        }

        private void WarningEffectDownRayCasting(Vector3 warningPos, bool onlyPosition = false)
        {
            warningPos.z = 0;
            Physics.Raycast(warningPos, Vector3.down, out RaycastHit hit, 100, LayerMask.GetMask("Ground"));
            PlayWarningEffectClientRPC(new Vector3(hit.point.x, hit.point.y, 0), onlyPosition);
        }

        private bool PredictClientPlayerPosition(PlayerBase playerBase)
        {
            //ANCHOR - 클라이언트 예측
            if (!playerBase.IsPlayerServer)
            {
                Vector3 velocity = playerBase.playerComp.Velocity.Value;
                Vector3 offset = velocity * (float)TimeManager.Instance.ClientServerTimeOffset;
                Vector3 predictPos = playerBase.transform.position + offset;

                BoxCollider playerCollider = playerBase.playerComp.ClientNetworkColliderComp;
                Vector3 boxCenter = playerCollider.bounds.center + offset;
                Vector3 boxHalfExtents = playerCollider.size * 0.5f;
                Quaternion boxOrientation = playerCollider.transform.rotation;
                bool isColliding = Physics.CheckBox(boxCenter, boxHalfExtents, boxOrientation, LayerMask.GetMask("Boss"));

                if (!isColliding)
                {
                    return true;
                }
                GetCapsuleColliderPoints(capsuleCollider, out Vector3 point1, out Vector3 point2);
            }
            return false;
        }

        void GetCapsuleColliderPoints(CapsuleCollider capsule, out Vector3 point1, out Vector3 point2)
        {
            // 캡슐의 월드 좌표에서의 중심 위치
            Vector3 center = capsule.transform.TransformPoint(capsule.center);

            // 캡슐 방향에 따른 로컬 축
            Vector3 dir = Vector3.zero;
            switch (capsule.direction)
            {
                case 0: // x축
                    dir = capsule.transform.right;
                    break;
                case 1: // y축 (기본)
                    dir = capsule.transform.up;
                    break;
                case 2: // z축
                    dir = capsule.transform.forward;
                    break;
            }

            // 캡슐의 절반 높이에서 반지름을 뺀 값 (캡슐의 구 부분이 아닌 실린더 부분의 끝까지의 거리)
            float height = Mathf.Max(0, (capsule.height / 2) - capsule.radius);

            // point1, point2 계산
            point1 = center + dir * height;
            point2 = center - dir * height;
        }

        public void SetPhase2ColliderOff()
        {
            meshCollider.isTrigger = true;
            meshCollider.tag = "Untagged";
            tag = "Untagged";
            capsuleCollider.enabled = false;
            SetAttacking(false);
        }

        [ClientRpc]
        private void SetPhase2CounterColliderClientRPC(bool enable)
        {
            if (enable)
            {
                meshCollider.tag = "Boss";
                tag = "Boss";
            }
            else
            {
                meshCollider.tag = "Untagged";
                tag = "Untagged";
            }
        }

        #endregion

        #region Sound
        private void PlayHandMoveSound()
        {
            if (BossPart == EBossParts.RIGHT_ARM)
                bossOne.sound.PlayRightHandMoveClientRPC();
            else
                bossOne.sound.PlayLeftHandMoveClientRPC();
        }

        private void StopHandMoveSound()
        {
            if (BossPart == EBossParts.RIGHT_ARM)
                bossOne.sound.StopRightHandMoveClientRPC();
            else
                bossOne.sound.StopLeftHandMoveClientRPC();
        }

        private void PlayHandSwingSound()
        {
            if (BossPart == EBossParts.RIGHT_ARM)
                bossOne.sound.PlayRightHandSwingClientRPC();
            else
                bossOne.sound.PlayLeftHandSwingClientRPC();
        }

        private void PlayCounterSwingSound()
        {
            if (BossPart == EBossParts.RIGHT_ARM)
                bossOne.sound.PlayRightCounterAttackClientRPC();
            else
                bossOne.sound.PlayLeftCounterAttackClientRPC();
        }

        private void StopCounterSwingSound()
        {
            if (BossPart == EBossParts.RIGHT_ARM)
                bossOne.sound.StopRightCounterAttackClientRPC();
            else
                bossOne.sound.StopLeftCounterAttackClientRPC();
        }

        private void StopHandSwingSound()
        {
            if (BossPart == EBossParts.RIGHT_ARM)
                bossOne.sound.StopRightHandSwingClientRPC();
            else
                bossOne.sound.StopLeftHandSwingClientRPC();
        }

        private void PlayDownAttackSound()
        {
            if (BossPart == EBossParts.RIGHT_ARM)
                bossOne.sound.PlayRightHandDownAttackClientRPC();
            else
                bossOne.sound.PlayLeftHandDownAttackClientRPC();
        }

        private void PlayGrabSound()
        {
            if (BossPart == EBossParts.RIGHT_ARM)
                bossOne.sound.PlayRightHandGrabClientRPC();
            else
                bossOne.sound.PlayLeftHandGrabClientRPC();
        }

        private void PlayNeutralizeDown()
        {
            if (BossPart == EBossParts.RIGHT_ARM)
                bossOne.sound.PlayRightNeutralizeDownClientRPC();
            else
                bossOne.sound.PlayLeftNeutralizeDownClientRPC();
        }
        #endregion

        #region HelpWindow

        [ClientRpc]
        private void ShowHelpCounterAttackClientRPC()
        {
            if (!bossOne.IsHelpFirstCounter)
            {
                bossOne.IsHelpFirstCounter = true;
                HelpWindowUI.Instance.RequestHelpText(HelpWindowUI.HelpType.COUNTER);
            }
        }

        [ClientRpc]
        private void ShowHelpCounterSuccessClientRPC()
        {
            if (!bossOne.IsHelpFirstCounterSuccess)
            {
                bossOne.IsHelpFirstCounterSuccess = true;
                HelpWindowUI.Instance.RequestHelpText(HelpWindowUI.HelpType.COUNTER_SUCCESS);
            }
        }
        #endregion
    }
}
