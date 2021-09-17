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
#if UNITY_2020_1_OR_NEWER
			additionalCameraData.allowXRRendering = false; // available from URP 10.0.0
#endif
			additionalCameraData.SetRenderer(DynamicShadowProjectorRendererData.instance.rendererIndex);
		}
		private ProjectorForSRP.ProjectorForSRP m_projectorForSRP;
		partial void OnRenderTextureCreated()
		{
			if (m_projectorForSRP == null)
			{
				m_projectorForSRP = GetComponent<ProjectorForSRP.ProjectorForSRP>();
				if (m_projectorForSRP == null)
				{
					Debug.LogError("Projector For LWRP component was not found!", this);
					return;
				}
			}
			m_projectorForSRP.propertyBlock.SetTexture(s_shadowTexParamID, m_shadowTexture);
			m_projectorForSRP.propertyBlock.SetFloat(s_mipLevelParamID, m_mipLevel);
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
