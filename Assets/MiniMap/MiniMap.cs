using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class MiniMap : MonoBehaviour, IPointerDownHandler {

    private Vector3 clickMousePos;
    private Vector3 clickPos;
  

    public void OnPointerDown(PointerEventData ped)
    {
        Vector3 localHit = transform.InverseTransformPoint(ped.pressPosition);
        Debug.Log("Localhit" + localHit);
        clickPos = new Vector3(transform.position.x + 256, 0, 0);
        clickMousePos = new Vector3((2000 / 256) * localHit.x + 50, Camera.main.transform.position.y, (2000 / 256) * localHit.y - 20);

        Camera.main.transform.position = clickMousePos;
    }
}
