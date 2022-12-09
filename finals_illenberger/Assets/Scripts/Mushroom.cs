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
        //col.GetComponent<Collider>().gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 1);

        GameManager.instance.collected = true;
        GameManager.instance.mushroom = null;
        //try to call for rpc too

        //#region RaiseEvent
        //object[] data = new object[] {nickName, finishOrder, viewId};
        /*object[] data = new object[] {}; //need to pass mushroom's last position in the map to alert hunters

        RaiseEventOptions raiseEventOpts = new RaiseEventOptions
        {
          Receivers = ReceiverGroup.All,
          CachingOption = EventCaching.AddToRoomCache
        };

        SendOptions sendOption = new SendOptions
        {
          Reliability = false
        };

        PhotonNetwork.RaiseEvent((byte) Constants.MushroomCollectedEventCode, data, raiseEventOpts, sendOption);*/
        //#endregion

        Destroy(this.gameObject);
      }
    }
}
