using UnityEngine;
using System.Collections;

public class CameraAction : MonoBehaviour {
	int count = 0;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		count++;
		if (count > 0 && count <= 100) {
			transform.position = (new Vector3 (transform.position.x + .01f, transform.position.y + .01f, transform.position.z));
		}
		if (count > 100 && count <= 300) {
			transform.position = (new Vector3 (transform.position.x - .01f, transform.position.y, transform.position.z));
		}
		if (count > 300 && count <= 500) {
			transform.position = (new Vector3 (transform.position.x + .01f, transform.position.y - .01f, transform.position.z));
		}
		if (count > 500 && count <= 700) {
			transform.position = (new Vector3 (transform.position.x - .01f, transform.position.y + .01f, transform.position.z));
		}
		if (count > 700 && count <= 800) {
			transform.position = (new Vector3 (transform.position.x + .01f, transform.position.y - .01f, transform.position.z));
		}
		if (count > 800) {
			count = 0;
		}
	}
}
