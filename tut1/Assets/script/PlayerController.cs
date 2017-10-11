using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;


public enum LifeCycle {ALIVE,DEAD};
public enum Effect {SHIELD,INVERSE,EXPLODE};

public class ShieldEffect : MonoBehaviour{

}

public class PlayerController : MonoBehaviour {

	public float speedVertical;
	private Rigidbody rb;
	private double score;
    private float highscore;
	public Text scoreText;
	public Text statusText;
	public Text winText;
    public Text highscoreText;
	private ParticleSystem explosion;
	private GameObject explosionContainer;
	private GameObject btnContainer;
	public float moveVertical = 1f;
	public float speedHorizontal = 1f;
	public float unlock = 2000f;
	public FuelController fuelCont;
	public AudioClip deathSound;
	public AudioClip endSound;
	public AudioClip fuelPickSound;
	public AudioClip inverseSound;
	public AudioClip shieldSound;
	public GameObject MapGenerator;
	private AudioSource effectSound;
	private int inv = 1;
	public LifeCycle PlayerLife;
	private List<Effect> PlayerEffects;
	float moveHorizontal;
	public float gyroSensitivity = 2.5f;
	public Dictionary<Effect,string> EffectTextmessage;
	public GameObject shieldGameObject;

	public LifeCycle getPlayerLife(){
		return PlayerLife;
	}

	// Use this for initialization
	void Start () {
		effectSound = GetComponent<AudioSource> ();
		rb = GetComponent<Rigidbody>();
		fuelCont = GetComponent<FuelController>();
		score = 0;
		highscore = PlayerPrefs.GetFloat("Highscore");
		PlayerEffects = new List<Effect>();
		EffectTextmessage = new Dictionary<Effect,string> ();
		EffectTextmessage.Add (Effect.INVERSE, "INVERSE!");
		EffectTextmessage.Add (Effect.SHIELD, "SHIELD!");
		PlayerLife = LifeCycle.ALIVE;
        setScore ();
		winText.text="";
		scoreText.text = "0";
		highscoreText.text = "" + highscore;
		statusText.text = "";
		explosion = this.gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<ParticleSystem>();
		explosionContainer = GameObject.Find ("explosionContainer");
		explosionContainer.SetActive (false);
		btnContainer = GameObject.Find ("Buttons");
		btnContainer.SetActive (false);
		shieldGameObject.GetComponent<MeshRenderer> ().material.color = new Color (255, 255, 255, 0f);
	}
	
	// Update is called once per frame
	void Update () {
      if (highscore < score)
        {
            highscore = (float) score;
            PlayerPrefs.SetFloat("Highscore", highscore);
        }
        calcScore ();
    }
	void FixedUpdate(){
		/*****************************
		 * Keyboard Movement
		 * ***************************/
		// apply movement with drag
				
			#if UNITY_STANDALONE
				if (PlayerLife.Equals (LifeCycle.ALIVE)) {
					//Debug.Log("Desktop Version");
					Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
					rb.AddForce (movement*speedHorizontal);
					// grab movement
					moveHorizontal = (Input.GetAxis ("Horizontal") * 2f * inv * speedHorizontal);
					// without drag
					rb.velocity = new Vector3 (moveHorizontal, 0.0f, moveVertical) * speedVertical;
				}
			#endif
			/*****************************
			 * Mobile gyro Movement
			 * ***************************/
			#if UNITY_ANDROID
				if (PlayerLife.Equals (LifeCycle.ALIVE)) {	
					//Debug.Log("Andrioid Version");
					moveHorizontal = Input.acceleration.x * gyroSensitivity;
					transform.Translate(moveHorizontal * inv, 0, moveVertical);
				}
			#endif
			calcScore ();		
	}

	void OnTriggerEnter(Collider other)	{
		if (other.gameObject.CompareTag ("fuel")) {
			other.gameObject.SetActive (false);
			setScore ();
            fuelCont.incFuel ();
			score++;
			effectSound.PlayOneShot (fuelPickSound);
		}
		if (other.gameObject.CompareTag ("obstacle")) {
			setScore ();
			if (!PlayerEffects.Contains(Effect.SHIELD)) {
				if (!PlayerLife.Equals(LifeCycle.DEAD)) {
					//PlayerEffects.Add (Effect.EXPLODE);
					explosionContainer.SetActive (true);
					explosion.Stop ();
					explosion.Play ();
					effectSound.PlayOneShot (deathSound);
					effectSound.PlayOneShot (endSound);
					PlayerLife = LifeCycle.DEAD;
					stopMove ();
				}
			} else {
				other.gameObject.SetActive (false);
				effectSound.PlayOneShot (deathSound);
				endEffect (Effect.SHIELD);		 
			}
		}
		/*
		if (other.gameObject.CompareTag ("finish")) {
			//Time.timeScale = 0;
			btnContainer.SetActive (true);
			winText.text="FINISHLINE!!";
		}*/
		if (other.gameObject.CompareTag ("inverse")) {			
			if (!PlayerEffects.Contains(Effect.INVERSE)) {
				startEffect (Effect.INVERSE);
				effectSound.PlayOneShot (inverseSound);
			} else {
				endEffect (Effect.INVERSE);	
			}
			other.gameObject.SetActive (false);
		}
		if (other.gameObject.CompareTag ("shield")) {
			if (!PlayerEffects.Contains(Effect.SHIELD)) {
				startEffect (Effect.SHIELD);
				effectSound.PlayOneShot (shieldSound);
			}
			other.gameObject.SetActive (false);
		}
	}

	void calcScore(){
		score = System.Math.Round(transform.position.z, 2);
		setScore ();
        if (score >= unlock) {
			//winText.text="Congratulations unlocked new area!!";
		}
	}

	void setScore(){
		scoreText.text = Mathf.Round((float)score).ToString ();
	}

	void startEffect(Effect effectToStart){
		switch (effectToStart) {
			case Effect.SHIELD:
				shieldGameObject.SetActive (true);
				break;	
			case Effect.INVERSE:
				inv = inv * (-1);
				break;
		}
		PlayerEffects.Add (effectToStart);
		statusText.text = EffectTextmessage [effectToStart];
		StartCoroutine (deactivateEffect (effectToStart));
	}

	private IEnumerator deactivateEffect(Effect effect){
		yield return new WaitForSeconds (10);
		switch (effect)  {
		case Effect.SHIELD:				
			endEffect (effect);
			break;	
		case Effect.INVERSE:
			if(PlayerEffects.Contains(Effect.INVERSE)){
				endEffect (effect);
			}
			break;
		}
	}

	void endEffect(Effect effectToEnd){
		switch (effectToEnd)  {
			case Effect.SHIELD:				
				shieldGameObject.SetActive (false);
				break;	
			case Effect.INVERSE:
				inv = inv * (-1);
				break;
		}
		PlayerEffects.Remove (effectToEnd);
		statusText.text = "";
	}

	public void stopMove(){
		speedVertical = 0f;
		speedHorizontal = 0f;
		rb.velocity = Vector3.zero;
		winText.text="You died!";
		btnContainer.SetActive (true);
	}
}
