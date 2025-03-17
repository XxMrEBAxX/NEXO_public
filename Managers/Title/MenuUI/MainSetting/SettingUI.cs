using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

namespace BirdCase
{
    public class SettingUI : MonoBehaviour
    {
        private readonly Color BLACK_COLOR = Color.black;
        private readonly Color WHITE_COLOR = Color.white;

        #region Setting Menu Buttons

        [Header("Setting Menu Button")]
        public GameObject GamePlayButton;
        public GameObject GraphicButton;
        public GameObject SoundButton;
        //public GameObject KeyBindingButton;

        [Header("Setting Menu Button Image")]
        public Sprite ButtonImage;
        public Sprite SelectButtonImage;

        [Header("Setting Menu Popups")]
        public GameObject gamePlayPopup;
        public GameObject graphicPopup;
        public GameObject soundPopup;
        //public GameObject keyMappingPopup;

        private Image gamePlayButtonImage;
        private Image graphicButtonImage;
        private Image soundButtonImage;
        //private Image keyBindingButtonImage;

        private TMP_Text gamePlayText;
        private TMP_Text graphicText;
        private TMP_Text soundText;
        //private TMP_Text keyBindingText;

        #endregion

        public GameObject SettingPanel; // 설정 팝업
        public GameObject AskPanel; // 설정 저장 여부 팝업

        #region Setting Scripts

        [Header("GamePlay")] public LanguageSetting languageSetting;
        public ShakeScreenSetting shakeScreenSetting;
        public HelpGuideSetting helpWindowSetting;

        [Header("Graphic")] public ResolutionOption resolutionSetting;
        public FullScreenOption fullScreenSetting;
        public VSyncOption vsyncSetting;
        public FrameOption frameSetting;

        [Header("Sound")] public SoundOption soundSetting;

        #endregion

        private void Start()
        {
            StartSetMenuSetting();
            LoadSetting();
        }

        // private void Update()
        // {// esc 키는 생각을 좀 더 해봐야 할 듯 무한으로 누르면 버튼을 무한으로 누르는 것과 같은 효과라 막고 기능 추가해야할 듯
        //     if(Keyboard.current.escapeKey.wasPressedThisFrame)
        //     {
        //         OnClickExitButton();
        //     }
        // }

        #region SelectButtons / ActivePopupSetting

        private void StartSetMenuSetting()
        {
            gamePlayButtonImage = GamePlayButton.GetComponent<Image>();
            graphicButtonImage = GraphicButton.GetComponent<Image>();
            soundButtonImage = SoundButton.GetComponent<Image>();
            //keyBindingButtonImage = KeyBindingButton.GetComponent<Image>();

            gamePlayText = GamePlayButton.GetComponentInChildren<TMP_Text>();
            graphicText = GraphicButton.GetComponentInChildren<TMP_Text>();
            soundText = SoundButton.GetComponentInChildren<TMP_Text>();
            //keyBindingText = KeyBindingButton.GetComponentInChildren<TMP_Text>();

            InitPopup();
            //CheckActivePopup();
        }

        public void SelectGamePlayButton()
        {
            gamePlayButtonImage.sprite = SelectButtonImage;
            graphicButtonImage.sprite = ButtonImage;
            soundButtonImage.sprite = ButtonImage;
            //keyBindingButtonImage.sprite = ButtonImage;

            gamePlayText.color = BLACK_COLOR;
            graphicText.color = WHITE_COLOR;
            soundText.color = WHITE_COLOR;
            //keyBindingText.color = WHITE_COLOR;
            
            gamePlayPopup.SetActive(true);
            graphicPopup.SetActive(false);
            soundPopup.SetActive(false);
            //keyMappingPopup.SetActive(false);
        }

        public void SelectGraphicButton()
        {
            gamePlayButtonImage.sprite = ButtonImage;
            graphicButtonImage.sprite = SelectButtonImage;
            soundButtonImage.sprite = ButtonImage;
            //keyBindingButtonImage.sprite = ButtonImage;

            gamePlayText.color = WHITE_COLOR;
            graphicText.color = BLACK_COLOR;
            soundText.color = WHITE_COLOR;
            //keyBindingText.color = WHITE_COLOR;
            
            gamePlayPopup.SetActive(false);
            graphicPopup.SetActive(true);
            soundPopup.SetActive(false);
            //keyMappingPopup.SetActive(false);
        }

        public void SelectSoundButton()
        {
            gamePlayButtonImage.sprite = ButtonImage;
            graphicButtonImage.sprite = ButtonImage;
            soundButtonImage.sprite = SelectButtonImage;
            //keyBindingButtonImage.sprite = ButtonImage;

            gamePlayText.color = WHITE_COLOR;
            graphicText.color = WHITE_COLOR;
            soundText.color = BLACK_COLOR;
            //keyBindingText.color = WHITE_COLOR;
            
            gamePlayPopup.SetActive(false);
            graphicPopup.SetActive(false);
            soundPopup.SetActive(true);
            //keyMappingPopup.SetActive(false);
        }

