#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 标记哪些骨骼需要导出
/// </summary>
public class GpuInstancedAnimationBoneExport:MonoBehaviour
{
    public List<Transform> bones;
}

#endif