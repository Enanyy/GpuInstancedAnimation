using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IUpdate
{
    void OnUpdate();
}

public enum BlendDirection
{
    Down,
    Top,
}

public class GpuInstancedAnimation : MonoBehaviour, IUpdate
{
    public Vector3 size = new Vector3(1, 2, 1);

    private Bounds mBounds;
    public Bounds bounds
    {
        get
        {
            if(mesh!= null)
            {
                mBounds.size = mesh.bounds.size;      
            }
            mBounds.center = transform.position;
          
            return mBounds;
        }
    }

    /// <summary>
    /// 是否被TargetCamera裁剪了
    /// </summary>
    public bool isCulling
    {
        get
        {
            return CameraManager.Instance.TestPlanesAABB(bounds) == false;
        }
    }

    public Mesh mesh;
    public Material material;
    public const int TargetFrameRate = 30; //帧率
    public const int BoneMatrixRowCount = 3;

    public string defaultAnimationClip = "idle";

    public List<GpuInstancedAnimationClip> animationClips;

    private static MaterialPropertyBlock materialPropertyBlock;

    private GpuInstancedAnimationClip mCurrentAnimationClip;
    private GpuInstancedAnimationClip mPreviousAnimationClip;
    private GpuInstancedAnimationClip mBlendAnimationClip;


    public float speed = 1;

    private int mFadeFrame = 0;
    private bool mFadeBegin = false;
    private bool mFadeEnd = false;
    private int mFadeBeginAt = 0;
    private float mFadeStrength = 1;


