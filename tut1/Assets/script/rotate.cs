using UnityEngine;
using System.Collections;

public class rotate : MonoBehaviour {
	public float speed = 1f;
	private int randomizier;
	// Use this for initialization
	void Start () {
		randomizier = Random.Range (0, 30);
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(new Vector3 (15+randomizier,30+randomizier,45+randomizier)*Time.deltaTime*(speed));
	}

}
