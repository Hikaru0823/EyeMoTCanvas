using KanKikuchi.AudioManager;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSound : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    [SerializeField] private bool playClickSound = true;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (playClickSound)
        {
            SEManager.Instance.Play(SEPath.CLICK);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SEManager.Instance.Play(SEPath.HOVER);
    }
}
