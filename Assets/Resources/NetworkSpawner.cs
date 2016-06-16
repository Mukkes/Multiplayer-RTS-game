﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkSpawner : NetworkBehaviour
{
	public GameObject goldDepositPrefab;
	public GameObject oreDepositPrefab;
	public GameObject stoneDepositPrefab;
	public List<Vector3> goldPositions = new List<Vector3>() {
		new Vector3(-200, 0, 40),
		new Vector3(200, 0, 40),
		new Vector3(0, 0, 240),
		new Vector3(0, 0, -160)
	};
	public List<Vector3> orePositions = new List<Vector3>() {
		new Vector3(-130, 0, -20),
		new Vector3(70, 0, -20),
		new Vector3(-30, 0, 80),
		new Vector3(-30, 0, -120),
		new Vector3(-100, 0, 40),
		new Vector3(100, 0, 40),
		new Vector3(0, 0, 140),
		new Vector3(0, 0, -60),
		new Vector3(-60, 0, 0),
		new Vector3(140, 0, 0),
		new Vector3(40, 0, 100),
		new Vector3(40, 0, -100)
	};
	public List<Vector3> stonePositions = new List<Vector3>() {
		new Vector3(-160, 0, 0),
		new Vector3(240, 0, 0),
		new Vector3(40, 0, 200),
		new Vector3(40, 0, -200)
	};

	public override void OnStartServer()
	{
		//SpawnDeposit(goldDepositPrefab, goldPositions);
		SpawnDeposit(oreDepositPrefab, orePositions);
		//SpawnDeposit(stoneDepositPrefab, stonePositions);
	}

	private void SpawnDeposit(GameObject prefab, List<Vector3> positions)
	{
		foreach (Vector3 position in positions)
		{
			Quaternion rotation = Quaternion.Euler(0, 0, 0);

			GameObject deposit = (GameObject)Instantiate(prefab, position, rotation);
			NetworkServer.Spawn(deposit);
		}
	}
}