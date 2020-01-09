//
// ShadowTextureRendererForLWRP.cs
//
// Dynamic Shadow Projector Extension For LWRP
//
// Copyright (c) 2019 NYAHOON GAMES PTE. LTD.
//

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.LWRP;
using DynamicShadowProjector.LWRP;

namespace DynamicShadowProjector
{
	public partial class ShadowTextureRenderer
	{
		public Shader m_opaqueShadowShaderForLWRP;
		public Shader m_transparentShadowShaderForLWRP;

#if UNITY_EDITOR
		partial void PartialInitialize()
		{
			UnityEngine.Rendering.Universal.UniversalAdditionalCameraData additionalCameraData = GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
			if (additionalCameraData == null)
			{
				additionalCameraData = gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
				additionalCameraData.hideFlags = HideFlags.NotEditable;
			}
			additionalCameraData.renderShadows = false;
			additionalCameraData.requiresColorOption = UnityEngine.Rendering.Universal.CameraOverrideOption.Off;
			additionalCameraData.requiresColorTexture = false;
			additionalCameraData.requiresDepthOption = UnityEngine.Rendering.Universal.CameraOverrideOption.Off;
			additionalCameraData.requiresDepthTexture = false;
			UnityEditor.SerializedObject serializedPipelineAsset = new UnityEditor.SerializedObject(UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset);
			UnityEditor.SerializedProperty rendererList = serializedPipelineAsset.FindProperty("m_RendererDataList");
			int rendererCount = rendererList.arraySize;
			int rendererIndex = -1;
			for (int i = 0; i < rendererCount; ++i)
			{
				UnityEditor.SerializedProperty rendererData = rendererList.GetArrayElementAtIndex(i);
				if (rendererData.objectReferenceValue is DynamicShadowProjectorRendererData)
				{
					rendererIndex = i;
					break;
				}
			}
			if (rendererIndex == -1)
			{
				rendererIndex = rendererCount;
				rendererList.InsertArrayElementAtIndex(rendererIndex);
				UnityEditor.SerializedProperty rendererData = rendererList.GetArrayElementAtIndex(rendererIndex);
				rendererData.objectReferenceValue = DynamicShadowProjectorRendererData.instance;
				serializedPipelineAsset.ApplyModifiedPropertiesWithoutUndo();
				Debug.Log("DynamicShadowProjectorRendererData was added to UniversalRenderPipelineAsset.", UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset);
			}
			UnityEditor.SerializedObject serializedObject = new UnityEditor.SerializedObject(additionalCameraData);
			serializedObject.FindProperty("m_RendererIndex").intValue = rendererIndex;
			serializedObject.ApplyModifiedPropertiesWithoutUndo();
			if (m_opaqueShadowShaderForLWRP == null)
			{
				m_opaqueShadowShaderForLWRP = Shader.Find("DynamicShadowProjector/Shadow/Opaque");
			}
			if (m_transparentShadowShaderForLWRP == null)
			{
				m_transparentShadowShaderForLWRP = Shader.Find("DynamicShadowProjector/Shadow/Transparent");
			}
		}
#endif
		internal void UpdateVisibilityAndPrepareRendering()
		{
			OnPreCull();
			m_camera.cullingMask = 0;
			m_camera.clearFlags = UnityEngine.CameraClearFlags.SolidColor;
			m_camera.enabled = enabled;
		}
		internal void ConfigureRenderTarget(RenderShadowTexturePass pass, ref UnityEngine.Rendering.Universal.CameraData cameraData)
		{
			PrepareRendering();
			if (useIntermediateTexture)
			{
				pass.ConfigureTarget(new RenderTargetIdentifier(m_temporaryRenderTarget));
				pass.ConfigureClear(ClearFlag.Color, m_camera.backgroundColor);
			}
			else
			{
				pass.ConfigureTarget(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget);
				cameraData.camera.allowMSAA = multiSampling != TextureMultiSample.x1;
				cameraData.cameraTargetDescriptor.msaaSamples = (int)multiSampling;
			}
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
