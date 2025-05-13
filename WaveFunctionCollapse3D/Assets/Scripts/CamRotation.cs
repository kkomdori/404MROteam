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
        // �¿�� �θ� ������Ʈ�� ����
        float mouse_Y = Input.GetAxis("Mouse Y");

        my += mouse_Y * rotSpeed;

        my = Mathf.Clamp(my, -90f, 90f);

        // y ���� �������� mx ��ŭ ȸ��; x ���� �������� -my ��ŭ ȸ��
        transform.eulerAngles = new Vector3(-my, transform.eulerAngles.y, transform.eulerAngles.z);
    }
}
