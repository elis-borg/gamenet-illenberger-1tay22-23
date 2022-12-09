using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicHighlight : MonoBehaviour
{
  //code by Andrew Thawley, slightly modified
  private bool Key;
  private Camera camera;
  public Color ogColor,
                newColor;
  public GameObject animal;

  public float range;

  void Start()
  {
    camera = GetComponent<PlayerSetup>().camera; 
    range = this.gameObject.GetComponent<Shooting>().gunRange;
  }

  void HighlightAnimal() {
      RaycastHit hit;
      //Ray forwardRay = new Ray (transform.position, transform.forward);
      Ray ray = camera.ViewportPointToRay(new Vector3(0.5F, 0.5F));


      if (Physics.Raycast (ray, out hit, range)) {
        Debug.DrawRay(camera.transform.position, camera.transform.forward * hit.distance, Color.yellow); //u can only view this in scene!

        if(hit.transform.gameObject.CompareTag("Mimic")) { //if a mimic and animal is empty then take the animal and store its original color before highlighting it
          if(animal == null) {
              animal = hit.transform.gameObject;
              //highlightedColor = new Color(hit.transform.GetComponent<Renderer>().material.color.r, hit.transform.GetComponent<Renderer>().material.color.g, hit.transform.GetComponent<Renderer>().material.color.b);
              if(hit.transform.childCount > 0){
                ogColor = hit.transform.GetComponent<SkinnedMeshRenderer>().material.color;
                hit.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_Color", newColor);
                Key = true;
              }
              else{
                ogColor = hit.transform.GetComponent<Renderer>().material.color;
                hit.transform.GetComponent<Renderer>().material.SetColor("_Color", newColor);
                Key = true;
              }
          }
        }
        else { //if not a mimic and animal not empty then, return to normal color
            if(animal != null) {
              if(hit.transform.childCount > 0){
                animal.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_Color", ogColor);
                animal = null;
              }
              else{
                animal.GetComponent<Renderer>().material.SetColor("_Color", ogColor); //gains an error sometimes
                animal = null;
              }
             }
             // Hitting something else.
             Key = false;
        }
      }
      else if (Key == true)
           {
               // not anymore.
               if(animal != null) {
                 if(hit.transform.childCount > 0){
                   animal.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_Color", ogColor);
                   animal = null;
                 }
                 else{
                   animal.GetComponent<Renderer>().material.SetColor("_Color", ogColor);
                   animal = null;
                 }
               }
               Debug.Log("Lost enemy");
               Key = false;
           }
  }

  // Update is called once per frame
  void Update()
  {
      HighlightAnimal();
  }
}
