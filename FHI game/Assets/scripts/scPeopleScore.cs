using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class scPeopleScore : MonoBehaviour {
    //Only 3 statics, one for lifes other for score and the last for level controller (Parsec)
	public static int TheScore=0;
	public static int Lifes=5;
	public static int Parsec=1;
	public static float TheSpeed=1.1f;
    public static int GiftTime = 0;
	public GameObject sparkleEffect;
    public AudioClip goodClick,wrongClick;
        	
	public Sprite goodSprite;
	public Sprite failSprite;
	public Text txtPoints,txtToMatch,txtWrong,txtTime,txtLifes,txtParsec,txtYear;
	public int coinValue;
	private Text otroText,otroMatch,otroWrong,otroLifes,otroParsec,otroYear;
	
	private GameObject goS;
	private GameObject ps;
	private SpriteRenderer sr;
	private int TheMatch;
	private int TheWrong;
    private AudioSource aSource;
	
	public int BadTries;
	
    void Awake()
    {

        aSource = GetComponent<AudioSource>();
    }
	
	void Start() {
		TheWrong=0;
		otroText=txtPoints.GetComponent<Text>();
		otroText.text=TheScore.ToString();
		LifeContoller();
		ParsecController();
	}
	
	public void showItemsToMatch(int itemsNum) {
		otroMatch=txtToMatch.GetComponent<Text>();
		TheMatch=itemsNum;
		otroMatch.text=TheMatch.ToString();
	}
	
	public void chgSpriteOk(string whatObject){
		goS = GameObject.Find(whatObject);
		sr=goS.GetComponent<SpriteRenderer> ();
		sr.sprite=goodSprite;
		vfxSparkling(goS);
		
	}
	
	public void vfxSparkling(GameObject psT){
		GameObject psx= Instantiate(sparkleEffect,psT.transform.position,psT.transform.rotation) as GameObject;
		psx.GetComponent<Renderer>().sortingLayerName="vfxLayer";
	}
	
	public void chgSpriteFail(string whatObject){
		goS = GameObject.Find(whatObject);
		sr=goS.GetComponent<SpriteRenderer> ();
		sr.sprite=failSprite;
	}
	
	public void goodMatch(string whatObject){
        //=== Play audio FX ==
        aSource.PlayOneShot(goodClick);
        //====================

		chgSpriteOk(whatObject);
		
		AddScore();
		
	}
	
	public void badMatch(string whatObject) {
        //=== Play audio FX ==
        aSource.PlayOneShot(wrongClick);
        //====================
        chgSpriteFail(whatObject);
		otroWrong=txtWrong.GetComponent<Text>();
		TheWrong++;
		otroWrong.text=TheWrong.ToString();
		if (TheWrong >= BadTries) {
			//Debug.Log ("End");
			Application.LoadLevel("scnFail");
		}
	}
	
	void LifeContoller() {
		otroLifes=txtLifes.GetComponent<Text>();
		otroLifes.text=Lifes.ToString();
		//Debug.Log ("Lifes: " + otroLifes.ToString());	
	}
	
	
	void ParsecController() {
		otroParsec=txtParsec.GetComponent<Text>();
		otroParsec.text="LEVEL " + Parsec.ToString();
		
		otroYear=txtYear.GetComponent<Text>();
		otroYear.text="Year : " + (2016 - Parsec);
		
	}

    public void MainLevel()
    {
        TheScore = 0;
        Lifes = 5;
        Parsec = 1;
        TheSpeed = 1.1f;
        GiftTime = 0;
        Application.LoadLevel("scnMain");
    }

    void AddScore(){
		float FactorCoin;
		int getGift;
		FactorCoin=gameObject.GetComponent<scPeopleController>().myTimer;
		TheScore+=coinValue * (int)FactorCoin;
		
		otroText.text=TheScore.ToString();
		otroMatch=txtToMatch.GetComponent<Text>();
		TheMatch--;
		otroMatch.text=TheMatch.ToString();
		if (TheMatch <=0) {
			getGift=scPeopleScore.Parsec  % 7;
			if (getGift == 0) {
				if (scPeopleScore.TheSpeed <= 5f) {
					scPeopleScore.TheSpeed+=0.25f;
				}
				Application.LoadLevel("scnRegalito");
			} else {
				Application.LoadLevel("scnExito");
			}
		}
	}
	
	
}
