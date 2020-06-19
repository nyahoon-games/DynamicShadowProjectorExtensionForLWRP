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
using System.Collections.Generic;

namespace DynamicShadowProjector.LWRP
{
	public class DynamicShadowProjectorRenderer : ScriptableRenderer
	{
		public class DynamicShadowProjectorComponents
		{
			public ShadowTextureRenderer shadowTextureRenderer { get; private set; }
			public DrawTargetObject drawTargetObject { get; private set; }
			public DrawSceneObject drawSceneObject { get; private set; }
			public void SetComponentsFromCamera(Camera camera)
			{
				shadowTextureRenderer = camera.GetComponent<ShadowTextureRenderer>();
				if (shadowTextureRenderer != null)
				{
					drawTargetObject = camera.GetComponent<DrawTargetObject>();
					drawSceneObject = camera.GetComponent<DrawSceneObject>();
					m_hasShadowTextureRenderer = true;
				}
				else
				{
					drawTargetObject = null;
					drawSceneObject = null;
					m_hasShadowTextureRenderer = false;
				}
			}
			public bool IsValid()
			{
				if (m_hasShadowTextureRenderer)
				{
					return shadowTextureRenderer != null;
				}
				return true;
			}
			private bool m_hasShadowTextureRenderer;
		}
		public DynamicShadowProjectorComponents GetDynamicShadowProjectorComponents(Camera camera)
		{
			DynamicShadowProjectorComponents components;
			if (!m_cameraToComponents.TryGetValue(camera, out components))
			{
				components = new DynamicShadowProjectorComponents();
				components.SetComponentsFromCamera(camera);
				m_cameraToComponents.Add(camera, components);
			}
			else if (!components.IsValid())
			{
				components.SetComponentsFromCamera(camera);
			}
			return components;
		}
		private Dictionary<Camera, DynamicShadowProjectorComponents> m_cameraToComponents;
		private RenderShadowTexturePass m_renderShadowTexturePass;
		public DynamicShadowProjectorRenderer(DynamicShadowProjectorRendererData data) : base(data)
		{
			m_cameraToComponents = new Dictionary<Camera, DynamicShadowProjectorComponents>();
			m_renderShadowTexturePass = new RenderShadowTexturePass(data, this);
		}
		public override void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, ref CameraData cameraData)
		{
			DynamicShadowProjectorComponents components = GetDynamicShadowProjectorComponents(cameraData.camera);
			ShadowTextureRenderer shadowTextureRenderer = components.shadowTextureRenderer;
			if (shadowTextureRenderer == null)
			{
#if UNITY_EDITOR
				Debug.LogWarning("No ShadowTextureRenderer found.");
#endif
				return;
			}
			shadowTextureRenderer.UpdateVisibilityAndPrepareRendering();
			cullingParameters.cullingMask = 0;
			if (shadowTextureRenderer.isProjectorVisible && shadowTextureRenderer.isActiveAndEnabled)
			{
				DrawSceneObject drawScene = components.drawSceneObject;
				if (drawScene != null)
				{
					cullingParameters.cullingMask = (uint)drawScene.cullingMask.value;
				}
				DrawTargetObject drawTarget = components.drawTargetObject;
				if (drawTarget != null)
				{
					drawTarget.SendMessage("OnPreCull");
				}
				shadowTextureRenderer.ConfigureRenderTarget(m_renderShadowTexturePass, ref cameraData);
			}
			cullingParameters.cullingOptions = CullingOptions.None;
			cullingParameters.shadowDistance = 0;
		}
		public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			renderingData.cameraData.isStereoEnabled = false;
			renderingData.cameraData.isSceneViewCamera = false;
			DynamicShadowProjectorComponents components = GetDynamicShadowProjectorComponents(renderingData.cameraData.camera);
			ShadowTextureRenderer shadowTextureRenderer = components.shadowTextureRenderer;
			if (shadowTextureRenderer != null && shadowTextureRenderer.isActiveAndEnabled && shadowTextureRenderer.isProjectorVisible)
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
