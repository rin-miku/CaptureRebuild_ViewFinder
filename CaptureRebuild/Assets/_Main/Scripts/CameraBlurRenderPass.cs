using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraBlurRenderPass : ScriptableRenderPass
{
    private Material blurMaterial;
    private RTHandle cameraColorTargetHandle;
    private RTHandle rtHandler;
    private RenderTextureDescriptor textureDescriptor;

    public CameraBlurRenderPass(CustomRPSettings customRPSettings)
    {
        blurMaterial = CoreUtils.CreateEngineMaterial(customRPSettings.blurShader);
        blurMaterial.SetFloat("_BlurSize", 0f);

        textureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
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

        var stack = VolumeManager.instance.stack;
        var cameraBlurSettings = stack.GetComponent<CameraBlurVolumeComponent>();
        if (!cameraBlurSettings.enableCameraBlur.value) return;
        blurMaterial.SetFloat("_BlurSize", cameraBlurSettings.blurSize.value);

        blurMaterial.SetVector("_BlurDirection", new Vector2(1.0f, 0.0f));
        Blit(cmd, cameraColorTargetHandle, rtHandler, blurMaterial);

        blurMaterial.SetVector("_BlurDirection", new Vector2(0.0f, 1.0f));
        Blit(cmd, rtHandler, cameraColorTargetHandle, blurMaterial);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}
