using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GpuInstancedAnimation : MonoBehaviour
{
    public Mesh mesh;
    public Material material;
    public const int TargetFrameRate = 30; //帧率

    public List<GpuInstancedAnimationClip> animationClips;

    private MaterialPropertyBlock materialPropertyBlock;

    private GpuInstancedAnimationClip currentAnimationClip;

    private int currentOffsetFrame = 0;
    private float currentTime = 0;
    public int currentFrame = 0;

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;

        if (currentAnimationClip != null)
        {
            if (currentAnimationClip.wrapMode == GpuInstancedAnimationClip.WrapMode.Once)
            {
                currentFrame = ((int)(currentTime * TargetFrameRate) + currentOffsetFrame);

                if (currentFrame >= currentAnimationClip.FrameCount - 1)
                {
                    currentFrame = currentAnimationClip.FrameCount - 1;
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


}
