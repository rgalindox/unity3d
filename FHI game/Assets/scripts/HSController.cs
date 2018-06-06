using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HSController : MonoBehaviour {

    private string secretKey = "hakunamatata"; // Edit this value and make sure it's the same as the one stored on the server
    public string addScoreURL = "http://games.adnsystems.com/addscore.php?"; //be sure to add a ? to your url
    public string highscoreURL = "http://games.adnsystems.com/display.php";

    public GameObject goScoreT,getInput;
    private Text gScore,gInput;
    private int ScoreP;

    void Start()
    {
        StartCoroutine(GetScores());
    }

    public void addScore()
    {

        int scoreT;
        string nameT;
        gInput = getInput.GetComponent<Text>();
        Debug.Log("Input" + gInput.text);
        scoreT = ScoreP = scPeopleScore.TheScore;
        nameT = gInput.text;
        StartCoroutine(PostScores(nameT, scoreT));

    }

    // remember to use StartCoroutine when calling this function!
    IEnumerator PostScores(string name, int score)
    {
        //This connects to a server side php script that will add the name and score to a MySQL DB.
        // Supply it with a string representing the players name and the players score.
        //string hash = MD5Test.Md5Sum(name + score + secretKey);
        string hash = "1";

        string post_url = addScoreURL + "name=" + WWW.EscapeURL(name) + "&score=" + score + "&hash=" + hash;

        // Post the URL to the site and create a download object to get the result.
        WWW hs_post = new WWW(post_url);
        yield return hs_post; // Wait until the download is done

        if (hs_post.error != null)
        {
            print("There was an error posting the high score: " + hs_post.error);
        } else
        {
            print("Success!!");
            StartCoroutine(GetScores());

        }
    }

    // Get the scores from the MySQL DB to display in a GUIText.
    // remember to use StartCoroutine when calling this function!
    IEnumerator GetScores()
    {
        


        Debug.Log("Loading Scores");
        WWW hs_get = new WWW(highscoreURL);
        yield return hs_get;

        if (hs_get.error != null)
        {
            print("There was an error getting the high score: " + hs_get.error);
        }
        else
        {
            // gameObject.guiText.text = hs_get.text; // this is a GUIText that will display the scores in game.
            Debug.Log(hs_get.text);
            gScore = goScoreT.GetComponent<Text>();
            gScore.text = hs_get.text;
        }
    }
}
