using UnityEngine;
using UnityEditor;

namespace DynamicShadowProjector.LWRP.Editor
{
    // define this class to prevent the use of ScriptableRendererDataEditor
    [CustomEditor(typeof(DynamicShadowProjectorRendererData))]
    public class DynamicShadowProjectorRendererDataEditor : UnityEditor.Editor
    {
    }
}
