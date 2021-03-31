//
// DynamicShadowProjectorRendererData.cs
//
// Dynamic Shadow Projector Extension For LWRP
//
// Copyright (c) 2019 NYAHOON GAMES PTE. LTD.
//

using UnityEngine.Rendering.Universal;
#if false && !UNITY_EDITOR
using System.Reflection;
#endif
namespace DynamicShadowProjector.LWRP
{
	public class DynamicShadowProjectorRendererData : ScriptableRendererData
	{
		private static DynamicShadowProjectorRendererData s_instance = null;
		private static DynamicShadowProjectorRenderer s_rendererInstance = null;
		public string[] m_sceneObjectShaderTagList = { "UniversalForward", "SRPDefaultUnlit" };
#if UNITY_EDITOR
		public static int FindDynamicShadowProjectorRenderer(UniversalRenderPipelineAsset pipelineAsset, bool insertIfNotExist)
		{
			UnityEditor.SerializedObject serializedPipelineAsset = new UnityEditor.SerializedObject(pipelineAsset);
			UnityEditor.SerializedProperty rendererList = serializedPipelineAsset.FindProperty("m_RendererDataList");
			int rendererCount = rendererList.arraySize;
			int indexToInsertRenderer = -1;
			for (int i = 0; i < rendererCount; ++i)
			{
				UnityEditor.SerializedProperty prop = rendererList.GetArrayElementAtIndex(i);
				if (prop.objectReferenceValue is DynamicShadowProjectorRendererData)
				{
					return i;
				}
				if (prop.objectReferenceValue == null && indexToInsertRenderer == -1)
				{
					indexToInsertRenderer = i;
				}
			}
			if (insertIfNotExist)
			{
				if (indexToInsertRenderer == -1)
				{
					indexToInsertRenderer = rendererCount;
					rendererList.InsertArrayElementAtIndex(indexToInsertRenderer);
				}
				UnityEditor.SerializedProperty prop = rendererList.GetArrayElementAtIndex(indexToInsertRenderer);
				prop.objectReferenceValue = DynamicShadowProjectorRendererData.instance;
				serializedPipelineAsset.ApplyModifiedPropertiesWithoutUndo();
				UnityEngine.Debug.Log("DynamicShadowProjectorRendererData was added to UniversalRenderPipelineAsset.", pipelineAsset);
				return indexToInsertRenderer;
			}
			return -1;
		}
#endif
		public static DynamicShadowProjectorRendererData instance
		{
			get
			{
				if (s_instance == null)
				{
					s_instance = new DynamicShadowProjectorRendererData();
#if UNITY_EDITOR
					string scriptPath = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.MonoScript.FromScriptableObject(s_instance));
					scriptPath = System.IO.Path.GetDirectoryName(scriptPath);
					string dataPath = scriptPath.Replace("Scripts", "Data");
					dataPath = System.IO.Path.Combine(dataPath, "DynamicShadowProjectorRenderer.asset");
					DynamicShadowProjectorRendererData test = UnityEditor.AssetDatabase.LoadAssetAtPath<DynamicShadowProjectorRendererData>(dataPath);
					if (test == null)
					{
						UnityEditor.AssetDatabase.CreateAsset(s_instance, dataPath);
						s_instance = UnityEditor.AssetDatabase.LoadAssetAtPath<DynamicShadowProjectorRendererData>(dataPath);
					}
					else
					{
						s_instance = test;
					}
					if (UniversalRenderPipeline.asset != null)
					{
						FindDynamicShadowProjectorRenderer(UniversalRenderPipeline.asset, true);
					}
#endif
				}
				return s_instance;
			}
		}
		private int m_rendererIndex = -1;
		public int rendererIndex
		{
			get
			{
				if (0 <= m_rendererIndex) return m_rendererIndex;
				UniversalRenderPipelineAsset pipelineAsset = UniversalRenderPipeline.asset;
				if (pipelineAsset == null)
				{
					return -1;
				}
#if UNITY_EDITOR
				m_rendererIndex = FindDynamicShadowProjectorRenderer(pipelineAsset, true);
				return m_rendererIndex;
#else
				int index = 0;
#if false
				TypeInfo typeInfo = typeof(UniversalRenderPipelineAsset).GetTypeInfo();
				MethodInfo getRenderer = typeInfo.GetMethod("GetRenderer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				object[] param = new object[] { index };
#endif
				do
				{
#if false
					param[0] = index;
					ScriptableRenderer renderer = (ScriptableRenderer)getRenderer.Invoke(pipelineAsset, param);
#else
					// if you get a compile error here, please update Universal RP package. Supported minimum version is v7.3.0.
					// Or, replace `#if false` with `#if true` in this file.
					ScriptableRenderer renderer = pipelineAsset.GetRenderer(index);
#endif
					if (renderer == s_rendererInstance)
					{
						m_rendererIndex = index;
						return index;
					}
					++index;
				} while (index < 100);
				UnityEngine.Debug.LogError("DynamicShadowProjectorRendererData was not found in the current render pipline asset.", UniversalRenderPipeline.asset);
				return -1;
#endif
			}
		}
		DynamicShadowProjectorRendererData()
		{
			s_instance = this;
		}
		protected override ScriptableRenderer Create()
		{
			if (s_rendererInstance == null)
			{
				s_rendererInstance = new DynamicShadowProjectorRenderer(this);
			}
			m_rendererIndex = -1; // reset index
			s_instance = this; // make sure that s_instance is used by the current render pipeline.
			return s_rendererInstance;
		}
	}
}
