﻿using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {

	private float mass = 1.0f;
	private float xVelocity = 0;
	private float yVelocity = 0;
	private float charge = 0;
	private bool charging = false;
	private const int screenWidth = 80;
	private const int screenHeight = 40;
	private static int color = 1;
	private int ability;
	private float lastshoot;
	//private GameObject spike;
	// Use this for initialization
	void Start () {
		ability = (int)(Random.value * 2);
		lastshoot = Time.time-100;
		gameObject.tag = "Player";
		if (color == 1) {
			this.GetComponent<SpriteRenderer>().color = Color.blue;
		}else if (color == 2) {
			this.GetComponent<SpriteRenderer>().color = Color.red;
		}else if (color == 3) {
			this.GetComponent<SpriteRenderer>().color = Color.green;
		}else if (color == 4) {
			this.GetComponent<SpriteRenderer>().color = Color.black;
		}else if (color == 5) {
			this.GetComponent<SpriteRenderer>().color = Color.cyan;
		}else if (color == 6) {
			this.GetComponent<SpriteRenderer>().color = Color.white;
		}else if (color == 7) {
			this.GetComponent<SpriteRenderer>().color = Color.grey;
		}else if (color == 8) {
			this.GetComponent<SpriteRenderer>().color = Color.yellow;
		}
		color++;
		//if(ability == 2){
			//spike = (GameObject)Network.Instantiate (Resources.Load ("Prefabs/attackspike", typeof(GameObject)), new Vector3(1212, 1123, 1232), Quaternion.identity, 0);
		//}
	}

	void OnTriggerStay2D(Collider2D coll){
		if(coll.gameObject.name == "poisongas(Clone)") {
			float lost = mass * 0.01f;
			mass -= lost;
			coll.gameObject.GetComponent<poisonGas>().setMass(lost + coll.gameObject.GetComponent<poisonGas>().getMass());
		}
	}                   

	void OnTriggerEnter2D(Collider2D coll){
		if(coll.gameObject.name == "mass(Clone)" && coll.gameObject.GetComponent<releasedMass>().getReady()) {
			mass += coll.gameObject.GetComponent<releasedMass>().getMass();
			Destroy(coll.gameObject);
		}else if(coll.gameObject.name == "bullet(Clone)" && coll.gameObject.GetComponent<bullet>().getReady()) {
			float lost = mass * 0.4f;
			mass *= 0.6f;
			float angle = Random.value * Mathf.PI*2;
			xVelocity = 40*Mathf.Cos(angle) * -1 * 0.5f/Mathf.Exp(mass/Mathf.PI/2);
			yVelocity = 40*Mathf.Sin(angle) * -1 * 0.5f/Mathf.Exp(mass/Mathf.PI/2);
			GameObject releaseMass = (GameObject)Network.Instantiate (Resources.Load ("Prefabs/mass", typeof(GameObject)), transform.position, Quaternion.identity, 0);
			object[] args = {
				lost,
				40 * Mathf.Cos (angle) * (lost) / Mathf.Exp ((lost) / Mathf.PI / 2),
				40 * Mathf.Sin (angle) * (lost) / Mathf.Exp ((lost) / Mathf.PI / 2)
			};
			releaseMass.GetComponent<releasedMass> ().GetComponent<NetworkView>().RPC("setVariable", RPCMode.All,args);
			Destroy(coll.gameObject);
		}else if(coll.gameObject.name == "attackspike(Clone)") {
			float lost = mass * 0.4f;
			mass *= 0.6f;
			float angle = Random.value * Mathf.PI*2;
			xVelocity = 40*Mathf.Cos(angle) * -1 * 0.5f/Mathf.Exp(mass/Mathf.PI/2);
			yVelocity = 40*Mathf.Sin(angle) * -1 * 0.5f/Mathf.Exp(mass/Mathf.PI/2);
			GameObject releaseMass = (GameObject)Network.Instantiate (Resources.Load ("Prefabs/mass", typeof(GameObject)), transform.position, Quaternion.identity, 0);
			object[] args = {
				lost,
				40 * Mathf.Cos (angle) * (lost) / Mathf.Exp ((lost) / Mathf.PI / 2),
				40 * Mathf.Sin (angle) * (lost) / Mathf.Exp ((lost) / Mathf.PI / 2)
			};
			releaseMass.GetComponent<releasedMass> ().GetComponent<NetworkView>().RPC("setVariable", RPCMode.All,args);
		}else if(coll.gameObject.name == "spike(Clone)") {
			if(ability == 2)
				mass *= 0.9f;
			else
				mass *= 0.75f;
			if (mass < 0.1f) {
				mass = 0.1f;
			}
			xVelocity = - xVelocity;
			yVelocity = - yVelocity;
		}else if(coll.gameObject.name == "wall(Clone)") {
			xVelocity = - xVelocity;
			yVelocity = - yVelocity;
		}
	}

	void OnTriggerExit2D(Collider2D coll){
		if(coll.gameObject.name == "mass(Clone)") {
			coll.gameObject.GetComponent<releasedMass>().setReady();
		}
		if(coll.gameObject.name == "bullet(Clone)") {
			coll.gameObject.GetComponent<bullet>().setReady();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (mass < 0.1f) {
			mass = 0.1f;
		}
		if (this.GetComponent<NetworkView>().isMine) {
			this.GetComponent<Rigidbody2D>().mass = mass;
			transform.localScale = new Vector3 (Mathf.Sqrt (mass / Mathf.PI) / 2, Mathf.Sqrt (mass / Mathf.PI) / 2, 1);
			Vector3 mousePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			transform.rotation = Quaternion.LookRotation (Vector3.forward, mousePosition - transform.position);
			if(Input.GetMouseButtonDown(1) && Time.time - lastshoot > 7 && ability == 0){
				lastshoot = Time.time;
				mass *= 0.7f;
				float angle = (transform.eulerAngles.z - 270) * Mathf.Deg2Rad;
				GameObject bullet = (GameObject)Network.Instantiate (Resources.Load ("Prefabs/bullet", typeof(GameObject)), transform.position, Quaternion.identity, 0);
				object[] args = {
					20 * Mathf.Cos (angle) / Mathf.Exp ((0.01f) / Mathf.PI / 2),
					20 * Mathf.Sin (angle) / Mathf.Exp ((0.01f) / Mathf.PI / 2)
				};
				bullet.GetComponent<bullet> ().GetComponent<NetworkView>().RPC("setVariable", RPCMode.All,args);
			}else if(Input.GetMouseButtonDown(1) && Time.time - lastshoot > 12 && ability == 1){
				lastshoot = Time.time;
				mass *= 0.7f;
				float angle = (transform.eulerAngles.z - 270) * Mathf.Deg2Rad;
				GameObject poisonbullet = (GameObject)Network.Instantiate (Resources.Load ("Prefabs/poisonbullet", typeof(GameObject)), transform.position, Quaternion.identity, 0);
				object[] args = {
					2 * Mathf.Cos (angle) / Mathf.Exp ((0.01f) / Mathf.PI / 2),
					2 * Mathf.Sin (angle) / Mathf.Exp ((0.01f) / Mathf.PI / 2)
				};
				poisonbullet.GetComponent<poisonBullet> ().GetComponent<NetworkView>().RPC("setVariable", RPCMode.All,args);
			}
			if (Input.GetMouseButton (0)) {
				charging = true;
				charge += 0.005f;
			} else {
				if (charging) {
					charging = false;
					if (charge > 0.5f)
						charge = 0.5f;
					move ();
				}
			}
			if (xVelocity < 0) {
				xVelocity += 0.02f;
			} else {
				xVelocity -= 0.02f;
			}
			if (yVelocity < 0) {
				yVelocity += 0.02f;
			} else {
				yVelocity -= 0.02f;
			}
			if (Mathf.Abs (xVelocity) < 0.1f) {
				xVelocity = 0;
			}
			if (Mathf.Abs (yVelocity) < 0.1f) {
				yVelocity = 0;
			}
			Vector3 board = Camera.main.WorldToViewportPoint(transform.position);
			if (board.x > 1)
				xVelocity = - Mathf.Abs (xVelocity);
			if (board.x < 0)
				xVelocity = Mathf.Abs (xVelocity);
			if (board.y > 1)
				yVelocity = - Mathf.Abs (yVelocity);
			if (board.y < 0)
				yVelocity = Mathf.Abs (yVelocity);
			transform.Translate (new Vector3 (xVelocity, yVelocity, 0) * Time.deltaTime, Camera.main.transform);
			Camera.main.GetComponent<SmoothCamera>().target = transform;
			//if(ability == 2){
				//spike.transform.localScale = new Vector3 (Mathf.Sqrt (mass / Mathf.PI) * 2, Mathf.Sqrt (mass / Mathf.PI) * 2, 1);
				//spike.transform.rotation = Quaternion.LookRotation (Vector3.forward, mousePosition - transform.position);
				//spike.transform.eulerAngles += new Vector3(0, 0, 180f);
				//spike.transform.position = new Vector3(transform.position.x + transform.localScale.x * 3 * Mathf.Cos((spike.transform.eulerAngles.z - 270) * Mathf.Deg2Rad), transform.position.y + transform.localScale.y * 3 * Mathf.Sin((spike.transform.eulerAngles.z - 270) * Mathf.Deg2Rad),0);
			//}
		}
	}

	public float getMass(){
		return mass;
	}

	void move(){
		float angle = (transform.eulerAngles.z - 270) * Mathf.Deg2Rad;
		xVelocity = 40*Mathf.Cos(angle) * -1 * charge/Mathf.Exp(mass/Mathf.PI/2);
		yVelocity = 40*Mathf.Sin(angle) * -1 * charge/Mathf.Exp(mass/Mathf.PI/2);
		float oldMass = mass;
		mass *= (1-charge)*0.9f;
		GameObject releaseMass = (GameObject)Network.Instantiate (Resources.Load ("Prefabs/mass", typeof(GameObject)), transform.position, Quaternion.identity, 0);
		object[] args = {
			oldMass - mass,
			40 * Mathf.Cos (angle) * (oldMass - mass) / Mathf.Exp ((oldMass - mass) / Mathf.PI / 2),
			40 * Mathf.Sin (angle) * (oldMass - mass) / Mathf.Exp ((oldMass - mass) / Mathf.PI / 2)
		};
		releaseMass.GetComponent<releasedMass> ().GetComponent<NetworkView>().RPC("setVariable", RPCMode.All,args);
		charge = 0;
	}

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info){
		Vector3 syncPosition = Vector3.zero;
		if (stream.isWriting) {
			syncPosition = transform.position;
			stream.Serialize(ref syncPosition);
		} else {
			stream.Serialize(ref syncPosition);
			transform.position = syncPosition;
		}
	}
}
