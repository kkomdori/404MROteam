using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Animator image1;
    public Animator title;
    public Animator tvFace;
    public Animator image2;
    public Animator startBtn;

    public void OpenUis(bool openUi)
    {
        image1.SetBool("isHidden", openUi);
        title.SetBool("isHidden", openUi);
        tvFace.SetBool("isHidden", openUi);
        image2.SetBool("isHidden", openUi);
        startBtn.SetBool("isHidden", openUi);
    }
}

