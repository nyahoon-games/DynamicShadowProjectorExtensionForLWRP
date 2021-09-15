//
// RenderShadowTexturePass.cs
//
// Dynamic Shadow Projector Extension For LWRP
//
// Copyright (c) 2019 NYAHOON GAMES PTE. LTD.
//

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.LWRP;

namespace DynamicShadowProjector.LWRP
{
	internal class RenderShadowTexturePass : ScriptableRenderPass
	{
		private Material m_overrideOpaqueMaterial;
		private Material m_overrideAlphaCutoffMaterial;
		private Material m_overrideTransparentMaterial;
		private ShaderTagId[] m_shaderTagIds;
		private DynamicShadowProjectorRenderer m_renderer;
		public RenderShadowTexturePass(DynamicShadowProjectorRendererData data, DynamicShadowProjectorRenderer renderer)
		{
			renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
			m_shaderTagIds = new ShaderTagId[data.m_sceneObjectShaderTagList.Length];
			for (int i = 0; i < data.m_sceneObjectShaderTagList.Length; ++i)
			{
				m_shaderTagIds[i] = new ShaderTagId(data.m_sceneObjectShaderTagList[i]);
			}
			m_renderer = renderer;
		}
		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{

		}
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			Camera camera = renderingData.cameraData.camera;
			DynamicShadowProjectorRenderer.DynamicShadowProjectorComponents components = m_renderer.GetDynamicShadowProjectorComponents(renderingData.cameraData.camera);
			ShadowTextureRenderer shadowTextureRenderer = components.shadowTextureRenderer;
			if (shadowTextureRenderer == null || !shadowTextureRenderer.isProjectorVisible)
			{
				return;
			}

			DrawSceneObject drawScene = components.drawSceneObject;
			if (drawScene != null)
			{
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
				DrawingSettings drawingSettings = new DrawingSettings(m_shaderTagIds[0], new SortingSettings(camera));
				for (int i = 1; i < m_shaderTagIds.Length; ++i)
				{
					drawingSettings.SetShaderPassName(i, m_shaderTagIds[i]);
				}
				// draw opaque objects
				drawingSettings.overrideMaterial = m_overrideOpaqueMaterial;
				drawingSettings.overrideMaterialPassIndex = 0;
				drawingSettings.enableDynamicBatching = renderingData.supportsDynamicBatching;
				drawingSettings.enableInstancing = true;
				drawingSettings.perObjectData = PerObjectData.None;
				FilteringSettings opaqueFilteringSettings = new FilteringSettings(new RenderQueueRange(RenderQueueRange.opaque.lowerBound, 2400), drawScene.cullingMask);
				context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref opaqueFilteringSettings);
				// draw alpha-cutoff objects
				drawingSettings.overrideMaterial = m_overrideAlphaCutoffMaterial;
				FilteringSettings alphacutoutFilteringSettings = new FilteringSettings(new RenderQueueRange(2400, RenderQueueRange.opaque.upperBound), drawScene.cullingMask);
				context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref alphacutoutFilteringSettings);
				// draw transparent objects
				drawingSettings.overrideMaterial = m_overrideTransparentMaterial;
				FilteringSettings transparentFilteringSettings = new FilteringSettings(new RenderQueueRange(RenderQueueRange.transparent.lowerBound, RenderQueueRange.transparent.upperBound), drawScene.cullingMask);
				context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref transparentFilteringSettings);
			}
			DrawTargetObject drawTarget = components.drawTargetObject;
			if (drawTarget != null)
			{
				context.ExecuteCommandBuffer(drawTarget.commandBuffer);
			}
			shadowTextureRenderer.ExecutePostRenderProcess(context);
		}
		public override void FrameCleanup(CommandBuffer cmd)
		{
		}
	}
}
