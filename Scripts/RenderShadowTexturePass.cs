//
// RenderShadowTexturePass.cs
//
// Dynamic Shadow Projector Extension For LWRP
//
// Copyright (c) 2019 NYAHOON GAMES PTE. LTD.
//

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DynamicShadowProjector.LWRP
{
	internal class RenderShadowTexturePass : ScriptableRenderPass
	{
		private Material m_overrideOpaqueMaterial;
		private Material m_overrideAlphaCutoffMaterial;
		private Material m_overrideTransparentMaterial;
		private ShadowTextureRenderer m_renderer;
		private bool m_rendered = false;

		public RenderShadowTexturePass(ShadowTextureRenderer renderer)
		{
			renderPassEvent = RenderPassEvent.BeforeRendering;
			m_renderer = renderer;
		}
		public void ResetFrame()
		{
			m_rendered = false;
		}
		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			if (m_rendered)
			{
				// in case of multipass VR, render pass will be called twice even if renderPassEvent == RenderPassEvent.BeforeRendering...
				return;
			}
			m_renderer.ConfigureRenderTarget(this);
		}
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			if (m_rendered)
			{
				// in case of multipass VR, render pass will be called twice even if renderPassEvent == RenderPassEvent.BeforeRendering...
				return;
			}
			CommandBuffer cmd = CommandBufferPool.Get();
			cmd.SetViewProjectionMatrices(m_renderer.projectorCamera.worldToCameraMatrix, m_renderer.projectorCamera.projectionMatrix);
			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
			DrawSceneObject drawScene = m_renderer.drawSceneObject;
			if (drawScene != null)
			{
				ScriptableCullingParameters cullingParameters = new ScriptableCullingParameters();
				if (m_renderer.projectorCamera.TryGetCullingParameters(false, out cullingParameters))
				{
					cullingParameters.cullingMask = (uint)drawScene.cullingMask.value;
					CullingResults cullingResults = context.Cull(ref cullingParameters);
					if (m_overrideOpaqueMaterial == null)
					{
						m_overrideOpaqueMaterial = new Material(drawScene.replacementShader);
					}
					else if (m_overrideOpaqueMaterial.shader != drawScene.replacementShader)
					{
						m_overrideOpaqueMaterial.shader = drawScene.replacementShader;
					}
					if (m_overrideAlphaCutoffMaterial == null)
					{
						m_overrideAlphaCutoffMaterial = new Material(drawScene.replacementShader);
						m_overrideAlphaCutoffMaterial.EnableKeyword("_ALPHATEST_ON");
						m_overrideAlphaCutoffMaterial.SetFloat("_DstBlend", 10.0f); // OneMinusSrcAlpha
					}
					else if (m_overrideAlphaCutoffMaterial.shader != drawScene.replacementShader)
					{
						m_overrideAlphaCutoffMaterial.shader = drawScene.replacementShader;
					}
					if (m_overrideTransparentMaterial == null)
					{
						m_overrideTransparentMaterial = new Material(drawScene.replacementShader);
						m_overrideTransparentMaterial.EnableKeyword("_ALPHATEST_ON");
						m_overrideTransparentMaterial.EnableKeyword("_ALPHABLEND_ON");
						m_overrideTransparentMaterial.SetFloat("_SrcBlend", 5.0f); // SrcAlpha
						m_overrideTransparentMaterial.SetFloat("_DstBlend", 10.0f); // OneMinusSrcAlpha
					}
					else if (m_overrideTransparentMaterial.shader != drawScene.replacementShader)
					{
						m_overrideTransparentMaterial.shader = drawScene.replacementShader;
					}
					DrawingSettings drawingSettings = new DrawingSettings(drawScene.shaderTagIds[0], new SortingSettings(m_renderer.projectorCamera));
					for (int i = 1; i < drawScene.shaderTagIds.Length; ++i)
					{
						drawingSettings.SetShaderPassName(i, drawScene.shaderTagIds[i]);
					}
					// draw opaque objects
					drawingSettings.overrideMaterial = m_overrideOpaqueMaterial;
					drawingSettings.overrideMaterialPassIndex = 0;
					drawingSettings.enableDynamicBatching = renderingData.supportsDynamicBatching;
					drawingSettings.enableInstancing = true;
					drawingSettings.perObjectData = PerObjectData.None;
					FilteringSettings opaqueFilteringSettings = new FilteringSettings(new RenderQueueRange(RenderQueueRange.opaque.lowerBound, 2400), drawScene.cullingMask);
					context.DrawRenderers(cullingResults, ref drawingSettings, ref opaqueFilteringSettings);
					// draw alpha-cutoff objects
					drawingSettings.overrideMaterial = m_overrideAlphaCutoffMaterial;
					FilteringSettings alphacutoutFilteringSettings = new FilteringSettings(new RenderQueueRange(2400, RenderQueueRange.opaque.upperBound), drawScene.cullingMask);
					context.DrawRenderers(cullingResults, ref drawingSettings, ref alphacutoutFilteringSettings);
					// draw transparent objects
					drawingSettings.overrideMaterial = m_overrideTransparentMaterial;
					FilteringSettings transparentFilteringSettings = new FilteringSettings(new RenderQueueRange(RenderQueueRange.transparent.lowerBound, RenderQueueRange.transparent.upperBound), drawScene.cullingMask);
					context.DrawRenderers(cullingResults, ref drawingSettings, ref transparentFilteringSettings);
				}
			}
			DrawTargetObject drawTarget = m_renderer.drawTargetObject;
			if (drawTarget != null)
			{
				context.ExecuteCommandBuffer(drawTarget.commandBuffer);
			}
			m_renderer.ExecutePostRenderProcess(context);
			m_rendered = true;
		}
		public override void FrameCleanup(CommandBuffer cmd)
		{
		}
	}
}
