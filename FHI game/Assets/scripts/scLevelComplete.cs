using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class scLevelComplete : MonoBehaviour {

	public GameObject goScoreT;
	private Text gScore;
	
	private int ScoreP;
	
	void Start() {
		ScoreP=scPeopleScore.TheScore;
		Debug.Log("Score: " + scPeopleScore.TheScore);
		gScore=goScoreT.GetComponent<Text>();
		gScore.text="Score: " + ScoreP;
		scPeopleScore.Parsec++;
		
	
	}
	
	
	public void StartNewLevel(){
		Application.LoadLevel("lab01");
	}
	
	
}
