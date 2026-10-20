using UnityEngine;

public class MouseCursor : MonoBehaviour
{

    [SerializeField] private Texture2D _cursorIcon;


    void Awake()
    {
        UIEvents.CursorShowedEvent += HandleCursorShowed;
        UIEvents.CursorHideEvent += HandleCursorHid;
    }
    void OnDestroy()
    {
        UIEvents.CursorShowedEvent -= HandleCursorShowed;
        UIEvents.CursorHideEvent -= HandleCursorHid;
    }
    private void Start()
    {
        Cursor.SetCursor(_cursorIcon, Vector2.zero, CursorMode.Auto);
    }

    void HandleCursorShowed()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    void HandleCursorHid()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
