using System;
using MyBox;
using UnityEngine;

namespace BirdCase
{
    [CreateAssetMenu(menuName = "Datas/PlayerData/PlayerDefaultData")]
    public class PlayerDefaultData : ScriptableObject
    {
        [Header("Player Stats")] 
        [OverrideLabel("최대 체력"), SerializeField] 
        [Tooltip("플레이어의 최대 체력입니다")]
        private int maxHP = 5;

        public int MaxHP => maxHP;

        [OverrideLabel("피격 무적 시간"), SerializeField]
        [Tooltip("플레이어가 피격당했을 때 잠시 무적이 되는 시간입니다")]
        private float damagedInvincibleTime = 0.5f;
        public float DamagedInvincibleTime => damagedInvincibleTime;
        
        [OverrideLabel("바닥 레이어"), SerializeField]
        [Tooltip("플레이어가 땅으로 인식할 레이어입니다 (점프가 가능한 곳)")]
        private LayerMask groundLayer;
        public LayerMask GroundLayer => groundLayer;
        
        public float GravityStrength { get; private set; }
        public float GravityScale { get; private set; }
        [Space(20), Header("Gravity")] 
        [OverrideLabel("중력 가속도"), SerializeField] 
        [Tooltip("플레이어의 기본 중력 가속도입니다")]
        private float fallGravityMultiple = 1; 
        public float FallGravityMultiple => fallGravityMultiple;
        
        [OverrideLabel("최대 낙하 속도"), SerializeField] 
        [Tooltip("플레이어가 떨어질 때 최대로 제한된 속도입니다")]
        private float maxFallSpeed = 20;
        public float MaxFallSpeed => maxFallSpeed;
        
        
        [Space(20), Header("Movement")]
        [OverrideLabel("움직임 속도"), SerializeField] 
        [Tooltip("플레이어의 기본 움직임 속도입니다")]
        private float moveSpeed = 4f;
        public float MoveSpeed => moveSpeed;
        
        [OverrideLabel("걷기 가속력"), SerializeField] 
        private float runAcceleration = 5;
        public float RunAccelAmount { get; private set; }

        [OverrideLabel("걷기 제동력"), SerializeField] 
        private float runDeceleration = 10;
        public float RunDecelAmount { get; private set; }

        [Space(5)] 
        [OverrideLabel("공중에 있을 때의 가속력 배율"), SerializeField, Range(0f, 1)]
        private float accelInAir = 0.8f;
        public float AccelInAir => accelInAir;
        
        [OverrideLabel("공중에 있을 때의 제동력 배율"), SerializeField, Range(0f, 1)]
        private float decelInAir = 0.8f;
        public float DecelInAir => decelInAir;
        
        [Space(5)]
        [Tooltip("최고 속력을 넘어도 플레이어가 입력중인 방향의 움직임이면 제동을 걸지 않음")]
        [OverrideLabel("이동 시 최고 속력 제동 여부"), SerializeField] 
        private bool doConserveMomentum = true;
        public bool DoConserveMomentum => doConserveMomentum;

        [Space(20), Header("Jump")] 
        [OverrideLabel("점프 높이"), SerializeField] 
        private float jumpHeight = 7;

        [OverrideLabel("점프 완료까지 걸리는 시간"), SerializeField]
        private float jumpTimeToApex = 0.3f;
        public float JumpForce { get; private set; }

        [OverrideLabel("점프행 중력 가속도"), SerializeField, Range(0f, 1)] 
        private float jumpHangGravityMultiple = 0.5f;
        public float JumpHangGravityMultiple => jumpHangGravityMultiple;

        [OverrideLabel("점프행 시 점프행 중력 가속도를 허용하는 속력"), SerializeField] 
        [Tooltip("작으면 작을 수록 점프 가속도가 0에 가까워야 함")]
        private float jumpHangTimeThreshold = 0.1f;
        public float JumpHangTimeThreshold => jumpHangTimeThreshold;

        [OverrideLabel("점프행 유지 시간"), SerializeField] 
        private float jumpHangTime = 0.1f;
        public float JumpHangTime => jumpHangTime;

        [OverrideLabel("점프행 움직임 가속"), SerializeField, Min(0.1f)] 
        private float jumpHangAccelerationMultiple = 1;
        public float JumpHangAccelerationMultiple => jumpHangAccelerationMultiple;

