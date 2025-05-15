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

    private void Start()
    {
        cc = GetComponent<CharacterController>();
        speed = walkSpeed;
        scrollbar = FindObjectOfType<Scrollbar>();
    }

    private void Update()
    {
        float xx = Input.GetAxisRaw("Horizontal");
        float zz = Input.GetAxisRaw("Vertical");

        Vector3 dir = new Vector3(xx, 0, zz).normalized;
        dir = Camera.main.transform.TransformDirection(dir);
        dir.y = 0; // y���� �Ʒ����� ���� ó��

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

        cc.Move(move * Time.deltaTime);  // y�� ���Ե� ��ü �̵�

        KeyboardInput();
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
                    speed = sneakSpeed; // ������ �ȱ�
                    scrollbar.size += 0.003f;
                }

                else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    if (scrollbar != null && scrollbar.size == 0f)
                    {
                        speed = walkSpeed; // ������ �ȱ�
                        
                    }
                    else
                    {
                        speed = runSpeed; // ���� �޸���
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
                speed = 0f; // ���� ����
            }
        }
    }
}
