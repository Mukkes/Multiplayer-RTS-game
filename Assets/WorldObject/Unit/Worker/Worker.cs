using RTS;
using UnityEngine;
using System.Collections.Generic;

public class Worker : Unit
{
	public AudioClip finishedJobSound;
	public float finishedJobVolume = 1.0f;
	public int buildSpeed;

	private int currentProjectId = -1;
	private Building currentProject;
	private bool building = false;
	private float amountBuilt = 0.0f;

	/*** Game Engine methods, all can be overridden by subclass ***/

	protected override void Awake()
	{
		base.Awake();
		objectName = "Worker";
		hitPoints = 50;
		maxHitPoints = 50;
		cost = 50;
		sellValue = 25;
	}

	protected override void Start()
	{
		base.Start();
		actions = new string[] { "Refinery", "WarFactory", "Turrent", "Wonder", "FarmHouse", "Barracks", "Headquarter" };
	}

	protected override void Update()
	{
		base.Update();
		if ((currentProjectId >= 0) && (!currentProject))
		{
			SetBuilding();
		}
		else if (!moving && !rotating)
		{
			if (building && currentProject)
			{
				if (currentProject.UnderConstruction())
				{
					amountBuilt += buildSpeed * Time.deltaTime;
					int amount = Mathf.FloorToInt(amountBuilt);
					if (amount > 0)
					{
						amountBuilt -= amount;
						currentProject.Construct(amount);
					}
				}
				else
				{
					if (audioElement != null) audioElement.Play(finishedJobSound);
					building = false;
					currentProjectId = -1;
					currentProject = null;
				}
			}
		}
	}

	/*** Public Methods ***/

	public override void PerformAction(string actionToPerform)
	{
		base.PerformAction(actionToPerform);
		CreateBuilding(actionToPerform);
	}

	public override void StartMove(Vector3 destination)
	{
		base.StartMove(destination);
		amountBuilt = 0.0f;
		building = false;
	}

	private void CreateBuilding(string buildingName)
	{
		Vector3 buildPoint = new Vector3(transform.position.x, transform.position.y, transform.position.z + 10);
		int worldObjectId = PlayerManager.GetUniqueWorldObjectId();
		if (player) player.CreateBuilding(worldObjectId, buildingName, buildPoint, this, playingArea);
	}

	public override void SetBuildingId(int buildingId)
	{
		currentProjectId = buildingId;
		currentProject = null;
	}

	private void SetBuilding()
	{
		Building building = PlayerManager.FindBuilding(playerId, currentProjectId);
		if (building)
		{
			currentProject = building;
			StartMove(currentProject.transform.position, currentProject.gameObject);
			this.building = true;
			currentProjectId = -1;
		}
	}

	public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller)
	{
		bool doBase = true;
		//only handle input if owned by a human player and currently selected
		if (player && player.human && player.isLocalPlayer && currentlySelected && !WorkManager.ObjectIsGround(hitObject))
		{
			Building building = hitObject.transform.parent.GetComponent<Building>();
			if (building)
			{
				if (building.UnderConstruction())
				{
					SetBuildingId(building.id);
					doBase = false;
				}
			}
		}
		if (doBase)
		{
			base.MouseClick(hitObject, hitPoint, controller);
		}
	}

	protected override void InitialiseAudio()
	{
		base.InitialiseAudio();
		if (finishedJobVolume < 0.0f) finishedJobVolume = 0.0f;
		if (finishedJobVolume > 1.0f) finishedJobVolume = 1.0f;
		List<AudioClip> sounds = new List<AudioClip>();
		List<float> volumes = new List<float>();
		sounds.Add(finishedJobSound);
		volumes.Add(finishedJobVolume);
		audioElement.Add(sounds, volumes);
	}

	protected override bool ShouldMakeDecision()
	{
		if (building) return false;
		return base.ShouldMakeDecision();
	}

	protected override void DecideWhatToDo()
	{
		base.DecideWhatToDo();
		List<WorldObject> buildings = new List<WorldObject>();
		foreach (WorldObject nearbyObject in nearbyObjects)
		{
			if (nearbyObject.GetPlayer() != player) continue;
			Building nearbyBuilding = nearbyObject.GetComponent<Building>();
			if (nearbyBuilding && nearbyBuilding.UnderConstruction()) buildings.Add(nearbyObject);
		}
		WorldObject nearestObject = WorkManager.FindNearestWorldObjectInListToPosition(buildings, transform.position);
		if (nearestObject)
		{
			Building closestBuilding = nearestObject.GetComponent<Building>();
			if (closestBuilding) SetBuildingId(closestBuilding.id);
		}
	}
}