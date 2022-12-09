using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CountdownManager : MonoBehaviourPunCallbacks
{   //make sure ur game manager is a singleton first
    public Text timerTxt;
    public Image blindsImg;

    //[SerializeField]
    private float timeToStartHunt = 20.0f;

    // Start is called before the first frame update
    void Start()
    {
        timerTxt = GameManager.instance.timeTxt;
        blindsImg = GameManager.instance.blindsImgUI;

        if(this.GetComponent<PlayerSetup>().roleTag == "shifter"){
          blindsImg.enabled = false;
          //GetComponent<PlayerMovement>().isControlEnabled = true;
          GetComponent<PlayerSetup>().playerUi.transform.Find("FireBtn").GetComponent<Button>().interactable = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
          if(PhotonNetwork.IsMasterClient){ //assigned to master client so that players' countdowns cna sync with masterclient
            if(timeToStartHunt > 0){
              timeToStartHunt -= Time.deltaTime;
              photonView.RPC("SetTime", RpcTarget.AllBuffered, timeToStartHunt);
            }
            else if(timeToStartHunt < 0) photonView.RPC("StartHunt", RpcTarget.AllBuffered);
          }
    }

    [PunRPC]
    public void SetTime(float time)
    {
      if(this.GetComponent<PlayerSetup>().roleTag == "shifter"){
        if(time > 0) timerTxt.text = "The hunters are coming in " + time.ToString("F1") + " seconds."; //resize textbox and add a black screen
        else timerTxt.text = "";
      }
      else{
        if(time > 0) timerTxt.text = "Arriving in the fae's forest in " + time.ToString("F1") + " seconds."; //resize textbox and add a black screen
        else timerTxt.text = "";
      }
    }

    [PunRPC]
    public void StartHunt()
    { //go back to vehicle movement and add isControlEnabled bool
      if(this.GetComponent<PlayerSetup>().roleTag == "hunter"){
        blindsImg.enabled = false;
        Debug.Log("blinds removed");
        GetComponent<PlayerMovement>().isControlEnabled = true;
        Debug.Log("controls enabled");
        GetComponent<PlayerSetup>().playerUi.transform.Find("FireBtn").GetComponent<Button>().interactable = true;
        Debug.Log("firing enabled");
      }
        GameManager.instance.cdTurnedOff = true;
        Debug.Log("countdown manager off");
        this.enabled = false;
    }
}
