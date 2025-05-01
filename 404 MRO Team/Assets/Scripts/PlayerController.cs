using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PlayerController : MonoBehaviour
{
    CharacterController cc;
    public float speed;
    public float walkSpeed;
    public float runSpeed;
    public float sneakSpeed; 
    public float jumpPower; 
    public bool isJumping;  // false

    float gravity = -20f;
    float yVelocity = 0;

    private void Start()
    {
        cc = GetComponent<CharacterController>();
        speed = walkSpeed;
    }

    private void Update()
    {
        float xx = Input.GetAxisRaw("Horizontal");
        float zz = Input.GetAxisRaw("Vertical");

        Vector3 dir = new Vector3(xx, 0, zz);
        dir = dir.normalized;
        dir = Camera.main.transform.TransformDirection(dir);

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
        dir.y = yVelocity;

        cc.Move(dir * speed * Time.deltaTime);

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
                }

                else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    speed = runSpeed; // �޸���
                }
                else
                {
                    speed = walkSpeed; // �⺻ �ȱ�
                }
                   
            }
            else
            {
                speed = 0f; // ���� ����
            }
        }
    }
}
