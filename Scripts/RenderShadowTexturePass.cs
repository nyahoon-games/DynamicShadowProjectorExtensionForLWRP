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
		private Material m_overrideTransparentMaterial;
		private ShaderTagId[] m_shaderTagIds;
		private DynamicShadowProjectorRenderer m_renderer;

		public RenderShadowTexturePass(DynamicShadowProjectorRendererData data, DynamicShadowProjectorRenderer renderer)
		{
			renderPassEvent = RenderPassEvent.BeforeRendering;
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

			CommandBuffer cmd = CommandBufferPool.Get();
			//ScriptableRenderer.SetCameraMatrices(cmd, ref renderingData.cameraData, true);
			cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
			context.ExecuteCommandBuffer(cmd);
			cmd.Clear();
			CommandBufferPool.Release(cmd);

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
					m_overrideOpaqueMaterial = new Material(shadowTextureRenderer.m_opaqueShadowShaderForLWRP);
				}
				else if (m_overrideOpaqueMaterial.shader != shadowTextureRenderer.m_opaqueShadowShaderForLWRP)
				{
					m_overrideOpaqueMaterial.shader = shadowTextureRenderer.m_opaqueShadowShaderForLWRP;
				}
				if (m_overrideTransparentMaterial == null)
				{
					m_overrideTransparentMaterial = new Material(shadowTextureRenderer.m_transparentShadowShaderForLWRP);
				}
				else if (m_overrideTransparentMaterial.shader != shadowTextureRenderer.m_transparentShadowShaderForLWRP)
				{
					m_overrideTransparentMaterial.shader = shadowTextureRenderer.m_transparentShadowShaderForLWRP;
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
				FilteringSettings opaqueFilteringSettings = new FilteringSettings(new RenderQueueRange(RenderQueueRange.opaque.lowerBound, 2500), drawScene.cullingMask);
				context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref opaqueFilteringSettings);
				// draw transparent objects
				drawingSettings.overrideMaterial = m_overrideTransparentMaterial;
				FilteringSettings transparentFilteringSettings = new FilteringSettings(new RenderQueueRange(2500, RenderQueueRange.transparent.upperBound), drawScene.cullingMask);
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
