using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UnblurredRenderPass : ScriptableRenderPass
{
    private ShaderTagId shaderTagId;
    private DrawingSettings drawingSettings;
    private FilteringSettings filteringSettings;
    private RenderStateBlock renderStateBlock;

    public UnblurredRenderPass(UnblurredRPSettings settings)
    {
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        shaderTagId = new ShaderTagId(settings.unblurredShaderTagId);
        filteringSettings = new FilteringSettings(RenderQueueRange.all, settings.unblurredLayMask);
        renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get("Unblurred");
        drawingSettings = CreateDrawingSettings(shaderTagId, ref renderingData, SortingCriteria.CommonOpaque);
        context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings, ref renderStateBlock);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}
