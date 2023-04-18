using UnityEngine;
using TMPro;

public class KeyboardButton : MonoBehaviour
{
    public enum KeyFunction
    {
        Key,
        Backspace
    }

    [SerializeField]
    private string _key;
    [SerializeField]
    private KeyFunction _keyFunction = KeyFunction.Key;
    [SerializeField]
    private TMP_Text _keyText;
    
    private Keyboard _keyboard;
    public string Key => _key;

    public KeyFunction Function => _keyFunction;

    void Start()
    {
        //transform.parent.TryGetComponent<Keyboard>(out var _keyboard); // null -> var
        _keyboard = transform.root.GetComponent<Keyboard>();
    }

    public void PressedKey()
    {
        _keyboard.OnKeyPressed(this);
    }

    private void OnValidate()
    {
        _keyText.text = _key;
    }
}
