using UnityEngine;
using System.Collections;

public class MiniMap : MonoBehaviour {
    private Camera miniMap;
    RaycastHit hit;
    // Use this for initialization
    void Start () {
        miniMap = GameObject.FindWithTag("MiniMapCamera").GetComponent<Camera>();
    }
	
	// Update is called once per frame
	void Update () {
        //klikken kan, maar de cords zijn van de gewone map en niet van de mini map, de cords zijn fcked up.
        if (miniMap.pixelRect.Contains(Input.mousePosition) && Input.GetMouseButtonDown(0))
        {
            Ray MouseRay = miniMap.ScreenPointToRay(Input.mousePosition);
            Debug.Log("clicked position:" + MouseRay.origin);
            Camera.main.transform.position = MouseRay.origin;
        }

      
        


    }


 
    
}
