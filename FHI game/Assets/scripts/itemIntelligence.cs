using UnityEngine;
using System.Collections;

public class itemIntelligence : MonoBehaviour {
	GameObject goSelection;
	
	string strObjectName,strT;

	//Important: use this one to detect an object is clicked

	void Start(){
		goSelection = GameObject.Find ("peopleController");
	}
	void OnMouseDown () {
		strObjectName = gameObject.name;
		string[] lineas = strObjectName.Split (new char[] {'_'});
		strT = lineas [0];
		strT = strT.Substring (4);
		goSelection.GetComponent<scPeopleController> ().CheckClicked(strT,gameObject.name);
	}
	
}
