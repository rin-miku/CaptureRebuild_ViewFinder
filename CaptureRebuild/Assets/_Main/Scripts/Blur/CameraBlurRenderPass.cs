using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraBlurRenderPass : ScriptableRenderPass
{
    private Material blurMaterial;
    private RTHandle cameraColorTargetHandle;
    private RTHandle rtHandler;
    private CameraBlurVolumeComponent blurVolume;
    private RenderTextureDescriptor textureDescriptor;

    private float[] weights;

    private void NormalizeWeights()
    {
        weights = new float[]
        {
            0.0030f, 0.0133f, 0.0298f, 0.0510f, 0.0702f, 0.0862f, 0.0939f, 0.0862f, 0.0702f, 0.0510f, 0.0298f, 0.0133f, 0.0030f
        };

        float weightSum = weights.Sum();
        weights = weights.Select(weight => weight / weightSum).ToArray();
    }

    public CameraBlurRenderPass(CameraBlurRPSettings settings)
    {
        NormalizeWeights();

        GameObject.Find("GlobalVolume")?.GetComponent<Volume>()?.profile.TryGet(out blurVolume);

        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing - 1;

        blurMaterial = CoreUtils.CreateEngineMaterial(settings.blurShader);
        blurMaterial.SetFloat("_BlurSize", 0f);
        blurMaterial.SetFloatArray("_Weights", weights);
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        textureDescriptor = cameraTextureDescriptor;
        textureDescriptor.graphicsFormat = GraphicsFormat.B10G11R11_UFloatPack32;
        textureDescriptor.depthBufferBits = 0;

        RenderingUtils.ReAllocateIfNeeded(ref rtHandler, textureDescriptor, wrapMode: TextureWrapMode.Clamp, name: "_BlurRT");
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get("CameraBlur");
        cameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
        if (cameraColorTargetHandle.rt == null) return;

        if (blurVolume == null || !blurVolume.enableCameraBlur.value) return;
        blurMaterial.SetFloat("_BlurSize", blurVolume.blurSize.value);

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
