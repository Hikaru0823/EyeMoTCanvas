using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class TabManager : MonoBehaviour
{
    [Header("Panel List")]
    public List<PanelItem> panels = new List<PanelItem>();

    [Header("Settings")]
    public int currentPanelIndex = 0;
    private int currentButtonIndex = 0;
    private int newPanelIndex;

    private GameObject currentPanel;
    private GameObject nextPanel;
    private GameObject currentButton;
    private GameObject nextButton;

    private Animator currentPanelAnimator;
    private Animator nextPanelAnimator;
    private Animator currentButtonAnimator;
    private Animator nextButtonAnimator;

    string panelFadeIn = "Panel In";
    string panelFadeOut = "Panel Out";
    string buttonFadeIn = "Normal to Pressed";
    string buttonFadeOut = "Pressed to Dissolve";

    [System.Serializable]
    public class PanelItem
    {
        public string panelName;
        public GameObject panelObject;
        public GameObject buttonObject;
    }

    void OnEnable()
    {
        if(panels[currentButtonIndex].buttonObject != null && panels[newPanelIndex].buttonObject != null)
        {   
            currentButton = panels[currentPanelIndex].buttonObject;
            currentButton.GetComponent<GazeController>().SetState(true);
            currentButtonAnimator = currentButton.GetComponent<Animator>();
            currentButtonAnimator.Play(buttonFadeIn);         
        }   
        if(panels[currentPanelIndex].panelObject != null && panels[newPanelIndex].panelObject != null)
        {   
            currentPanel = panels[currentPanelIndex].panelObject;
            currentPanelAnimator = currentPanel.GetComponent<Animator>();
            currentPanelAnimator.Play(panelFadeIn);
        }
        StartCoroutine("DisablePreviousPanel");
    }

    public void OpenPanel(string newPanel)
    {
        for (int i = 0; i < panels.Count; i++)
        {
            if (panels[i].panelName == newPanel)
            {
                newPanelIndex = i;
                break;
            }
        }

        if (newPanelIndex != currentPanelIndex)
        {
            StopCoroutine("DisablePreviousPanel");

            //パネルの管理
            if(panels[currentPanelIndex].panelObject != null && panels[newPanelIndex].panelObject != null)
            {
                //移動前パネルと移動後パネルを取得
                currentPanel = panels[currentPanelIndex].panelObject;
                currentPanelIndex = newPanelIndex;
                nextPanel = panels[currentPanelIndex].panelObject;
                nextPanel.SetActive(true);

                //パネルのアニメーション管理
                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                nextPanelAnimator = nextPanel.GetComponent<Animator>();
                currentPanelAnimator.Play(panelFadeOut);
                nextPanelAnimator.Play(panelFadeIn);
            }

            StartCoroutine("DisablePreviousPanel");

            //ボタンの管理
            if(panels[currentButtonIndex].buttonObject != null && panels[newPanelIndex].buttonObject != null)
            {
                //移動前ボタンと移動後ボタンを取得
                currentButton = panels[currentButtonIndex].buttonObject;
                currentButtonIndex = newPanelIndex;
                nextButton = panels[currentButtonIndex].buttonObject;

                //ボタンのアニメーション管理
                currentButtonAnimator = currentButton.GetComponent<Animator>();
                nextButtonAnimator = nextButton.GetComponent<Animator>();    
                currentButton.GetComponent<GazeController>().SetState(false);
                currentButtonAnimator.Play(buttonFadeOut);
                nextButton.GetComponent<GazeController>().SetState(true);
                nextButtonAnimator.Play(buttonFadeIn);
            }
        }
    }

    IEnumerator DisablePreviousPanel()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        for (int i = 0; i < panels.Count; i++)
        {
            if (i == currentPanelIndex)
                continue;

            panels[i].panelObject.gameObject.SetActive(false);
        }
    }
}
