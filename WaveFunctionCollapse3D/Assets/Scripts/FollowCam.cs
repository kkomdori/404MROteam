using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FollowCam : MonoBehaviour
{

    // 따라가야 할 대상을 연결할 변수
    public Transform targetTr;
    //Main Camera 자신의 transform 컴포넌트
    private Transform camTr;

    // 따라갈 대상으로부터 떨어질 거리
    [Range(1.0f, 20.0f)]
    public float distance; // 10

    // Y축으로 이동할 높이
    [Range(0.0f, 10.0f)]
    public float height; // 2

    // 카메라 LookAt의 Offset 값
    public float targetOffset; // 2

    // 반응 속도
    public float damping; // 10

    // SmoothDamp에서 사용할 변수
    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        // Main Camera 자신의 Transform 콤포넌트를 추출
        camTr = transform;
    }

    private void LateUpdate()
    {
        // 추적해야 할 대상의 뒤쪽으로 distance 만큼 이동
        // 높이를 (대상의 pivot point 로 부터) height 만큼 이동
        Vector3 pos = targetTr.position + (-targetTr.forward * distance) + (Vector3.up * height);

        // 보간 방법 1.
        // 구면 선형 보간 함수를 사용해 부드럽게 위치를 변경; 첫 리턴값 = 시작 위치로부터 시간 t 만큼 경과 후의 위치
        //camTr.position = Vector3.Slerp(
        //    camTr.position, // 시작 위치
        //    pos, // 목표 위치
        //    Time.deltaTime * damping); // 시간 t 간격으로 위치 리턴 : 작을 수록 느리게 이동

        // 보간 방법 2.
        camTr.position = Vector3.SmoothDamp(
            camTr.position, // 시작 위치
            pos, // 목표 위치
            ref velocity, // 현재 프레임 당 속도 캐스팅
            damping); // 목표 위치까지 도달할 시간 : 클 수록 느리게 이동

        // Camera를 피벗 좌표를 향해 회전
        camTr.LookAt(targetTr.position + (targetTr.up * targetOffset)); // 평지: Vector3.up == targetTr.up, 기울었을 때: Vector3.up != targetTr.up
    }
}
