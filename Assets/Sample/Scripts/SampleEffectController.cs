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
        if(instancedAnimation)
        {
            Vector3 position = instancedAnimation.GetBonePosition(boneName);

            mEffect.transform.position = position;
        }
    }
}

