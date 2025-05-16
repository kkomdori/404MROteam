using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class TitleUIManager : MonoBehaviour
{
    public Animator pannel;

    public void OpenPannel(bool pn)
    {
        pannel.SetBool("isHidden", pn);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OpenPannel(true); // 무조건 true로 설정
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
