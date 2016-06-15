﻿using UnityEngine;
using System.Collections;
using RTS;
using Newtonsoft.Json;
using UnityEngine.Networking;

public class Tank : Unit {

	public string projectileName;
	private Quaternion aimRotation;

	protected override void Awake()
	{
		base.Awake();
	}

	// Use this for initialization
	protected override void Start () {
		base.Start();
	}

	protected override void Update()
	{
		base.Update();
		if (aiming)
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, aimRotation, weaponAimSpeed);
			CalculateBounds();
			//sometimes it gets stuck exactly 180 degrees out in the calculation and does nothing, this check fixes that
			Quaternion inverseAimRotation = new Quaternion(-aimRotation.x, -aimRotation.y, -aimRotation.z, -aimRotation.w);
			if (transform.rotation == aimRotation || transform.rotation == inverseAimRotation)
			{
				aiming = false;
			}
		}
	}

	public override bool CanAttack()
	{
		return true;
	}

	protected override void AimAtTarget()
	{
		base.AimAtTarget();
		aimRotation = Quaternion.LookRotation(target.transform.position - transform.position);
	}

	protected override void UseWeapon()
	{
		base.UseWeapon();
		CmdCreateProjectile(target.playerId, target.id);
	}

	[Command]
	private void CmdCreateProjectile(int targetPlayerId, int targetId)
	{
		Vector3 spawnPoint = transform.position;
		spawnPoint.x += (2.1f * transform.forward.x);
		spawnPoint.y += 1.4f;
		spawnPoint.z += (2.1f * transform.forward.z);
		GameObject gameObject = (GameObject)Instantiate(ResourceManager.GetWorldObject(projectileName), spawnPoint, transform.rotation);
		Projectile projectile = gameObject.GetComponentInChildren<Projectile>();
		projectile.SetRange(0.9f * weaponRange);
		projectile.SetTarget(targetPlayerId, targetId);
		NetworkServer.Spawn(gameObject);
	}
}
