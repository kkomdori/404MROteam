using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotate : MonoBehaviour
{
    public float rotSpeed;

    float mx = 0;

    private void Update()
    {
        float mouse_X = Input.GetAxis("Mouse X");

        mx += mouse_X * rotSpeed;

        transform.eulerAngles = new Vector3(0, mx, 0);
    }
}
