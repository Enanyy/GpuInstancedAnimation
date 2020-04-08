using System;
using UnityEngine;
[Serializable]
public class GpuInstancedAnimationBone
{
    public string boneName;
    public int index;
    public float blendWeight;
}
[Serializable]
public struct GpuInstancedAnimationBoneFrame
{
    public Vector3 localPosition;
    public Quaternion rotation;

    public static GpuInstancedAnimationBoneFrame Lerp(GpuInstancedAnimationBoneFrame a,GpuInstancedAnimationBoneFrame b, float factor)
    {
        GpuInstancedAnimationBoneFrame frame = new GpuInstancedAnimationBoneFrame();
        frame.localPosition = Vector3.Lerp(a.localPosition, b.localPosition, factor);
        frame.rotation = Quaternion.Lerp(a.rotation, b.rotation, factor);

        return frame;
    }
}

[Serializable]
public class GpuInstancedAnimationBoneHeight
{
    public Transform bone;
    public int index;
    public float height;
}

