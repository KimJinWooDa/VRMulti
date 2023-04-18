using UnityEngine;
using UnityEngine.UI;

public class ShowKeyboard : MonoBehaviour
{
    [SerializeField] private InputField _inputField;
    [SerializeField] private GameObject keyBoard;

    private bool isOn;
    private void Awake()
    {
        _inputField = GetComponent<InputField>();
    }

    void Update()
    {
        if (_inputField.isFocused && !isOn)
        {
            keyBoard.SetActive(true);
            isOn = true;
        }
    }

    public void Submit()
    {
        keyBoard.SetActive(false);
        isOn = false;
    }
}
