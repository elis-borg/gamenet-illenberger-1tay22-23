using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Projectile : MonoBehaviour
{
  public GameObject source;

  public int projectileDamage;

void OnTriggerEnter(Collider col)
  {
    if(!source){
      Destroy(this.gameObject);
      return;
    }

    if (col.GetComponent<Collider>().gameObject.CompareTag("Player") && !col.GetComponent<Collider>().gameObject.GetComponent<PhotonView>().IsMine){
      col.GetComponent<Collider>().gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, projectileDamage);
    }

    /*if (col.gameObject.tag == "Player"){
      col.gameObject.GetComponent<Shooting>().currentHealth--;
      float hp = col.gameObject.GetComponent<Shooting>().currentHealth;
      //Debug.Log("Tank HP left: " + hp);
    }*/

    Destroy(this.gameObject);
  }
}
