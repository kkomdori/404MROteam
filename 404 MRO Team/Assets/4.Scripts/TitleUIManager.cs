using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleUIManager : MonoBehaviour
{
    public List<GameObject> panelList; // HGPanel, STPanel 등을 등록
    public Animator panelAnimator;

    public List<GameObject> dlgList; // Settings Dlg 리스트

    private void Start()
    {
        SelectSettingsTab(dlgList[0]);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideAllPanels();
        }
    }

    // 특정 패널만 보여주고 나머지는 숨김
    public void OpenPannelUI(GameObject targetPanel)
    {
        foreach (GameObject panel in panelList)
        {
            bool isTarget = (panel == targetPanel);
            panel.SetActive(isTarget);

            Animator anim = panel.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetBool("isHidden", !isTarget);
            }
        }
    }

    public void HideAllPanels()
    {
        foreach (GameObject panel in panelList)
        {
            Animator anim = panel.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetBool("isHidden", true);
            }
        }
    }
    public void ClosePanel(GameObject targetPanel)
    {
        Animator anim = targetPanel.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("isHidden", true);
        }
    }

    public void SelectSettingsTab(GameObject targetDlg)
    {
        foreach (GameObject dlg in dlgList)
        {
            dlg.SetActive(dlg == targetDlg);
        }
    }

    public void GoToMenuScene()
    {
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
