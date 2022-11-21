using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime; //need to import this to have raiseevent variables
using ExitGames.Client.Photon; //for send options

public class LapController : MonoBehaviourPunCallbacks
{
    public List<GameObject> lapTriggers = new List<GameObject>();

    public enum RaiseEventsCode
    {
      WhoFinishedEventCode = 0
    }

    private int finishOrder = 0;

    private void OnEnable()
    {
      PhotonNetwork.NetworkingClient.EventReceived += OnEvent; //this is how to add listeners to all listeners in event
    }

    private void OnDisable()
    {
      PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }
    //on enable and on disable but first add a callback whenever an event is called
    void OnEvent (EventData photonEvent)
    {
      if(photonEvent.Code == (byte)RaiseEventsCode.WhoFinishedEventCode){
        //retrieve data that has been passed
        object[] data = (object[]) photonEvent.CustomData;

        string nickNameOfFinishedPlayer = (string)data[0];
        finishOrder = (int)data[1];
        int viewId = (int)data[2];

        Debug.Log(nickNameOfFinishedPlayer + " " + finishOrder);

        GameObject orderUiTxt = RacingGameManager.instance.finisherTxtUI[finishOrder-1];
        orderUiTxt.SetActive(true);

        if(viewId == photonView.ViewID) {//this is you
          orderUiTxt.GetComponent<Text>().text = finishOrder + " " + nickNameOfFinishedPlayer + "(YOU)";
          orderUiTxt.GetComponent<Text>().color = Color.red;
        }
        else{
          orderUiTxt.GetComponent<Text>().text = finishOrder + " " + nickNameOfFinishedPlayer;
        }
      }
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach(GameObject go in RacingGameManager.instance.lapTriggers) lapTriggers.Add(go);
    }

    private void OnTriggerEnter(Collider col)
    {
      if(lapTriggers.Contains(col.gameObject)){
        int indexOfTrigger = lapTriggers.IndexOf(col.gameObject);
        Debug.Log("lap " + indexOfTrigger);

        lapTriggers[indexOfTrigger].SetActive(false);
      }

      if(col.gameObject.tag == "FinishTrigger"){
        GameFinish();
      }
    }

    public void GameFinish()
    {
      GetComponent<PlayerSetup>().camera.transform.parent = null;
      GetComponent<VehicleMovement>().enabled = false;

      finishOrder++; //needs to be incremented

      string nickName = photonView.Owner.NickName;
      int viewId = photonView.ViewID; //also send it in the object below, for the purposes of seeing current standing in realtime in the canvas

      //event data
      object[] data = new object[] {nickName, finishOrder, viewId}; //will also need ur finishOrder and to store nickname of player who finished, data is the what is gonna be passed to raisevent

      RaiseEventOptions raiseEventOpts = new RaiseEventOptions //initialize below
      {
        Receivers = ReceiverGroup.All,
        CachingOption = EventCaching.AddToRoomCache
      };

      SendOptions sendOption = new SendOptions
      {
        Reliability = false
      };

      PhotonNetwork.RaiseEvent((byte) RaiseEventsCode.WhoFinishedEventCode, data, raiseEventOpts, sendOption); //pass the parameters data and raiseeventoptions, last parameter is send options
    }
}
