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

    private Texture2D mAnimationTexture;
    public Texture2D animationTexture
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
    public int pixelCountPerFrame
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
        if(string.IsNullOrEmpty(boneName)==false && bones!= null)
        {
            for(int i = 0; i < bones.Count; i++)
            {
                if(bones[i].boneName == boneName)
                {
                    return bones[i];
                }
            }
        }
        return null;
    }

    public Vector3 GetBonePosition(string boneName)
    {
        GpuInstancedAnimationBone bone = GetBone(boneName); 
        if(bone!= null)
        {
            Matrix4x4 matrix4X4 = GetBoneMatrix4x4(bone);
            if(matrix4X4!= Matrix4x4.zero)
            {
                Vector4 v4 = matrix4X4 * bone.localPosition;

                return transform.TransformPoint(new Vector3(v4.x, v4.y, v4.z));
            }
        }
        return Vector3.zero;
    }
    public Matrix4x4 GetBoneMatrix4x4(GpuInstancedAnimationBone bone)
    {
        if(bone!= null)
        {
            int clampedIndex = currentFrame * pixelCountPerFrame;

            int matrixIndex = clampedIndex + bone.boneIndex * BoneMatrixRowCount;

            Color row0 = Tex2Dlod(matrixIndex);
            Color row1 = Tex2Dlod(matrixIndex + 1);
            Color row2 = Tex2Dlod(matrixIndex + 2);

            return new Matrix4x4(new Vector4(row0.r, row0.g, row0.b, row0.a),
                new Vector4(row1.r, row1.g, row1.b, row1.a),
                new Vector4(row2.r, row2.g, row2.b, row2.a),
                new Vector4(0, 0, 0, 0));

        }
        return Matrix4x4.zero;
    }

    private Color Tex2Dlod(int index)
    {
        if(animationTexture)
        {
            int x = index / animationTexture.width;
            int y = index % animationTexture.width;

            return animationTexture.GetPixel(x, y);
        }
        return Color.clear;
    }
}
