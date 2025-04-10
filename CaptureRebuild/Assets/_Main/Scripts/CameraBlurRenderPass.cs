using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraBlurRenderPass : ScriptableRenderPass
{
    private Material blurMaterial;
    private RTHandle cameraColorTargetHandle;
    private RTHandle rtHandler;
    private RenderTextureDescriptor textureDescriptor;
    private float[] weights = new float[]
    {
        0.0030f, 0.0133f, 0.0298f, 0.0510f, 0.0702f, 0.0862f, 0.0939f, 0.0862f, 0.0702f, 0.0510f, 0.0298f, 0.0133f, 0.0030f
    };

    public CameraBlurRenderPass(CustomRPSettings customRPSettings)
    {
        float sum = weights.Sum();
        for(int i = 0; i < weights.Length; i++)
        {
            weights[i] /= sum;
        }

        renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

        blurMaterial = CoreUtils.CreateEngineMaterial(customRPSettings.blurShader);
        blurMaterial.SetFloat("_BlurSize", 0f);
        blurMaterial.SetFloatArray("_Weights", weights);

        textureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.RGB111110Float, 0);
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        textureDescriptor.width = cameraTextureDescriptor.width;
        textureDescriptor.height = cameraTextureDescriptor.height;

        RenderingUtils.ReAllocateIfNeeded(ref rtHandler, textureDescriptor, wrapMode: TextureWrapMode.Clamp, name: "_BlurRT1");
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get("CameraBlur");
        cameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;

        CameraBlurVolumeComponent cameraBlurSettings = VolumeManager.instance.stack.GetComponent<CameraBlurVolumeComponent>();
        if (!cameraBlurSettings.enableCameraBlur.value) return;
        blurMaterial.SetFloat("_BlurSize", cameraBlurSettings.blurSize.value);

        blurMaterial.SetVector("_BlurDirection", new Vector2(1.0f, 0.0f));
        Blit(cmd, cameraColorTargetHandle, rtHandler, blurMaterial);

        blurMaterial.SetVector("_BlurDirection", new Vector2(0.0f, 1.0f));
        Blit(cmd, rtHandler, cameraColorTargetHandle, blurMaterial);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public void Dispose()
    {
        rtHandler?.Release();
        rtHandler = null;
    }
}
