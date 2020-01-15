using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleSetup : MonoBehaviour
{
    public SampleUI UIController;
    public List<GameObject> Prefabs = new List<GameObject>();

    private int Count = 0;

    public float radius = 10;

    private bool Controll = false;
    private bool ShowEffect = false;
    // Start is called before the first frame update
    void Start()
    {
        if(UIController)
        {
            UIController.ObjectCountInputFieldValueChanged += (string v) =>
            {

                int.TryParse(v, out Count);
            };

            UIController.InstantiateButtonClicked += InstantiateObject;
            UIController.ControllToggleValueChanged += (v) => {
                Controll = v;
            };

            UIController.ShowEffectToggleValueChanged += (v) => { ShowEffect = v; };
        }


    }

    void InstantiateObject()
    {
        for(int i = 0; i < Count; ++i)
        {
            Vector2 v = Random.insideUnitCircle * radius;

            GameObject prefab = Prefabs[Random.Range(0, Prefabs.Count)];

            GameObject go = Instantiate(prefab) as GameObject;
            go.transform.position = new Vector3(v.x, 0, v.y);
            go.SetActive(true);

            if(Controll)
            {
                go.AddComponent<SamplePlay>();
            }

            SamplePlayEffect samplePlayEffect = go.GetComponent<SamplePlayEffect>();
            if(samplePlayEffect!= null)
            {
                samplePlayEffect.enabled = ShowEffect;
            }

            GpuInstancedAnimation animation = go.GetComponent<GpuInstancedAnimation>();

            int index = Random.Range(0, animation.animationClips.Count);
            var animationFrame = animation.animationClips[index];
            animation.speed = Random.Range(0.1f, 3);
            animation.Play(animationFrame.Name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
