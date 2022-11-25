using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CountdownManager : MonoBehaviourPunCallbacks
{   //make sure ur game manager is a singleton first
    public Text timerTxt;

    //[SerializeField]
    private float timeToStartRace = 3.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        timerTxt = RacingGameManager.instance.timeTxt;
    }

    // Update is called once per frame
    void Update()
    {
        if(PhotonNetwork.IsMasterClient){ //assigned to master client so that players' countdowns cna sync with masterclient
          if(timeToStartRace > 0){
            timeToStartRace -= Time.deltaTime;
            photonView.RPC("SetTime", RpcTarget.AllBuffered, timeToStartRace);
          }
          else if(timeToStartRace < 0) photonView.RPC("StartRace", RpcTarget.AllBuffered);
        }

    }

    [PunRPC]
    public void SetTime(float time)
    {
      if(time > 0)timerTxt.text = time.ToString("F1");
      else timerTxt.text = "";
    }

    [PunRPC]
    public void StartRace()
    { //go back to vehicle movement and add isControlEnabled bool
      GetComponent<VehicleMovement>().isControlEnabled = true;
      GetComponent<PlayerSetup>().playerUi.transform.Find("FireBtn").GetComponent<Button>().interactable = true;
      this.enabled = false;
    }
}
