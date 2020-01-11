using System;


[Serializable]
public class GpuInstancedAnimationClip
{
    public enum WrapMode
    {
        Once,
        Loop,
        ClampForever,
    }

    public string Name;
    public int StartFrame;
    public int EndFrame;
    public int FrameCount;

    public WrapMode wrapMode = WrapMode.Once;

    public GpuInstancedAnimationClip(string name, int startFrame, int endFrame, int frameCount)
    {
        Name = name;
        StartFrame = startFrame;
        EndFrame = endFrame;
        FrameCount = frameCount;
    }
}