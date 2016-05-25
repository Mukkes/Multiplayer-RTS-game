using UnityEngine;
using System.Collections;
using RTS;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Networking;

public abstract class WorldObject : NetworkBehaviour
{
	[SyncVar]
	public int hitPoints;
	[SyncVar]
	public int id = -1;
	[SyncVar]
	public int playerId = -1;

	public string objectName;
	public Texture2D buildImage;
	public int cost;
	public int sellValue;
	public int maxHitPoints;
	public Player player;
	public GameObject playerObject;
	public float weaponRange = 10.0f;
	public float weaponRechargeTime = 1.0f;
	public float weaponAimSpeed = 1.0f;
	public AudioClip attackSound, selectSound, useWeaponSound;
	public float attackVolume = 1.0f, selectVolume = 1.0f, useWeaponVolume = 1.0f;
	public float detectionRange = 20.0f;
	// Check if network things has been done.
	public bool handleNetwork = true;
	
	protected AudioElement audioElement;
	protected string[] actions = { };
	protected bool currentlySelected = false;
	protected Bounds selectionBounds;
	protected Rect playingArea = new Rect(0.0f, 0.0f, 0.0f, 0.0f);
	protected GUIStyle healthStyle = new GUIStyle();
	protected float healthPercentage = 1.0f;
	protected WorldObject target = null;
	protected bool attacking = false;
	protected bool movingIntoPosition = false;
	protected bool aiming = false;
	protected List<WorldObject> nearbyObjects;

	private float currentWeaponChargeTime;
	private List<Material> oldMaterials = new List<Material>();
	//we want to restrict how many decisions are made to help with game performance
	//the default time at the moment is a tenth of a second
	private float timeSinceLastDecision = 0.0f, timeBetweenDecisions = 0.1f;

	protected virtual void Awake()
	{
		selectionBounds = ResourceManager.InvalidBounds;
		CalculateBounds();
	}

	protected virtual void Start()
	{
		if (handleNetwork)
			HandleNetwork();
		
		InitialiseAudio();
	}

	protected virtual void Update()
	{
		if (handleNetwork)
		{
			HandleNetwork();
		}
		else if (player.isLocalPlayer && player.human)
		{
			if (ShouldMakeDecision()) DecideWhatToDo();
			currentWeaponChargeTime += Time.deltaTime;
			if (attacking && !movingIntoPosition && !aiming) PerformAttack();
		}
		if (!isLocalPlayer)
			CalculateBounds();
	}

	protected virtual void HandleNetwork()
	{
		if (playerId >= 0)
		{
			player = PlayerManager.FindPlayer(playerId);
			if (player != null)
			{
				SetPlayer();
				handleNetwork = false;
			}
		}
	}

	protected virtual void OnGUI()
	{
		if (currentlySelected && !ResourceManager.MenuOpen) DrawSelection();
	}

	public void SetId(int id)
	{
		this.id = id;
	}

	public virtual void SetSelection(bool selected, Rect playingArea)
	{
		currentlySelected = selected;
		if (selected)
		{
			this.playingArea = playingArea;
			if (audioElement != null) audioElement.Play(selectSound);
		}
	}

	public string[] GetActions()
	{
		return actions;
	}

	public virtual void PerformAction(string actionToPerform)
	{
		//it is up to children with specific actions to determine what to do with each of those actions
	}

