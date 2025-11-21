using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DwellPaintButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image dwellCircle;
        private bool isOnMouse = false;
        private static float mouseOverTime = 1.2f;
        public const float Max_MouseOverTime = 3.0f;
        public const float Min_MouseOverTime = 0.2f;
        private float delta = 0.0f;

    void Start()
    {
        if(dwellCircle == null)
        {
            dwellCircle = transform.Find("DwellImage").GetComponent<Image>();
        }
        dwellCircle.fillAmount = 0;
    }

    // Update is called once per frame
    void Update()
        {
            if(dwellCircle == null)
            {
                return;
            }   
            if (!isOnMouse)
            {
                delta = 0;
                dwellCircle.fillAmount = delta / mouseOverTime;
                return;
            }

            delta += Time.deltaTime;
            dwellCircle.fillAmount = delta / mouseOverTime;
            if (delta < mouseOverTime) return;

            GetComponent<Button>().onClick.Invoke();
            delta = 0;
            dwellCircle.fillAmount = delta / mouseOverTime;
            isOnMouse = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isOnMouse = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isOnMouse = false;
        }


        public static void Set_MouseOverTime(float overtime) { mouseOverTime = overtime; }

    public void OnPointerClick(PointerEventData eventData)
    {
        isOnMouse = false;
    }
}