using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BirdCase
{
    public class Bootstrap : MonoBehaviour
    {
        private void Start()
        {
            SceneManager.LoadScene(1);
        }
    }
}
