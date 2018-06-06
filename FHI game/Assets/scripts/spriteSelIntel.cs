using UnityEngine;
using System.Collections;

public class spriteSelIntel : MonoBehaviour {

	public float fallSpeed;
	
	// Update is called once per frame
	void Update () {
		//transform.Translate(movespeed, 0, 0)
		fallSpeed=scPeopleScore.TheSpeed;
		gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - fallSpeed * Time.deltaTime, 0);
	}
}
