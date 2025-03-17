using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace NKStudio
{
    public sealed class UIBlurFeaturePass : ScriptableRendererFeature
    {
        internal class UIBlurPass : ScriptableRenderPass
        {
            // Material
            private readonly Material _material;

            // Blur Settings
            private int _blurIteration = 3;
            private float _blurOffset = 1.0f;
            private bool _alwaysShow;

            // Constants
            private static readonly int DownSampleTexPropertyName = Shader.PropertyToID("_DownSampleTex");
            private static readonly int OriginTexPropertyName = Shader.PropertyToID("_OriginTex");
            private static readonly int BlurTexPropertyName = Shader.PropertyToID("_BlurTex");
            private static readonly int BlurOffsetPropertyName = Shader.PropertyToID("_blurOffset");

            public UIBlurPass(
                RenderPassEvent injectionPoint, Material material)
            {
                // 렌더 패스 이벤트를 설정합니다.
                renderPassEvent = injectionPoint;

                // 렌더 패스에 사용할 머티리얼을 설정합니다.
                _material = material;

                // BackBuffer를 Input으로 사용할 수 없으므로 중간 텍스처를 사용합니다.
                requiresIntermediateTexture = true;
            }

            /// <summary>
            /// 블러에 대한 세팅을 셋업합니다.
            /// </summary>
            /// <param name="blurIteration">블러를 이터레이션할 횟수</param>
            /// <param name="blurOffset">블러 오프셋</param>
            /// <param name="alwaysShow">플레이 모드가 되지 않아도 블러가 연출될지 처리합니다.</param>
            public void Setup(int blurIteration, float blurOffset, bool alwaysShow)
            {
                _blurIteration = blurIteration;
                _blurOffset = blurOffset;
                _alwaysShow = alwaysShow;
            }

            private class MipMapPassData
            {
                internal TextureHandle Source;
                internal TextureHandle[] Scratches;
                internal Material TargetMaterial;
                internal float BlurOffset;
                internal bool AlwaysShow;
            }

            // 이 정적 메서드는 패스를 실행하는 데 사용되며 RenderGraph 렌더 패스에 RenderFunc 대리자로 전달됩니다.
            static void ExecuteMipmapPass(MipMapPassData data, UnsafeGraphContext context)
            {
                if (data.AlwaysShow || Application.isPlaying)
                {
                    context.cmd.SetGlobalTexture(OriginTexPropertyName, data.Source);
                    context.cmd.SetGlobalFloat(BlurOffsetPropertyName, data.BlurOffset);
                }

                var unsafeCmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
                var source = data.Source;
                var stepCount = data.Scratches.Length;
                for (int i = 0; i < stepCount; i++)
                {
                    if (data.AlwaysShow || Application.isPlaying)
                        context.cmd.SetGlobalTexture(DownSampleTexPropertyName, source);

                    context.cmd.SetRenderTarget(data.Scratches[i]); // 그림을 그릴 대상체를 지정합니다.
                    Blitter.BlitTexture(unsafeCmd, data.Source, new Vector4(1, 1, 0, 0), data.TargetMaterial,
                        0); // Draw!
                    source = data.Scratches[i]; // Set Next Source
                }

                if (data.AlwaysShow || Application.isPlaying)
                    context.cmd.SetGlobalTexture(BlurTexPropertyName, source);
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                // UniversalResourceData에는 활성 색상 및 깊이 텍스처를 포함하여 렌더러에서 사용하는 모든 텍스처 핸들이 포함됩니다.
                // 활성 색상 및 깊이 텍스처는 카메라가 렌더링하는 기본 색상 및 깊이 버퍼입니다.
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                // add a raster render pass to the render graph, specifying the name and the data type that will be passed to the ExecutePass function
                using (var builder = renderGraph.AddUnsafePass<MipMapPassData>("Blur UI Mipmap", out var passData))
                {
                    // 플레이 모드가 되지 않아도 동작시킬지 여부를 설정합니다.
                    passData.AlwaysShow = _alwaysShow;

                    // 프레임 데이터를 통해 활성 색상 텍스처를 가져오고 이를 블릿의 소스 텍스처로 설정합니다.
                    passData.Source = resourceData.cameraColor;

                    // Setup Material
                    passData.TargetMaterial = _material;

                    // 텍스처의 설명을 가져와서 수정합니다.
                    var descriptor = passData.Source.GetDescriptor(renderGraph);
                    descriptor.msaaSamples = MSAASamples.None; // blit 작업에 대해 MSAA를 비활성화합니다.
                    descriptor.clearBuffer = false;

                    // 반복 횟수의 2배로 만들어서 절반은 다운 샘플링으로 활용하고, 나머지 절반은 업 샘플링으로 활용합니다.
                    int scratchesCount = Mathf.Max(_blurIteration * 2, 1);

                    int sourceSizeWidth = descriptor.width;
                    int sourceSizeHeight = descriptor.height;

                    passData.Scratches = new TextureHandle[scratchesCount];
                    passData.BlurOffset = _blurOffset;

                    // 다운 샘플링 Blit 반복
                    for (int i = 0; i < scratchesCount - 1; i++)
                    {
                        int downsampleIndex = SimplePingPong(i, _blurIteration - 1);
                        descriptor.name = $"Blur UI Mipmap_{i}";
                        descriptor.width = sourceSizeWidth >> downsampleIndex + 1;
                        descriptor.height = sourceSizeHeight >> downsampleIndex + 1;

                        passData.Scratches[i] = renderGraph.CreateTexture(descriptor);
                        builder.UseTexture(passData.Scratches[i], AccessFlags.ReadWrite);
                    }

                    // 최종 스케일 업 결과물
                    descriptor.width = sourceSizeWidth;
                    descriptor.height = sourceSizeHeight;
                    descriptor.name = $"Blur UI Mipmap_{scratchesCount - 1}";
                    passData.Scratches[scratchesCount - 1] = renderGraph.CreateTexture(descriptor);
                    builder.UseTexture(passData.Scratches[scratchesCount - 1], AccessFlags.ReadWrite);

                    // UseTexture()를 통해 src 텍스처를 이 패스에 대한 입력 종속성으로 선언합니다.
                    builder.UseTexture(passData.Source);

                    // 일반적으로 이 패스가 컬링되므로 이 샘플의 시연 목적으로 이 패스에 대한 컬링을 비활성화합니다.
                    // 대상 텍스처는 다른 곳에서는 사용되지 않기 때문에
                    builder.AllowPassCulling(false);

                    // 패스를 실행할 때 렌더 그래프에 의해 호출되는 렌더 패스 대리자에 ExecutePass 함수를 할당합니다.
                    builder.SetRenderFunc((MipMapPassData data, UnsafeGraphContext context) =>
                        ExecuteMipmapPass(data, context));
                }
            }

            private static int SimplePingPong(int t, int max)
            {
                if (t > max) return 2 * max - t;
                return t;
            }
        }

        /// <summary>
        /// 렌더 객체 렌더러 기능에 사용되는 설정 클래스입니다.
        /// </summary>
        [System.Serializable]
        public class RenderObjectsSettings
        {
            public RenderPassEvent Event = RenderPassEvent.AfterRenderingTransparents;

            [Header("Blur Settings")] [Range(1, 5)]
            public int BlurIteration = 3;

            [Range(0.1f, 3.0f)] public float BlurOffset = 1.0f;

            [Tooltip("플레이 모드가 되지 않아도 블러가 연출될지 처리합니다.")]
            public bool AlwaysShow;
        }

        public RenderObjectsSettings Settings = new();

        private UIBlurPass _uiBlurPass;

        private Material _blurMaterial;

        public override void Create()
        {
            // 피쳐의 이름을 지정합니다. (Option)
            name = "UI Blur";

            // 렌더 패스 이벤트가 BeforeRenderingPrePasses보다 작으면 BeforeRenderingPrePasses로 설정합니다.
            if (Settings.Event < RenderPassEvent.BeforeRenderingPrePasses)
                Settings.Event = RenderPassEvent.BeforeRenderingPrePasses;

            // 블러 머티리얼을 생성합니다.
            _blurMaterial =
                CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/Universal Render Pipeline/UI/ScreenBlurRT"));

            // 블러 패스를 생성합니다.
            _uiBlurPass = new UIBlurPass(Settings.Event, _blurMaterial);
            _uiBlurPass.Setup(Settings.BlurIteration, Settings.BlurOffset, Settings.AlwaysShow);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.Game
                || UniversalRenderer.IsOffscreenDepthTexture(ref renderingData.cameraData))
                return;

            renderer.EnqueuePass(_uiBlurPass);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CoreUtils.Destroy(_blurMaterial);
                _blurMaterial = null;
            }
        }
    }
}