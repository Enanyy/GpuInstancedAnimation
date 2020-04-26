using System;
using UnityEngine;
public class SamplePlayEffect : MonoBehaviour
{
    public GameObject Prefab;

    public string boneName;

    private GpuInstancedAnimation instancedAnimation;

    private GameObject mEffect;
   
    private void OnEnable()
    {
        if(instancedAnimation == null)
        {
            instancedAnimation = GetComponent<GpuInstancedAnimation>();
        }
        if(instancedAnimation == null)
        {
            return;
        }
        instancedAnimation.onAnimationClipBegin += OnAnimationClipBegin;
        instancedAnimation.onAnimationClipEnd += OnAnimationClipEnd;
    }
    private void OnDisable()
    {
        if (instancedAnimation == null)
        {
            return;
        }
        instancedAnimation.onAnimationClipBegin -= OnAnimationClipBegin;
        instancedAnimation.onAnimationClipEnd -= OnAnimationClipEnd;
    }
    void OnAnimationClipEnd(GpuInstancedAnimationClip clip)
    {
        if(clip!= null && clip.Name == "attack01")
        {
            if(mEffect== null)
            {
                mEffect = Instantiate(Prefab) as GameObject;
                mEffect.SetActive(true);
            }
            if (mEffect.activeSelf)
            {
                mEffect.SetActive(false);
            }
        }
    }
    void OnAnimationClipBegin(GpuInstancedAnimationClip clip)
    {
        if (clip != null && clip.Name == "attack01")
        {
            if (mEffect == null)
            {
                mEffect = Instantiate(Prefab) as GameObject;
                mEffect.SetActive(true);
            }
            if (mEffect.activeSelf == false)
            {
                mEffect.SetActive(true);
            }
        }
    }
    private void Update()
    {
        if (instancedAnimation != null && mEffect)
        {
            var frame = instancedAnimation.GetBoneFrame(boneName);

            frame.Transform(transform.localToWorldMatrix, out Vector3 worldPosition, out Vector3 worldForward);
            mEffect.transform.position = worldPosition;
            mEffect.transform.forward = worldForward;
        }
    }
}

