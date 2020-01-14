using System;
using UnityEngine;

public class GpuInstancedAnimationFrame
{
    public GpuInstancedAnimation animation;

    public GpuInstancedAnimationClip CurrentAnimationClip { get; private set; }

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
            if (CurrentAnimationClip != null)
            {
                if (animation != null)
                {
                    if (mCurrentFrame == 0)
                    {
                        animation.OnAnimationClipBegin(this);
                    }
                    animation.OnAnimationClipUpdate(this);
                    if (mCurrentFrame >= CurrentAnimationClip.FrameCount - 1)
                    {
                        animation.OnAnimationClipEnd(this);
                    }
                }
            }
        }
    }

    public int GetCurrentFrame()
    {
         return CurrentAnimationClip != null ? CurrentAnimationClip.StartFrame + CurrentFrame : CurrentFrame;
    }

    public GpuInstancedAnimationFrame(GpuInstancedAnimation animation)
    {
        this.animation = animation;
    }
    public void Reset(GpuInstancedAnimationClip clip, int offsetFrame)
    {
        CurrentAnimationClip = clip;
        mCurrentOffsetFrame = offsetFrame;
        mCurrentFrame = 0;
        mCurrentTime = 0;
    }

    public void Update()
    {
        mCurrentTime += Time.deltaTime;

        if (CurrentAnimationClip != null)
        {
            if (CurrentAnimationClip.wrapMode == GpuInstancedAnimationClip.WrapMode.Once)
            {
                CurrentFrame = ((int)(mCurrentTime * GpuInstancedAnimation.TargetFrameRate) + mCurrentOffsetFrame);

                if (CurrentFrame >= CurrentAnimationClip.FrameCount)
                {
                    CurrentFrame = 0;//重置到第0帧
                }
            }
            else if (CurrentAnimationClip.wrapMode == GpuInstancedAnimationClip.WrapMode.ClampForever)
            {
                CurrentFrame = ((int)(mCurrentTime * GpuInstancedAnimation.TargetFrameRate) + mCurrentOffsetFrame);

                if (CurrentFrame >= CurrentAnimationClip.FrameCount - 1)
                {
                    CurrentFrame = CurrentAnimationClip.FrameCount - 1;//固定在最后一帧
                }
            }
            else
            {
                CurrentFrame = ((int)(mCurrentTime * GpuInstancedAnimation.TargetFrameRate) + mCurrentOffsetFrame) % CurrentAnimationClip.FrameCount;
            }  
        }
    }
}
