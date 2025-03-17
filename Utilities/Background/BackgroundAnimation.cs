using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BirdCase
{
    public class BackgroundAnimation : MonoBehaviour
    {
        [SerializeField] private Material oneEmission;
        [SerializeField] private Material twoEmission;

        [SerializeField] private Material groundEmission;

        private CancellationTokenSource cancel;

        [ContextMenu("LightDown")]
        public void LightDownStart()
        {
            LightDownAnimation().Forget();
        }

        private async UniTaskVoid LightDown()
        {
            oneEmission.DisableKeyword("_EMISSION");
            await UniTask.Delay(100, cancellationToken: cancel.Token);
            twoEmission.DisableKeyword("_EMISSION");
        }

        public async UniTaskVoid LightDownAnimation()
        {
            for (int i = 0; i < 4; i++)
            {
                LightDown().Forget();
                await UniTask.Delay(100, cancellationToken: cancel.Token);
                LightUp().Forget();
                await UniTask.Delay(300, cancellationToken: cancel.Token);
            }

            for (int i = 0; i < 7; i++)
            {
                LightDown().Forget();
                await UniTask.Delay(Mathf.RoundToInt(Random.Range(0f, 1f) * 100), cancellationToken: cancel.Token);
                LightUp().Forget();
                await UniTask.Delay(Mathf.RoundToInt(Random.Range(0.5f, 1f) * 100), cancellationToken: cancel.Token);
            }

            await UniTask.Delay(200, cancellationToken: cancel.Token);
            LightDown().Forget();
        }

        private void Awake()
        {
            cancel = new CancellationTokenSource();
            oneEmission.EnableKeyword("_EMISSION");
            twoEmission.EnableKeyword("_EMISSION");
        }

        private void OnDestroy()
        {
            cancel?.Cancel();
            cancel?.Dispose();
            cancel = null;
        }

        [ContextMenu("LightUp")]
        public void LightUpStart()
        {
            LightUpAnimation().Forget();
        }

        private async UniTaskVoid LightUp()
        {
            oneEmission.EnableKeyword("_EMISSION");
            await UniTask.Delay(Mathf.RoundToInt(Random.Range(0f, 1f) * 100), cancellationToken: cancel.Token);
            twoEmission.EnableKeyword("_EMISSION");
        }

        public async UniTaskVoid LightUpAnimation()
        {
            for (int i = 0; i < 7; i++)
            {
                LightUp().Forget();
                await UniTask.Delay(Mathf.RoundToInt(Random.Range(0f, 1f) * 100), cancellationToken: cancel.Token);
                LightDown().Forget();
                await UniTask.Delay(Mathf.RoundToInt(Random.Range(0.5f, 1f) * 100), cancellationToken: cancel.Token);
            }

            for (int i = 0; i < 4; i++)
            {
                LightUp().Forget();
                await UniTask.Delay(100, cancellationToken: cancel.Token);
                LightDown().Forget();
                await UniTask.Delay(300, cancellationToken: cancel.Token);
            }

            await UniTask.Delay(200, cancellationToken: cancel.Token);
            LightUp().Forget();
        }

        private void GroundLightDown()
        {
            groundEmission.DisableKeyword("_EMISSION");
        }

        private void GroundLightUp()
        {
            groundEmission.EnableKeyword("_EMISSION");
        }

        [ContextMenu("GroundLight")]
        public void GroundLightStart()
        {
            Debug.Log("GroundLightStart");
            OnDestroy();
            cancel = new CancellationTokenSource();
            GroundLightAnimation(cancel).Forget();
        }

        private async UniTaskVoid GroundLightAnimation(CancellationTokenSource cancel)
        {
            try
            {
                for (int i = 0; i < 7; i++)
                {
                    GroundLightUp();
                    await UniTask.Delay(Mathf.RoundToInt(Random.Range(0f, 1f) * 100), cancellationToken: cancel.Token);
                    cancel.Token.ThrowIfCancellationRequested();
                    GroundLightDown();
                    await UniTask.Delay(Mathf.RoundToInt(Random.Range(0.5f, 1f) * 100), cancellationToken: cancel.Token);
                    cancel.Token.ThrowIfCancellationRequested();
                }

                await UniTask.Delay(200, cancellationToken: cancel.Token);
                cancel.Token.ThrowIfCancellationRequested();
                GroundLightUp();
            }
            catch { }
        }
    }
}
