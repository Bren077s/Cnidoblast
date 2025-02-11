﻿using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {

	private string typeName = "Cnidoblast";
	private string gameName = "RoomName";
	private bool started = false;
	public float timer;
	private float volume = 0.77f;

	private void StartServer(){
		//MasterServer.ipAddress = "127.0.0.1";
		Network.minimumAllocatableViewIDs = 500;
		Network.InitializeServer (8, 25000, !Network.HavePublicAddress());
		MasterServer.RegisterHost (typeName, gameName, "An epic cell game!");
	}

	void OnServerInitialized(){
		Debug.Log ("Server Initialized");
		started = false;
		//SpawnPlayer ();
	}

	private void SpawnPlayer (){
		Network.Instantiate (Resources.Load ("Prefabs/player", typeof(GameObject)), Camera.main.ViewportToWorldPoint(new Vector3(Random.value, Random.value, 10)), Quaternion.identity, 0);
	}

	private void Begin(){
		this.GetComponent<NetworkView> ().RPC ("SpawnPlayers",RPCMode.All,null);
		started = true;
		timer = Time.time;
		int spikes = (int)(Random.value * 6) + 2;
		for(int i = 0; i < spikes; i++)
			Network.Instantiate (Resources.Load ("Prefabs/spike", typeof(GameObject)), Camera.main.ViewportToWorldPoint(new Vector3(Random.value, Random.value, 10)), Quaternion.identity, 0);
		int walls = (int)(Random.value * 2) + 1;
		for(int i = 0; i < walls; i++)
			Network.Instantiate (Resources.Load ("Prefabs/wall", typeof(GameObject)), Camera.main.ViewportToWorldPoint(new Vector3(Random.value, Random.value, 10)), Quaternion.identity, 0);
	}

	private void finish(){
		timer = Time.time;
		started = false;
		this.GetComponent<NetworkView> ().RPC ("EndPlayers",RPCMode.All,null);
		Network.Disconnect();
		foreach (GameObject o in Object.FindObjectsOfType<GameObject>()) {
			if(o.name != "Main Camera" && o.name != "background")
				Destroy (o);
		}
	}

	void OnGUI(){
		if (Network.isServer && !started && !(Time.time - timer > 60)) {
			if(GUI.Button(new Rect(100, 20, 250, 100), "Start"))
				Begin();
		}
		if (Time.time - timer > 60 && Network.isServer) {
			if(GUI.Button(new Rect(100, 130, 250, 100), "Reset"))
				finish();
			GameObject[] objects = GameObject.FindGameObjectsWithTag("Player");
			if(objects.Length > 0){
			GameObject winner = objects[0];
				for(int i = 0; i < objects.Length; i++){
					if(objects[i].GetComponent<Controller>().getMass() > winner.GetComponent<Controller>().getMass())
						winner = objects[i];
				}
				GUI.TextField(new Rect(100, 20, 400, 100),"Game Over " + winner.GetComponent<SpriteRenderer>().color + " wins!");
			}
		}
		if (!Network.isClient && !Network.isServer) {

			volume = GUI.VerticalSlider(new Rect(70, 20, 20, 210), volume, 1.0f, 0.0f);
			GUI.Label(new Rect(65, 240, 20, 20), "Vol");

			Camera.main.GetComponent<AudioSource>().volume = volume;

			if(GUI.Button(new Rect(100, 20, 250, 100), "Start Server"))
				StartServer();

			if(GUI.Button(new Rect(100, 130, 250, 100), "Refresh Host List"))
				RefreshHostList();

			gameName = GUI.TextField(new Rect(100, 240, 250, 100),gameName, 25);

			if(GUI.Button(new Rect(100, 350, 250, 100), "Reset"))
				finish();

			if(GUI.Button(new Rect(100, 460, 250, 100), "Quit"))
				Application.Quit();

			if(hostList != null){
				for(int i = 0; i < hostList.Length; i++){
					if(GUI.Button(new Rect(400, 20 + (110 * i), 300, 100), hostList[i].gameName))
						JoinServer(hostList[i]);
				}
			}
		}
	}

	private HostData[] hostList;

	private void RefreshHostList(){
		MasterServer.RequestHostList (typeName);
	}

	void OnMasterServerEvent(MasterServerEvent msEvent){
		if (msEvent == MasterServerEvent.HostListReceived)
			hostList = MasterServer.PollHostList();
		if (msEvent == MasterServerEvent.RegistrationSucceeded)
			Debug.Log ("Sever Registered");
	}

	private void JoinServer(HostData hostData){
		Network.Connect (hostData);
	}

	void OnConnectedToServer(){
		Debug.Log("Joined Server");
		//SpawnPlayer ();
	}

	[RPC]
	public void SpawnPlayers(){
		if (!started) {
			started = true;
			SpawnPlayer ();
		}
	}

	[RPC]
	public void EndPlayers(){
		started = false;
	}

	// Use this for initialization
	void Start () {
		Screen.SetResolution (1024, 768, false);
		MasterServer.ClearHostList();
		MasterServer.RequestHostList (typeName);
	}
	
	// Update is called once per frame
	void Update () {
		if (Random.value < 0.01 && Network.isServer && started) {
			GameObject releaseMass = (GameObject)Network.Instantiate (Resources.Load ("Prefabs/mass", typeof(GameObject)), Camera.main.ViewportToWorldPoint(new Vector3(Random.value, Random.value, 10)), Quaternion.identity, 0);
			object[] args = {
				Random.value/10,
				0.0f,
				0.0f
			};
			releaseMass.GetComponent<releasedMass> ().GetComponent<NetworkView>().RPC("setVariable", RPCMode.All,args);
			releaseMass.GetComponent<releasedMass> ().GetComponent<NetworkView>().RPC("makeReady", RPCMode.All,null);
		}
		if(MasterServer.PollHostList().Length != 0){
			hostList = MasterServer.PollHostList();
			MasterServer.ClearHostList();
		}
		if (Network.isServer && started) {
			this.GetComponent<NetworkView> ().RPC ("SpawnPlayers",RPCMode.All,null);
		}
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Network.Disconnect();
			started = false;
		}
	}
}
