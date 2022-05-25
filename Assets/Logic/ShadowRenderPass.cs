using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ShadowRenderPass : ScriptableRenderPass
{
    string profilerTag;

    Material blitMaterial;
    RenderTargetIdentifier cameraColorTargetIdent;
    ScriptableRenderer renderer;
    RenderTargetHandle shadowMap;

    LayerMask layerMask;

    FilteringSettings filteringSettings;

    public ShadowRenderPass(RenderPassEvent passEvent, Material blitMaterial, LayerMask layerMask, string tag = "ShadowRenderPass")
    {
        this.profilerTag = tag;
        this.renderPassEvent = passEvent;
        this.blitMaterial = blitMaterial;
        this.layerMask = layerMask;
    }

    public void Setup(ScriptableRenderer renderer)
    {
        this.renderer = renderer;
        filteringSettings = new FilteringSettings();
        filteringSettings.layerMask = layerMask;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        cmd.GetTemporaryRT(shadowMap.id, cameraTextureDescriptor);
    }

    public static event Func<RenderTexture> onDrawShadows;

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        ref CameraData cameraData = ref renderingData.cameraData;
        Camera camera = cameraData.camera;

        DrawingSettings drawingSettings = new DrawingSettings(new ShaderTagId(profilerTag), new SortingSettings(camera) { criteria = SortingCriteria.CommonTransparent });
        drawingSettings.overrideMaterial = ShadowConfig.GetConfig().positionMaterial;
        drawingSettings.overrideMaterialPassIndex = 0;

        var cmd = CommandBufferPool.Get(profilerTag);
        cmd.Clear();

        

        // if(renderingData.cameraData.camera.gameObject.CompareTag("MainCamera")) {
        //     var rt = onDrawShadows?.Invoke();
        //     if(rt) {
        //         cmd.Blit(rt.depthBuffer, shadowMap.id, blitMaterial, 0);

        //         cmd.Blit(shadowMap.id, renderer.cameraColorTarget);
        //     }
        // }

        context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);

        context.ExecuteCommandBuffer(cmd);

        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(shadowMap.id);
    }
}