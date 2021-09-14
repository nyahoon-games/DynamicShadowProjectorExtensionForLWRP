using UnityEngine;

namespace DynamicShadowProjector.LWRP.Demo
{
	[RequireComponent(typeof(PropertyBlockForTransparentReceiver))]
	public class FlowTexture : MonoBehaviour
	{
		private static int s_baseMapSTId = Shader.PropertyToID("_BaseMap_ST");

		public Vector2 velocity;

		Renderer m_renderer;
		PropertyBlockForTransparentReceiver m_propertyBlockForTransparentReceiver;
		MaterialPropertyBlock m_materialPropertyBlock;
		Vector4 m_textureScaleOffset;
		void Start()
		{
			m_renderer = GetComponent<Renderer>();
			m_propertyBlockForTransparentReceiver = GetComponent<PropertyBlockForTransparentReceiver>();
			m_textureScaleOffset = m_renderer.sharedMaterial.GetVector(s_baseMapSTId);
			m_materialPropertyBlock = new MaterialPropertyBlock();
		}

		void Update()
		{
			m_textureScaleOffset.z += velocity.x * Time.deltaTime;
			m_textureScaleOffset.w += velocity.y * Time.deltaTime;
			while (m_textureScaleOffset.z < 0) m_textureScaleOffset.z += 1.0f;
			while (m_textureScaleOffset.w < 0) m_textureScaleOffset.w += 1.0f;
			while (1 < m_textureScaleOffset.z) m_textureScaleOffset.z -= 1.0f;
			while (1 < m_textureScaleOffset.w) m_textureScaleOffset.w -= 1.0f;
			// Apply property changes to MaterialPropertyBlock instead of Material.
			// Otherwise, the shadow texture rendered by FogShadowProjector will not have texture animation.
			m_renderer.GetPropertyBlock(m_materialPropertyBlock);
			m_materialPropertyBlock.SetVector(s_baseMapSTId, m_textureScaleOffset);
			m_renderer.SetPropertyBlock(m_materialPropertyBlock);
			m_propertyBlockForTransparentReceiver.UpdatePropertyBlock();
		}
	}
}
