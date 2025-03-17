using System;
using Cysharp.Threading.Tasks;
using FMOD.Studio;
using FMODUnity;
using Mono.CSharp;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BirdCase
{
    public class CharacterSelection : NetworkBehaviour
    {
        [SerializeField]
        private float gameStartDelay = 5;
        
        [Header("RIA")]
        [SerializeField]
        private GameObject ria;
        [SerializeField]
        private GameObject riaLight;
        [SerializeField]
        private Material riaSelectedMaterial;
        [SerializeField]
        private Material riaWallMaterial;
        [SerializeField]
        private Material riaSelectedFloorMaterial;
        [SerializeField]
        private Material riaFloorMaterial;
        [SerializeField]
        private MeshRenderer riaWallRenderer;
        [SerializeField]
        private MeshRenderer riaFloorRenderer;

        [Header("NIA")]
        [SerializeField]
        private GameObject nia;
        [SerializeField]
        private GameObject niaLight;
        [SerializeField]
        private Material niaSelectedMaterial;
        [SerializeField]
        private Material niaWallMaterial;
        [SerializeField]
        private Material niaSelectedFloorMaterial;
        [SerializeField]
        private Material niaFloorMaterial;
        [SerializeField]
        private MeshRenderer niaWallRenderer;
        [SerializeField]
        private MeshRenderer niaFloorRenderer;

        [Header("UI")]
        [SerializeField]
        private GameObject riaMe;
        [SerializeField]
        private GameObject niaMe;
        [SerializeField]
        private TextMeshProUGUI gameStartTimerText;
        [SerializeField]
        private ActivePanelEffect timerPanel;
        
        [Header("Button")]
        [SerializeField]
        private Button riaButton;
        [SerializeField]
        private Button niaButton;

        [Header("Sound")] 
        [SerializeField] 
        private EventReference riaSelectSound;
        [SerializeField]
        private EventReference niaSelectSound;
        [SerializeField] 
        private EventReference timerSound;
        
        [Header("Cursor")]
        [SerializeField]
        private Texture2D riaCursorTexture;
        [SerializeField]
        private Texture2D niaCursorTexture;

        private Animator riaAnimator;
        private Animator niaAnimator;
        private MeshRenderer riaMeshRenderer;
        private MeshRenderer niaMeshRenderer;
        
        private NetworkVariable<ulong> riaSelectedPlayerId = new NetworkVariable<ulong>(ulong.MaxValue, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<ulong> niaSelectedPlayerId = new NetworkVariable<ulong>(ulong.MaxValue, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        private NetworkVariable<float> gameStartTimer = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private bool isPlayersReady = false;
        private bool isCharacterSelected = false;

        private Vector2 cursorHotspot;

        private Tutorial tutorial;
        
        private EventInstance timerSoundInstance;
        
        private void Awake()
        {
            tutorial = FindFirstObjectByType<Tutorial>();
            riaAnimator = ria.GetComponentInChildren<Animator>();
            niaAnimator = nia.GetComponentInChildren<Animator>();
            cursorHotspot = new Vector2(riaCursorTexture.width * 0.5f, riaCursorTexture.height * 0.5f);
        }
        
        private void Start()
        {
            SoundManager.Instance.PlayCharacterSelectionBGM();
            timerPanel.gameObject.SetActive(false);
            DisableFacial(riaAnimator).Forget();
            DisableFacial(niaAnimator).Forget();
            riaLight.SetActive(false);
            niaLight.SetActive(false);
            riaWallRenderer.material = riaWallMaterial;
            niaWallRenderer.material = niaWallMaterial;
            riaFloorRenderer.material = riaFloorMaterial;
            niaFloorRenderer.material = niaFloorMaterial;
            riaMe.SetActive(false);
            niaMe.SetActive(false);
            
            riaButton.onClick.AddListener(() =>
            {
                SelectRiaServerRPC(NetworkManager.Singleton.LocalClientId);
            });
            
            niaButton.onClick.AddListener(() =>
            {
                SelectNiaServerRPC(NetworkManager.Singleton.LocalClientId);
            });
            
            riaSelectedPlayerId.OnValueChanged += (ulong oldId, ulong newId) =>
            {
                if (newId == ulong.MaxValue)
                {
                    riaMe.SetActive(false);

                    isPlayersReady = false;
                    
                    RiaSelection(false);
                    if(oldId == NetworkManager.Singleton.LocalClientId && niaSelectedPlayerId.Value != NetworkManager.Singleton.LocalClientId)
                        ChangeCursor(false, true);
                }
                else if(newId == NetworkManager.Singleton.LocalClientId)
                {
                    riaMe.SetActive(true);

                    SoundManager.Instance.Play(riaSelectSound, SoundManager.Banks.SFX);
                    RiaSelection(true);
                    ChangeCursor(true, true);
                }
                else if (newId != NetworkManager.Singleton.LocalClientId)
                {
                    riaMe.SetActive(false);
                    
                    RiaSelection(true);
                }
                else
                {
                    Debug.LogError("Invalid Player Id");
                }
            };
            
            niaSelectedPlayerId.OnValueChanged += (ulong oldId, ulong newId) =>
            {
                if (newId == ulong.MaxValue)
                {
                    niaMe.SetActive(false);

                    isPlayersReady = false;
                    
                    NiaSelection(false);
                    if(oldId == NetworkManager.Singleton.LocalClientId && riaSelectedPlayerId.Value != NetworkManager.Singleton.LocalClientId)
                        ChangeCursor(false, false);
                }
                else if(newId == NetworkManager.Singleton.LocalClientId)
                {
                    niaMe.SetActive(true);

                    SoundManager.Instance.Play(niaSelectSound, SoundManager.Banks.SFX);
                    NiaSelection(true);
                    ChangeCursor(true, false);
                }
                else if (newId != NetworkManager.Singleton.LocalClientId)
                {
                    niaMe.SetActive(false);
                    
                    NiaSelection(true);
                }
                else
                {
                    Debug.LogError("Invalid Player Id");
                }
            };
            
            gameStartTimer.OnValueChanged += (float oldTime, float newTime) =>
            {
                if (newTime == 0)
                {
                    if (timerSoundInstance.isValid())
                    {
                        timerSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    }
                    gameStartTimerText.text = gameStartDelay.ToString();
                    timerPanel.gameObject.SetActive(false);
                }
                else if(oldTime == 0 && newTime != 0)
                {
                    timerPanel.gameObject.SetActive(true);
                    if (timerSoundInstance.isValid())
                    {
                        timerSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    }
                    timerSoundInstance = SoundManager.Instance.Play(timerSound, SoundManager.Banks.SFX);
                    gameStartTimerText.text = gameStartDelay.ToString();
                }
                else
                {
                    gameStartTimerText.text = (gameStartDelay - gameStartTimer.Value).ToString("0");
                }
            };
        }

        private async UniTaskVoid GameStartTimer()
        {
            isPlayersReady = true;
            while (gameStartTimer.Value < gameStartDelay)
            {
                gameStartTimer.Value += Time.unscaledDeltaTime;

                if (!isPlayersReady)
                {
                    gameStartTimer.Value = 0;
                    return;
                }
                
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: this.GetCancellationTokenOnDestroy());
            }
            
            timerPanel.gameObject.SetActive(false);
            isCharacterSelected = true;
            GameManager.Instance.SetCharacterId(riaSelectedPlayerId.Value, niaSelectedPlayerId.Value);
            tutorial.StartTutorialClientRPC(riaSelectedPlayerId.Value, niaSelectedPlayerId.Value);
        }
        
        private void ChangeCursor(bool isSelect, bool isRia)
        {
            Cursor.SetCursor(isSelect ? (isRia ? riaCursorTexture : niaCursorTexture) : null, cursorHotspot, CursorMode.ForceSoftware);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SelectNiaServerRPC(ulong clientId)
        {
            if (niaSelectedPlayerId.Value == clientId)
            {
                niaSelectedPlayerId.Value = ulong.MaxValue;
            }
            else if (niaSelectedPlayerId.Value == ulong.MaxValue)
            {
                niaSelectedPlayerId.Value = clientId;
                
                if (riaSelectedPlayerId.Value == clientId)
                {
                    riaSelectedPlayerId.Value = ulong.MaxValue;
                }
                else if(riaSelectedPlayerId.Value != clientId && riaSelectedPlayerId.Value != ulong.MaxValue && !isPlayersReady)
                {
                    GameStartTimer().Forget();
                }
            }
        }
        
        private void NiaSelection(bool isSelect)
        {
            niaLight.SetActive(isSelect);
            if (isSelect)
            {
                niaWallRenderer.material = niaSelectedMaterial;
                niaFloorRenderer.material = niaSelectedFloorMaterial;
            }
            else
            {
                niaWallRenderer.material = niaWallMaterial;
                niaFloorRenderer.material = niaFloorMaterial;
            }
            niaAnimator.SetBool("IsSelect", isSelect);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SelectRiaServerRPC(ulong clientId)
        {
            if (riaSelectedPlayerId.Value == clientId)
            {
                riaSelectedPlayerId.Value = ulong.MaxValue;
            }
            else if (riaSelectedPlayerId.Value == ulong.MaxValue)
            {
                riaSelectedPlayerId.Value = clientId;
                
                if (niaSelectedPlayerId.Value == clientId)
                {
                    niaSelectedPlayerId.Value = ulong.MaxValue;
                }
                else if(niaSelectedPlayerId.Value != clientId && niaSelectedPlayerId.Value != ulong.MaxValue && !isPlayersReady)
                {
                    GameStartTimer().Forget();
                }
            }
        }

        private void RiaSelection(bool isSelect)
        {
            riaLight.SetActive(isSelect);
            if (isSelect)
            {
                riaWallRenderer.material = riaSelectedMaterial;
                riaFloorRenderer.material = riaSelectedFloorMaterial;
            }
            else
            {
                riaWallRenderer.material = riaWallMaterial;
                riaFloorRenderer.material = riaFloorMaterial;
            }
            riaAnimator.SetBool("IsSelect", isSelect);
        }

        private async UniTaskVoid DisableFacial(Animator animator)
        {
            while (!isCharacterSelected)
            {
                if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Facial"))
                {
                    animator.SetLayerWeight(1, 0);
                }
                else
                {
                    animator.SetLayerWeight(1, 1);
                }
                
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: this.GetCancellationTokenOnDestroy());
            }
        }
    }
}
