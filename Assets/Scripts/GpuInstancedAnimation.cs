using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IUpdate
{
    void OnUpdate();
}

public enum BlendDirection
{
    Top,
    Down,
}

public class GpuInstancedAnimation : MonoBehaviour,IUpdate
{
    public Mesh mesh;
    public Material material;
    public const int TargetFrameRate = 30; //帧率
    public const int BoneMatrixRowCount = 3;


    public List<GpuInstancedAnimationClip> animationClips;

    private static MaterialPropertyBlock materialPropertyBlock;

    private GpuInstancedAnimationClip mCurrentAnimationClip;
    private GpuInstancedAnimationClip mPreviousAnimationClip;
    private GpuInstancedAnimationClip mBlendAnimationClip;
    private GpuInstancedAnimationClip mBlendPreviousAnimationClip;

    public float speed = 1;

    private int mFadeFrame = 0;

    private int mBlendFadeFrame = 0;

    public bool isBlending
    {
        get { return mBlendAnimationClip != null; }
    }
    public string playingAnimationClip
    {
        get
        {
            if(mCurrentAnimationClip!=null)
            {
                return mCurrentAnimationClip.Name;
            }
            return null;
        }
    }

    public BlendDirection blendDirection { get; private set; }

    public event System.Action<GpuInstancedAnimationClip> onAnimationClipBegin;
    public event System.Action<GpuInstancedAnimationClip> onAnimationClipUpdate;
    public event System.Action<GpuInstancedAnimationClip> onAnimationClipEnd;

    public List<GpuInstancedAnimationBone> bones;
    public List<GpuInstancedAnimationBoneFrame> boneFrames;

    public void OnAnimationClipBegin(GpuInstancedAnimationClip clip)
    {
        onAnimationClipBegin?.Invoke(clip);
    }
    public void OnAnimationClipUpdate(GpuInstancedAnimationClip clip)
    {
        onAnimationClipUpdate?.Invoke(clip);
    }
    public void OnAnimationClipEnd(GpuInstancedAnimationClip clip)
    {
        onAnimationClipEnd?.Invoke(clip);
    }
    private int mLayer = 0;
    private void Awake()
    {
        materialPropertyBlock  = new MaterialPropertyBlock();
        mLayer = gameObject.layer;
    }
    // Update is called once per frame
    public void OnUpdate()
    {
        if (mCurrentAnimationClip != null)
        {
            mCurrentAnimationClip.Update();
            int currentFrame = mCurrentAnimationClip.GetCurrentFrame();
            int previousFrame = 0;
            if(mPreviousAnimationClip != null)
            {
                mPreviousAnimationClip.Update();
                
                previousFrame = mPreviousAnimationClip.GetCurrentFrame();

                if (mPreviousAnimationClip.CurrentFrame >= mPreviousAnimationClip.EndFrame - 1)
                {
                    mPreviousAnimationClip = null;
                }
            }
            float fadeStrength = 1;
            if (mPreviousAnimationClip != null)
            {
                if (mFadeFrame > 0 && mCurrentAnimationClip.CurrentFrame < mFadeFrame)
                {
                    fadeStrength = mCurrentAnimationClip.CurrentFrame * 1f / mFadeFrame;
                }
                else
                {
                    mFadeFrame = 0;
                }
            }

            int blendFrame = 0;
            int blendPreviousFrame = 0;
            if (mBlendAnimationClip != null)
            {
                if (mBlendAnimationClip != mCurrentAnimationClip && mBlendAnimationClip != mPreviousAnimationClip)
                {
                    mBlendAnimationClip.Update();
                }
                blendFrame = mBlendAnimationClip.GetCurrentFrame();

                if(mBlendPreviousAnimationClip!=null)
                {
                    if (mBlendPreviousAnimationClip != mCurrentAnimationClip && mBlendPreviousAnimationClip != mPreviousAnimationClip)
                    {
                        mBlendPreviousAnimationClip.Update();
                    }
                    blendPreviousFrame = mBlendPreviousAnimationClip.GetCurrentFrame();

                    if (mBlendPreviousAnimationClip.CurrentFrame >= mBlendPreviousAnimationClip.EndFrame - 1)
                    {
                        mBlendPreviousAnimationClip = null;
                    }
                }
            }
            float blendFadeStrength = 1;

            if (mBlendPreviousAnimationClip != null)
            {
                if (mBlendFadeFrame > 0 && mBlendPreviousAnimationClip.CurrentFrame < mBlendFadeFrame)
                {
                    blendFadeStrength = mBlendPreviousAnimationClip.CurrentFrame * 1f / mBlendFadeFrame;
                }
                else
                {
                    mBlendFadeFrame = 0;
                }
            }
        
            materialPropertyBlock.SetInt("_CurrentFrame", currentFrame);
            materialPropertyBlock.SetInt("_PreviousFrame", previousFrame);
            materialPropertyBlock.SetFloat("_FadeStrength", fadeStrength);
            materialPropertyBlock.SetInt("_BlendFrame", blendFrame);
            materialPropertyBlock.SetInt("_BlendPreviousFrame", blendPreviousFrame);
            materialPropertyBlock.SetFloat("_BlendFadeStrength", blendFadeStrength);
            materialPropertyBlock.SetFloat("_BlendDirection", blendDirection == BlendDirection.Top ? 1 : 0);
        }

        Graphics.DrawMesh(mesh, transform.localToWorldMatrix, material, mLayer, null, 0, materialPropertyBlock);
    }

