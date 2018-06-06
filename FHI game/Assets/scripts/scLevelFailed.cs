using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class scLevelFailed : MonoBehaviour {

	public GameObject goScoreT;
	private Text gScore;
	
	private int ScoreP;
	
	void Start() {
		ScoreP=scPeopleScore.TheScore;
		Debug.Log("Score: " + scPeopleScore.TheScore);
		gScore=goScoreT.GetComponent<Text>();
		gScore.text="Score: " + ScoreP;
		//Decrement Lifes
		scPeopleScore.Lifes--;
		if (scPeopleScore.Lifes <= 0 ) {
			Application.LoadLevel("GameOver");	
		
		}
	}
	
	
	public void RestartLevel(){
		Application.LoadLevel("lab01");
	}
}
