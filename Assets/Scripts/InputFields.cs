using TMPro;
using UnityEngine;

public class InputFields : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;
    [SerializeField] GameObject textPromptIfEmpty;

    void Start()
    {
        if(inputField == null) inputField = GetComponent<TMP_InputField>();

        if(inputField.text == "" && textPromptIfEmpty != null) textPromptIfEmpty.SetActive(true);
    }

    public void OnValueChanged()
    {
        if(textPromptIfEmpty != null) 
        {
            if(inputField.text == "") textPromptIfEmpty.SetActive(true);
            else textPromptIfEmpty.SetActive(false);
        }
    }
}
