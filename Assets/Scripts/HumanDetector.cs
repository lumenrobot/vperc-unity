using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HumanDetector : MonoBehaviour {

	public GameObject model1;
	public GameObject model2;
	private int Flag;
	public static HumanDetector INSTANCE;
	public Dictionary<string, Vector3> humanPositions = new Dictionary<string, Vector3>();

	// Use this for initialization
	void Start () {
		HumanDetector.INSTANCE = this; 
		model1.SetActive (false);
		model2.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		foreach (var entry in humanPositions) {
			GameObject human = GameObject.Find (entry.Key);
			human.transform.position = Vector3.MoveTowards(human.transform.position, entry.Value, 1.0f * Time.deltaTime);
		}
	}

	public void OnHumanDetected(HumanDetected humanDetected) {
		Debug.LogFormat ("Creating human {0} at {1}", humanDetected.humanId, humanDetected.position);
		GameObject human;
		if ((Flag % 2) == 0) {
			human = Object.Instantiate (model1);
		} else {
			human = Object.Instantiate (model2);
		}
		Flag++;

		human.SetActive (true);
		human.name = humanDetected.humanId;
		human.transform.position = new Vector3 ((float) humanDetected.position.x, 
		                                        (float) humanDetected.position.y, 
		                                        (float) -humanDetected.position.z);
		humanPositions [humanDetected.humanId] = human.transform.position;

	}

	public void OnHumanMoving(HumanMoving humanMoving)
	{

		Debug.LogFormat ("Moving {0} to {1}", humanMoving.humanId, humanMoving.position);
		humanPositions [humanMoving.humanId] = new Vector3 ((float) humanMoving.position.x, 
		                                                    (float) humanMoving.position.y, 
		                                                    (float) -humanMoving.position.z);

		GameObject human = GameObject.Find (humanMoving.humanId);

		if (human == null) {
			// fault tolerant
			Debug.LogFormat ("Force-creating human {0} at {1}", humanMoving.humanId, humanMoving.position);
			if ((Flag % 2) == 0) {
				human = Object.Instantiate (model1);
			} else {
				human = Object.Instantiate (model2);
			}
			Flag++;
			human.SetActive (true);
			human.name = humanMoving.humanId;
			human.transform.position = humanPositions [humanMoving.humanId];
		}

		var dz = humanPositions [humanMoving.humanId].z - human.transform.position.z;
		var dx = humanPositions [humanMoving.humanId].x - human.transform.position.x;
		var angle1 = System.Math.Atan2(dz, dx);
		var angle = angle1 * (180.0 / Mathf.PI);
		if (dz > 0)
			angle = angle + 180.0;
		//human.transform.Rotate( new Vector3(0,(float)angle,0), Time.deltaTime, Space.World);
		//human.transform.eulerAngles = new Vector3 (0, (float)angle, 0);//, Time.deltaTime, Space.World);
		human.transform.rotation = Quaternion.LookRotation (new Vector3 (dx, 0f, dz));
		Debug.LogFormat ("Rotate {0} to {1}. dz:{2} dx:{3} for {4}",
		                 angle, human.transform.rotation.eulerAngles.y,
		                 humanPositions [humanMoving.humanId].z - human.transform.position.z,
		                 humanPositions [humanMoving.humanId].x - human.transform.position.x,
		                 human.name);
		

		/*
		human.transform.position=new Vector3 ((float) humanMoving.position.x, 
		                                      (float) humanMoving.position.y, 
		                                      (float) -humanMoving.position.z);
		                                      */
	}
	
}
