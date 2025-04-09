using System;
using UnityEngine.Rendering.Universal;

[Serializable]
public class CameraBlurRenderFeature : ScriptableRendererFeature
{
    public CustomRPSettings customRPSettings;
    private CameraBlurRenderPass cameraBlurPass;
    private CameraOverlayRenderPass cameraOverlayPass;

    public override void Create()
    {
        if (customRPSettings != null)
        {
            cameraBlurPass = new CameraBlurRenderPass(customRPSettings);
            cameraOverlayPass = new CameraOverlayRenderPass(customRPSettings);
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (cameraBlurPass != null)
        {
            renderer.EnqueuePass(cameraBlurPass);
        }

        if (cameraOverlayPass != null)
        {
            renderer.EnqueuePass(cameraOverlayPass);
        }
    }
}
