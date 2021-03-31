using UnityEngine;
using UnityEditor.Build;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DynamicShadowProjector.LWRP.Editor
{
	public class BuildPreprocessor : IPreprocessBuildWithReport
	{
		public int callbackOrder => 0;

		public void OnPreprocessBuild(BuildReport report)
		{
			UniversalRenderPipelineAsset currentPipelineAsset = UniversalRenderPipeline.asset;
			if (currentPipelineAsset != null)
			{
				int rendererIndex = DynamicShadowProjectorRendererData.FindDynamicShadowProjectorRenderer(currentPipelineAsset, false);
				if (0 <= rendererIndex)
				{
					// there exists Dynamic Shadow Projector Renderer in the current render pipeline asset.
					// check if all the render pipline asset in Quality Settings have it too.
					for (int i = 0; ; ++i)
					{
						try
						{
							UniversalRenderPipelineAsset pipelineAsset = QualitySettings.GetRenderPipelineAssetAt(i) as UniversalRenderPipelineAsset;
							if (pipelineAsset != null && pipelineAsset != currentPipelineAsset)
							{
								DynamicShadowProjectorRendererData.FindDynamicShadowProjectorRenderer(pipelineAsset, true);
							}
						}
						catch (System.IndexOutOfRangeException)
						{
							break;
						}
					}
				}
			}
		}
	}
}
