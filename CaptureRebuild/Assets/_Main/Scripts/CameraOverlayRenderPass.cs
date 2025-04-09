using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraOverlayRenderPass : ScriptableRenderPass
{
    private ShaderTagId shaderTagId = new ShaderTagId("UniversalForward");
    private DrawingSettings drawingSettings;
    private FilteringSettings filteringSettings;
    private RenderStateBlock renderStateBlock;

    public CameraOverlayRenderPass(CustomRPSettings customRPSettings)
    {
        filteringSettings = new FilteringSettings(RenderQueueRange.all, customRPSettings.overlayLayerMask);
        renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get("CameraBlur");
        drawingSettings = CreateDrawingSettings(shaderTagId, ref renderingData, SortingCriteria.CommonOpaque);
        context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings, ref renderStateBlock);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}