	public virtual void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller)
	{
		//only handle input if currently selected
		if (currentlySelected && hitObject && !WorkManager.ObjectIsGround(hitObject))
		{
			WorldObject worldObject = hitObject.transform.parent.GetComponent<WorldObject>();
			//clicked on another selectable object
			if (worldObject)
			{
				Resource resource = hitObject.transform.parent.GetComponent<Resource>();
				if (resource && resource.isEmpty()) return;
				Player owner = hitObject.transform.root.GetComponent<Player>();
				if (owner)
				{ //the object is controlled by a player
					if (player && player.human && player.isLocalPlayer)
					{ //this object is controlled by a human player
					  //start attack if object is not owned by the same player and this object can attack, else select
						if (player.username != owner.username && CanAttack()) BeginAttack(worldObject);
						else ChangeSelection(worldObject, controller);
					}
					else ChangeSelection(worldObject, controller);
				}
				else ChangeSelection(worldObject, controller);
			}
		}
	}

	private void ChangeSelection(WorldObject worldObject, Player controller)
	{
		//this should be called by the following line, but there is an outside chance it will not
		SetSelection(false, playingArea);
		if (controller.SelectedObject) controller.SelectedObject.SetSelection(false, playingArea);
		controller.SelectedObject = worldObject;
		worldObject.SetSelection(true, controller.hud.GetPlayingArea());
	}

	protected virtual void DrawSelection()
	{
		GUI.skin = ResourceManager.SelectBoxSkin;
		Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);
		//Draw the selection box around the currently selected object, within the bounds of the playing area
		GUI.BeginGroup(playingArea);
		DrawSelectionBox(selectBox);
		GUI.EndGroup();
	}

	public void CalculateBounds()
	{
		selectionBounds = new Bounds(transform.position, Vector3.zero);
		foreach (Renderer r in GetComponentsInChildren<Renderer>())
		{
			selectionBounds.Encapsulate(r.bounds);
		}
	}

	[Command]
	protected void CmdSetHitPoints(int hitPoints)
	{
		this.hitPoints = hitPoints;
	}

	protected virtual void DrawSelectionBox(Rect selectBox)
	{
		GUI.Box(selectBox, "");
		CalculateCurrentHealth(0.35f, 0.65f);
		DrawHealthBar(selectBox, "");
	}

	protected virtual void CalculateCurrentHealth(float lowSplit, float highSplit)
	{
		healthPercentage = (float)hitPoints / (float)maxHitPoints;
		if (healthPercentage > highSplit) healthStyle.normal.background = ResourceManager.HealthyTexture;
		else if (healthPercentage > lowSplit) healthStyle.normal.background = ResourceManager.DamagedTexture;
		else healthStyle.normal.background = ResourceManager.CriticalTexture;
	}

	protected void DrawHealthBar(Rect selectBox, string label)
	{
		healthStyle.padding.top = -20;
		healthStyle.fontStyle = FontStyle.Bold;
		GUI.Label(new Rect(selectBox.x, selectBox.y - 7, selectBox.width * healthPercentage, 5), label, healthStyle);
	}

	public virtual void SetHoverState(GameObject hoverObject)
	{
		//only handle input if owned by a human player and currently selected
		if (player && player.human && player.isLocalPlayer && currentlySelected)
		{
			//something other than the ground is being hovered over
			if (!WorkManager.ObjectIsGround(hoverObject))
			{
				Player owner = hoverObject.transform.root.GetComponent<Player>();
				Unit unit = hoverObject.transform.parent.GetComponent<Unit>();
				Building building = hoverObject.transform.parent.GetComponent<Building>();
				if (owner)
				{ //the object is owned by a player
					if (owner.username == player.username) player.hud.SetCursorState(CursorState.Select);
					else if (CanAttack()) player.hud.SetCursorState(CursorState.Attack);
					else player.hud.SetCursorState(CursorState.Select);
				}
				else if (unit || building && CanAttack()) player.hud.SetCursorState(CursorState.Attack);
				else player.hud.SetCursorState(CursorState.Select);
			}
		}
	}

	public bool IsOwnedBy(Player owner)
	{
		if (player && player.Equals(owner))
		{
			return true;
		}
		else {
			return false;
		}
	}

	public Bounds GetSelectionBounds()
	{
		return selectionBounds;
	}

	public void SetColliders(bool enabled)
	{
		Collider[] colliders = GetComponentsInChildren<Collider>();
		foreach (Collider collider in colliders) collider.enabled = enabled;
	}

	public void SetTransparentMaterial(Material material, bool storeExistingMaterial)
	{
		if (storeExistingMaterial) oldMaterials.Clear();
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in renderers)
		{
			if (storeExistingMaterial) oldMaterials.Add(renderer.material);
			renderer.material = material;
		}
	}

	public void RestoreMaterials()
	{
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		if (oldMaterials.Count == renderers.Length)
		{
			for (int i = 0; i < renderers.Length; i++)
			{
				renderers[i].material = oldMaterials[i];
			}
		}
	}

	public void SetPlayingArea(Rect playingArea)
	{
		this.playingArea = playingArea;
	}

	public void SetPlayerId(int id)
	{
		playerId = id;
	}

	protected void SetPlayer()
	{
		SetParent();
		SetTeamColor();
	}

	public abstract void SetParent();

	public virtual bool CanAttack()
	{
		//default behaviour needs to be overidden by children
		return false;
	}

	public void SetTeamColor()
	{
		TeamColor[] teamColors = GetComponentsInChildren<TeamColor>();
		foreach (TeamColor teamColor in teamColors) teamColor.GetComponent<Renderer>().material.color = player.teamColor;
	}

	protected virtual void BeginAttack(WorldObject target)
	{
		if (audioElement != null) audioElement.Play(attackSound);
		this.target = target;
		if (TargetInRange())
		{
			attacking = true;
			PerformAttack();
		}
		else AdjustPosition();
	}

	private bool TargetInRange()
	{
		Vector3 targetLocation = target.transform.position;
		Vector3 direction = targetLocation - transform.position;
		if (direction.sqrMagnitude < weaponRange * weaponRange)
		{
			return true;
		}
		return false;
	}

	private void AdjustPosition()
	{
		Unit self = this as Unit;
		if (self)
		{
			movingIntoPosition = true;
			Vector3 attackPosition = FindNearestAttackPosition();
			self.StartMove(attackPosition);
			attacking = true;
		}
		else attacking = false;
	}

	private Vector3 FindNearestAttackPosition()
	{
		Vector3 targetLocation = target.transform.position;
		Vector3 direction = targetLocation - transform.position;
		float targetDistance = direction.magnitude;
		float distanceToTravel = targetDistance - (0.9f * weaponRange);
		return Vector3.Lerp(transform.position, targetLocation, distanceToTravel / targetDistance);
	}

	private void PerformAttack()
	{
		if (!target)
		{
			attacking = false;
			return;
		}
		if (!TargetInRange()) AdjustPosition();
		else if (!TargetInFrontOfWeapon()) AimAtTarget();
		else if (ReadyToFire()) UseWeapon();
	}

	private bool TargetInFrontOfWeapon()
	{
		Vector3 targetLocation = target.transform.position;
		Vector3 direction = targetLocation - transform.position;
		if (direction.normalized == transform.forward.normalized) return true;
		else return false;
	}

	protected virtual void AimAtTarget()
	{
		aiming = true;
		//this behaviour needs to be specified by a specific object
	}

	private bool ReadyToFire()
	{
		if (currentWeaponChargeTime >= weaponRechargeTime) return true;
		return false;
	}

	protected virtual void UseWeapon()
	{
		if (audioElement != null && Time.timeScale > 0) audioElement.Play(useWeaponSound);
		currentWeaponChargeTime = 0.0f;
		//this behaviour needs to be specified by a specific object
	}

	public void TakeDamage(int damage)
	{
		hitPoints -= damage;
		if (hitPoints <= 0) Destroy(gameObject);
	}

	protected virtual void InitialiseAudio()
	{
		List<AudioClip> sounds = new List<AudioClip>();
		List<float> volumes = new List<float>();
		if (attackVolume < 0.0f) attackVolume = 0.0f;
		if (attackVolume > 1.0f) attackVolume = 1.0f;
		sounds.Add(attackSound);
		volumes.Add(attackVolume);
		if (selectVolume < 0.0f) selectVolume = 0.0f;
		if (selectVolume > 1.0f) selectVolume = 1.0f;
		sounds.Add(selectSound);
		volumes.Add(selectVolume);
		if (useWeaponVolume < 0.0f) useWeaponVolume = 0.0f;
		if (useWeaponVolume > 1.0f) useWeaponVolume = 1.0f;
		sounds.Add(useWeaponSound);
		volumes.Add(useWeaponVolume);
		audioElement = new AudioElement(sounds, volumes, objectName, this.transform);
	}

	/**
	 * A child class should only determine other conditions under which a decision should
	 * not be made. This could be 'harvesting' for a harvester, for example. Alternatively,
	 * an object that never has to make decisions could just return false.
	 */
	protected virtual bool ShouldMakeDecision()
	{
		if (!attacking && !movingIntoPosition && !aiming)
		{
			//we are not doing anything at the moment
			if (timeSinceLastDecision > timeBetweenDecisions)
			{
				timeSinceLastDecision = 0.0f;
				return true;
			}
			timeSinceLastDecision += Time.deltaTime;
		}
		return false;
	}

	protected virtual void DecideWhatToDo()
	{
		//determine what should be done by the world object at the current point in time
		Vector3 currentPosition = transform.position;
		nearbyObjects = WorkManager.FindNearbyObjects(currentPosition, detectionRange);

		if (CanAttack())
		{
			List<WorldObject> enemyObjects = new List<WorldObject>();
			foreach (WorldObject nearbyObject in nearbyObjects)
			{
				Resource resource = nearbyObject.GetComponent<Resource>();
				if (resource) continue;
				if (nearbyObject.GetPlayer() != player) enemyObjects.Add(nearbyObject);
			}
			WorldObject closestObject = WorkManager.FindNearestWorldObjectInListToPosition(enemyObjects, currentPosition);
			if (closestObject) BeginAttack(closestObject);
		}
	}

	public Player GetPlayer()
	{
		return player;
	}
}
