using System;
using UnityEngine.Rendering.Universal;

[Serializable]
public class CameraBlurRenderFeature : ScriptableRendererFeature
{
    public CustomRPSettings customRPSettings;
    private CameraBlurRenderPass cameraBlurPass;

    public override void Create()
    {
        if (customRPSettings != null)
        {
            cameraBlurPass = new CameraBlurRenderPass(customRPSettings);
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (cameraBlurPass != null)
        {
            renderer.EnqueuePass(cameraBlurPass);
        }
    }
}
