using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FollowCam : MonoBehaviour
{

    // ���󰡾� �� ����� ������ ����
    public Transform targetTr;
    //Main Camera �ڽ��� transform ������Ʈ
    private Transform camTr;

    // ���� ������κ��� ������ �Ÿ�
    [Range(1.0f, 20.0f)]
    public float distance; // 10

    // Y������ �̵��� ����
    [Range(0.0f, 10.0f)]
    public float height; // 2

    // ī�޶� LookAt�� Offset ��
    public float targetOffset; // 2

    // ���� �ӵ�
    public float damping; // 10

    // SmoothDamp���� ����� ����
    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        // Main Camera �ڽ��� Transform ������Ʈ�� ����
        camTr = transform;
    }

    private void LateUpdate()
    {
        // �����ؾ� �� ����� �������� distance ��ŭ �̵�
        // ���̸� (����� pivot point �� ����) height ��ŭ �̵�
        Vector3 pos = targetTr.position + (-targetTr.forward * distance) + (Vector3.up * height);

        // ���� ��� 1.
        // ���� ���� ���� �Լ��� ����� �ε巴�� ��ġ�� ����; ù ���ϰ� = ���� ��ġ�κ��� �ð� t ��ŭ ��� ���� ��ġ
        //camTr.position = Vector3.Slerp(
        //    camTr.position, // ���� ��ġ
        //    pos, // ��ǥ ��ġ
        //    Time.deltaTime * damping); // �ð� t �������� ��ġ ���� : ���� ���� ������ �̵�

        // ���� ��� 2.
        camTr.position = Vector3.SmoothDamp(
            camTr.position, // ���� ��ġ
            pos, // ��ǥ ��ġ
            ref velocity, // ���� ������ �� �ӵ� ĳ����
            damping); // ��ǥ ��ġ���� ������ �ð� : Ŭ ���� ������ �̵�

        // Camera�� �ǹ� ��ǥ�� ���� ȸ��
        camTr.LookAt(targetTr.position + (targetTr.up * targetOffset)); // ����: Vector3.up == targetTr.up, ������ ��: Vector3.up != targetTr.up
    }
}
