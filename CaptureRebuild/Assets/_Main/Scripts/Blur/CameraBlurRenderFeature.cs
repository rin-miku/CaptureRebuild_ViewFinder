using System;
using UnityEngine.Rendering.Universal;

[Serializable]
public class CameraBlurRenderFeature : ScriptableRendererFeature
{
    public CameraBlurRPSettings cameraBlurRPSettings;
    public UnblurredRPSettings unblurredRPSettings;

    private CameraBlurRenderPass cameraBlurRenderPass;
    private UnblurredRenderPass unblurredRenderPass;

    public override void Create()
    {
        if (cameraBlurRPSettings != null)
        {
            cameraBlurRenderPass = new CameraBlurRenderPass(cameraBlurRPSettings);
        }

        if (unblurredRPSettings != null)
        {
            unblurredRenderPass = new UnblurredRenderPass(unblurredRPSettings);
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (cameraBlurRenderPass != null)
        {
            renderer.EnqueuePass(cameraBlurRenderPass);
        }

        if (unblurredRenderPass != null)
        {
            renderer.EnqueuePass(unblurredRenderPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && cameraBlurRenderPass != null)
        {
            cameraBlurRenderPass.Dispose();
        }
    }
}
