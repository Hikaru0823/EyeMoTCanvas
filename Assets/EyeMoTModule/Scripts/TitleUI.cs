using KanKikuchi.AudioManager;
using UnityEngine;

public class TitleUI : MonoBehaviour
{
    [SerializeField] private Animator titleCanvs;
    private CanvasGroup modulecanvs;
    [SerializeField] private CanvasGroup mainCanvas;
    [SerializeField] private GameObject camera;

    bool isActive  =false;
    bool isfadeOut = false;


    void Awake()
    {
        //modulecanvs = FindFirstObjectByType<ExitManager>().transform.GetChild(0).GetComponent<CanvasGroup>();
        if(!ES3.Load<bool>("FirstOpen", false))
        {
            ES3.Save<bool>("FirstOpen", true);
            titleCanvs.Play("Panel In");
            camera.SetActive(true);
            modulecanvs.alpha = 0;
            mainCanvas.alpha = 0;
            modulecanvs.blocksRaycasts = false;
            mainCanvas.blocksRaycasts = false;
            isActive = true;
        }
        else
        {
            camera.SetActive(false);
            titleCanvs.gameObject.SetActive(false);
            modulecanvs.alpha = 1;
            mainCanvas.alpha = 1;
            modulecanvs.blocksRaycasts = true;
            mainCanvas.blocksRaycasts = true;
            isActive = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isActive)
        {
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    OnInPut();
                }
            }
        }
    }

    void FixedUpdate()
    {
        if(isfadeOut)
        {
            modulecanvs.alpha += Time.deltaTime;
            mainCanvas.alpha += Time.deltaTime;
            if(mainCanvas.alpha >= 1 || modulecanvs.alpha >= 1)
            {
                modulecanvs.blocksRaycasts = true;
                mainCanvas.blocksRaycasts = true;    
                isfadeOut = false;
            }
        }
    }

    public void OnInPut()
    {
        camera.SetActive(false);
        if(!isActive) return;
        isActive = false;
        isfadeOut  =true;
        titleCanvs.Play("Panel Out");
        SEManager.Instance.Play(SEPath.CLICK);
    }

    void OnApplicationQuit()
    {
        ES3.Save<bool>("FirstOpen", false);
    }
}
