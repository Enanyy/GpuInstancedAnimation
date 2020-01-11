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

    private GpuInstancedAnimationClip currentAnimationClip;

    private int currentOffsetFrame = 0;
    private float currentTime = 0;
    public int currentFrame = 0;

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
        currentTime += Time.deltaTime;

        if (currentAnimationClip != null)
        {
            if (currentAnimationClip.wrapMode == GpuInstancedAnimationClip.WrapMode.Once)
            {
                currentFrame = ((int)(currentTime * TargetFrameRate) + currentOffsetFrame);

                if (currentFrame >= currentAnimationClip.FrameCount)
                {
                    currentFrame = 0;//重置到第0帧
                }
            }
            else if(currentAnimationClip.wrapMode  == GpuInstancedAnimationClip.WrapMode.ClampForever)
            {
                currentFrame = ((int)(currentTime * TargetFrameRate) + currentOffsetFrame);

                if (currentFrame >= currentAnimationClip.FrameCount - 1)
                {
                    currentFrame = currentAnimationClip.FrameCount - 1;//固定在最后一帧
                }
            }
            else
            {
                currentFrame = ((int)(currentTime * TargetFrameRate) + currentOffsetFrame) % currentAnimationClip.FrameCount;

            }

            if (materialPropertyBlock == null)
            {
                materialPropertyBlock = new MaterialPropertyBlock();
            }
           
            materialPropertyBlock.SetInt("_CurrentFrame", currentAnimationClip.StartFrame + currentFrame);
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

        currentAnimationClip = animationClips.Find((x) => x.Name == clip);

        if (currentAnimationClip != null)
        {
            currentOffsetFrame = offsetFrame;
            currentTime = 0;
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
            if (currentAnimationClip != null)
            {
                frameIndex = currentAnimationClip.StartFrame + currentFrame;
            }
            int index = frameIndex * bones.Count + bone.index;

            return boneFrames[index];
        }
        return null;
    }
   
}
