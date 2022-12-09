using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 20,
                  gravity = -9.81f,
                  groundDist = 0.4f,
                  jumpHeight = 3f;
    private float currentSpeed = 0;
    private float rotationSpeed = 200;

    private Vector3 velocity;

    public CharacterController controller;

    public bool isControlEnabled;

    public Transform groundCheck;
    public LayerMask groundMask;
    bool isGrounded;

    void Update()
    {
        if(isControlEnabled){
          /*float translation = Input.GetAxis("Vertical") * speed * Time.deltaTime;
          float rotation = Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;

          transform.Translate(0,0, translation);
          currentSpeed = translation;

          transform.Rotate(0, rotation, 0);*/

          isGrounded = Physics.CheckSphere(groundCheck.position, groundDist, groundMask);

          if(isGrounded && velocity. y < 0){
            velocity.y = -2f;
          }

          float x = Input.GetAxis("Horizontal");
          float z = Input.GetAxis("Vertical");

          Vector3 move = transform.right * x + transform.forward * z;

          controller.Move(move * speed * Time.deltaTime);

          if(Input.GetButton("Jump") && isGrounded){
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
          }

          velocity.y += gravity * Time.deltaTime;

          controller.Move(velocity * Time.deltaTime);
        }
    }
}
