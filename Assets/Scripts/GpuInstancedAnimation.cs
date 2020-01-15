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

    private static MaterialPropertyBlock materialPropertyBlock;

    private GpuInstancedAnimationClip mCurrentAnimationClip;

    public float speed = 1;

    private GpuInstancedAnimationClip mPreviousAnimationClip;

    private int mFadeFrame = 0;

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
    void Update()
    {
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

        if (mCurrentAnimationClip != null)
        {
            mCurrentAnimationClip.Reset(this, offsetFrame);

            mFadeFrame = Mathf.Clamp(fadeFrame, 0, mCurrentAnimationClip.FrameCount);
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
