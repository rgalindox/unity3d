using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;

public class scPeopleController : MonoBehaviour {

	public GameObject boardItem;
	public GameObject boardSelection;
	public GameObject boardWall;
	public Text elTiempo;
	public Sprite elWall,elWall1,elWall2,elWall3;
	public float MiTiempo,myTimer;
	public int boardLayout;
    
	
	private Sprite[] sprites;
	private string[] spnames;
	private string[] spselected;
	private string[] sptomatch;
	private int[] spnocount;
	private SpriteRenderer sr;
	private float posX,posY,stepX,stepY;
	private int itemsToSelect, itemsIndex, tempTime;
	private int score;
	private Text MyTime;
	private GameObject goToDelete;
	private int rndWall,rndAtlas;
    private string elAtlas;

    int valor,indice1,indice2;
	int siExiste;
	void Start() {
        
		myTimer = MiTiempo;
		score = 0;
        elAtlas = "AtlasOne";



        if (scPeopleScore.Parsec > 28)
        {
            itemsToSelect = 4;
            rndAtlas = rndWall = Random.Range(1, 5);
            switch (rndWall)
            {
                case 1:
                    elAtlas = "AtlasOne";
                    break;
                case 2:
                    elAtlas = "AtlasTwo";
                    break;
                case 3:
                    elAtlas = "AtlasThree";
                    break;
                case 4:
                    elAtlas = "AtlasFour";
                    break;


            }

        }


        if (scPeopleScore.Parsec <= 28)
        {
            elAtlas = "AtlasFour";
            itemsToSelect = 4;
        }

        if (scPeopleScore.Parsec <= 21)
        {
            elAtlas = "AtlasThree";
            itemsToSelect = 4;
        }

        if (scPeopleScore.Parsec <= 14)
        {
            elAtlas = "AtlasTwo";
            itemsToSelect = Random.Range(3, 5);
        }

        if (scPeopleScore.Parsec <= 7)
        {
            elAtlas = "AtlasOne";
            itemsToSelect = 3;
        }

        sprites = Resources.LoadAll<Sprite>(elAtlas);
        rndWall = Random.Range(1, 4);

        //Play music
        

        switch (rndWall)
        {
            case 1:
                elWall = elWall1;
                break;
            case 2:
                elWall = elWall2;
                break;
            case 3:
                elWall = elWall3;
                break;

        }

		BoardShape();
		
		gameObject.GetComponent<scPeopleScore>().showItemsToMatch(itemsToSelect);	
		itemsIndex = 0;

		addSpriteNames();
		pickSprites (itemsToSelect);
		buildBoard ();

		StartSpawnSelected ();
		Invoke ("hideParsecText",1);
		

	}

	void FixedUpdate() {
		myTimer -= 1.0f * Time.deltaTime;
		MyTime=elTiempo.GetComponent<Text>();
		tempTime=(int)myTimer;
		if (tempTime<=9) {
			MyTime.text="0:0" + tempTime.ToString();
		} else {
			MyTime.text="0:" + tempTime.ToString();
		}
		if (tempTime <= 0 ) {
			Application.LoadLevel("scnFail");
		}
	}
	
	void BoardShape() {
		boardLayout = Random.Range(1,7);
		if (boardLayout==1) {
			spnocount=new int[9];
			spnocount[0]=0;
			spnocount[1]=6;
			spnocount[2]=17;
			spnocount[3]=23;
			spnocount[4]=24;
			spnocount[5]=25;
			spnocount[6]=31;
			spnocount[7]=42;
			spnocount[8]=48;	
		}
		if (boardLayout==2) {
			spnocount=new int[8];
			spnocount[0]=2;
			spnocount[1]=3;
			spnocount[2]=4;
			spnocount[3]=10;
			spnocount[4]=45;
			spnocount[5]=46;
			spnocount[6]=38;
			spnocount[7]=44;	
		}
		if (boardLayout==3) {
			spnocount=new int[16];
			spnocount[0]=0;
			spnocount[1]=1;
			spnocount[2]=5;
			spnocount[3]=6;
			spnocount[4]=7;
			spnocount[5]=8;
			spnocount[6]=12;
			spnocount[7]=13;
			spnocount[8]=35;
			spnocount[9]=36;	
			spnocount[10]=40;
			spnocount[11]=41;
			spnocount[12]=42;
			spnocount[13]=43;
			spnocount[14]=47;
			spnocount[15]=48;		
		}
		if (boardLayout==4) {
			spnocount=new int[10];
			spnocount[0]=0;
			spnocount[1]=1;
			spnocount[2]=2;
			spnocount[3]=14;
			spnocount[4]=7;
			spnocount[5]=34;
			spnocount[6]=48;
			spnocount[7]=47;
			spnocount[8]=46;
			spnocount[9]=41;			
		}
		if (boardLayout==5) {
			spnocount=new int[12];
			spnocount[0]=3;
			spnocount[1]=10;
			spnocount[2]=17;
			spnocount[3]=27;
			spnocount[4]=31;
			spnocount[5]=38;
			spnocount[6]=45;
			spnocount[7]=21;
			spnocount[8]=22;
			spnocount[9]=23;	
			spnocount[10]=25;
			spnocount[11]=26;		
		}
		if (boardLayout==6) {
			spnocount=new int[1];
			spnocount[0]=24;	
		}
		
	}
	
