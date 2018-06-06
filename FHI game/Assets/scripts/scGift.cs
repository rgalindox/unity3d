using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class scGift : MonoBehaviour {

	public GameObject goScoreT,theArtifact;
    public Sprite spActual;
	private Text gScore;
	
	private int ScoreP;
    private Sprite spNuevo;
    private SpriteRenderer spRend;
	
	void Start() {
		ScoreP=scPeopleScore.TheScore;
		Debug.Log("Score: " + scPeopleScore.TheScore);
		gScore=goScoreT.GetComponent<Text>();
		gScore.text="Score: " + ScoreP;
		scPeopleScore.Parsec++;

        //Load sprites
        scPeopleScore.GiftTime++;
        spNuevo = Resources.Load<Sprite>("inventory" + scPeopleScore.GiftTime);
        Debug.Log("Sprite: " + "inventory" + scPeopleScore.GiftTime);
        chgSprite();
	}

    public void chgSprite()
    {
        spRend = theArtifact.GetComponent<SpriteRenderer>();
        spRend.sprite = spNuevo;
        Debug.Log("si llega");

    }

    public void StartNewLevel(){
		Application.LoadLevel("lab01");
	}
	
}
