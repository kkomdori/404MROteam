using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    CharacterController cc;
    public float speed;
    public float walkSpeed;
    public float runSpeed;
    public float sneakSpeed; 
    public float jumpPower; 
    public bool isJumping;  // false
    private Scrollbar scrollbar;
        

    float gravity = -20f;
    float yVelocity = 0;
    public float interactDistance = 3f;


    private void Start()
    {
        cc = GetComponent<CharacterController>();
        speed = walkSpeed;
        scrollbar = FindObjectOfType<Scrollbar>();
    }

    private void Update()
    {
        if (cc == null || !cc.enabled)
            return;
        float xx = Input.GetAxisRaw("Horizontal");
        float zz = Input.GetAxisRaw("Vertical");

        Vector3 dir = new Vector3(xx, 0, zz).normalized;
        dir = Camera.main.transform.TransformDirection(dir);
        dir.y = 0; // y축은 아래에서 따로 처리

        Vector3 move = dir * speed;

        if (isJumping && cc.collisionFlags == CollisionFlags.Below)
        {
            isJumping = false;
            yVelocity = 0;
        }

        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            yVelocity = jumpPower;
            isJumping = true;
        }

        yVelocity += gravity * Time.deltaTime;
        move.y = yVelocity;
        if (cc.enabled)
        {
            // y축 포함된 전체 이동
            cc.Move(move * Time.deltaTime);
        }

        KeyboardInput();
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                if (hit.collider.CompareTag("Hideable"))
                {
                    Debug.Log("hit");
                    LockerDoor locker = hit.collider.GetComponentInParent<LockerDoor>();
                    if (locker != null)
                    {
                        locker.ToggleDoor();
                    }
                }
            }
        }

    }


    void KeyboardInput()
    {
        float xx = Input.GetAxis("Horizontal");
        float zz = Input.GetAxis("Vertical");

        {
            if (xx != 0 || zz != 0)
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    speed = sneakSpeed; // 조용히 걷기
                    scrollbar.size += 0.003f;
                }

                else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    if (scrollbar != null && scrollbar.size == 0f)
                    {
                        speed = walkSpeed; // 강제로 걷기
                        
                    }
                    else
                    {
                        speed = runSpeed; // 정상 달리기
                        scrollbar.size -= 0.003f;
                    }
                }
                else
                {
                    speed = walkSpeed;
                    scrollbar.size += 0.003f;
                }
                
                   
            }
            else
            {
                speed = 0f; // 멈춘 상태
            }
        }
    }
}
