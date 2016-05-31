using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class MiniMap : MonoBehaviour, IPointerDownHandler
{

    private Vector3 clickMousePos;
    private Vector3 clickPos;
    private bool mouseDown;
    public GameObject itemPrefab;
    public Transform ParentPanel;

    public void OnPointerDown(PointerEventData ped)
    {

        Vector3 localHit = transform.InverseTransformPoint(ped.pressPosition);
        Debug.Log("Localhit" + localHit);
        clickPos = new Vector3(transform.position.x + 256, 0, 0) ;
        clickMousePos = new Vector3((2000/256)*localHit.x, Camera.main.transform.position.y, (2000/256)*localHit.y);

        Camera.main.transform.position = clickMousePos;
    }





}
