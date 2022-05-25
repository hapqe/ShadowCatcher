using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DrawShadows : ScriptableRendererFeature
{
    [System.Serializable]
    public class ShadowFeatureSettings
    {
        public bool enabled = true;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        public Material blitMaterial;
        public LayerMask layerMask;
    }

    public ShadowFeatureSettings settings = new ShadowFeatureSettings();

    RenderTargetHandle shadowMap;
    ShadowRenderPass shadowRenderPass;


    public override void Create()
    {
        shadowRenderPass = new ShadowRenderPass(settings.renderPassEvent, settings.blitMaterial, settings.layerMask);
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if(!settings.enabled) return;

        shadowRenderPass.Setup(renderer);
        
        renderer.EnqueuePass(shadowRenderPass);
    }
}
