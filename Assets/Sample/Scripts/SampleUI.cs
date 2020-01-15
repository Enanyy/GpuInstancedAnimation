using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SampleUI : MonoBehaviour
{
    [SerializeField]
    InputField ObjectCountinputField;
    [SerializeField]
    Button InstantiateButton;
    [SerializeField]
    Toggle ShowEffectToggle;
    [SerializeField]
    Toggle ControllToggle;

   
    public event UnityAction<string> ObjectCountInputFieldValueChanged = delegate { };
    public event UnityAction InstantiateButtonClicked = delegate { };
    public event UnityAction<bool> ShowEffectToggleValueChanged = delegate { };
    public event UnityAction<bool> ControllToggleValueChanged = delegate { };


    private void Awake()
    {
      
        ObjectCountinputField
            .onValueChanged
            .AddListener((v) => ObjectCountInputFieldValueChanged(v));

        InstantiateButton
            .onClick
            .AddListener(() => InstantiateButtonClicked());

        ShowEffectToggle.onValueChanged.AddListener((v) => ShowEffectToggleValueChanged(v));
        ControllToggle.onValueChanged.AddListener((v) => ControllToggleValueChanged(v));
    }
}
