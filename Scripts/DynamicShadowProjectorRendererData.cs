//
// DynamicShadowProjectorRendererData.cs
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
	[System.Obsolete]
	public class DynamicShadowProjectorRendererData : ScriptableRendererData
	{
		public string[] m_sceneObjectShaderTagList = { "UniversalForward", "SRPDefaultUnlit" };
#if UNITY_EDITOR
		public static void DeleteDynamicShadowProjectorRenderer(UniversalRenderPipelineAsset pipelineAsset)
		{
			UnityEditor.SerializedObject serializedPipelineAsset = new UnityEditor.SerializedObject(pipelineAsset);
			UnityEditor.SerializedProperty rendererList = serializedPipelineAsset.FindProperty("m_RendererDataList");
			for (int i = 0; i < rendererList.arraySize; ++i)
			{
				UnityEditor.SerializedProperty prop = rendererList.GetArrayElementAtIndex(i);
				if (prop.objectReferenceValue == null || prop.objectReferenceValue is DynamicShadowProjectorRendererData)
				{
					rendererList.DeleteArrayElementAtIndex(i--);
				}
			}
			serializedPipelineAsset.ApplyModifiedProperties();
		}
#endif
		protected override ScriptableRenderer Create()
		{
#if UNITY_EDITOR
			// no longer used. should be removed from RenderPipelineAsset
			if (GraphicsSettings.renderPipelineAsset is UniversalRenderPipelineAsset)
			{
				DeleteDynamicShadowProjectorRenderer(GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset);
			}
			try
			{
				for (int level = 0; ; ++level)
				{
					RenderPipelineAsset pipelineAsset = QualitySettings.GetRenderPipelineAssetAt(level);
					if (pipelineAsset is UniversalRenderPipelineAsset)
					{
						DeleteDynamicShadowProjectorRenderer(pipelineAsset as UniversalRenderPipelineAsset);
					}
				}
			}
			catch (System.IndexOutOfRangeException)
			{

			}
#endif
			return null;
		}
	}
}
