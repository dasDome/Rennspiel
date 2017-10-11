using UnityEngine;
using System.Collections;

public class FuelController : MonoBehaviour {
	public float maxFuel = 100f;
	public float currFuel = 0f;
	public float fuelConsumption = 10f;
	public float fuelBonus = 10f;
	public PlayerController playerCont;

	public GameObject fuelBar;

	// Use this for initialization
	void Start () {
		playerCont = GetComponent<PlayerController> ();
		currFuel = 90f;
		// calls every second
		InvokeRepeating ("consume", 0f, 0.15f);
	}

	void consume(){
		if (playerCont.getPlayerLife().Equals(LifeCycle.ALIVE)) {			
			currFuel--;
			float percentage = currFuel / maxFuel;
			if (percentage <= 0.0f) {
				playerCont.stopMove ();
				setFuelBar (0f);
			} else {
				if (percentage > 1) {
					percentage = 1;
				}
				setFuelBar (percentage);
			}
		}
	}

	public void incFuel(){
		currFuel += (currFuel + fuelBonus) >= maxFuel ? Mathf.Abs(currFuel - maxFuel) : fuelBonus;
	}
	public void decFuel(){
		currFuel -= fuelConsumption;
	}

	public void setFuelBar(float percentage){
		// amount between 0 and 1 for empty to full
		fuelBar.transform.localScale = new Vector3 (percentage, fuelBar.transform.localScale.y, fuelBar.transform.localScale.z);
	}
}
