using System;
using UnityEngine;

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

    private GpuInstancedAnimation mAnimation;
    private int mCurrentOffsetFrame = 0;
    private float mCurrentTime = 0;
    private int mCurrentFrame = 0;
    public int CurrentFrame
    {
        get
        {
            return mCurrentFrame;
        }
        set
        {
            mCurrentFrame = value;

            if (mAnimation != null)
            {
                if (mCurrentFrame == 0)
                {
                    mAnimation.OnAnimationClipBegin(this);
                }
                mAnimation.OnAnimationClipUpdate(this);
                if (mCurrentFrame >= FrameCount - 1)
                {
                    mAnimation.OnAnimationClipEnd(this);
                }
            }

        }
    }

    public int GetCurrentFrame()
    {
        return StartFrame + CurrentFrame ;
    }

    public void Reset(GpuInstancedAnimation animation, int offsetFrame)
    {
        mAnimation = animation;

        mCurrentOffsetFrame = offsetFrame;
        mCurrentFrame = 0;
        mCurrentTime = 0;
    }

    public void Update()
    {
        mCurrentTime += Time.deltaTime;

        if (wrapMode == WrapMode.Once)
        {
            CurrentFrame = ((int)(mCurrentTime * GpuInstancedAnimation.TargetFrameRate) + mCurrentOffsetFrame);

            if (CurrentFrame >= FrameCount)
            {
                CurrentFrame = 0;//重置到第0帧
            }
        }
        else if (wrapMode == GpuInstancedAnimationClip.WrapMode.ClampForever)
        {
            CurrentFrame = ((int)(mCurrentTime * GpuInstancedAnimation.TargetFrameRate) + mCurrentOffsetFrame);

            if (CurrentFrame >= FrameCount - 1)
            {
                CurrentFrame = FrameCount - 1;//固定在最后一帧
            }
        }
        else
        {
            CurrentFrame = ((int)(mCurrentTime * GpuInstancedAnimation.TargetFrameRate) + mCurrentOffsetFrame) % FrameCount;
        }
    }
}