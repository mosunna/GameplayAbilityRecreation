
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterMovement : MonoBehaviour
{
    private CharacterController controller;

    public float speed = 5f;
    public float jumpForce = 2f;
    private Vector3 velocity;

    public float gravity = -12f;

    private Vector3 movementDirection;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        if(controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        //movementDirection = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");
        movementDirection = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");

        controller.Move(movementDirection * speed * Time.deltaTime); // Horizontal movement

        // Gravity handling
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime); // Vertical Movement
    }
}
