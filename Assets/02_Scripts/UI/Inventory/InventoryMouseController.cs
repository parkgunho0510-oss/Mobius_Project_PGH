using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryMouseController : MonoBehaviour
{
    private GraphicRaycaster _gr;
    private PointerEventData _ped;
    private List<RaycastResult> _rrList = new();

    public void Init(GraphicRaycaster gr)
    {
        _gr = gr;
        _ped = new PointerEventData(EventSystem.current);
    }

    public void UpdateMousePosition(Vector2 mousePosition)
    {
        if (_ped == null) return;

        _ped.position = mousePosition;
    }


    /// <summary>
    /// 마우스가 UI오브젝트 위에 있는지 확인
    /// </summary>
    /// <returns></returns>
    public bool IsOverUI()
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject();
    }


    /// <summary> 레이캐스트하여 얻은 첫 번째 UI에서 컴포넌트 찾아 리턴 </summary>
    public T RaycastAndGetFirstComponent<T>() where T : Component
    {
        if (_gr == null || _ped == null)
            return null;

        _rrList.Clear();

        _gr.Raycast(_ped, _rrList);

        if (_rrList.Count == 0)
            return null;

        return _rrList[0].gameObject.GetComponent<T>();
    }
}