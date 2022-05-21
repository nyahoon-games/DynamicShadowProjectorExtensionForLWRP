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
		private RenderShadowTexturePass m_renderPass;
		private int m_currentFrameCount = -1;

		// use struct to prevent Unity Engine from restoring the object reference after rebuild scripts.
		private struct ProjectorReference
		{
			public ProjectorForLWRP.ProjectorForLWRP projectorForLWRP;
		}
		ProjectorReference m_refProjector = new ProjectorReference();
		public DrawTargetObject drawTargetObject
		{
			get;
			private set;
		}
		public DrawSceneObject drawSceneObject
		{
			get;
			private set;
		}
		private ProjectorForLWRP.ProjectorForLWRP projectorForLWRP
		{
			get { return m_refProjector.projectorForLWRP; }
			set { m_refProjector.projectorForLWRP = value; }
		}

		partial void PartialInitialize()
		{
			m_camera.enabled = false;
			drawTargetObject = GetComponent<DrawTargetObject>();
			drawSceneObject = GetComponent<DrawSceneObject>();
		}
		private void OnRenderProjector(Camera camera)
		{
			if (!enabled)
			{
				return;
			}
			if (m_currentFrameCount == Time.frameCount)
			{
				// do not render shadow texture more than once per frame.
				return;
			}
			m_currentFrameCount = Time.frameCount;
			OnPreCull();
			if (!isProjectorVisible)
			{
				return;
			}
			if (drawTargetObject != null)
			{
				drawTargetObject.OnPreCull();
			}
			if (m_renderPass == null)
			{
				m_renderPass = new RenderShadowTexturePass(this);
			}
			m_renderPass.ResetFrame();
			ProjectorForLWRP.ProjectorRendererFeature.AddRenderPass(camera, m_renderPass);
			if (m_isTexturePropertyChanged)
			{
				CreateRenderTexture();
			}
		}
		partial void OnRenderTextureCreated()
		{
			if (projectorForLWRP == null)
			{
				projectorForLWRP = GetComponent<ProjectorForLWRP.ProjectorForLWRP>();
				projectorForLWRP.onAddProjectorToRenderer += OnRenderProjector;
				if (projectorForLWRP == null)
				{
					//Debug.LogError("Projector For LWRP component was not found!", this);
					return;
				}
			}
			projectorForLWRP.propertyBlock.SetTexture(s_shadowTexParamID, shadowTexture);
			projectorForLWRP.propertyBlock.SetFloat(s_mipLevelParamID, m_mipLevel);
		}
		internal void ConfigureRenderTarget(RenderShadowTexturePass pass)
		{
			PrepareRendering();
			if (useIntermediateTexture)
			{
				pass.ConfigureTarget(new RenderTargetIdentifier(m_temporaryRenderTarget));
			}
			else
			{
				pass.ConfigureTarget(new RenderTargetIdentifier(shadowTexture));
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
				srcId = new RenderTargetIdentifier(shadowTexture);
			}
			AddPostRenderPassCommands(postProcessCommandBuffer, srcId);
			context.ExecuteCommandBuffer(postProcessCommandBuffer);
			m_shadowTextureValid = true;
			m_camera.targetTexture = shadowTexture;
			m_camera.clearFlags = CameraClearFlags.Nothing;
			ReleaseTemporaryRenderTarget();
		}
	}
}
