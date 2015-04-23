﻿using UnityEngine;
using System.Collections;

public class HumanDetector : MonoBehaviour {

	public GameObject model;
	public static HumanDetector INSTANCE;

	// Use this for initialization
	void Start () {
		HumanDetector.INSTANCE = this;
		model.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnHumanDetected(HumanDetected humanDetected) {
		Debug.LogFormat ("Creating human {0} at {1}", humanDetected.humanId, humanDetected.position);
		GameObject human = Object.Instantiate (model);
		human.SetActive (true);
		human.name = humanDetected.humanId;
		human.transform.position = new Vector3 ((float) humanDetected.position.x, 
		                                        (float) humanDetected.position.y, 
		                                        (float) -humanDetected.position.z);
	}

}
