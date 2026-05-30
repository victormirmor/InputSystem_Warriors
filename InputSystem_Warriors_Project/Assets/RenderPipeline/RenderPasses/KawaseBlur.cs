using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class KawaseBlur : ScriptableRendererFeature
{
    [System.Serializable]
    public class KawaseBlurSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material blurMaterial = null;

        [Range(2, 15)]
        public int blurPasses = 1;

        [Range(1, 4)]
        public int downsample = 1;
        public bool copyToFramebuffer;
        public string targetName = "_blurTexture";
    }

    public KawaseBlurSettings settings = new KawaseBlurSettings();

    class CustomRenderPass : ScriptableRenderPass
    {
        public Material blurMaterial;
        public int passes;
        public int downsample;
        public bool copyToFramebuffer;
        public string targetName;        
        private string profilerTag;

        private class PassData
        {
            public TextureHandle source;
            public Material material;
            public float offset;
        }

        public CustomRenderPass(string profilerTag)
        {
            this.profilerTag = profilerTag;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (blurMaterial == null) return;

            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            TextureHandle activeColor = resourceData.activeColorTexture;
            if (!activeColor.IsValid()) return;

            // Configurar tamaño con Downsampling
            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            desc.width = Mathf.Max(1, desc.width / downsample);
            desc.height = Mathf.Max(1, desc.height / downsample);
            desc.depthBufferBits = 0;

            // Creamos las dos texturas temporales para el ping-pong
            TextureHandle rt1 = renderGraph.CreateTexture(new TextureDesc(desc) { name = "tmpBlurRT1" });
            TextureHandle rt2 = renderGraph.CreateTexture(new TextureDesc(desc) { name = "tmpBlurRT2" });

            TextureHandle currentSource = activeColor;
            TextureHandle currentDest = rt1;

            // --- PRIMER PASE ---
            ExecuteBlurPass(renderGraph, currentSource, currentDest, 1.5f);

            // --- PASES INTERMEDIOS (Ping-Pong) ---
            for (int i = 1; i < passes - 1; i++)
            {
                currentSource = currentDest;
                currentDest = (currentDest == rt1) ? rt2 : rt1;

                ExecuteBlurPass(renderGraph, currentSource, currentDest, 0.5f + i);
            }

            // --- ÚLTIMO PASE ---
            float finalOffset = 0.5f + passes - 1f;
            if (copyToFramebuffer)
            {
                // Devolvemos el resultado a la pantalla principal
                ExecuteBlurPass(renderGraph, currentDest, activeColor, finalOffset);
            }
            else
            {
                // Guardamos el resultado en la textura final y la volvemos global
                TextureHandle finalDest = (currentDest == rt1) ? rt2 : rt1;
                ExecuteBlurPass(renderGraph, currentDest, finalDest, finalOffset);
                
                // Hacemos que la textura final sea global para otros shaders en un pase posterior dedicado
                MakeTextureGlobal(renderGraph, finalDest, targetName);
            }
        }

        // Método auxiliar para registrar cada pase individual de Blit en el Render Graph
        private void ExecuteBlurPass(RenderGraph renderGraph, TextureHandle source, TextureHandle destination, float offset)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(profilerTag, out var passData))
            {
                passData.source = source;
                passData.material = blurMaterial;
                passData.offset = offset;

                builder.UseTexture(passData.source, AccessFlags.Read);
                builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    data.material.SetFloat("_offset", data.offset);
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }
        }

        // Método auxiliar para exponer la textura globalmente al final del grafo
        private void MakeTextureGlobal(RenderGraph renderGraph, TextureHandle texture, string name)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("ExposeBlurTexture", out var passData))
            {
                passData.source = texture;
                builder.UseTexture(passData.source, AccessFlags.Read);
                
                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    context.cmd.SetGlobalTexture(name, data.source);
                });
            }
        }
    }

    CustomRenderPass scriptablePass;

    public override void Create()
    {
        scriptablePass = new CustomRenderPass("KawaseBlur")
        {
            blurMaterial = settings.blurMaterial,
            passes = settings.blurPasses,
            downsample = settings.downsample,
            copyToFramebuffer = settings.copyToFramebuffer,
            targetName = settings.targetName,
            renderPassEvent = settings.renderPassEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(scriptablePass);
    }
}