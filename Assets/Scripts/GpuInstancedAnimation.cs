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
            if(CurrentAnimationClip!= null)
            {
                if(mCurrentFrame == 0)
                {
                    onAnimationClipBegin?.Invoke(CurrentAnimationClip);
                }

                if(mCurrentFrame >= CurrentAnimationClip.FrameCount -1)
                {
                    onAnimationClipEnd?.Invoke(CurrentAnimationClip);
                }
            }
        }
    }
    public event System.Action<GpuInstancedAnimationClip> onAnimationClipBegin;
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
    // Update is called once per frame
    void Update()
    {
        mCurrentTime += Time.deltaTime;

        if (CurrentAnimationClip != null)
        {
            if (CurrentAnimationClip.wrapMode == GpuInstancedAnimationClip.WrapMode.Once)
            {
                CurrentFrame = ((int)(mCurrentTime * TargetFrameRate) + mCurrentOffsetFrame);

                if (CurrentFrame >= CurrentAnimationClip.FrameCount)
                {
                    CurrentFrame = 0;//重置到第0帧
                }
            }
            else if(CurrentAnimationClip.wrapMode  == GpuInstancedAnimationClip.WrapMode.ClampForever)
            {
                CurrentFrame = ((int)(mCurrentTime * TargetFrameRate) + mCurrentOffsetFrame);

                if (CurrentFrame >= CurrentAnimationClip.FrameCount - 1)
                {
                    CurrentFrame = CurrentAnimationClip.FrameCount - 1;//固定在最后一帧
                }
            }
            else
            {
                CurrentFrame = ((int)(mCurrentTime * TargetFrameRate) + mCurrentOffsetFrame) % CurrentAnimationClip.FrameCount;

            }

            if (materialPropertyBlock == null)
            {
                materialPropertyBlock = new MaterialPropertyBlock();
            }
           
            materialPropertyBlock.SetInt("_CurrentFrame", CurrentAnimationClip.StartFrame + CurrentFrame);
        }
        
        Matrix4x4 matrix = transform.localToWorldMatrix;

        Graphics.DrawMesh(mesh, matrix, material, gameObject.layer, null, 0, materialPropertyBlock);
    }

    public void Play(string clip, int offsetFrame = 0)
    {
        if (animationClips == null)
        {
            return;
        }

        CurrentAnimationClip = animationClips.Find((x) => x.Name == clip);

        if (CurrentAnimationClip != null)
        {
            mCurrentOffsetFrame = offsetFrame;
            mCurrentTime = 0;
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
            if (CurrentAnimationClip != null)
            {
                frameIndex = CurrentAnimationClip.StartFrame + CurrentFrame;
            }
            int index = frameIndex * bones.Count + bone.index;

            return boneFrames[index];
        }
        return null;
    }
   
}
