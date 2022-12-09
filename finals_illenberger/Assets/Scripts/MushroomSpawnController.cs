using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime; //need to import this to have raiseevent variables
using ExitGames.Client.Photon; //for send options

public class MushroomSpawnController : MonoBehaviourPunCallbacks
{
    public List<GameObject> mushroomSpawns = new List<GameObject>();

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
      if(photonEvent.Code == (byte)Constants.WhoFinishedEventCode){
        //retrieve data that has been passed
        object[] data = (object[]) photonEvent.CustomData;

        string nickNameOfFinishedPlayer = (string)data[0];
        finishOrder = (int)data[1];
        int viewId = (int)data[2];

        Debug.Log(nickNameOfFinishedPlayer + " " + finishOrder);

        GameObject orderUiTxt = GameManager.instance.finisherTxtUI[finishOrder-1];
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
        //foreach(GameObject go in GameManager.instance.mushroomSpawns) mushroomSpawns.Add(go);
    }

    private void OnTriggerEnter(Collider col)
    {
      if(mushroomSpawns.Contains(col.gameObject)){
        int indexOfTrigger = mushroomSpawns.IndexOf(col.gameObject);
        Debug.Log("lap " + indexOfTrigger);

        mushroomSpawns[indexOfTrigger].SetActive(false);
      }

      if(col.gameObject.tag == "FinishTrigger"){
        GameFinish();
      }
    }

    public void GameFinish()
    {
      GetComponent<PlayerSetup>().GetComponent<Camera>().transform.parent = null;
      GetComponent<PlayerMovement>().enabled = false;

      finishOrder++; //needs to be incremented
      GameManager.instance.playersDone++;

      string nickName = photonView.Owner.NickName;
      int viewId = photonView.ViewID; //also send it in the object below, for the purposes of seeing current standing in realtime in the canvas

      //#region Event Data
      object[] data = new object[] {nickName, finishOrder, viewId}; //will also need ur finishOrder and to store nickname of player who finished, data is whats gonna be passed to raisevent

      RaiseEventOptions raiseEventOpts = new RaiseEventOptions //initialize below
      {
        Receivers = ReceiverGroup.All,
        CachingOption = EventCaching.AddToRoomCache
      };

      SendOptions sendOption = new SendOptions
      {
        Reliability = false
      };

      PhotonNetwork.RaiseEvent((byte) Constants.WhoFinishedEventCode, data, raiseEventOpts, sendOption); //pass the parameters data and raiseeventoptions, last parameter is send options
      //#endregion
    }
}
