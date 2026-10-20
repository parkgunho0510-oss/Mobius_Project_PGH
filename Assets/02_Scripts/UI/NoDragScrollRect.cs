using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NoDragScrollRect : ScrollRect
{
    public override void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그 스크롤 막기
    }

    public override void OnDrag(PointerEventData eventData)
    {
        // 드래그 스크롤 막기
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        // 드래그 스크롤 막기
    }
}
