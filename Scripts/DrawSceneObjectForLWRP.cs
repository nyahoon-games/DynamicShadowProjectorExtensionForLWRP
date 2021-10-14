using UnityEngine;
using UnityEngine.Rendering;

namespace DynamicShadowProjector
{
	public partial class DrawSceneObject
	{
		[SerializeField]
		private string[] m_shaderTagList = null;
		public string[] shaderTagList
		{
			get { return m_shaderTagList; }
			set
			{
				m_shaderTagList = value;
				UpdateShaderTagIdList();
			}
		}

		private static ShaderTagId[] s_defaultShaderTagIdList = null;
		private ShaderTagId[] m_shaderTagIdList;
		public ShaderTagId[] shaderTagIds
		{
			get
			{
				if (m_shaderTagIdList == null)
				{
					UpdateShaderTagIdList();
				}
				return m_shaderTagIdList;
			}
		}
		public void UpdateShaderTagIdList()
		{
			if (m_shaderTagList == null || m_shaderTagList.Length == 0)
			{
				if (s_defaultShaderTagIdList == null)
				{
					s_defaultShaderTagIdList = new ShaderTagId[2];
					s_defaultShaderTagIdList[0] = new ShaderTagId("LightweightForward");
					s_defaultShaderTagIdList[1] = new ShaderTagId("SRPDefaultUnlit");
				}
				m_shaderTagIdList = s_defaultShaderTagIdList;
			}
			else
			{
				if (m_shaderTagIdList == null || m_shaderTagIdList.Length != m_shaderTagList.Length)
				{
					m_shaderTagIdList = new ShaderTagId[m_shaderTagList.Length];
				}
				for (int i = 0; i < m_shaderTagList.Length; ++i)
				{
					m_shaderTagIdList[i] = new ShaderTagId(m_shaderTagList[i]);
				}
			}
		}
	}
}
