using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SampleUIController : MonoBehaviour
{
    [SerializeField]
    InputField ObjectCountinputField;
    [SerializeField]
    Button InstantiateButton;

   
    public event UnityAction<string> ObjectCountInputFieldValueChanged = delegate { };
    public event UnityAction InstantiateButtonClicked = delegate { };

    private void Awake()
    {
      
        ObjectCountinputField
            .onValueChanged
            .AddListener((v) => ObjectCountInputFieldValueChanged(v));

        InstantiateButton
            .onClick
            .AddListener(() => InstantiateButtonClicked());
    }
}
