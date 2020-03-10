//
// DynamicShadowProjectorRenderer.cs
//
// Dynamic Shadow Projector Extension For LWRP
//
// Copyright (c) 2019 NYAHOON GAMES PTE. LTD.
//

using UnityEngine;
using UnityEngine.Rendering;


namespace DynamicShadowProjector.LWRP
{
	public class DynamicShadowProjectorRenderer : UnityEngine.Rendering.Universal.ScriptableRenderer
	{
		private RenderShadowTexturePass m_renderShadowTexturePass;
		public DynamicShadowProjectorRenderer(DynamicShadowProjectorRendererData data) : base(data)
		{
			m_renderShadowTexturePass = new RenderShadowTexturePass(data);
		}
		public override void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, ref UnityEngine.Rendering.Universal.CameraData cameraData)
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
				DrawTargetObject drawTarget = shadowTextureRenderer.GetComponent<DrawTargetObject>();
				if (drawTarget != null)
				{
					drawTarget.SendMessage("OnPreCull");
				}
				shadowTextureRenderer.ConfigureRenderTarget(m_renderShadowTexturePass, ref cameraData);
			}
			cullingParameters.cullingOptions = CullingOptions.None;
			cullingParameters.shadowDistance = 0;
		}
		public override void Setup(ScriptableRenderContext context, ref UnityEngine.Rendering.Universal.RenderingData renderingData)
		{
			renderingData.cameraData.isStereoEnabled = false;
			ShadowTextureRenderer shadowTextureRenderer = renderingData.cameraData.camera.GetComponent<ShadowTextureRenderer>();
			if (shadowTextureRenderer != null && shadowTextureRenderer.enabled && shadowTextureRenderer.isProjectorVisible)
			{
				EnqueuePass(m_renderShadowTexturePass);
			}
		}
		public override void SetupLights(ScriptableRenderContext context, ref UnityEngine.Rendering.Universal.RenderingData renderingData)
		{

		}
		public override void FinishRendering(CommandBuffer cmd)
		{
		}
	}
}
