﻿//
// DynamicShadowProjectorRendererData.cs
//
// Dynamic Shadow Projector Extension For LWRP
//
// Copyright (c) 2019 NYAHOON GAMES PTE. LTD.
//

using UnityEngine.Rendering.LWRP;

namespace DynamicShadowProjector.LWRP
{
	public class DynamicShadowProjectorRendererData : ScriptableRendererData
	{
		private static DynamicShadowProjectorRendererData s_instance = null;
		private static DynamicShadowProjectorRenderer s_rendererInstance = null;
		public string[] m_sceneObjectShaderTagList = { "LightweightForward", "SRPDefaultUnlit" };
		public static DynamicShadowProjectorRendererData instance
		{
			get
			{
				if (s_instance == null)
				{
					DynamicShadowProjectorRendererData rendererData = new DynamicShadowProjectorRendererData();
#if UNITY_EDITOR
					string scriptPath = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.MonoScript.FromScriptableObject(rendererData));
					scriptPath = System.IO.Path.GetDirectoryName(scriptPath);
					string dataPath = scriptPath.Replace("Scripts", "Data");
					dataPath = System.IO.Path.Combine(dataPath, "DynamicShadowProjectorRenderer.asset");
					UnityEditor.AssetDatabase.CreateAsset(rendererData, dataPath);
					rendererData = UnityEditor.AssetDatabase.LoadAssetAtPath<DynamicShadowProjectorRendererData>(dataPath);
#endif
					s_instance = rendererData;
				}
				return s_instance;
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
			return s_rendererInstance;
		}
	}
}
