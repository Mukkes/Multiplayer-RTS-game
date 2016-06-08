using UnityEngine;
using System.Collections.Generic;
using RTS;
using UnityEngine.Networking;

public class Building : WorldObject
{
	[SyncVar]
	private bool needsBuilding = false;
	[SyncVar]
	public bool isTempBuilding = true;
	
	public AudioClip finishedJobSound;
	public float finishedJobVolume = 1.0f;
	public Texture2D rallyPointImage;
	public float maxBuildProgress;
	public Texture2D sellImage;
	
	protected Queue<string> buildQueue;
	public Vector3 rallyPoint;

	private float currentBuildProgress = 0.0f;
	public Vector3 spawnPoint;

	protected override void Awake()
	{
		base.Awake();
		buildQueue = new Queue<string>();
		SetStartAndRallyPoint();
		hitPoints = 0;
	}

	protected override void Start()
	{
		base.Start();
	}

	protected override void Update()
	{
		base.Update();
		ProcessBuildQueue();
	}

	protected override void DrawSelection()
	{
		base.DrawSelection();
		if (needsBuilding) DrawBuildProgress();
	}

	private void SetStartAndRallyPoint()
	{
		float spawnX = selectionBounds.center.x + transform.forward.x * selectionBounds.extents.x + transform.forward.x * 10;
		float spawnZ = selectionBounds.center.z + transform.forward.z + selectionBounds.extents.z + transform.forward.z * 10;
		spawnPoint = new Vector3(spawnX, 0.0f, spawnZ);
		rallyPoint = spawnPoint;
	}
	
	private void DrawBuildProgress()
	{
		GUI.skin = ResourceManager.SelectBoxSkin;
		Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);
		//Draw the selection box around the currently selected object, within the bounds of the main draw area
		GUI.BeginGroup(playingArea);
		CalculateCurrentHealth(0.5f, 0.99f);
		DrawHealthBar(selectBox, "Building ...");
		GUI.EndGroup();
	}

	[Command]
	private void CmdSetNeedsBuilding(bool needsBuilding)
	{
		this.needsBuilding = needsBuilding;
	}

	protected void CreateUnit(string unitName)
	{
		GameObject unit = ResourceManager.GetUnit(unitName);
		Unit unitObject = unit.GetComponent<Unit>();
		if (player && unitObject) player.RemoveResource(ResourceType.Money, unitObject.cost);
		buildQueue.Enqueue(unitName);
	}

	protected void ProcessBuildQueue()
	{
		if (buildQueue.Count > 0)
		{
			currentBuildProgress += Time.deltaTime * ResourceManager.BuildSpeed;
			if (currentBuildProgress > maxBuildProgress)
			{
				if (player)
				{
					if (audioElement != null) audioElement.Play(finishedJobSound);
					int worldObjectId = PlayerManager.GetUniqueWorldObjectId();
					PlayerCreateUnit(worldObjectId);
				}
				currentBuildProgress = 0.0f;
			}
		}
	}

	protected virtual void PlayerCreateUnit(int worldObjectId)
	{
		player.CmdAddUnit(worldObjectId, buildQueue.Dequeue(), spawnPoint, rallyPoint, transform.rotation, id);
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

	public string[] getBuildQueueValues()
	{
		string[] values = new string[buildQueue.Count];
		int pos = 0;
		foreach (string unit in buildQueue) values[pos++] = unit;
		return values;
	}

	public float getBuildPercentage()
	{
		return currentBuildProgress / maxBuildProgress;
	}

	public override void SetSelection(bool selected, Rect playingArea)
	{
		base.SetSelection(selected, playingArea);
		if (player)
		{
			RallyPoint flag = player.GetComponentInChildren<RallyPoint>();
			if (selected)
			{
				if (flag && player.human && player.isLocalPlayer && spawnPoint != ResourceManager.InvalidPosition && rallyPoint != ResourceManager.InvalidPosition)
				{
					flag.transform.localPosition = rallyPoint;
					flag.transform.forward = transform.forward;
					flag.Enable();
				}
			}
			else {
				if (flag && player.human && player.isLocalPlayer) flag.Disable();
			}
		}
	}

	public bool hasSpawnPoint()
	{
		return spawnPoint != ResourceManager.InvalidPosition && rallyPoint != ResourceManager.InvalidPosition;
	}

	public override void SetHoverState(GameObject hoverObject)
	{
		base.SetHoverState(hoverObject);
		//only handle input if owned by a human player and currently selected
		if (player && player.human && player.isLocalPlayer && currentlySelected)
		{
			if (WorkManager.ObjectIsGround(hoverObject))
			{
				if (player.hud.GetPreviousCursorState() == CursorState.RallyPoint) player.hud.SetCursorState(CursorState.RallyPoint);
			}
		}
	}

	public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller)
	{
		base.MouseClick(hitObject, hitPoint, controller);
		//only handle iput if owned by a human player and currently selected
		if (player && player.human && player.isLocalPlayer && currentlySelected)
		{
			if (WorkManager.ObjectIsGround(hitObject))
			{
				if ((player.hud.GetCursorState() == CursorState.RallyPoint || player.hud.GetPreviousCursorState() == CursorState.RallyPoint) && hitPoint != ResourceManager.InvalidPosition)
				{
					SetRallyPoint(hitPoint);
				}
			}
		}
	}

	public void SetRallyPoint(Vector3 position)
	{
		rallyPoint = position;
		if (player && player.human && player.isLocalPlayer && currentlySelected)
		{
			RallyPoint flag = player.GetComponentInChildren<RallyPoint>();
			if (flag) flag.transform.localPosition = rallyPoint;
		}
	}

	public void Sell()
	{
		if (player) player.AddResource(ResourceType.Money, sellValue);
		if (currentlySelected) SetSelection(false, playingArea);
		Destroy(this.gameObject);
	}

	public void StartConstruction()
	{
		CalculateBounds();
		SetStartAndRallyPoint();
		CmdSetNeedsBuilding(true);
		hitPoints = 0;
	}

	public bool UnderConstruction()
	{
		return needsBuilding;
	}

	public void Construct(int amount)
	{
		CmdSetHitPoints(hitPoints + amount);
		if (hitPoints >= maxHitPoints)
		{
			CmdSetHitPoints(maxHitPoints);
			CmdSetNeedsBuilding(false);
			CmdSetIsTempBuilding(false);
			RestoreMaterials();
			SetTeamColor();
		}
	}

	public override void SetParent()
	{
		Buildings buildings = player.GetComponentInChildren<Buildings>();
		transform.parent = buildings.transform;
	}

	[Command]
	public void CmdSetIsTempBuilding(bool isTempBuilding)
	{
		this.isTempBuilding = isTempBuilding;
	}
}
