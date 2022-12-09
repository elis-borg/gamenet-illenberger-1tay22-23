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

    // Start is called before the first frame update
    void Start()
    {
        this.camera = transform.Find("Camera").GetComponent<Camera>();
        GameManager.instance.playerList.Add(this.gameObject);

        if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("fp")){ //try to make a key for roles
          GetComponent<PlayerMovement>().enabled = photonView.IsMine;
          //camera.GetComponent<MouseLook>().enabled = photonView.IsMine;
          camera.enabled = photonView.IsMine; //never do the GetComponent<Camera>(), it will cause u errors
          shooting = this.GetComponent<Shooting>();
          mouseLook = camera.GetComponent<MouseLook>();
        }
        else if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("qp")){
          GetComponent<PlayerMovement>().enabled = photonView.IsMine;
          //camera.GetComponent<MouseLook>().enabled = photonView.IsMine;
          camera.enabled = photonView.IsMine;
          shooting = this.GetComponent<Shooting>();
          mouseLook = camera.GetComponent<MouseLook>();

          if(photonView.IsMine){
            playerUi = Instantiate(playerUiPrefab);

            playerNameText = playerUi.transform.Find("Player Health and Name").transform.GetChild(1).GetComponent<Text>();
            playerNameText.text = photonView.Owner.NickName;
            shooting.healthbar = playerUi.transform.Find("Player Health and Name").transform.GetChild(0).transform.GetChild(0).GetComponent<Image>();

            playerUi.transform.Find("QuitBtn").GetComponent<Button>().onClick.AddListener(() => GameManager.instance.LeaveRoom());
            playerUi.transform.Find("FireBtn").GetComponent<Button>().onClick.AddListener(() => shooting.Fire());
          }
        }
    }
}
