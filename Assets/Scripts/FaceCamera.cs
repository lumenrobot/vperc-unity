using UnityEngine;
using System.Collections;

public class FaceCamera : MonoBehaviour {

	public GameObject theCamera;
	public GameObject avatar;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = avatar.transform.position + new Vector3 (0.5f, 1f, -1f);
		transform.LookAt(transform.localPosition + theCamera.transform.rotation * -Vector3.back,
		                 theCamera.transform.rotation * Vector3.up);
	}
}
