using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace RTS
{
	public static class PlayerManager
	{
		private struct PlayerDetails
		{
			private string name;
			private int avatar;
			public PlayerDetails(string name, int avatar)
			{
				this.name = name;
				this.avatar = avatar;
			}
			public string Name { get { return name; } }
			public int Avatar { get { return avatar; } }
		}

		private static List<PlayerDetails> players = new List<PlayerDetails>();
		private static PlayerDetails currentPlayer;
		private static Texture2D[] avatars;

		public static void SelectPlayer(string name, int avatar)
		{
			//check player doesnt already exist
			bool playerExists = false;
			foreach (PlayerDetails player in players)
			{
				if (player.Name == name)
				{
					currentPlayer = player;
					playerExists = true;
				}
			}
			if (!playerExists)
			{
				PlayerDetails newPlayer = new PlayerDetails(name, avatar);
				players.Add(newPlayer);
				currentPlayer = newPlayer;
				Directory.CreateDirectory("SavedGames" + Path.DirectorySeparatorChar + name);
			}
			Save();
		}

		public static string GetPlayerName()
		{
			return currentPlayer.Name == "" ? "Unknown" : currentPlayer.Name;
		}

		public static void SetAvatarTextures(Texture2D[] avatarTextures)
		{
			avatars = avatarTextures;
		}

		public static Texture2D GetPlayerAvatar()
		{
			if (avatars == null) return null;
			if (currentPlayer.Avatar >= 0 && currentPlayer.Avatar < avatars.Length) return avatars[currentPlayer.Avatar];
			return null;
		}

		public static void Save()
		{
			JsonSerializer serializer = new JsonSerializer();
			serializer.NullValueHandling = NullValueHandling.Ignore;
			using (StreamWriter sw = new StreamWriter("SavedGames" + Path.DirectorySeparatorChar + "Players.json"))
			{
				using (JsonWriter writer = new JsonTextWriter(sw))
				{
					writer.WriteStartObject();

					writer.WritePropertyName("Players");
					writer.WriteStartArray();
					foreach (PlayerDetails player in players) SavePlayer(writer, player);
					writer.WriteEndArray();

					writer.WriteEndObject();
				}
			}
		}

		private static void SavePlayer(JsonWriter writer, PlayerDetails player)
		{
			writer.WriteStartObject();

			writer.WritePropertyName("Name");
			writer.WriteValue(player.Name);
			writer.WritePropertyName("Avatar");
			writer.WriteValue(player.Avatar);

			writer.WriteEndObject();
		}

		public static void Load()
		{
			players.Clear();

			string filename = "SavedGames" + Path.DirectorySeparatorChar + "Players.json";
			if (File.Exists(filename))
			{
				//read contents of file
				string input;
				using (StreamReader sr = new StreamReader(filename))
				{
					input = sr.ReadToEnd();
				}
				if (input != null)
				{
					//parse contents of file
					using (JsonTextReader reader = new JsonTextReader(new StringReader(input)))
					{
						while (reader.Read())
						{
							if (reader.Value != null)
							{
								if (reader.TokenType == JsonToken.PropertyName)
								{
									if ((string)reader.Value == "Players") LoadPlayers(reader);
								}
							}
						}
					}
				}
			}
		}

		private static void LoadPlayers(JsonTextReader reader)
		{
			while (reader.Read())
			{
				if (reader.TokenType == JsonToken.StartObject) LoadPlayer(reader);
				else if (reader.TokenType == JsonToken.EndArray) return;
			}
		}

		private static void LoadPlayer(JsonTextReader reader)
		{
			string currValue = "", name = "";
			int avatar = 0;
			while (reader.Read())
			{
				if (reader.Value != null)
				{
					if (reader.TokenType == JsonToken.PropertyName)
					{
						currValue = (string)reader.Value;
					}
					else {
						switch (currValue)
						{
							case "Name": name = (string)reader.Value; break;
							case "Avatar": avatar = (int)(System.Int64)reader.Value; break;
							default: break;
						}
					}
				}
				else {
					if (reader.TokenType == JsonToken.EndObject)
					{
						players.Add(new PlayerDetails(name, avatar));
						return;
					}
				}
			}
		}

		public static string[] GetPlayerNames()
		{
			string[] playerNames = new string[players.Count];
			for (int i = 0; i < playerNames.Length; i++) playerNames[i] = players[i].Name;
			return playerNames;
		}

		public static int GetAvatar(string playerName)
		{
			for (int i = 0; i < players.Count; i++)
			{
				if (players[i].Name == playerName) return players[i].Avatar;
			}
			return 0;
		}

		public static string[] GetSavedGames()
		{
			DirectoryInfo directory = new DirectoryInfo("SavedGames" + Path.DirectorySeparatorChar + currentPlayer.Name);
			FileInfo[] files = directory.GetFiles();
			string[] savedGames = new string[files.Length];
			for (int i = 0; i < files.Length; i++)
			{
				string filename = files[i].Name;
				savedGames[i] = filename.Substring(0, filename.IndexOf("."));
			}
			return savedGames;
		}

		private static Color[] colors =
		{
			Color.red,
			Color.blue,
			Color.green,
			Color.yellow
		};

		public static Color GetTeamColor(int id)
		{
			Color color = Color.black;

			if (id < colors.Length)
				color = colors[id];

			return color;
		}

		private static int uniquePlayerId = 0;

		public static int GetUniquePlayerId()
		{
			int id = uniquePlayerId;
			uniquePlayerId++;
			return id;
		}

		public static Player FindPlayer(int id)
		{
			foreach (GameObject gameObject in Object.FindObjectsOfType(typeof(GameObject)) as GameObject[])
			{
				Player player = gameObject.GetComponent<Player>();
				if ((player != null) && (player.id == id))
					return player;
			}
			return null;
		}

		private static Vector3[] spawnPoints =
		{
			new Vector3(-200, 0, 0),
			new Vector3(200, 0, 0),
			new Vector3(0, 0, 200),
			new Vector3(0, 0, -200)
		};

		public static Vector3 GetSpawnPoint(int id)
		{
			Vector3 spawnPoint = new Vector3();

			if (id < spawnPoints.Length)
				spawnPoint = spawnPoints[id];

			return spawnPoint;
		}
		
		private static int uniqueWorldObjectId = 0;

		public static int GetUniqueWorldObjectId()
		{
			int id = uniqueWorldObjectId;
			uniqueWorldObjectId++;
			return id;
		}

		public static Unit FindUnit(int playerId, int unitId)
		{
			Player player = FindPlayer(playerId);
			if (player)
			{
				Units units = player.GetComponentInChildren<Units>();
				foreach(Transform child in units.transform)
				{
					Unit unit = child.gameObject.GetComponent<Unit>();
					if ((unit) && (unit.id == unitId))
						return unit;
				}
			}
			return null;
		}

		public static Building FindBuilding(int playerId, int buildingId)
		{
			foreach (GameObject gameObject in Object.FindObjectsOfType(typeof(GameObject)) as GameObject[])
			{
				Building building = gameObject.GetComponent<Building>();
				if ((building != null) && (building.playerId == playerId) && (building.id == buildingId))
					return building;
			}

			Player player = FindPlayer(playerId);
			if (player)
			{
				Buildings buildings = player.GetComponentInChildren<Buildings>();
				foreach (Transform child in buildings.transform)
				{
					Building building = child.gameObject.GetComponent<Building>();
					if ((building) && (building.id == buildingId))
						return building;
				}
			}
			return null;
		}

		public static WorldObject FindWorldObject(int playerId, int id)
		{
			foreach (GameObject gameObject in Object.FindObjectsOfType(typeof(GameObject)) as GameObject[])
			{
				WorldObject worldObject = gameObject.GetComponent<WorldObject>();
				if ((worldObject != null) && (worldObject.playerId == playerId) && (worldObject.id == id))
					return worldObject;
			}
			return null;
		}

		public static void Reset()
		{
			uniqueWorldObjectId = 0;
			uniquePlayerId = 0;
		}
	}
}