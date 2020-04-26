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
    public Vector3 localForward;


    public static GpuInstancedAnimationBoneFrame Lerp(GpuInstancedAnimationBoneFrame a,GpuInstancedAnimationBoneFrame b, float factor)
    {
        GpuInstancedAnimationBoneFrame frame = new GpuInstancedAnimationBoneFrame();
        frame.localPosition = Vector3.Lerp(a.localPosition, b.localPosition, factor);
        frame.localForward = Vector3.Lerp(a.localForward, b.localForward, factor);

        return frame;
    }
    public void Transform(Matrix4x4 localToWorld, out Vector3 worldPosition, out Vector3 worldForward)
    {
        var position = localToWorld * new Vector4(localPosition.x, localPosition.y, localPosition.z, 1);
        worldPosition = new Vector3(position.x, position.y, position.z);
        var zero = localToWorld * new Vector4(0, 0, 0, 1);
        var forward = localToWorld * new Vector4(localForward.x, localForward.y, localForward.z, 1);
        worldForward = new Vector3(forward.x - zero.x, forward.y - zero.y, forward.z - zero.z).normalized;
    }

    public void Transform(Transform transform, out Vector3 worldPosition, out Vector3 worldForward)
    {
        worldPosition = transform.TransformPoint(localPosition);
        worldForward = transform.TransformVector(localForward);
    }
}

[Serializable]
public class GpuInstancedAnimationBoneHeight
{
    public Transform bone;
    public int index;
    public float height;
}

