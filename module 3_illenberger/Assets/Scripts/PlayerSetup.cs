using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public Camera camera;

    public GameObject playerUiPrefab;

    [SerializeField]
    TextMeshProUGUI playerNameText;

    private Shooting shooting;

    // Start is called before the first frame update
    void Start()
    {
        this.camera = transform.Find("Camera").GetComponent<Camera>();
        playerNameText.text = photonView.Owner.NickName;

        if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("rc")){
          GetComponent<VehicleMovement>().enabled = photonView.IsMine;
          GetComponent<LapController>().enabled = photonView.IsMine;
          camera.enabled = photonView.IsMine;
        }
        else if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("dr")){
          GetComponent<VehicleMovement>().enabled = photonView.IsMine;
          GetComponent<LapController>().enabled = photonView.IsMine;
          camera.enabled = photonView.IsMine;
          shooting = this.GetComponent<Shooting>();

          if(photonView.IsMine){
            GameObject playerUi = Instantiate(playerUiPrefab);
            playerUi.transform.Find("QuitBtn").GetComponent<Button>().onClick.AddListener(() => RacingGameManager.instance.LeaveRoom());
            //dont forget to do leftroom override in manager and load lobby scene.
            playerUi.transform.Find("FireBtn").GetComponent<Button>().onClick.AddListener(() => shooting.Fire());
          }
        }
    }
}
