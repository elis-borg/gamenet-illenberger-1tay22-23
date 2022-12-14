using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Mushroom : MonoBehaviour
{
  //this script handles the floaty movement of the mushroom and the collision events

  //floating code from Donovan Keith
  public float degreesPerSecond = 15.0f;
  public float amplitude = 0.5f;
  public float frequency = 1f;

  public float timer = 60.0f; //if unclaimed after 60 seconds respawns to a different spot.

  // Position Storage Variables
  Vector3 posOffset = new Vector3 ();
  Vector3 tempPos = new Vector3 ();

  // Use this for initialization
  void Start () {
      // Store the starting position & rotation of the object
      posOffset = transform.position;
  }

  // Update is called once per frame
  void Update () {
      // Spin object around Y-Axis
      transform.Rotate(new Vector3(0f, Time.deltaTime * degreesPerSecond, 0f), Space.World);

      // Float up/down with a Sin()
      tempPos = posOffset;
      tempPos.y += Mathf.Sin (Time.fixedTime * Mathf.PI * frequency) * amplitude;

      transform.position = tempPos;
  }

  void OnTriggerEnter(Collider col)
    {

      if (col.GetComponent<Collider>().gameObject.CompareTag("Player") && col.GetComponent<PlayerSetup>().roleTag == "shifter"){
        GameObject fairy = col.GetComponent<Collider>().gameObject;
        fairy.GetComponent<PhotonView>().RPC("PickupShroom", RpcTarget.AllBuffered);
        MushroomSpawner.instance.mushroom = null;
        PhotonNetwork.Destroy(this.gameObject);
      }
    }
}
