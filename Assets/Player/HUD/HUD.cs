using UnityEngine;
using System.Collections;
using RTS;

public class HUD : MonoBehaviour
{

	private const int ORDERS_BAR_WIDTH = 150;
	private const int RESOURCE_BAR_HEIGHT = 40;
	private const int SELECTION_NAME_HEIGHT = 21;

	private Player player;

	public GUISkin resourceSkin, ordersSkin, selectBoxSkin;

	// Use this for initialization
	void Start()
	{
		player = transform.root.GetComponent<Player>();
		ResourceManager.StoreSelectBoxItems(selectBoxSkin);
	}

	// Update is called once per frame
	void OnGUI()
	{
		if (player && player.human)
		{
			DrawOrdersBar();
			DrawResourceBar();
		}
	}

	private void DrawOrdersBar()
	{
		GUI.skin = ordersSkin;
		GUI.BeginGroup(new Rect(Screen.width - ORDERS_BAR_WIDTH, RESOURCE_BAR_HEIGHT, ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(0, 0, ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT), "");

		string selectionName = "";
		if (player.SelectedObject)
		{
			selectionName = player.SelectedObject.objectName;
		}
		if (!selectionName.Equals(""))
		{
			GUI.Label(new Rect(0, 10, ORDERS_BAR_WIDTH, SELECTION_NAME_HEIGHT), selectionName);
		}

		GUI.EndGroup();
	}

	private void DrawResourceBar()
	{
		GUI.skin = resourceSkin;
		GUI.BeginGroup(new Rect(0, 0, Screen.width, RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(0, 0, Screen.width, RESOURCE_BAR_HEIGHT), "");
		GUI.EndGroup();
	}

	public bool MouseInBounds()
	{
		//Screen coordinates start in the lower-left corner of the screen
		//not the top-left of the screen like the drawing coordinates do
		Vector3 mousePos = Input.mousePosition;
		bool insideWidth = mousePos.x >= 0 && mousePos.x <= Screen.width - ORDERS_BAR_WIDTH;
		bool insideHeight = mousePos.y >= 0 && mousePos.y <= Screen.height - RESOURCE_BAR_HEIGHT;
		return insideWidth && insideHeight;
	}

	public Rect GetPlayingArea()
	{
		return new Rect(0, RESOURCE_BAR_HEIGHT, Screen.width - ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT);
	}
}
