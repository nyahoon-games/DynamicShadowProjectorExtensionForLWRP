//
// DynamicShadowProjectorRenderer.cs
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
	public class DynamicShadowProjectorRenderer : ScriptableRenderer
	{
		private RenderShadowTexturePass m_renderShadowTexturePass;
		public DynamicShadowProjectorRenderer(DynamicShadowProjectorRendererData data) : base(data)
		{
			m_renderShadowTexturePass = new RenderShadowTexturePass(data);
		}
		public override void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, ref CameraData cameraData)
		{
			ShadowTextureRenderer shadowTextureRenderer = cameraData.camera.GetComponent<ShadowTextureRenderer>();
			if (shadowTextureRenderer == null)
			{
#if UNITY_EDITOR
				Debug.LogWarning("No ShadowTextureRenderer found.");
#endif
				return;
			}
			shadowTextureRenderer.UpdateVisibilityAndPrepareRendering();
			cullingParameters.cullingMask = 0;
			if (shadowTextureRenderer.isProjectorVisible)
			{
				DrawSceneObject drawScene = cameraData.camera.GetComponent<DrawSceneObject>();
				if (drawScene != null)
				{
					cullingParameters.cullingMask = (uint)drawScene.cullingMask.value;
				}
			}
			cullingParameters.cullingOptions = CullingOptions.None;
			cullingParameters.shadowDistance = 0;
			shadowTextureRenderer.ConfigureRenderTarget(m_renderShadowTexturePass, ref cameraData);
		}
		public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			ShadowTextureRenderer shadowTextureRenderer = renderingData.cameraData.camera.GetComponent<ShadowTextureRenderer>();
			if (shadowTextureRenderer != null && shadowTextureRenderer.enabled && shadowTextureRenderer.isProjectorVisible)
			{
				EnqueuePass(m_renderShadowTexturePass);
			}
		}
		public override void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData)
		{

		}
		public override void FinishRendering(CommandBuffer cmd)
		{
		}
	}
}
