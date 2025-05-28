using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenBox : MonoBehaviour
{
    private Animator animator;
    private bool isOpen = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void BoxOpen()
    {
        isOpen = !isOpen;
        animator.SetBool("isOpen", isOpen);
    }
}
