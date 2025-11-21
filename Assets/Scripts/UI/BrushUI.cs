using UnityEngine;
using UnityEngine.EventSystems;

public class BrushUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Resources")]
    public Animator ButtonAnimator;

    void OnEnable()
    {
        if (ButtonAnimator == null)
                ButtonAnimator = gameObject.GetComponent<Animator>();
    }


    public void OnPointerEnter(PointerEventData eventData)
        {
#if !UNITY_ANDROID && !UNITY_IOS
            if (!ButtonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Selected"))
                ButtonAnimator.Play("Highlighted");
#endif
        }

        public void OnPointerExit(PointerEventData eventData)
        {
#if !UNITY_ANDROID && !UNITY_IOS
            if (!ButtonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Selected"))
                ButtonAnimator.Play("Normal");
#endif
        }
}
