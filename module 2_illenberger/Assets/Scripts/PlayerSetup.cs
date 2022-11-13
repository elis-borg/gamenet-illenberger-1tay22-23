using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public GameObject fpsModel;
    public GameObject nonFpsModel;

    // Start is called before the first frame update
    void Start()
    {
      fpsModel.SetActive(photonView.IsMine); //player view
      nonFpsModel.SetActive(!photonView.IsMine);  //opponent view
    }

    // Update is called once per frame
    void Update()
    {

    }
}
