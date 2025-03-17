using UnityEngine;
using UnityEngine.SceneManagement;

namespace BirdCase
{
    public class NextScene : MonoBehaviour
    {
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
