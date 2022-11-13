using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerMovementCtrlr : MonoBehaviour
{
    public Joystick joystick;
    public FixedTouchField fixedTouchFld;

    private RigidbodyFirstPersonController rigidbodyFirstPersonCtrlr;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
      rigidbodyFirstPersonCtrlr = this.GetComponent<RigidbodyFirstPersonController>();
      animator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate() //if ure using rigidbody or any kind of physics, transform should be for late update
    {
      rigidbodyFirstPersonCtrlr.joystickInputAxis.x = joystick.Horizontal;
      rigidbodyFirstPersonCtrlr.joystickInputAxis.y = joystick.Vertical;
      rigidbodyFirstPersonCtrlr.mouseLook.lookInputAxis = fixedTouchFld.TouchDist;

      animator.SetFloat("horizontal", joystick.Horizontal);
      animator.SetFloat("vertical",joystick.Vertical);

      //is our player running or not
      if(Mathf.Abs(joystick.Horizontal) > 0.9 || Mathf.Abs(joystick.Vertical) > 0.9){
        animator.SetBool("isRunning", true);
        //tweaking speed
        rigidbodyFirstPersonCtrlr.movementSettings.ForwardSpeed = 10;
      }
      else{
        animator.SetBool("isRunning", false);
        rigidbodyFirstPersonCtrlr.movementSettings.ForwardSpeed = 5;
      }
    }
}
