using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HumanDetector : MonoBehaviour {

	public GameObject model;
	public static HumanDetector INSTANCE;
	public Dictionary<string, Vector3> humanPositions = new Dictionary<string, Vector3>();

	// Use this for initialization
	void Start () {
		HumanDetector.INSTANCE = this;
		model.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		foreach (var entry in humanPositions) {
			GameObject human = GameObject.Find (entry.Key);
			human.transform.position = Vector3.MoveTowards(human.transform.position, entry.Value, 2.0f * Time.deltaTime);
		}
	}

	public void OnHumanDetected(HumanDetected humanDetected) {
		Debug.LogFormat ("Creating human {0} at {1}", humanDetected.humanId, humanDetected.position);
		GameObject human = Object.Instantiate (model);
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
			human = Object.Instantiate (model);
			human.SetActive (true);
			human.name = humanMoving.humanId;
			human.transform.position = humanPositions [humanMoving.humanId];
		}

		/*
		human.transform.position=new Vector3 ((float) humanMoving.position.x, 
		                                      (float) humanMoving.position.y, 
		                                      (float) -humanMoving.position.z);
		                                      */
	}
	
}
