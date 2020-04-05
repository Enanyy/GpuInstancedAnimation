using System;
using UnityEngine;
[Serializable]
public class GpuInstancedAnimationBone
{
    public string boneName;
    public int index;
}
[Serializable]
public class GpuInstancedAnimationBoneFrame
{
    public Vector3 localPosition;
    public Quaternion rotation;
}

[Serializable]
public class GpuInstancedAnimationBoneHeight
{
    public Transform bone;
    public int index;
    public float height;
}

