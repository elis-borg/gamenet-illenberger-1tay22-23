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
  GameObject animal;

  void Start()
  {
    camera = transform.Find("Camera").GetComponent<Camera>();
  }

  void RayCastChangeColorOnHit() {
      RaycastHit hit;
      //Ray forwardRay = new Ray (transform.position, transform.forward);
      Ray forwardRay = camera.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));

      if (Physics.Raycast (forwardRay, out hit, 50.0f)) {
        if(hit.transform.gameObject.CompareTag("Mimic")) {
          if(animal == null) {
              animal = hit.transform.gameObject;
              //highlightedColor = new Color(hit.transform.GetComponent<Renderer>().material.color.r, hit.transform.GetComponent<Renderer>().material.color.g, hit.transform.GetComponent<Renderer>().material.color.b);
              if(hit.transform.childCount > 0){
                ogColor = hit.transform.GetChild(0).GetComponent<Renderer>().material.color;
                hit.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", newColor);
                Key = true;
              }
              else{
                ogColor = hit.transform.GetComponent<Renderer>().material.color;
                hit.transform.GetComponent<Renderer>().material.SetColor("_Color", newColor);
                Key = true;
              }
          }
        }
        else {
            if(animal != null) {
              if(hit.transform.childCount > 0){
                animal.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", ogColor);
                animal = null;
              }
              else{
                animal.GetComponent<Renderer>().material.SetColor("_Color", ogColor);
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
                   animal.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", ogColor);
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
      RayCastChangeColorOnHit();
  }
}
