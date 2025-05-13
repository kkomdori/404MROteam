using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamRotation : MonoBehaviour
{
    public float rotSpeed;

    float my = 0;

    private void Start()
    {
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        // 좌우는 부모 오브젝트에 의존
        float mouse_Y = Input.GetAxis("Mouse Y");

        my += mouse_Y * rotSpeed;

        my = Mathf.Clamp(my, -90f, 90f);

        // y 축을 기준으로 mx 만큼 회전; x 축을 기준으로 -my 만큼 회전
        transform.eulerAngles = new Vector3(-my, transform.eulerAngles.y, transform.eulerAngles.z);
    }
}
