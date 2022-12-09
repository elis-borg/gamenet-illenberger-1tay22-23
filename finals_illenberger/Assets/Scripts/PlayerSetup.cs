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
        playerNameText.text = photonView.Owner.NickName; //works

        if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("fp")){ //try to make a key for roles
          GetComponent<PlayerMovement>().enabled = photonView.IsMine;
          camera.enabled = photonView.IsMine; //never do the GetComponent<Camera>(), it will cause u errors
          shooting = this.GetComponent<Shooting>(); 
        }
        else if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("qp")){
          GetComponent<PlayerMovement>().enabled = photonView.IsMine;
          camera.enabled = photonView.IsMine;
          shooting = this.GetComponent<Shooting>();

          if(photonView.IsMine){
            playerUi = Instantiate(playerUiPrefab);

            playerUi.transform.Find("QuitBtn").GetComponent<Button>().onClick.AddListener(() => GameManager.instance.LeaveRoom());
            playerUi.transform.Find("FireBtn").GetComponent<Button>().onClick.AddListener(() => shooting.Fire());
          }
        }
    }
}
