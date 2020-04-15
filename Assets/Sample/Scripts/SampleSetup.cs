using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SampleSetup : MonoBehaviour
{
    [SerializeField]
    InputField ObjectCountinputField;
    [SerializeField]
    Button InstantiateButton;
    [SerializeField]
    Toggle ShowEffectToggle;
    [SerializeField]
    Toggle ControllToggle;
    [SerializeField]
    Button IdleButton;
    [SerializeField]
    Button RunButton;
    [SerializeField]
    Button AttackButton;
    [SerializeField]
    Button DieButton;
    [SerializeField]
    Button BlendButton;

    public List<GameObject> Prefabs = new List<GameObject>();

    private int Count = 0;

    public float radius = 10;

    private bool Controll = false;
    private bool ShowEffect = false;

    private List<IUpdate> mUpdateList = new List<IUpdate>();

    private SamplePlay samplePlay;
    // Start is called before the first frame update

    private GpuInstancedAnimation controllAnimation;
    void Start()
    {
        SetButtonActive(false);

        if (ObjectCountinputField)
        {
            ObjectCountinputField.onValueChanged.AddListener(s =>
            {
                int.TryParse(s, out Count);

            });
        }

        if (InstantiateButton)
        {
            InstantiateButton.onClick.AddListener(InstantiateObject);
        }

        if (ControllToggle)
        {
            ControllToggle.onValueChanged.AddListener(v => { 
                Controll = v;
                SetButtonActive(v);

                if(controllAnimation)
                {
                    if (Controll)
                    {
                        if (samplePlay == null)
                        {
                            samplePlay = controllAnimation.gameObject.AddComponent<SamplePlay>();
                        }
                    }
                    else
                    {
                        if (samplePlay)
                        {
                            Destroy(samplePlay);
                            samplePlay = null;
                        }
                    }
                }
                
            });
        }
        if (ShowEffectToggle)
        {
            ShowEffectToggle.onValueChanged.AddListener(v => { 
                ShowEffect = v;
                if(controllAnimation)
                {
                    SamplePlayEffect samplePlayEffect = controllAnimation.GetComponent<SamplePlayEffect>();
                    if (samplePlayEffect != null)
                    {
                        samplePlayEffect.enabled = ShowEffect;
                    }
                }
            });
        }
        if(IdleButton)
        {
            IdleButton.onClick.AddListener(() => {

                if (samplePlay) samplePlay.PlayIdle();
            });
        }
        if (RunButton)
        {
            RunButton.onClick.AddListener(() =>
            {

                if (samplePlay) samplePlay.PlayRun();
            });
        }
        if (AttackButton)
        {
            AttackButton.onClick.AddListener(() =>
            {

                if (samplePlay) samplePlay.PlayAttack();
            });
        }
        if (DieButton)
        {
            DieButton.onClick.AddListener(() =>
            {

                if (samplePlay) samplePlay.PlayDie();
            });
        }
        if (BlendButton)
        {
            BlendButton.onClick.AddListener(() =>
            {

                if (samplePlay) samplePlay.PlayBlend();
            });
        }
    }
    private void SetButtonActive(bool active)
    {
        if (IdleButton) IdleButton.gameObject.SetActive(active);
        if (RunButton) RunButton.gameObject.SetActive(active);
        if (AttackButton) AttackButton.gameObject.SetActive(active);
        if (DieButton) DieButton.gameObject.SetActive(active);
        if (BlendButton) BlendButton.gameObject.SetActive(active);
    }
    void InstantiateObject()
    {
        for(int i = 0; i < Count; ++i)
        {
            Vector2 v = Random.insideUnitCircle * radius;

            GameObject prefab = Prefabs[Count == 1 ? 0 : Random.Range(0, Prefabs.Count)];

            GameObject go = Instantiate(prefab) as GameObject;
            go.transform.position = Count == 1?new Vector3(0,5,-10): new Vector3(v.x, 0, v.y);
            go.SetActive(true);

            if(Controll && Count == 1)
            {
                samplePlay = go.AddComponent<SamplePlay>();
            }

            SamplePlayEffect samplePlayEffect = go.GetComponent<SamplePlayEffect>();
            if (samplePlayEffect != null)
            {
                samplePlayEffect.enabled = ShowEffect && Count == 1; 
            }

            GpuInstancedAnimation animation = go.GetComponent<GpuInstancedAnimation>();

            int index = Random.Range(0, animation.animationClips.Count);
            var animationFrame = animation.animationClips[index];
            animation.speed = Count == 1? 1: Random.Range(0.5f, 3);
            animation.Play(animationFrame.Name);

            mUpdateList.Add(animation);

            if(Count == 1)
            {
                controllAnimation = animation;
            }
            if(Count > 1)
            {
                foreach(var clip in animation.animationClips)
                {
                    clip.wrapMode = GpuInstancedAnimationClip.WrapMode.Loop;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0, Count = mUpdateList.Count; i < Count; ++i)
        {
            mUpdateList[i].OnUpdate();
        }
    }
}