        [OverrideLabel("점프행 움직임 속력 가속"), SerializeField, Min(0.1f)] 
        private float jumpHangMaxSpeedMultiple = 1;
        public float JumpHangMaxSpeedMultiple => jumpHangMaxSpeedMultiple;
        
        [Space(20), Header("Dead & Revive")] 
        [OverrideLabel("자동 부활 시간"), SerializeField] 
        private float autoReviveTime = 15f;
        public float AutoReviveTime => autoReviveTime;
        
        [OverrideLabel("부활 키 입력 시간"), SerializeField] 
        private float reviveInputTime = 5f;
        public float ReviveInputTime => reviveInputTime;
        
        [OverrideLabel("부활 가능 사거리"), SerializeField]
        private float reviveRange = 3f;
        public float ReviveRange => reviveRange;
        
        [OverrideLabel("부활 이후 무적 시간"), SerializeField]
        private float reviveInvincibleTime = 1f;
        public float ReviveInvincibleTime => reviveInvincibleTime;
        
        [OverrideLabel("부활 시킨 이후 쿨타임"), SerializeField]
        private float reviveCoolTime = 2f;
        public float ReviveCoolTime => reviveCoolTime;

        [Space(20), Header("Assists")]
        [Tooltip("땅이나 벽에서 떨어졌을 때 점프가 가능한 시간")]
        [OverrideLabel("코요테 타임"), SerializeField]
        private float coyoteTime;
        public float CoyoteTime => coyoteTime;

        [Tooltip("점프 선 입력시 이 시간안에 땅에 닿을 시 점프")]
        [OverrideLabel("점프 입력 버퍼 시간"), SerializeField]
        private float jumpBufferTime;
        public float JumpBufferTime => jumpBufferTime;
        
        [Tooltip("플레이어가 받는 y축의 힘을 제한합니다 (점프, 폭발, 넉백 모두모두 포함)")]
        [OverrideLabel("플레이어 y velocity값 제한"), SerializeField]
        private float playerYVelocityLimit = 20f;
        public float PlayerYVelocityLimit => playerYVelocityLimit;
        
        [Space(20), Header("Shield")]
        [OverrideLabel("보호막 지속 시간"), SerializeField]
        private float shieldDuration = 3f;
        public float ShieldDuration => shieldDuration;
        
        [OverrideLabel("보호막 쿨타임"), SerializeField]
        private float shieldCooldownTime = 5f;
        public float ShieldCoolDownTime => shieldCooldownTime;
        
        [OverrideLabel("보호막 활성화 시 이동 속도 감소율"), SerializeField, Range(0f, 1)]
        private float shieldMoveSpeedDecrease = 0.3f;
        public float ShieldMoveSpeedDecrease => shieldMoveSpeedDecrease;
        
        [OverrideLabel("보호막 활성화 시 점프 높이 감소율"), SerializeField, Range(0f, 1)]
        private float shieldJumpHeightDecrease = 0.3f;
        public float ShieldJumpHeightDecrease => shieldJumpHeightDecrease;
        
        [OverrideLabel("보호막 파괴 시 재생 시간"), SerializeField]
        private float shieldRegenTime = 15f;
        public float ShieldRegenTime => shieldRegenTime;
        
        [OverrideLabel("보호막 파괴 시 무적 시간"), SerializeField]
        private float invincibleTimeAfterShieldBroken = 0.5f;
        public float InvincibleTimeAfterShieldBroken => invincibleTimeAfterShieldBroken;
        
        private void OnValidate()
        {
            //Calculate gravity strength using the formula (gravity = 2 * jumpHeight / timeToJumpApex^2) 
            GravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);

            //Calculate the rigidbody's gravity scale (ie: gravity strength relative to unity's gravity value, see project settings/Physics2D)
            GravityScale = GravityStrength / Physics.gravity.y;

            //Calculate are run acceleration & deceleration forces using formula: amount = ((1 / Time.fixedDeltaTime) * acceleration) / runMaxSpeed
            RunAccelAmount = (50 * runAcceleration) / moveSpeed;
            RunDecelAmount = (50 * runDeceleration) / moveSpeed;

            //Calculate jumpForce using the formula (initialJumpVelocity = gravity * timeToJumpApex)
            JumpForce = Mathf.Abs(GravityStrength) * jumpTimeToApex;

            runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, moveSpeed);
            runDeceleration = Mathf.Clamp(runDeceleration, 0.01f, moveSpeed);
        }

        private void Awake()
        {
            OnValidate();
        }
    }
}
