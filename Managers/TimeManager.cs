using System;
using System.ComponentModel;
using Unity.Netcode;
using UnityEngine;

namespace BirdCase
{
    [RequireComponent(typeof(NetworkObject))]
    public class TimeManager : NetworkSingleton<TimeManager>
    {
        [ReadOnly(true)] private const float DEFAULT_TIMESCALE = 1.0f;
        private NetworkVariable<float> timeScale = new NetworkVariable<float>(DEFAULT_TIMESCALE);
        private NetworkVariable<float> playerTimeScale = new NetworkVariable<float>(DEFAULT_TIMESCALE);
        public float OriginTimeScale { get; private set; } = DEFAULT_TIMESCALE;
        public float PreviousTimeScale { get; private set; }
        public bool AbleSetTimeScale { get; set; } = true;

#if UNITY_EDITOR
        [Range(0.0f, 1.0f)]
        public float timeScaleEditor = DEFAULT_TIMESCALE;
        [Range(0.0f, 1.0f)]
        public float playerTimeScaleEditor = DEFAULT_TIMESCALE;

        private void OnValidate()
        {
            timeScale.Value = timeScaleEditor;
            playerTimeScale.Value = playerTimeScaleEditor;
        }
#endif

        public float TimeScale
        {
            get
            {
                return timeScale.Value;
            }
            set
            {
                if (AbleSetTimeScale)
                {
                    timeScale.Value = value;
                    Time.timeScale = timeScale.Value;
                }
            }
        }

        public float PlayerTimeScale
        {
            get
            {
                return playerTimeScale.Value;
            }
            set
            {
                if(!IsServer)
                    return;
                    
                if (AbleSetTimeScale)
                {
                    playerTimeScale.Value = value;
                    OnPlayerTimeScaleChanged?.Invoke(playerTimeScale.Value);
                }
            }
        }

        public event Action<float> OnPlayerTimeScaleChanged;
        public event Action<float> OnTimeScaleChanged;

        public double ClientServerTimeOffset { get; private set; } = 0;
        public float ClientServerTimeOffsetAsFloat { get; private set; } = 0;
        private void Update()
        {
            if (NetworkManager == null)
                return;
            
            if (!IsServer && NetworkManager.IsConnectedClient)
            {
                SendServerTimeToServerRPC(NetworkManager.ServerTime.Time);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendServerTimeToServerRPC(double localTime)
        {
            double time = NetworkManager.ServerTime.Time - localTime;
            ClientServerTimeOffset = Clamp(time, 0, 0.2);
            ClientServerTimeOffsetAsFloat = (float)ClientServerTimeOffset;
        }

        protected override void OnAwake() { }

        private void Start()
        {
            timeScale.OnValueChanged += OnTimeScaleValueChanged;
            playerTimeScale.OnValueChanged += OnPlayerTimeScaleValueChanged;
            
            SetFixedDeltaTime(DEFAULT_TIMESCALE);
        }

        private void OnTimeScaleValueChanged(float previousValue, float newValue)
        {
            Time.timeScale = newValue;
            SetFixedDeltaTime(newValue);
            CameraManager.Instance?.ChangeTimeDamping();
#if UNITY_EDITOR
            timeScaleEditor = newValue;
#endif
        }

        private void OnPlayerTimeScaleValueChanged(float previousValue, float newValue)
        {
            OnTimeScaleChanged?.Invoke(newValue);
            CameraManager.Instance?.ChangeTimeDamping();
            SoundManager.Instance.pitch = newValue;
#if UNITY_EDITOR
            playerTimeScaleEditor = newValue;
#endif
        }

        public float GetPlayerTimeScaleValue()
        {
            if (TimeScale == 0)
                return 0;
            return PlayerTimeScale / TimeScale;
        }

        private void SetFixedDeltaTime(float scale)
        {
            int frameRate = Application.targetFrameRate;
            if (frameRate < 0)
                frameRate = 60;

            float fixedDeltaTime = scale / frameRate;
            // 0.00185f 는 모니터 주사율 540hz 기준 ㄷㄷ
            Time.fixedDeltaTime = Mathf.Max(fixedDeltaTime, 0.00185f);
        }

        public void SetForceTimeScale(float scale)
        {
            bool origin = AbleSetTimeScale;
            AbleSetTimeScale = true;
            TimeScale = scale;
            AbleSetTimeScale = origin;
        }

        public void SetOriginTimeScale()
        {
            TimeScale = OriginTimeScale;
        }

        public void SetOriginPlayerTimeScale()
        {
            PlayerTimeScale = OriginTimeScale;
        }

        public float GetDeltaTime()
        {
            return Time.deltaTime;
        }

        public float GetUnscaledDeltaTime()
        {
            return Time.unscaledDeltaTime;
        }

        public float GetFixedDeltaTime()
        {
            return Time.fixedDeltaTime;
        }

        public float GetUnscaledFixedDeltaTime()
        {
            return Time.fixedUnscaledDeltaTime;
        }

        public float GetTime()
        {
            return Time.time;
        }

        public float GetUnscaledTime()
        {
            return Time.unscaledTime;
        }

        public static double Clamp(double value, double min, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}
