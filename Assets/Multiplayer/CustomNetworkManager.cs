﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using RTS;

public class CustomNetworkManager : NetworkManager {
	
	private void SpawnWorker(Player player)
	{
		string unitName = "Worker";
		Vector3 spawnPoint = PlayerManager.GetSpawnPoint(player.id);
		int worldObjectId = PlayerManager.GetUniqueWorldObjectId();
		
		player.AddUnit(worldObjectId, unitName, new Vector3(spawnPoint.x + 10, spawnPoint.y, spawnPoint.z), default(Quaternion));

		worldObjectId = PlayerManager.GetUniqueWorldObjectId();

		player.CmdAddBuilding(worldObjectId, "Headquarter", spawnPoint, new Rect());
	}

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		var playerObject = (GameObject)Instantiate(playerPrefab, new Vector3(), Quaternion.identity);
		NetworkServer.AddPlayerForConnection(conn, playerObject, playerControllerId);
		Player player = playerObject.GetComponent<Player>();
		int playerId = PlayerManager.GetUniquePlayerId();
		player.SetId(playerId);

		SpawnWorker(player);
	}
}
