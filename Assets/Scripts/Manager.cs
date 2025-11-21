using UnityEngine;

public class Manager : Singleton<Manager>
{
    [SerializeField] private Animator optionMenuAnimator;
    [SerializeField] private Animator rightOptionTabAnimator;
    [SerializeField] private Animator leftOptionTabAnimator;
    bool isOptionMenuOpen = false;
    string panelFadeIn = "Panel In";
    string panelFadeOut = "Panel Out";

    void Start()
    {
        //optionMenuAnimator.Play(ResourcesManager.Panel_Out);
        ColorParetUI.Instance.Animator.Play(ResourcesManager.Panel_In);
        BrushSelecterUI.Instance.Animator.Play(ResourcesManager.Panel_In);
        BGSelecterUI.Instance.Animator.Play(ResourcesManager.Panel_In);
        rightOptionTabAnimator.Play(ResourcesManager.Panel_In);
        leftOptionTabAnimator.Play(ResourcesManager.Panel_In);
    }   

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitApplication();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleOptionMenu();
        }
    }

    public void ToggleOptionMenu()
    {
        isOptionMenuOpen = !isOptionMenuOpen;
        optionMenuAnimator.Play(isOptionMenuOpen ? panelFadeIn : panelFadeOut);
    }
    public void QuitApplication()
    {
        #if !UNITY_WEBGL
        Application.Quit();
        #endif
    }
}
