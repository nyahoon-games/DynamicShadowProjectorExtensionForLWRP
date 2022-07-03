using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace DynamicShadowProjector.LWRP.Editor
{
	[InitializeOnLoad]
	public class DeleteObsoleteObjects : IProcessSceneWithReport
	{
		public int callbackOrder => 0;

		static DeleteObsoleteObjects()
		{
			DeleteMissingRendererData();
			// Stop calling the following function which may generate "Not responding" windows (Issue #2)
			// DeleteAdditionalCameraDataFromDynamicShadowProjectorPrefab();
			EditorSceneManager.sceneOpened += DeleteAdditionalCameraDataFromDynamicShadowProjector;
		}

		static void DeleteMissingRendererDataFromURPAsset(UniversalRenderPipelineAsset pipelineAsset)
		{
			SerializedObject serializedPipelineAsset = new SerializedObject(pipelineAsset);
			SerializedProperty rendererList = serializedPipelineAsset.FindProperty("m_RendererDataList");
			bool deleted = false;
			for (int i = 0; i < rendererList.arraySize; ++i)
			{
				SerializedProperty prop = rendererList.GetArrayElementAtIndex(i);
				if (prop.objectReferenceValue == null || !(prop.objectReferenceValue is ScriptableRendererData))
				{
					rendererList.DeleteArrayElementAtIndex(i--);
					deleted = true;
				}
			}
			if (deleted)
			{
				serializedPipelineAsset.ApplyModifiedProperties();
			}
		}
		static void DeleteMissingRendererData()
		{
			if (GraphicsSettings.renderPipelineAsset is UniversalRenderPipelineAsset)
			{
				DeleteMissingRendererDataFromURPAsset(GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset);
			}
			try
			{
				for (int level = 0; ; ++level)
				{
					RenderPipelineAsset pipelineAsset = QualitySettings.GetRenderPipelineAssetAt(level);
					if (pipelineAsset is UniversalRenderPipelineAsset)
					{
						DeleteMissingRendererDataFromURPAsset(pipelineAsset as UniversalRenderPipelineAsset);
					}
				}
			}
			catch (System.IndexOutOfRangeException)
			{

			}
		}
		static void DeleteAdditionalCameraDataFromDynamicShadowProjector(GameObject gameObject)
		{
			ShadowTextureRenderer[] shadowTextureRenderers = gameObject.GetComponentsInChildren<ShadowTextureRenderer>(true);
			foreach (ShadowTextureRenderer shadowTextureRenderer in shadowTextureRenderers)
			{
				UniversalAdditionalCameraData cameraData;
				if (shadowTextureRenderer.TryGetComponent(out cameraData))
				{
					Undo.DestroyObjectImmediate(cameraData);
					Debug.Log("UniversalAdditionalCameraData component was removed from " + shadowTextureRenderer.name + " because Dynamic Shadow Projector no longer uses camera for rendering shadow textures in URP.");
				}
			}
		}
		static void DeleteAdditionalCameraDataFromDynamicShadowProjector(Scene scene)
		{
			GameObject[] gameObjects = scene.GetRootGameObjects();
			foreach (GameObject gameObject in gameObjects)
			{
				DeleteAdditionalCameraDataFromDynamicShadowProjector(gameObject);
			}
		}
		static void DeleteAdditionalCameraDataFromDynamicShadowProjector(Scene scene, OpenSceneMode mode)
		{
			DeleteAdditionalCameraDataFromDynamicShadowProjector(scene);
		}
		static void DeleteAdditionalCameraDataFromDynamicShadowProjectorPrefab()
		{
			string[] dynamicShadowProjectors = AssetDatabase.FindAssets("t:Prefab");
			foreach (string guid in dynamicShadowProjectors)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
				GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
				ShadowTextureRenderer[] shadowTextureRenderers = gameObject.GetComponentsInChildren<ShadowTextureRenderer>(true);
				bool deleted = false;
				foreach (ShadowTextureRenderer shadowTextureRenderer in shadowTextureRenderers)
				{
					GameObject projectorPrefab = PrefabUtility.GetCorrespondingObjectFromSource(shadowTextureRenderer.gameObject) as GameObject;
					UniversalAdditionalCameraData cameraData;
					if (projectorPrefab.TryGetComponent(out cameraData))
					{
						Undo.DestroyObjectImmediate(cameraData);
						deleted = true;
					}
				}
				gameObject.SetActive(false);
				Object.DestroyImmediate(gameObject);
				if (deleted)
				{
					EditorUtility.SetDirty(prefab);
				}
			}
		}
		public void OnProcessScene(Scene scene, BuildReport report)
		{
			DeleteAdditionalCameraDataFromDynamicShadowProjector(scene);
		}
	}
}