	int IsNoBoardItem(int exlItem) {
	    int flagY;
	    flagY=0;
		for (int ij = 0; ij <= spnocount.Length -1 ; ij++) {
			if (exlItem==spnocount[ij]) {
				flagY=1;
			}
		}
		return flagY;
	}

	void addSpriteNames() {
		spnames = new string[50];

		for (int ij = 0; ij <= 49; ij++) {
			valor = Random.Range (0,100);
			if (valorExists(""+valor)==0) {
				spnames[ij]= ""+valor;
				
			} else {
				ij--;
			}
		}
	}

	void pickSprites(int itemsTS) {
		spselected = new string[itemsTS];
		sptomatch = new string[itemsTS];
		for (int ij = 0; ij <= itemsTS -1; ij++) {
			valor = Random.Range (0,49);
			if ((pickExists(""+valor,itemsTS)==0) && (IsNoBoardItem(valor)==0)) {
				spselected[ij]=""+valor;
			} else {
				ij--;
			}
		}
	}

	void buildBoard() {
		indice1 = 0;
		indice2 = 0;
		posX = -3.35f;
		posY = 1.874f;
		stepX = 0.6f;
		stepY = -0.6f;
		string strT;
		strT = "";
		for (int ij=0; ij <=48; ij++) {
			indice1 ++;
			posX=posX+stepX;
			drawBoardItems(posX,posY,spnames[ij],ij);
			strT = strT + "#" + spnames[ij];
			if (indice1 >=7) {
				//Debug.Log(strT + "\n");
				strT="";
				indice1=0;
				posX=-3.35f;
				posY=posY+stepY;
			}
		}
	}

	void StartSpawnSelected() {
		posX= 2.513f;
		posY=1.874f;
		stepX = 0.522f;
		stepY = -0.6f;
		itemsIndex = 0;
		InvokeRepeating("buildSelected", 1, 0.9f);
			
	
	}

	void buildSelected() {
        //posX= 2.513f;
        //posY=1.874f;
        //stepX = 0.522f;
        //stepY = -0.6f;

        if (itemsIndex == 4)
        {
            posX = 2.513f + stepX * 2;
        }
        drawSelectedItems(posX,posY,spselected[itemsIndex],itemsIndex);
		posX=posX+stepX;		
		itemsIndex++;

      


        if (itemsIndex >= itemsToSelect) {
			CancelInvoke("buildSelected");
			itemsIndex = 0;
		}
	}
	
	void hideParsecText() {
		goToDelete= GameObject.Find ("txtParsec");
		Destroy(goToDelete);
	}
	
	void drawBoardItems(float ondeX,float ondeY, string elItem, int posItem) {
		int spToDraw;
		if (IsNoBoardItem(posItem)==0) {
			GameObject miItem = Instantiate (boardItem, new Vector3 (ondeX, ondeY, -5), Quaternion.identity) as GameObject;
			miItem.name = "Item" + posItem + "_" + elItem;
			sr=miItem.GetComponent<SpriteRenderer> ();
			spToDraw = int.Parse( elItem) ;
			sr.sprite = sprites [spToDraw];
		} else {
			GameObject miItem = Instantiate (boardWall, new Vector3 (ondeX, ondeY, -5), Quaternion.identity) as GameObject;
			miItem.name = "Item" + posItem + "_" + elItem;
			sr=miItem.GetComponent<SpriteRenderer> ();
			spToDraw = int.Parse( elItem) ;
			sr.sprite=elWall;
		}
	}

	void drawSelectedItems(float ondeX,float ondeY, string elItem, int posItem) {
		int spToDraw,spFromBoard;
		spFromBoard=int.Parse(elItem); //What item on the board is selected
		GameObject miItem = Instantiate (boardSelection, new Vector3 (ondeX, ondeY, 0), Quaternion.identity) as GameObject;
		miItem.name = "Select" + posItem + "_" + spFromBoard;
		sr=miItem.GetComponent<SpriteRenderer> ();
		spToDraw = int.Parse( spnames[spFromBoard]) ;
		sr.sprite = sprites [spToDraw];
		sptomatch [itemsIndex] = ""+ spFromBoard;
	}

	public void CheckClicked(string strGuySelected,string strTheObjectClicked) {
		score++;
		Debug.Log ("Si llega a click" + strGuySelected);
		if (lookMatch (strGuySelected) == 1) {
			//Debug.Log ("Si hay match");
			myTimer+=3;
			gameObject.GetComponent<scPeopleScore>().goodMatch(strTheObjectClicked);
		} else {
			//Debug.Log ("NO hay match");
			gameObject.GetComponent<scPeopleScore>().badMatch(strTheObjectClicked);
		}

	}

	int lookMatch(string strCompare){
		int flag1;
		flag1 = 0;
		for (int ik=0; ik <=itemsToSelect -1; ik++) {
			if (sptomatch[ik] == strCompare) {
				flag1=1;
				//Destroy the object selected from the SelectedItems Window
				goToDelete= GameObject.Find ("Select" + ik + "_" + sptomatch[ik]);
				Destroy(goToDelete);
			}
		}
		return flag1;
	}

	int valorExists(string levalue) {
		siExiste = 0;
		for (int yi=0; yi <=49; yi++) {
			if (spnames[yi]==levalue) {
				siExiste=1;
			}
		}
		return siExiste;
	}

	int pickExists(string levalue,int itemsTS) {
		siExiste = 0;
		for (int yi=0; yi <=itemsTS -1; yi++) {
			if (spselected[yi]==levalue) {
				siExiste=1;
			}
		}
		return siExiste;
	}

}
