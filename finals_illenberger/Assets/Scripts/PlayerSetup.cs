using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public Camera camera;
    public GameObject playerUiPrefab,
                      playerUi;

    public string roleTag;

    [SerializeField]
    Text playerNameText;

    private Shooting shooting;
    private MouseLook mouseLook;
    private MimicHighlight mimicHL;

    // Start is called before the first frame update
    void Start()
    {
        this.camera = transform.Find("Camera").GetComponent<Camera>();
        GameManager.instance.playerList.Add(this.gameObject);

        //if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("fp"))
        //if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("qp"))

          GetComponent<PlayerMovement>().enabled = photonView.IsMine;
          camera.enabled = photonView.IsMine; //never do the GetComponent<Camera>(), it will cause u errors
          camera.GetComponent<MouseLook>().enabled = photonView.IsMine;
          shooting = this.GetComponent<Shooting>();
          mouseLook = camera.GetComponent<MouseLook>();

          if(roleTag == "shifter") mimicHL = this.GetComponent<MimicHighlight>();

          if(photonView.IsMine){
            playerUi = Instantiate(playerUiPrefab);

            playerNameText = playerUi.transform.Find("Player Health and Name").transform.GetChild(1).GetComponent<Text>();
            playerNameText.text = photonView.Owner.NickName;
            shooting.healthbar = playerUi.transform.Find("Player Health and Name").transform.GetChild(0).transform.GetChild(0).GetComponent<Image>();

            playerUi.transform.Find("QuitBtn").GetComponent<Button>().onClick.AddListener(() => GameManager.instance.LeaveRoom());
            playerUi.transform.Find("FireBtn").GetComponent<Button>().onClick.AddListener(() => shooting.Fire());


            if(roleTag == "hunter") GetComponent<CountdownManager>().blindsImg = playerUi.transform.Find("Blinds").GetComponent<Image>();
            GetComponent<CountdownManager>().timerTxt = playerUi.transform.Find("NotificationText").GetComponent<Text>();
          }
    }
}
