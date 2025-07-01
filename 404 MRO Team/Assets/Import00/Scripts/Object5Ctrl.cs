using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object5Ctrl : MonoBehaviour
{
    private void Update()
    {
        StartCoroutine(Reaction());
    }

    IEnumerator Reaction()
    {
        while(true)
        {
        Debug.Log("Runtime ½ÇÇà : " + gameObject.name);
        yield return new WaitForSeconds(1f);
        }
    }
}
