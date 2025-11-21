using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionTab : MonoBehaviour
{
    [SerializeField] OptionTabItem[] tabItems;
    OptionTabItem currentSelectedItem;
    void Start()
    {
        foreach (var item in tabItems)
        {
            item.Button.onClick.AddListener(() =>
            {
                OnTabItemSelected(item);
            });
        }
    }

    void OnTabItemSelected(OptionTabItem selectedItem)
    {
        if(selectedItem == currentSelectedItem)
        {
            selectedItem.ButtonAnimator.Play("Disabled");
            selectedItem.PanelAnimator.Play(ResourcesManager.Panel_Out);
        }
        foreach (var item in tabItems)
        {
            if (item == selectedItem)
            {
                if (!item.ButtonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Selected"))
                    item.ButtonAnimator.Play("Selected");
                if (!item.PanelAnimator.GetCurrentAnimatorStateInfo(0).IsName(ResourcesManager.Panel_In))
                    item.PanelAnimator.Play(ResourcesManager.Panel_In);
                continue;
            }
            if (item.ButtonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Selected"))
                item.ButtonAnimator.Play("Disabled");
            if (item.PanelAnimator.GetCurrentAnimatorStateInfo(0).IsName(ResourcesManager.Panel_In))
                item.PanelAnimator.Play(ResourcesManager.Panel_Out);
        }
        currentSelectedItem = selectedItem;
    }

    public void CloseAllTabs()
    {
        foreach (var item in tabItems)
        {
            if (item.ButtonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Selected"))
                item.ButtonAnimator.Play("Disabled");
            if (item.PanelAnimator.GetCurrentAnimatorStateInfo(0).IsName(ResourcesManager.Panel_In))
                item.PanelAnimator.Play(ResourcesManager.Panel_Out);
        }
        currentSelectedItem = null;
    }
}

[System.Serializable]
public class OptionTabItem
{
    public Button Button;
    public Animator ButtonAnimator;
    public Animator PanelAnimator;
}
