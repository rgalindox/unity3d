using UnityEngine;
using System.Collections;
using UnityEngine.UI;



public class scGameOver : MonoBehaviour {

	public GameObject goScoreT;
    public GameObject goYearT;
	private Text gScore,gYear;
	
	private int ScoreP;
	
	void Start() {
		ScoreP=scPeopleScore.TheScore;
		//Debug.Log("Score: " + scPeopleScore.TheScore);
		gScore=goScoreT.GetComponent<Text>();
		gScore.text="Your Final Score: " + ScoreP;
        gYear = goYearT.GetComponent<Text>();
        gYear.text = "You travel to year: " + scPeopleScore.Parsec;
	}
	
	
	public void RestartLevel(){
		scPeopleScore.Lifes=5;
		scPeopleScore.TheScore=0;
		scPeopleScore.TheSpeed=1;
		scPeopleScore.Parsec=1;
		Application.LoadLevel("scnMain");
	}
}