        public void SelectKeyBindingButton()
        {
            gamePlayButtonImage.sprite = ButtonImage;
            graphicButtonImage.sprite = ButtonImage;
            soundButtonImage.sprite = ButtonImage;
            //keyBindingButtonImage.sprite = SelectButtonImage;

            gamePlayText.color = WHITE_COLOR;
            graphicText.color = WHITE_COLOR;
            soundText.color = WHITE_COLOR;
            //keyBindingText.color = BLACK_COLOR;
            
            gamePlayPopup.SetActive(false);
            graphicPopup.SetActive(false);
            soundPopup.SetActive(false);
            //keyMappingPopup.SetActive(true);
        }

        /// <summary>
        /// 게임플레이 세팅으로 변경합니다.
        /// </summary>
        public void InitPopup()
        {
            SelectGamePlayButton();
        }

        /// <summary>
        /// 현재 켜져있는 팝업으로 버튼을 선택합니다.
        /// </summary>
        private void CheckActivePopup()
        {
            if (gamePlayPopup.activeSelf)
            {
                SelectGamePlayButton();
            }
            else if (graphicPopup.activeSelf)
            {
                SelectGraphicButton();
            }
            else if (soundPopup.activeSelf)
            {
                SelectSoundButton();
            }
            // else if (keyMappingPopup.activeSelf)
            // {
            //     SelectKeyBindingButton();
            // }
        }
        
        #endregion

        #region SettingApplyCheckButtons

        /// <summary>
        /// 설정창 Apply 버튼을 눌렀을 때 호출되는 함수입니다.
        /// </summary>
        public void OnClickApplyButton()
        {
            ApplySetting();

            if (AskPanel.activeSelf)
            {
                AskPanel.SetActive(false);
                SettingPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 설정창 Exit 버튼을 눌렀을 때 호출되는 함수입니다.
        /// </summary>
        public void OnClickExitButton()
        {
            CheckSetting();
        }

        /// <summary>
        /// 바꾼 설정을 이전 설정으로 되돌립니다.
        /// </summary>
        public void OnClickCancelButton()
        {
            if (AskPanel.activeSelf)
            {
                AskPanel.SetActive(false);
                SettingPanel.SetActive(false);
            }
            
            CancelSetting();
        }

        /// <summary>
        /// 모든 설정을 초기화 합니다.
        /// </summary>
        public void OnClickInitButton()
        {
            PlayerPrefs.DeleteAll();
            InitSetting();
        }

        #endregion

        #region SettingManager

        private void InitSetting()
        {
            languageSetting.InitSetting();
            shakeScreenSetting.InitShakeScreen();
            helpWindowSetting.InitHelpGuide();

            resolutionSetting.InitResolution();
            fullScreenSetting.InitFullScreen();
            vsyncSetting.InitVSync();
            frameSetting.InitFrame();

            soundSetting.InitSound();
        }

        private void ApplySetting()
        {
            languageSetting.ApplyLanguage();
            shakeScreenSetting.ApplyShakeScreen();
            helpWindowSetting.ApplyHelpGuide();

            resolutionSetting.ApplyResolution();
            fullScreenSetting.ApplyFullScreen();
            vsyncSetting.ApplyVSync();
            frameSetting.ApplyFrame();

            soundSetting.ApplySound();
            PlayerPrefs.Save();
        }

        public void LoadSetting()
        {
            languageSetting.LoadLanguage();
            shakeScreenSetting.LoadShakeScreen();
            helpWindowSetting.LoadHelpGuide();

            resolutionSetting.LoadResolution();
            fullScreenSetting.LoadFullScreen();
            vsyncSetting.LoadVSync();
            frameSetting.LoadFrame();

            soundSetting.LoadSound();
        }

        private void CancelSetting()
        {
            languageSetting.CancelLanguage();
            shakeScreenSetting.CancelShakeScreen();
            helpWindowSetting.CancelHelpGuide();

            resolutionSetting.CancelResolution();
            fullScreenSetting.CancelFullScreen();
            vsyncSetting.CancelVSync();
            frameSetting.CancelFrame();

            soundSetting.CancelSound();
        }

        /// <summary>
        /// 바뀐 설정이 있는 지 확인합니다.
        /// </summary>
        private void CheckSetting()
        {
            if (languageSetting.IsChangedLanguage || shakeScreenSetting.IsChangedShakeScreen || helpWindowSetting.IsChangedHelpGuide)
            {
                AskPanel.SetActive(true);
                return;
            }
            
            // if (resolutionSetting.IsChangeResolution)
            // {
            //     Debug.Log("해상도 선택에서 변경점 발생");
            //     AskPanel.SetActive(true);
            //     return;
            // }
                        // if (frameSetting.IsChangeFrame)
                          // {
                          //     Debug.Log("프레임 선택에서 변경점 발생");
                          //     AskPanel.SetActive(true);
                          //     return;
                          // }
                          
            if (fullScreenSetting.IsChangedFullScreen)
            {
                Debug.Log("전체화면 선택에서 변경점 발생");
                AskPanel.SetActive(true);
                return;
            }
            
            if (vsyncSetting.IsChangedVSync)
            {
                Debug.Log("vsync 선택에서 변경점 발생");
                AskPanel.SetActive(true);
                return;
            }
            
            if (soundSetting.IsChangedSound)
            {
                AskPanel.SetActive(true);
                return;
            }
            
            SettingPanel.SetActive(false);
        }

        #endregion
    }
}