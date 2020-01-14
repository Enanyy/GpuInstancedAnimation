using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GpuInstancedAnimation : MonoBehaviour
{
    public Mesh mesh;
    public Material material;
    public const int TargetFrameRate = 30; //帧率
    public const int BoneMatrixRowCount = 3;


    public List<GpuInstancedAnimationClip> animationClips;

    private MaterialPropertyBlock materialPropertyBlock;

    private GpuInstancedAnimationClip mCurrentAnimationClip;
    private GpuInstancedAnimationClip mPreviousAnimationClip;

    private int mFadeFrame = 0;

    public event System.Action<GpuInstancedAnimationClip> onAnimationClipBegin;
    public event System.Action<GpuInstancedAnimationClip> onAnimationClipUpdate;
    public event System.Action<GpuInstancedAnimationClip> onAnimationClipEnd;

    public List<GpuInstancedAnimationBone> bones;
    public List<GpuInstancedAnimationBoneFrame> boneFrames;

    private Texture2D mAnimationTexture;
    public Texture2D AnimationTexture
    {
        get
        {
            if(mAnimationTexture== null)
            {
                if(material)
                {
                    mAnimationTexture = material.GetTexture("_AnimTex") as Texture2D;
                }
            }
            return mAnimationTexture;
        }
    }
    private int mPixelCountPerFrame = -1;
    public int PixelCountPerFrame
    {
        get
        {
            if(mPixelCountPerFrame < 0)
            {
                if(material)
                {
                    mPixelCountPerFrame = material.GetInt("_PixelCountPerFrame");
                }
            }
            return mPixelCountPerFrame;
        }
    }

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
    // Update is called once per frame
    void Update()
    {
        if (materialPropertyBlock == null)
        {
            materialPropertyBlock = new MaterialPropertyBlock();
        }

        if (mCurrentAnimationClip != null)
        {
            mCurrentAnimationClip.Update();
            materialPropertyBlock.SetInt("_CurrentFrame", mCurrentAnimationClip.GetCurrentFrame());

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

            materialPropertyBlock.SetInt("_PreviousFrame", previousFrame);
            materialPropertyBlock.SetFloat("_FadeStrength", fadeStrength);

        }

        Matrix4x4 matrix = transform.localToWorldMatrix;

        Graphics.DrawMesh(mesh, matrix, material, gameObject.layer, null, 0, materialPropertyBlock);
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

        if (mCurrentAnimationClip != null)
        {
            mCurrentAnimationClip.Reset(this, offsetFrame);

            mFadeFrame = fadeFrame;
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

    public GpuInstancedAnimationBoneFrame GetBoneFrame(string boneName)
    {
        var bone = GetBone(boneName);
        if (bone != null)
        {
            int frameIndex = 0;
            if (mCurrentAnimationClip != null )
            {
                frameIndex = mCurrentAnimationClip.GetCurrentFrame();
            }
            int index = frameIndex * bones.Count + bone.index;

            return boneFrames[index];
        }
        return null;
    }
   
}
