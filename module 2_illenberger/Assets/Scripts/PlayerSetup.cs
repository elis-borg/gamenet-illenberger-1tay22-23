using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityStandardAssets.Characters.FirstPerson;
using TMPro;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public GameObject fpsModel;
    public GameObject nonFpsModel;

    public GameObject playerUiPrefab;

    [SerializeField]
    TextMeshProUGUI playerNameText;

    public PlayerMovementCtrlr playerMovementCtrlr;
    public Camera fpsCamera;

    private Animator animator;
    public Avatar fpsAvatar, nonFpsAvatar; //since we're using different animator avatars, need to also assign which one to use

    private Shooting shooting;
    // Start is called before the first frame update
    void Start()
    {
      playerMovementCtrlr = this.GetComponent<PlayerMovementCtrlr>();
      animator = this.GetComponent<Animator>();

      fpsModel.SetActive(photonView.IsMine); //player view
      nonFpsModel.SetActive(!photonView.IsMine);  //opponent view
      animator.SetBool("isLocalPlayer", photonView.IsMine);
      animator.avatar = photonView.IsMine ? fpsAvatar : nonFpsAvatar;

      shooting = this.GetComponent<Shooting>();

      if(photonView.IsMine){
        GameObject playerUi = Instantiate(playerUiPrefab);
        playerMovementCtrlr.fixedTouchFld = playerUi.transform.Find("RotationTouchFld").GetComponent<FixedTouchField>();
        playerMovementCtrlr.joystick = playerUi.transform.Find("Fixed Joystick").GetComponent<Joystick>();
        fpsCamera.enabled = true;

        playerUi.transform.Find("FireBtn").GetComponent<Button>().onClick.AddListener(() => shooting.Fire());
      }
      else{
        playerMovementCtrlr.enabled = false;
        GetComponent<RigidbodyFirstPersonController>().enabled = false;
        fpsCamera.enabled = false;
      }

      playerNameText.text = photonView.Owner.NickName;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
