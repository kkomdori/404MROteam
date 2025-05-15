using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DroneCameraController : MonoBehaviour
{
    [Header("Speed Settings")]
    public float moveSpeed = 10f;         // 기본 이동 속도
    public float boostMultiplier = 4f;    // Shift 누를 때 속도 배수
    public float ascendSpeed = 5f;        // 상승/하강 속도

    [Header("Mouse Look")]
    public float mouseSensitivity = 3f;
    public float pitchLimit = 85f;        // 상하 회전 제한

    private float yaw = 0f;
    private float pitch = 0f;

    void Start()
    {
        // 커서 잠금 및 보이지 않게
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 초기 회전값 설정
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    private void HandleMouseLook()
    {
        // 마우스 입력
        float mx = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float my = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        yaw += mx;
        pitch -= my;
        pitch = Mathf.Clamp(pitch, -pitchLimit, pitchLimit);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void HandleMovement()
    {
        // WASD (X,Z) 이동
        Vector3 inputDir = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0f,
            Input.GetAxisRaw("Vertical")
        );

        // 상하 이동 (Space: 상승, C: 하강)
        if (Input.GetKey(KeyCode.Space)) inputDir.y += 1f;
        if (Input.GetKey(KeyCode.C) ||
            Input.GetKey(KeyCode.LeftControl)) inputDir.y -= 1f;

        inputDir = inputDir.normalized;

        // 속도 계산 (Shift로 부스트)
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift)
                                   ? boostMultiplier
                                   : 1f);

        // 월드가 아닌 카메라 로컬 기준으로 이동
        Vector3 move = transform.TransformDirection(inputDir) * speed * Time.deltaTime;
        transform.position += move;
    }
}
