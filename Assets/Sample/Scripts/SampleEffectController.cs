using System;
using UnityEngine;
public class SampleEffectController : MonoBehaviour
{
    public GameObject Prefab;

    public string boneName;

    private GpuInstancedAnimation instancedAnimation;

    private GameObject mEffect;
    private void Awake()
    {
        instancedAnimation = GetComponent<GpuInstancedAnimation>();
        mEffect = Instantiate(Prefab) as GameObject;
        mEffect.SetActive(true);
    }
    private void Update()
    {
       if(instancedAnimation!= null)
        {
            var frame = instancedAnimation.GetBoneFrame(boneName);
            if(frame!= null)
            {
                mEffect.transform.position = transform.TransformPoint(frame.localPosition);
                mEffect.transform.rotation = frame.rotation;
            }
        }
    }
}

