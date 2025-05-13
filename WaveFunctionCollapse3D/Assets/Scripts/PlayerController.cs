using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // Move setting
    [Header("Move")]
    public float moveSpeed; 
    public float jumpPower;
    public bool isJumping;
    public float gravity;

    // Rotate setting
    [Header("Rotate")]
    public float rotSpeed;
    float mx = 0;

    float yVelocity = 0f;
    CharacterController cc;

    void Start()
    {
        // �÷��� ȭ�� Ŭ�� �� Ŀ�� ��� �� ������ �ʰ�
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Move
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 dir = new Vector3(h, 0f, v);

        // �ɸ����� ���� (���� ī�޶�) ������ ���� ��ǥ �������� �ٲ�
        dir = Camera.main.transform.TransformDirection(dir);
        dir = dir.normalized;

        if (isJumping && cc.collisionFlags == CollisionFlags.Below)
        {
            isJumping = false;
            yVelocity = 0f; // ���� ��� �����ϴ� ���� �ذ�
        }

        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            yVelocity = jumpPower;
            isJumping = true;
        }

        yVelocity += gravity * Time.deltaTime;
        dir.y = yVelocity;

        //transform.Translate(dir * moveSpeed * Time.deltaTime); cc.Move() �� ��ü
        cc.Move(dir * moveSpeed * Time.deltaTime);

        // Rotate
        float mouse_X = Input.GetAxis("Mouse X");
        mx += mouse_X * rotSpeed;
        transform.eulerAngles = new Vector3(0, mx, 0); // y ���� �������� mx ��ŭ ȸ��
    }
}
