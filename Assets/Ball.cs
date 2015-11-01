using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter (Collision col)
	{
		Debug.Log ("col!");
		if(col.gameObject.name == "Ball")
		{
			//Destroy(col.gameObject);
			Debug.Log ("Collision!");
		}
	}
	void OnTriggerEnter(Collider col) 
	{    
		Debug.Log ("tri!");
		if (col.gameObject.name == "Player") 
		{    
			Debug.Log ("Player trigger!");
		}
	}
}
