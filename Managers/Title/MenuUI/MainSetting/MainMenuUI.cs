using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace BirdCase
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private float addButtonFontSize = 10;
        private float defaultButtonFontSize;
        //[SerializeField] private RectTransform selectButton;
        private List<Button> buttons;

        private void Start()
        {
            buttons = new List<Button>();

            foreach (var button in GetComponentsInChildren<Button>())
            {
                buttons.Add(button);
            }
            defaultButtonFontSize = buttons[0].GetComponentInChildren<TMP_Text>().fontSize;

            if (PlayerPrefs.GetInt("Ending") == 1)
            {
                buttons[2].GetComponent<Image>().color = Color.white;
                buttons[2].GetComponentInChildren<TMP_Text>().color = Color.white;
            }
        }

        public void HoverButton(int index)
        {
            var mesh = buttons[index].GetComponentInChildren<TMP_Text>();
            mesh.fontSize = defaultButtonFontSize + addButtonFontSize;

            // selectButton.gameObject.SetActive(true);
            // selectButton.SetParent(buttons[index].transform);
            // selectButton.SetAsFirstSibling();
            // selectButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200, 0);
        }

        public void ExitButton(int index)
        {
            var mesh = buttons[index].GetComponentInChildren<TMP_Text>();
            mesh.fontSize = defaultButtonFontSize;

            //selectButton.gameObject.SetActive(false);
        }

        public void GameExit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }

        public void Credit()
        {
            if (PlayerPrefs.GetInt("Ending") == 1)
                SceneManager.LoadScene("CreditScene");
        }
    }
}
