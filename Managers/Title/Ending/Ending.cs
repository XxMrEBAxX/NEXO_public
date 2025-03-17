using UnityEngine;
using UnityEngine.InputSystem;

namespace BirdCase
{
    public class Ending : MonoBehaviour
    {
        private void Start()
        {
            SoundManager.Instance.StopAllSounds();
            TimeManager.Instance.TimeScale = 1;
            TimeManager.Instance.PlayerTimeScale = 1;
            PlayerPrefs.SetInt("Ending", 1);
        }

        private void Update()
        {
            if (Keyboard.current.spaceKey.isPressed)
            {
                TimeManager.Instance.TimeScale = 10;
            }
            else
            {
                TimeManager.Instance.TimeScale = 1;
            }
        }
    }
}