    public void Play(string clipName, int offsetFrame = 0, int fadeFrame = 0)
    {
        if (animationClips == null)
        {
            return;
        }
        if(mCurrentAnimationClip!= null && mCurrentAnimationClip.Name == clipName)
        {
            return;
        }

        mPreviousAnimationClip = mCurrentAnimationClip;

        mCurrentAnimationClip = animationClips.Find((x) => x.Name == clipName);

        SetBlend(null);

        if (mCurrentAnimationClip != null)
        {
            mCurrentAnimationClip.Reset(this, offsetFrame);

            mFadeFrame = Mathf.Clamp(fadeFrame, 0, mCurrentAnimationClip.FrameCount);
        }
    }
    public void SetBlend(string clipName, BlendDirection direction = BlendDirection.Down, int fadeFrame = 0)
    {
        if (animationClips == null)
        {
            return;
        }
        if(string.IsNullOrEmpty(clipName))
        {
            mBlendAnimationClip = null; 
            mBlendPreviousAnimationClip = null;

            return;
        }
        blendDirection = direction;
        if(mBlendAnimationClip!= null)
        {
            mBlendPreviousAnimationClip = mBlendAnimationClip;
        }
        else
        {
            if (mCurrentAnimationClip != null)
            {
                mBlendPreviousAnimationClip = mCurrentAnimationClip;
            }
        }

        mBlendAnimationClip = animationClips.Find(x => x.Name == clipName);
        if(mBlendAnimationClip!=null)
        {
            mBlendFadeFrame = Mathf.Clamp(fadeFrame, 0, mBlendAnimationClip.FrameCount);
        }
    }
    public GpuInstancedAnimationBone GetBone(string boneName)
    {
        if (bones == null || string.IsNullOrEmpty(boneName))
        {
            return null;
        }

        for (int i = 0, count = bones.Count; i < count; ++i)
        {
            var bone = bones[i];
            if(bone.boneName == boneName)
            {
                return bone;
            }
        }

        return null;
    }

    private static GpuInstancedAnimationBoneFrame boneFrame = new GpuInstancedAnimationBoneFrame();
    public GpuInstancedAnimationBoneFrame GetBoneFrame(string boneName)
    {
        var bone = GetBone(boneName);
        if (bone != null)
        {
            int currentFrame = 0;
            if (mCurrentAnimationClip != null )
            {
                currentFrame = mCurrentAnimationClip.GetCurrentFrame();
            }
            int currentIndex = currentFrame * bones.Count + bone.index;

            var currentBoneFrame = boneFrames[currentIndex];

            if(mFadeFrame > 0 && mCurrentAnimationClip.CurrentFrame < mFadeFrame &&  mPreviousAnimationClip != null)
            {
                int previousFrame = mPreviousAnimationClip.GetCurrentFrame();

                int previousIndex = previousFrame * bones.Count + bone.index;

                var previousBoneFrame = boneFrames[previousIndex];

                float fadeStrength = mCurrentAnimationClip.CurrentFrame * 1f / mFadeFrame;

                boneFrame.localPosition = Vector3.Lerp(previousBoneFrame.localPosition, currentBoneFrame.localPosition, fadeStrength);
                boneFrame.rotation = Quaternion.Lerp(previousBoneFrame.rotation, currentBoneFrame.rotation, fadeStrength);

                return boneFrame;
            }

            return currentBoneFrame;
        }
        return null;
    }
   
}
