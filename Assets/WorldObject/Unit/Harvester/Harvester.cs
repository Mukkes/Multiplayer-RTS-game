using UnityEngine;
using RTS;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Harvester : Unit
{

	public float capacity;
	public int resourceStoreId = -1;
	public Building resourceStore;
	public float collectionAmount, depositAmount;
	public AudioClip emptyHarvestSound, harvestSound, startHarvestSound;
	public float emptyHarvestVolume = 0.5f, harvestVolume = 0.5f, startHarvestVolume = 1.0f;

	private float currentDeposit = 0.0f;
	private bool harvesting = false, emptying = false;
	private float currentLoad = 0.0f;
	private ResourceType harvestType;
	private Resource resourceDeposit;

	/*** Game Engine methods, all can be overridden by subclass ***/

	protected override void Start()
	{
		base.Start();
		harvestType = ResourceType.Unknown;
	}

	protected override void Update()
	{
		base.Update();
		if ((resourceStoreId >= 0) && (!resourceStore))
		{
			SetBuilding();
		}
		else if (!rotating && !moving)
		{
			if (harvesting || emptying)
			{
				Arms[] arms = GetComponentsInChildren<Arms>();
				foreach (Arms arm in arms) arm.GetComponent<Renderer>().enabled = true;
				if (harvesting)
				{
					Collect();
					if ((currentLoad >= capacity) && (resourceStore))
					{
						//make sure that we have a whole number to avoid bugs
						//caused by floating point numbers
						currentLoad = Mathf.Floor(currentLoad);
						harvesting = false;
						emptying = true;
						foreach (Arms arm in arms) arm.GetComponent<Renderer>().enabled = false;
						StartMove(resourceStore.transform.position, resourceStore.gameObject);
					}
				}
				else {
					Deposit();
					if (currentLoad <= 0)
					{
						emptying = false;
						foreach (Arms arm in arms) arm.GetComponent<Renderer>().enabled = false;
						if (!resourceDeposit.isEmpty())
						{
							harvesting = true;
							StartMove(resourceDeposit.transform.position, resourceDeposit.gameObject);
						}
					}
				}
			}
		}
	}

	/* Public Methods */

	public override void SetHoverState(GameObject hoverObject)
	{
		base.SetHoverState(hoverObject);
		//only handle input if owned by a human player and currently selected
		if (player && player.human && player.isLocalPlayer && currentlySelected)
		{
			if (!WorkManager.ObjectIsGround(hoverObject))
			{
				Resource resource = hoverObject.transform.parent.GetComponent<Resource>();
				if (resource && !resource.isEmpty()) player.hud.SetCursorState(CursorState.Harvest);
			}
		}
	}

	public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller)
	{
		base.MouseClick(hitObject, hitPoint, controller);
		//only handle input if owned by a human player
		if (player && player.human && player.isLocalPlayer)
		{
			if (!WorkManager.ObjectIsGround(hitObject))
			{
				Resource resource = hitObject.transform.parent.GetComponent<Resource>();
				if (resource && !resource.isEmpty())
				{
					//make sure that we select harvester remains selected
					if (player.SelectedObject) player.SelectedObject.SetSelection(false, playingArea);
					SetSelection(true, playingArea);
					player.SelectedObject = this;
					StartHarvest(resource);
				}
			}
			else StopHarvest();
		}
	}

	/* Private Methods */

	private void StartHarvest(Resource resource)
	{
		if (audioElement != null) audioElement.Play(startHarvestSound);
		resourceDeposit = resource;
		StartMove(resource.transform.position, resource.gameObject);
		//we can only collect one resource at a time, other resources are lost
		if (harvestType == ResourceType.Unknown || harvestType != resource.GetResourceType())
		{
			harvestType = resource.GetResourceType();
			currentLoad = 0.0f;
		}
		harvesting = true;
		emptying = false;
	}

	private void StopHarvest()
	{

	}

	private void Collect()
	{
		if (audioElement != null && Time.timeScale > 0) audioElement.Play(harvestSound);
		float collect = collectionAmount * Time.deltaTime;
		//make sure that the harvester cannot collect more than it can carry
		if (currentLoad + collect > capacity) collect = capacity - currentLoad;
		if (resourceDeposit.isEmpty())
		{
			Arms[] arms = GetComponentsInChildren<Arms>();
			foreach (Arms arm in arms) arm.GetComponent<Renderer>().enabled = false;
			DecideWhatToDo();
		}
		else {
			resourceDeposit.Remove(collect);
		}
		currentLoad += collect;
	}

	private void Deposit()
	{
		currentLoad = Mathf.Floor(currentLoad);
		if (audioElement != null && Time.timeScale > 0) audioElement.Play(emptyHarvestSound);
		currentDeposit += depositAmount * Time.deltaTime;
		int deposit = Mathf.FloorToInt(currentDeposit);
		if (deposit >= 1)
		{
			if (deposit > currentLoad) deposit = Mathf.FloorToInt(currentLoad);
			currentDeposit -= deposit;
			currentLoad -= deposit;
			ResourceType depositType = harvestType;
			if (harvestType == ResourceType.Ore) depositType = ResourceType.Money;
			player.AddResource(depositType, deposit);
		}
	}

	protected override void DrawSelectionBox(Rect selectBox)
	{
		base.DrawSelectionBox(selectBox);
		float percentFull = currentLoad / capacity;
		float maxHeight = selectBox.height - 4;
		float height = maxHeight * percentFull;
		float leftPos = selectBox.x + selectBox.width - 7;
		float topPos = selectBox.y + 2 + (maxHeight - height);
		float width = 5;
		Texture2D resourceBar = ResourceManager.GetResourceHealthBar(harvestType);
		if (resourceBar) GUI.DrawTexture(new Rect(leftPos, topPos, width, height), resourceBar);
	}

	public override void SetBuildingId(int buildingId)
	{
		base.SetBuildingId(buildingId);
		resourceStoreId = buildingId;
	}

	public void SetBuilding()
	{
		Building creator = PlayerManager.FindBuilding(playerId, resourceStoreId);
		if (creator)
		{
			resourceStore = creator;
			resourceStoreId = -1;
			Debug.Log("Harvester: resourceStore is set.");
		}
	}

	protected override void InitialiseAudio()
	{
		base.InitialiseAudio();
		List<AudioClip> sounds = new List<AudioClip>();
		List<float> volumes = new List<float>();
		if (emptyHarvestVolume < 0.0f) emptyHarvestVolume = 0.0f;
		if (emptyHarvestVolume > 1.0f) emptyHarvestVolume = 1.0f;
		sounds.Add(emptyHarvestSound);
		volumes.Add(emptyHarvestVolume);
		if (harvestVolume < 0.0f) harvestVolume = 0.0f;
		if (harvestVolume > 1.0f) harvestVolume = 1.0f;
		sounds.Add(harvestSound);
		volumes.Add(harvestVolume);
		if (startHarvestVolume < 0.0f) startHarvestVolume = 0.0f;
		if (startHarvestVolume > 1.0f) startHarvestVolume = 1.0f;
		sounds.Add(startHarvestSound);
		volumes.Add(startHarvestVolume);
		audioElement.Add(sounds, volumes);
	}

	protected override bool ShouldMakeDecision()
	{
		if (harvesting || emptying) return false;
		return base.ShouldMakeDecision();
	}

	protected override void DecideWhatToDo()
	{
		base.DecideWhatToDo();
		List<WorldObject> resources = new List<WorldObject>();
		foreach (WorldObject nearbyObject in nearbyObjects)
		{
			Resource resource = nearbyObject.GetComponent<Resource>();
			if (resource && !resource.isEmpty()) resources.Add(nearbyObject);
		}
		WorldObject nearestObject = WorkManager.FindNearestWorldObjectInListToPosition(resources, transform.position);
		if (nearestObject)
		{
			Resource closestResource = nearestObject.GetComponent<Resource>();
			if (closestResource) StartHarvest(closestResource);
		}
		else if (harvesting)
		{
			harvesting = false;
			if ((currentLoad > 0.0f) && (resourceStore))
			{
				//make sure that we have a whole number to avoid bugs
				//caused by floating point numbers
				currentLoad = Mathf.Floor(currentLoad);
				emptying = true;
				Arms[] arms = GetComponentsInChildren<Arms>();
				foreach (Arms arm in arms) arm.GetComponent<Renderer>().enabled = false;
				StartMove(resourceStore.transform.position, resourceStore.gameObject);
			}
		}
	}
}