#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 标记哪些骨骼需要导出
/// </summary>
public class GpuInstancedAnimationBoneExport:MonoBehaviour
{
    public List<Transform> bones;
    public List<GpuInstancedAnimationBoneHeight> boneHeights = new List<GpuInstancedAnimationBoneHeight>();


    [ContextMenu("Setup")]
    private void Setup()
    {
        SkinnedMeshRenderer renderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if(renderer!=null)
        {
            for(int i = 0; i < renderer.bones.Length; ++i)
            {
                var bone = renderer.bones[i];
                var boneHeight = GetBoneHeight(bone);
                if(boneHeight == null)
                {
                    boneHeight = new GpuInstancedAnimationBoneHeight();
                    boneHeight.bone = bone;
                    boneHeights.Add(boneHeight);
                }
                boneHeight.index = i;
            }
        }
    }

    public GpuInstancedAnimationBoneHeight GetBoneHeight(Transform bone)
    {
        for(int i = 0; i< boneHeights.Count; ++i)
        {
            if(boneHeights[i].bone == bone)
            {
                return boneHeights[i];
            }
        }
        return null;
    }
    public float GetBoneHeightWeight(int index)
    {
        for(int i = 0; i < boneHeights.Count; ++i)
        {
            if(boneHeights[i].index == index)
            {
                return boneHeights[i].height;
            }
        }
        return 1;
    }
}

#endif