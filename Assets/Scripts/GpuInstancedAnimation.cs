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

    private GpuInstancedAnimationFrame mCurrentAnimationFrame;
    private GpuInstancedAnimationFrame mPreviousAnimationFrame;

    private int mFadeFrame = 0;

    public event System.Action<GpuInstancedAnimationClip> onAnimationClipBegin;
    public event System.Action<GpuInstancedAnimationClip,int> onAnimationClipUpdate;
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

    public void OnAnimationClipBegin(GpuInstancedAnimationFrame frame)
    {
        onAnimationClipBegin?.Invoke(frame.CurrentAnimationClip);
    }
    public void OnAnimationClipUpdate(GpuInstancedAnimationFrame frame)
    {
        onAnimationClipUpdate?.Invoke(frame.CurrentAnimationClip, frame.CurrentFrame);
    }
    public void OnAnimationClipEnd(GpuInstancedAnimationFrame frame)
    {
        onAnimationClipEnd?.Invoke(frame.CurrentAnimationClip);
    }
    // Update is called once per frame
    void Update()
    {
        if (materialPropertyBlock == null)
        {
            materialPropertyBlock = new MaterialPropertyBlock();
        }

        if(mPreviousAnimationFrame!= null)
        {
            mPreviousAnimationFrame.Update();
        }

        if (mCurrentAnimationFrame != null)
        {
            mCurrentAnimationFrame.Update();
            materialPropertyBlock.SetInt("_CurrentFrame", mCurrentAnimationFrame.GetCurrentFrame());

            float fadeStrength = 1;
            if(mPreviousAnimationFrame!= null)
            {

            }
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

        GpuInstancedAnimationFrame temp = mPreviousAnimationFrame;
        mPreviousAnimationFrame = mCurrentAnimationFrame;

        var clip = animationClips.Find((x) => x.Name == clipName);

        if (clip != null)
        {
            if(temp == null)
            {
                temp = new GpuInstancedAnimationFrame(this);
            }
            temp.Reset(clip, offsetFrame);

            mCurrentAnimationFrame = temp;

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
            if (mCurrentAnimationFrame!=null )
            {
                frameIndex = mCurrentAnimationFrame.GetCurrentFrame();
            }
            int index = frameIndex * bones.Count + bone.index;

            return boneFrames[index];
        }
        return null;
    }
   
}
