using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public Animator image1;
    public Animator title;
    public Animator tvFrame;
    public Animator image2;
    public Animator startBtn;

    public List<GameObject> stageUIList;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OpenUis(true); // 무조건 true로 설정
            CloseAllStageUI();
            EventSystem.current.SetSelectedGameObject(null); // 선택된 UI 해제
        }
    }

    public void GoToTitle()
    {
        SceneManager.LoadScene("Title");
    }
    public void Stage1()
    {
        SceneManager.LoadScene("Stage_1");
        SceneManager.LoadScene("Play", LoadSceneMode.Additive);
        SceneManager.LoadScene("Play_GUI", LoadSceneMode.Additive);
    }
    public void Stage2()
    {
        SceneManager.LoadScene("Stage_2");
        SceneManager.LoadScene("Play", LoadSceneMode.Additive);
        SceneManager.LoadScene("Play_GUI", LoadSceneMode.Additive);
    }
    public void Stage3()
    {
        SceneManager.LoadScene("Stage_3");
        SceneManager.LoadScene("Play", LoadSceneMode.Additive);
        SceneManager.LoadScene("Play_GUI", LoadSceneMode.Additive);
    }

    public void OpenUis(bool openUi)
    {
        image1.SetBool("isHidden", openUi);
        title.SetBool("isHidden", openUi);
        tvFrame.SetBool("isHidden", openUi);
        image2.SetBool("isHidden", openUi);
        startBtn.SetBool("isHidden", openUi);
    }
    public void OpenStageUI(GameObject targetParent)
    {
        foreach (GameObject parent in stageUIList)
        {
            Animator[] childAnimators = parent.GetComponentsInChildren<Animator>(true);
            bool isTarget = (parent == targetParent);

            foreach (Animator anim in childAnimators)
            {
                if (HasParameter(anim, "isHidden"))
                    anim.SetBool("isHidden", !isTarget);
            }
        }
    }

    public void CloseAllStageUI()
    {
        foreach (GameObject parent in stageUIList)
        {
            Animator[] childAnimators = parent.GetComponentsInChildren<Animator>(true); // 자식 포함
            foreach (Animator anim in childAnimators)
            {
                if (HasParameter(anim, "isHidden"))
                    anim.SetBool("isHidden", true);
            }
        }
    }
    private bool HasParameter(Animator anim, string paramName)
    {
        foreach (var param in anim.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

}

