using UnityEngine;
using System.Collections.Generic;
using RTS;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
	[SyncVar]
	public int id = -1;
	[SyncVar]
	private bool findingPlacement = false;
	[SyncVar]
	private int tempBuildingId = -1;
	[SyncVar]
	private int tempCreatorId = -1;
	[SyncVar]
	public string username;

	public int startMoney, startMoneyLimit, startPower, startPowerLimit;
	public bool human;
	public HUD hud;
	public Material notAllowedMaterial, allowedMaterial;
	public Color teamColor;
	// Check if network things has been done.
	public bool handleNetwork = true;
	
	private Dictionary<ResourceType, int> resources, resourceLimits;
	private Building tempBuilding;
	private Unit tempCreator;
	
	public WorldObject SelectedObject
	{
		get;
		set;
	}

	void Awake()
	{
		resources = InitResourceList();
		resourceLimits = InitResourceList();
	}

	// Use this for initialization
	void Start()
	{
		if (handleNetwork)
			HandleNetwork();
		hud = GetComponentInChildren<HUD>();
		AddStartResourceLimits();
		AddStartResources();
		if (isLocalPlayer)
		{
			CmdSetUsername(PlayerManager.GetPlayerName());
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (handleNetwork)
			HandleNetwork();
		
		if (human && isLocalPlayer)
		{
			hud.SetResourceValues(resources, resourceLimits);
			if ((tempBuildingId >= 0) && (!tempBuilding))
			{
				SetTempBuilding();
			}
			else if ((tempCreatorId >= 0) && (!tempCreator))
			{
				SetTempCreator();
			}
			else if (findingPlacement)
			{
				tempBuilding.CalculateBounds();
				if (CanPlaceBuilding()) tempBuilding.SetTransparentMaterial(allowedMaterial, false);
				else tempBuilding.SetTransparentMaterial(notAllowedMaterial, false);
			}
		}
	}

	private void HandleNetwork()
	{
		if (id >= 0)
		{
			teamColor = PlayerManager.GetTeamColor(id);
			handleNetwork = false;
		}
	}

	private void SetTempBuilding()
	{
		Building building = PlayerManager.FindBuilding(id, tempBuildingId);
		if (building)
		{
			tempBuilding = building;
			tempBuildingId = -1;
			tempBuilding.SetTransparentMaterial(notAllowedMaterial, true);
			tempBuilding.SetColliders(false);
		}
	}

	private void SetTempCreator()
	{
		Unit unit = PlayerManager.FindUnit(id, tempCreatorId);
		if (unit)
		{
			tempCreator = unit;
			tempCreatorId = -1;
		}
	}

	private Dictionary<ResourceType, int> InitResourceList()
	{
		Dictionary<ResourceType, int> list = new Dictionary<ResourceType, int>();
		list.Add(ResourceType.Money, 0);
		list.Add(ResourceType.Power, 0);
		return list;
	}

	private void AddStartResourceLimits()
	{
		IncrementResourceLimit(ResourceType.Money, startMoneyLimit);
		IncrementResourceLimit(ResourceType.Power, startPowerLimit);
	}

	private void AddStartResources()
	{
		AddResource(ResourceType.Money, startMoney);
		AddResource(ResourceType.Power, startPower);
	}

	[Command]
	private void CmdSetFindingPlacement(bool findingPlacement)
	{
		this.findingPlacement = findingPlacement;
	}

	[Command]
	private void CmdSetUsername(string username)
	{
		this.username = username;
	}

	public void SetId(int id)
	{
		this.id = id;
	}

	public void AddResource(ResourceType type, int amount)
	{
		resources[type] += amount;
	}

	public void IncrementResourceLimit(ResourceType type, int amount)
	{
		resourceLimits[type] += amount;
	}
	
	public void AddUnit(int unitId, string unitName, Vector3 spawnPoint, Quaternion rotation)
	{
		AddUnit(unitId, unitName, spawnPoint, rotation, -1);
	}
	
	public void AddUnit(int unitId, string unitName, Vector3 spawnPoint, Quaternion rotation, int creator)
	{
		CmdAddUnit(unitId, unitName, spawnPoint, spawnPoint, rotation, creator);
	}

	[Command]
	public void CmdAddUnit(int unitId, string unitName, Vector3 spawnPoint, Vector3 rallyPoint, Quaternion rotation, int creator)
	{
		GameObject newUnit = (GameObject)Instantiate(ResourceManager.GetUnit(unitName), spawnPoint, rotation);
		Unit unitObject = newUnit.GetComponent<Unit>();
		if (unitObject)
		{
			unitObject.SetId(unitId);
			unitObject.SetPlayerId(id);
			unitObject.SetBuildingId(creator);
			if (spawnPoint != rallyPoint) unitObject.SetDestination(rallyPoint);
			NetworkServer.SpawnWithClientAuthority(newUnit, connectionToClient);
		}
		else Destroy(newUnit);
	}

	public void CreateBuilding(int buildingId, string buildingName, Vector3 buildPoint, Unit creator, Rect playingArea)
	{
		if (findingPlacement) CancelBuildingPlacement();
		CmdCreateBuilding(buildingId, buildingName, buildPoint, creator.id, playingArea);
	}

	[Command]
	public void CmdCreateBuilding(int buildingId, string buildingName, Vector3 buildPoint, int creatorId, Rect playingArea)
	{
		GameObject newBuilding = (GameObject)Instantiate(ResourceManager.GetBuilding(buildingName), buildPoint, new Quaternion());
		Building tempBuilding = newBuilding.GetComponent<Building>();
		if (tempBuilding)
		{
			tempBuildingId = buildingId;
			tempCreatorId = creatorId;
			findingPlacement = true;
			tempBuilding.SetId(buildingId);
			tempBuilding.SetPlayerId(id);
			tempBuilding.hitPoints = 0;
			tempBuilding.SetColliders(true);
			tempBuilding.SetPlayingArea(playingArea);
			NetworkServer.SpawnWithClientAuthority(newBuilding, connectionToClient);
		}
		else Destroy(newBuilding);
	}

	public bool IsFindingBuildingLocation()
	{
		return findingPlacement;
	}

	public void FindBuildingLocation()
	{
		Vector3 newLocation = WorkManager.FindHitPoint(Input.mousePosition);
		newLocation.y = 0;
		tempBuilding.transform.position = newLocation;
	}

	public bool CanPlaceBuilding()
	{
		Bounds placeBounds = tempBuilding.GetSelectionBounds();
		//shorthand for the coordinates of the center of the selection bounds
		float cx = placeBounds.center.x;
		float cy = placeBounds.center.y;
		float cz = placeBounds.center.z;
		//shorthand for the coordinates of the extents of the selection box
		float ex = placeBounds.extents.x;
		float ey = placeBounds.extents.y;
		float ez = placeBounds.extents.z;

		//Determine the screen coordinates for the corners of the selection bounds
		List<Vector3> corners = new List<Vector3>();
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy + ey, cz + ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy + ey, cz - ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy - ey, cz + ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy + ey, cz + ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy - ey, cz - ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy - ey, cz + ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy + ey, cz - ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy - ey, cz - ez)));

		foreach (Vector3 corner in corners)
		{
			GameObject hitObject = WorkManager.FindHitObject(corner);
			if (hitObject && !WorkManager.ObjectIsGround(hitObject))
			{
				WorldObject worldObject = hitObject.transform.parent.GetComponent<WorldObject>();
				if (worldObject && placeBounds.Intersects(worldObject.GetSelectionBounds()))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void StartConstruction()
	{
		findingPlacement = false;
		CmdSetFindingPlacement(false);
		tempBuilding.SetColliders(true);
		tempCreator.SetBuildingId(tempBuilding.id);
		tempBuilding.StartConstruction();
		RemoveResource(ResourceType.Money, tempBuilding.cost);
		tempBuildingId = -1;
		tempBuilding = null;
	}

	public void CancelBuildingPlacement()
	{
		findingPlacement = false;
		CmdSetFindingPlacement(false);
		CmdDestroy(tempBuilding.gameObject);
		tempBuildingId = -1;
		tempBuilding = null;
		tempCreatorId = -1;
		tempCreator = null;
	}

	[Command]
	public void CmdDestroy(GameObject gameObject)
	{
		NetworkServer.Destroy(gameObject);
	}
	
	public bool IsDead()
	{
		Building[] buildings = GetComponentsInChildren<Building>();
		Unit[] units = GetComponentsInChildren<Unit>();
		if (buildings != null && buildings.Length > 0) return false;
		if (units != null && units.Length > 0) return false;
		return true;
	}

	public int GetResourceAmount(ResourceType type)
	{
		return resources[type];
	}

	public void RemoveResource(ResourceType type, int amount)
	{
		resources[type] -= amount;
	}

	public Building GetTempBuilding()
	{
		return tempBuilding;
	}
}
