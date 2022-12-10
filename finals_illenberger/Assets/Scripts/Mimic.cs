using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Mimic : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    public string mName;
    public int mSpeed;

    GameObject parent;
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Debug.Log("custom instantiate data being accessed");
        object[] data = info.photonView.InstantiationData;
        //Transform parent = (GameObject)data[0].transform //photon cant pass gameobjects or transforms
        parent = PhotonView.Find((int)data[0]).gameObject;

        this.transform.SetParent(parent.transform);
    }
}
