using UnityEngine;
using UnityEngine.UI;

public class HandleScreenTouchButton : MonoBehaviour
{
    [SerializeField] private Button _screenTouchButton;
    
    void Start()
    {
        _screenTouchButton.onClick.RemoveAllListeners();
        _screenTouchButton.onClick.AddListener(GameObject.FindFirstObjectByType<persistentData>().OnTouchInput);
    }
}
