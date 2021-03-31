//
// ShadowTextureRendererForLWRP.cs
//
// Dynamic Shadow Projector Extension For LWRP
//
// Copyright (c) 2019 NYAHOON GAMES PTE. LTD.
//

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DynamicShadowProjector.LWRP;

namespace DynamicShadowProjector
{
	public partial class ShadowTextureRenderer
	{
		public Shader m_opaqueShadowShaderForLWRP;
		public Shader m_transparentShadowShaderForLWRP;

		partial void PartialInitialize()
		{
			UniversalAdditionalCameraData additionalCameraData = GetComponent<UniversalAdditionalCameraData>();
			if (additionalCameraData == null)
			{
				additionalCameraData = gameObject.AddComponent<UniversalAdditionalCameraData>();
				additionalCameraData.hideFlags = HideFlags.NotEditable;
			}
			additionalCameraData.renderShadows = false;
			additionalCameraData.requiresColorOption = CameraOverrideOption.Off;
			additionalCameraData.requiresColorTexture = false;
			additionalCameraData.requiresDepthOption = CameraOverrideOption.Off;
			additionalCameraData.requiresDepthTexture = false;
			additionalCameraData.SetRenderer(DynamicShadowProjectorRendererData.instance.rendererIndex);
			if (m_opaqueShadowShaderForLWRP == null)
			{
				m_opaqueShadowShaderForLWRP = Shader.Find("DynamicShadowProjector/Shadow/Opaque");
			}
			if (m_transparentShadowShaderForLWRP == null)
			{
				m_transparentShadowShaderForLWRP = Shader.Find("DynamicShadowProjector/Shadow/Transparent");
			}
		}
		internal void UpdateVisibilityAndPrepareRendering()
		{
			OnPreCull();
			m_camera.cullingMask = 0;
			m_camera.clearFlags = UnityEngine.CameraClearFlags.SolidColor;
			m_camera.enabled = enabled;
		}
		internal void ConfigureRenderTarget(RenderShadowTexturePass pass, ref CameraData cameraData)
		{
			PrepareRendering();
			if (useIntermediateTexture)
			{
				pass.ConfigureTarget(new RenderTargetIdentifier(m_temporaryRenderTarget));
			}
			else
			{
				pass.ConfigureTarget(new RenderTargetIdentifier(m_shadowTexture));
			}
			pass.ConfigureClear(ClearFlag.Color, m_camera.backgroundColor);
		}
		internal void ExecutePostRenderProcess(ScriptableRenderContext context)
		{
			postProcessCommandBuffer.Clear();
			RenderTargetIdentifier srcId;
			if (useIntermediateTexture)
			{
				srcId = new RenderTargetIdentifier(m_temporaryRenderTarget);
			}
			else
			{
				srcId = new RenderTargetIdentifier(m_shadowTexture);
			}
			AddPostRenderPassCommands(postProcessCommandBuffer, srcId);
			context.ExecuteCommandBuffer(postProcessCommandBuffer);
			m_shadowTextureValid = true;
			m_camera.targetTexture = m_shadowTexture;
			m_camera.clearFlags = CameraClearFlags.Nothing;
			ReleaseTemporaryRenderTarget();
		}
	}
}
