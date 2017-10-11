using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class sceneController : MonoBehaviour {
	public Object lvl1;
	public Object lvl2;
	public Object lvl3;
	//used in levels
	public void returnToHub(){
		SceneManager.LoadScene("Hub");
	}
	public void restartGame(){
		Scene scene = SceneManager.GetActiveScene(); 
		SceneManager.LoadScene(scene.name, LoadSceneMode.Single);
	}


	// hubworld
	public void loadLvl1(){
		SceneManager.LoadScene("final");
	}
	public void loadLvl2(){
		SceneManager.LoadScene(lvl2.name);
	}
	public void loadLvl3(){
		SceneManager.LoadScene(lvl3.name);
	}
}
