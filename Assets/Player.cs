using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {


	// Use this for initialization
	void Start () {

	}

	void OnTriggerEnter2D(Collider2D col) 
	{    
		Debug.Log ("tri!");
		if (col.gameObject.name.Contains("Ball")) 
		{    
			//Set depth
			UI2DSprite ui2dspr = gameObject.GetComponent<UI2DSprite> ();
			//ui2dspr.flip = UIBasicSprite.Flip;
			ui2dspr.color = Color.red;
			Debug.Log ("Trigger!");
		}
	}

	void OnTriggerExit2D(Collider2D col) 
	{    
		Debug.Log ("tri!");
		if (col.gameObject.name.Contains("Ball")) 
		{    
			//Set depth
			UI2DSprite ui2dspr = gameObject.GetComponent<UI2DSprite> ();
			//ui2dspr.flip = UIBasicSprite.Flip;
			ui2dspr.color = Color.white;
			Debug.Log ("bye!");
		}
	}
}
