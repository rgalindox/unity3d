using UnityEngine;
using System.Collections;

public class scMainMenu : MonoBehaviour {

	public void MainLevel(){
		Application.LoadLevel("lab01");
	}

    public void HowToLevel()
    {
        Application.LoadLevel("scnHowTo");
    }
}
