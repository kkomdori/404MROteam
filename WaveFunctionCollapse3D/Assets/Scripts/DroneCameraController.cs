using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DroneCameraController : MonoBehaviour
{
    [Header("Speed Settings")]
    public float moveSpeed = 10f;         // �⺻ �̵� �ӵ�
    public float boostMultiplier = 4f;    // Shift ���� �� �ӵ� ���
    public float ascendSpeed = 5f;        // ���/�ϰ� �ӵ�

    [Header("Mouse Look")]
    public float mouseSensitivity = 3f;
    public float pitchLimit = 85f;        // ���� ȸ�� ����

    private float yaw = 0f;
    private float pitch = 0f;

    void Start()
    {
        // Ŀ�� ��� �� ������ �ʰ�
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // �ʱ� ȸ���� ����
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
        // ���콺 �Է�
        float mx = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float my = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        yaw += mx;
        pitch -= my;
        pitch = Mathf.Clamp(pitch, -pitchLimit, pitchLimit);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void HandleMovement()
    {
        // WASD (X,Z) �̵�
        Vector3 inputDir = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0f,
            Input.GetAxisRaw("Vertical")
        );

        // ���� �̵� (Space: ���, C: �ϰ�)
        if (Input.GetKey(KeyCode.Space)) inputDir.y += 1f;
        if (Input.GetKey(KeyCode.C) ||
            Input.GetKey(KeyCode.LeftControl)) inputDir.y -= 1f;

        inputDir = inputDir.normalized;

        // �ӵ� ��� (Shift�� �ν�Ʈ)
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift)
                                   ? boostMultiplier
                                   : 1f);

        // ���尡 �ƴ� ī�޶� ���� �������� �̵�
        Vector3 move = transform.TransformDirection(inputDir) * speed * Time.deltaTime;
        transform.position += move;
    }
}
