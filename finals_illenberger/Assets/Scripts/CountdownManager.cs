using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CountdownManager : MonoBehaviourPunCallbacks
{   //make sure ur game manager is a singleton first
    public Text timerTxt;
    public Image blindsImg;
    public Sprite buttonImg;

    [SerializeField]
    private float timeToStartHunt = 20.0f; //20

    // Start is called before the first frame update
    void Start()
    {
        if(this.GetComponent<PlayerSetup>().roleTag == "hunter"){
          if(blindsImg == null) blindsImg = GetComponent<PlayerSetup>().playerUi.transform.Find("Blinds").GetComponent<Image>(); //if playersetup doesnt do it properly
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

          if(this.GetComponent<PlayerSetup>().roleTag == "hunter" && blindsImg == null){
            blindsImg = GetComponent<PlayerSetup>().playerUi.transform.Find("Blinds").GetComponent<Image>();
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
    {
      if(this.GetComponent<PlayerSetup>().roleTag == "hunter"){
        blindsImg.enabled = false;
        GetComponent<PlayerMovement>().isControlEnabled = true;
        //GetComponent<BoxCollider>().enabled = false; //turn off this catchnet when movement is enabled
        GetComponent<PlayerSetup>().camera.GetComponent<MouseLook>().isControlEnabled = true;
        GetComponent<PlayerSetup>().playerUi.transform.Find("FireBtn").GetComponent<Button>().interactable = true;
        GetComponent<PlayerSetup>().playerUi.transform.Find("FireBtn").GetComponent<Image>().sprite = buttonImg;
      }
        GameManager.instance.cdTurnedOff = true;
        if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("qp")){
          GameManager.instance.timerIsRunning = true;
        }
        this.enabled = false;
    }
}
