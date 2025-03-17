using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;

namespace BirdCase
{
    public class ClashManager : NetworkBehaviour
    {
        [SerializeField] private PlayableDirector playableDirector;
        [SerializeField] private GameObject cinemachineCamera;
        public event Action endEvent;
        private void Awake()
        {
            cinemachineCamera.SetActive(false);
        }

        private void Update()
        {
            
        }

        [ContextMenu("Play Clash")]
        public void PlayClash()
        {
            //CameraManager.Instance.IgnoreTimeScale = true;
            CameraManager.Instance.IsUIActive = false;
            CameraManager.Instance.StartCounterVignette(2);
            playableDirector.Play();
            playableDirector.stopped += (playableDirector) => EndClash();
            cinemachineCamera.SetActive(true);
        }

        public void EndClash()
        {
            CameraManager.Instance.IsUIActive = true;
            playableDirector.Stop();
            cinemachineCamera.SetActive(false);
            endEvent?.Invoke();
        }
    }
}