    public bool isBlending
    {
        get { return mBlendAnimationClip != null; }
    }
    public string playingAnimationClip
    {
        get
        {
            if (mCurrentAnimationClip != null)
            {
                return mCurrentAnimationClip.Name;
            }
            return null;
        }
    }
    private int mBlendFadeFrame = 0;
    private bool mBlendBegin = false;
    private bool mBlendEnd = false;
    private int mBlendBeginAt = 0;
    private float mBlendFadeStrength = 1;

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
        materialPropertyBlock = new MaterialPropertyBlock();
        mLayer = gameObject.layer;
    }

    // Update is called once per frame
    public void OnUpdate()
    {
        if (mCurrentAnimationClip != null)
        {
            if (mCurrentAnimationClip.wrapMode == GpuInstancedAnimationClip.WrapMode.Once)
            {
                if (mCurrentAnimationClip.CurrentFrame >= mCurrentAnimationClip.FrameCount)
                {
                    if (mBlendAnimationClip == null)
                    {
                        Play(defaultAnimationClip, 0, mFadeFrame);
                    }
                }
            }

            mCurrentAnimationClip.Update();
            int currentFrame = mCurrentAnimationClip.RealFrame;
            int previousFrame = 0;
            if (mPreviousAnimationClip != null)
            {
                mPreviousAnimationClip.Update();

                previousFrame = mPreviousAnimationClip.RealFrame;
            }


            mFadeStrength = 1;
            if (mCurrentAnimationClip != null && mFadeFrame > 0)
            {
                if (mCurrentAnimationClip.wrapMode == GpuInstancedAnimationClip.WrapMode.Once)
                {
                    if (mBlendAnimationClip != null && mFadeEnd == false && mCurrentAnimationClip.CurrentFrame + mFadeFrame >= mCurrentAnimationClip.FrameCount - 1)
                    {
                        mFadeEnd = true;
                        mFadeBeginAt = mCurrentAnimationClip.CurrentFrame;
                        mPreviousAnimationClip = mBlendAnimationClip;
                    }
                }

                int offsetFrame = mCurrentAnimationClip.CurrentFrame - mFadeBeginAt;
                if (mFadeFrame > 0 && (mFadeBegin || mFadeEnd) && offsetFrame <= mFadeFrame)
                {
                    mFadeStrength = offsetFrame * 1f / mFadeFrame;
                    if (mFadeBegin)
                    {
                        if (offsetFrame >= mFadeFrame)
                        {
                            mFadeBegin = false;
                        }
                    }
                    if (mFadeEnd)
                    {
                        mFadeStrength = 1 - mFadeStrength;
                    }
                }

                if (mCurrentAnimationClip.wrapMode == GpuInstancedAnimationClip.WrapMode.Once)
                {
                    if (mCurrentAnimationClip.CurrentFrame >= mCurrentAnimationClip.FrameCount - 1)
                    {
                        if (mBlendAnimationClip != null)
                        {
                            mCurrentAnimationClip = mBlendAnimationClip;
                            mBlendAnimationClip = null;
                        }

                        mPreviousAnimationClip = null;

                        mFadeEnd = false;
                    }
                }
            }
            else
            {
                mFadeEnd = false;
                mFadeBeginAt = 0;
            }

            int blendFrame = 0;

            mBlendFadeStrength = 1;
            if (mBlendAnimationClip != null)
            {
                if (mBlendAnimationClip != mCurrentAnimationClip
                    && mBlendAnimationClip != mPreviousAnimationClip)
                {
                    mBlendAnimationClip.Update();
                }
                blendFrame = mBlendAnimationClip.RealFrame;

                if (mBlendAnimationClip.wrapMode == GpuInstancedAnimationClip.WrapMode.Once)
                {
                    if (mBlendEnd == false && mBlendAnimationClip.CurrentFrame + mBlendFadeFrame >= mBlendAnimationClip.FrameCount - 1)
                    {
                        mBlendEnd = true;
                        mBlendBeginAt = mBlendAnimationClip.CurrentFrame;
                    }
                }

                int offsetFrame = mBlendAnimationClip.CurrentFrame - mBlendBeginAt;
                if (mBlendFadeFrame > 0 && (mBlendBegin || mBlendEnd) && offsetFrame <= mBlendFadeFrame)
                {
                    mBlendFadeStrength = offsetFrame * 1f / mBlendFadeFrame;
                    if (mBlendBegin)
                    {
                        if (offsetFrame >= mBlendFadeFrame)
                        {
                            mBlendBegin = false;
                        }
                    }
                    if (mBlendEnd)
                    {
                        mBlendFadeStrength = 1 - mBlendFadeStrength;
                    }
                }

                if (mBlendAnimationClip.wrapMode == GpuInstancedAnimationClip.WrapMode.Once)
                {
                    if (mBlendAnimationClip.CurrentFrame >= mBlendAnimationClip.FrameCount - 1)
                    {
                        mBlendAnimationClip = null;
                        mBlendEnd = false;
                    }
                }
            }
        
            materialPropertyBlock.SetInt("_CurrentFrame", currentFrame);
            materialPropertyBlock.SetInt("_PreviousFrame", previousFrame);
            materialPropertyBlock.SetFloat("_FadeStrength", mFadeStrength);
            materialPropertyBlock.SetInt("_BlendFrame", blendFrame);
            materialPropertyBlock.SetFloat("_BlendFadeStrength", mBlendFadeStrength);
            materialPropertyBlock.SetFloat("_BlendDirection", (int)blendDirection);
        }

        if (isCulling == false)
        {
            Graphics.DrawMesh(mesh, transform.localToWorldMatrix, material, mLayer, null, 0, materialPropertyBlock);
        }
    }

    public void Play(string clipName, int offsetFrame = 0, int fadeFrame = 0)
    {
        if (animationClips == null)
        {
            return;
        }
        if (mCurrentAnimationClip != null && mCurrentAnimationClip.Name == clipName)
        {
            if (mCurrentAnimationClip.CurrentFrame == 0 && mCurrentAnimationClip.wrapMode == GpuInstancedAnimationClip.WrapMode.Once)
            {
                mCurrentAnimationClip.Reset(this, offsetFrame);
                mFadeFrame = Mathf.Clamp(fadeFrame, 0, mCurrentAnimationClip.FrameCount);
                mFadeEnd = false;
                mFadeBeginAt = mCurrentAnimationClip.CurrentFrame;
            }
            return;
        }

        mPreviousAnimationClip = mCurrentAnimationClip;

        mCurrentAnimationClip = animationClips.Find((x) => x.Name == clipName);

        PlayBlend(null);

        if (mCurrentAnimationClip != null)
        {
            mCurrentAnimationClip.Reset(this, offsetFrame);

            mFadeFrame = Mathf.Clamp(fadeFrame, 0, mCurrentAnimationClip.FrameCount / 2);
            mFadeBeginAt = mCurrentAnimationClip.CurrentFrame;
            mFadeBegin = true;
            mFadeEnd = false;
        }
    }
    public void PlayBlend(string clipName, BlendDirection direction = BlendDirection.Down, int fadeFrame = 0)
    {
        if (animationClips == null)
        {
            return;
        }
        if (string.IsNullOrEmpty(clipName))
        {
            mBlendAnimationClip = null;

            return;
        }
        if(mBlendAnimationClip!= null && mBlendAnimationClip.Name == clipName)
        {
            return;
        }
        blendDirection = direction;

        mBlendAnimationClip = animationClips.Find(x => x.Name == clipName);
        if (mBlendAnimationClip != null)
        {
            mBlendAnimationClip.Reset(this, 0);
            mBlendBeginAt = mBlendAnimationClip.CurrentFrame;
            mBlendFadeFrame = Mathf.Clamp(fadeFrame, 0, mBlendAnimationClip.FrameCount / 2);
            mBlendEnd = false;
            mBlendBegin = true;
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
            if (bone.boneName == boneName)
            {
                return bone;
            }
        }

        return null;
    }
    public GpuInstancedAnimationBoneFrame GetBoneFrame(string boneName)
    {
        GpuInstancedAnimationBone bone = GetBone(boneName);
        if (bone != null)
        {
            int currentFrame = 0;
            if (mCurrentAnimationClip != null)
            {
                currentFrame = mCurrentAnimationClip.RealFrame;
            }
            GpuInstancedAnimationBoneFrame previousBoneFrame = new GpuInstancedAnimationBoneFrame();
            GpuInstancedAnimationBoneFrame boneFrame = GetBoneFrame(currentFrame,bone.index);
            if ( (mFadeBegin || mFadeEnd) && mPreviousAnimationClip != null)
            {            
                previousBoneFrame = GetBoneFrame(mPreviousAnimationClip.RealFrame, bone.index);

                boneFrame = GpuInstancedAnimationBoneFrame.Lerp(previousBoneFrame, boneFrame, mFadeStrength);
            }
           
            if (mBlendAnimationClip != null)
            {               
                GpuInstancedAnimationBoneFrame blendBoneFrame = GetBoneFrame(mBlendAnimationClip.RealFrame,bone.index);

                if ((mFadeBegin || mFadeEnd) && mPreviousAnimationClip != null)
                {
                    blendBoneFrame = GpuInstancedAnimationBoneFrame.Lerp(previousBoneFrame, blendBoneFrame, mFadeStrength);
                }

                if ( mBlendBegin|| mBlendEnd)
                {
                    blendBoneFrame = GpuInstancedAnimationBoneFrame.Lerp(boneFrame, blendBoneFrame, mBlendFadeStrength);
                }

                float blendWeight = bone.blendWeight;
                float factor = Mathf.Abs(1 - blendWeight - (int)blendDirection);

                boneFrame = GpuInstancedAnimationBoneFrame.Lerp(boneFrame, blendBoneFrame, factor);
            }

            return boneFrame;
        }
        return new GpuInstancedAnimationBoneFrame() ;
    }

    private GpuInstancedAnimationBoneFrame GetBoneFrame(int frame, int boneIndex)
    {
        int frameIndex = frame * bones.Count + boneIndex;
        if (frameIndex >= 0 && frameIndex < boneFrames.Count)
        {
            GpuInstancedAnimationBoneFrame blendBoneFrame = boneFrames[frameIndex];

            return blendBoneFrame;
        }
        return new GpuInstancedAnimationBoneFrame();
    }


}

