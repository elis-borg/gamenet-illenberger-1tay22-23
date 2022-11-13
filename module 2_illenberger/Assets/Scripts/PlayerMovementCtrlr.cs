using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerMovementCtrlr : MonoBehaviour
{
    public Joystick joystick;

    private RigidbodyFirstPersonController rigidbodyFirstPersonCtrlr;

    // Start is called before the first frame update
    void Start()
    {
      rigidbodyFirstPersonCtrlr = this.GetComponent<RigidbodyFirstPersonController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate() //if ure using rigidbody or any kind of physics, transform should be for late update
    {
      rigidbodyFirstPersonCtrlr.joystickInputAxis.x = joystick.Horizontal;
      rigidbodyFirstPersonCtrlr.joystickInputAxis.y = joystick.Vertical;

    }
}
