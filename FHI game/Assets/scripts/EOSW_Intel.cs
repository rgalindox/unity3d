using UnityEngine;
using System.Collections;

public class EOSW_Intel : MonoBehaviour {

	//void OnCollisionEnter2D(Collision2D col){
	void OnTriggerEnter2D(Collider2D col){
		Debug.Log ("Destroy: " + col.gameObject.name);
		col.gameObject.transform.position = new Vector3(col.gameObject.transform.position.x, 1.874f, 0);
		//Destroy (col.gameObject);
	}
}
