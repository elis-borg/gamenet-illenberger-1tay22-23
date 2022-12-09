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
    TextMeshProUGUI playerNameText;

    private Shooting shooting;

    // Start is called before the first frame update
    void Start()
    {
        this.camera = transform.Find("Camera").GetComponent<Camera>();
        GameManager.instance.playerList.Add(this.gameObject);
        playerNameText.text = photonView.Owner.NickName;

        if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("fp")){ //try to make a key for roles
          GetComponent<PlayerMovement>().enabled = photonView.IsMine;
          //GetComponent<MushroomSpawnController>().enabled = photonView.IsMine;
          GetComponent<Camera>().enabled = photonView.IsMine;
        }
        else if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("qp")){
          GetComponent<PlayerMovement>().enabled = photonView.IsMine;
          //GetComponent<MushroomSpawnController>().enabled = photonView.IsMine;
          GetComponent<Camera>().enabled = photonView.IsMine;
          shooting = this.GetComponent<Shooting>();
          Debug.Log("camera and movement script's photonview are true and shooting script has been retrieved");


          if(photonView.IsMine){
            playerUi = Instantiate(playerUiPrefab);
            Debug.Log("ui for this player created");

            playerUi.transform.Find("QuitBtn").GetComponent<Button>().onClick.AddListener(() => GameManager.instance.LeaveRoom());
            playerUi.transform.Find("FireBtn").GetComponent<Button>().onClick.AddListener(() => shooting.Fire());
          }
        }
    }
}
