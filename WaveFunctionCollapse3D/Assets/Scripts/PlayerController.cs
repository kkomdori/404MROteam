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
        // 플레이 화면 클릭 시 커서 잠금 및 보이지 않게
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

        // 케릭터의 정면 (메인 카메라) 방향을 월드 좌표 기준으로 바꿈
        dir = Camera.main.transform.TransformDirection(dir);
        dir = dir.normalized;

        if (isJumping && cc.collisionFlags == CollisionFlags.Below)
        {
            isJumping = false;
            yVelocity = 0f; // 값이 계속 감소하는 문제 해결
        }

        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            yVelocity = jumpPower;
            isJumping = true;
        }

        yVelocity += gravity * Time.deltaTime;
        dir.y = yVelocity;

        //transform.Translate(dir * moveSpeed * Time.deltaTime); cc.Move() 로 대체
        cc.Move(dir * moveSpeed * Time.deltaTime);

        // Rotate
        float mouse_X = Input.GetAxis("Mouse X");
        mx += mouse_X * rotSpeed;
        transform.eulerAngles = new Vector3(0, mx, 0); // y 축을 기준으로 mx 만큼 회전
    }
}
