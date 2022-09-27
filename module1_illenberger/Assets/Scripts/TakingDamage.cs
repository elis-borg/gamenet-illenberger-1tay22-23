using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class TakingDamage : MonoBehaviourPunCallbacks
{
    [SerializeField]
    Image healthbar; //the fill is at 1, need a percentage on how to fill it up

    private float startHealth = 100;
    public float health;

    // Start is called before the first frame update
    void Start()
    {
        health = startHealth;
        healthbar.fillAmount = health/startHealth;
    }

    [PunRPC]
    public void  TakeDamage(int damage) //call it when u receive dmg/ when ray hits player
    {
      health -= damage;
      Debug.Log(photonView.Owner.NickName + "'s HP is at " + health);

      healthbar.fillAmount = health/startHealth; //updates bar across server

      if(health <= 0){
        Die();
      }
    }

    private void Die()
    {
      Debug.Log("Dead");
      //need to make sure leaving the room is owned by actual player
      if(photonView.IsMine){
        GameMngr.instance.LeaveRoom(); //ensures u dont make the other leave the room / its the local player when it reaches 0
      }
    }
}
