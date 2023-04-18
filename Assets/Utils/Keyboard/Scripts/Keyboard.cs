using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Keyboard : MonoBehaviour
{
    [FormerlySerializedAs("_inputField")] [SerializeField]
    private InputField join_inputField;
    

    public void OnInputFieldSelected(InputField inputField)
    {
        join_inputField = inputField;
        join_inputField.Select();
    }

    public void OnKeyPressed(KeyboardButton keyboardButton)
    {
        if (join_inputField == null || !join_inputField.interactable) return;

        if (!SafeToType()) return;

        switch (keyboardButton.Function)
        {
            case KeyboardButton.KeyFunction.Key:
                join_inputField.text += keyboardButton.Key; //.ToUpper()
                break;
            case KeyboardButton.KeyFunction.Backspace:
                if (join_inputField.text.Length > 0)
                {
                    join_inputField.text = join_inputField.text[..^1];
                }
                break;
        }
    }

    private bool SafeToType()
    {
        return true;
    }
}
