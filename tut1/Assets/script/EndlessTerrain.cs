using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour {

	public const float maxViewDst = 200;
	public Transform viewer;
	public static Vector2 viewerPosition;
	public GameObject obsticalZero;
	public GameObject obsticalOne;
	public GameObject ground;
	public GameObject specialFuel;
	public GameObject specialShield;
	public GameObject specialReverse;
	private GameLevel currentGameLevel;
	int chunkSize;
	int chunksVisibleInViewDst;
	int xDestructionThreshold;
	int yDestructionThreshold;
	int offsetObjectrate = 0;
	List<GameLevel> gameLevel = new List<GameLevel>();

	Dictionary<Vector2,TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2,TerrainChunk>();
	List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

	void Start(){
		chunkSize = 21 - 1;//421 - 1;
		chunksVisibleInViewDst = Mathf.RoundToInt (maxViewDst / chunkSize);
		xDestructionThreshold = chunksVisibleInViewDst - 1;
		yDestructionThreshold = 2;

		initGameLevel();
	}

	void Update(){
		//Playerobject
		viewerPosition = new Vector2 (viewer.position.x, viewer.position.z);
		//Level up
		if(isCheckpoint(viewerPosition.y) & viewerPosition.y >= 0){			
			//player.speedVertical += 10;
		}
		//Debug.Log(currentGameLevel.GetDistance () + "  -  " + (currentGameLevel.GetObjectRate()+offsetObjectrate));
		UpdateVisibleChunks ();
	}

	void UpdateVisibleChunks(){

		//Deaktiviert alle sichtbaren Chunks vom letzten Update
		for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
			terrainChunksVisibleLastUpdate [i].SetVisible (false);
		}

		terrainChunksVisibleLastUpdate.Clear ();

		//Current Chunkcoord (x/y) --> Playposition / Chunksize
		int currentChunkCoordX = Mathf.RoundToInt (viewerPosition.x / chunkSize);
		int currentChunkCoordY = Mathf.RoundToInt (viewerPosition.y / chunkSize);

		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) {
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) {				
				Vector2 viewedChunkCoord = new Vector2 (currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

				//Check if Chunk already exist according to chunkcoord
				if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)) {
					if((viewedChunkCoord.y < currentChunkCoordY - yDestructionThreshold) || ((viewedChunkCoord.x < currentChunkCoordX - xDestructionThreshold)) || ((viewedChunkCoord.x > currentChunkCoordX + xDestructionThreshold))) {						
						terrainChunkDictionary [viewedChunkCoord].destroyChunk ();
						terrainChunkDictionary.Remove (viewedChunkCoord);
					}else{
						terrainChunkDictionary [viewedChunkCoord].UpdateTerrainChunk ();
						if (terrainChunkDictionary [viewedChunkCoord].IsVisible ()) {						
							terrainChunksVisibleLastUpdate.Add (terrainChunkDictionary [viewedChunkCoord]);
						}
					}
				} else {
					if (!(viewedChunkCoord.y < currentChunkCoordY - yDestructionThreshold) && (!(viewedChunkCoord.x < currentChunkCoordX - xDestructionThreshold)) && (!(viewedChunkCoord.x > currentChunkCoordX + xDestructionThreshold))) {
						terrainChunkDictionary.Add (viewedChunkCoord, new TerrainChunk (viewedChunkCoord, chunkSize, createNewChunk ()));
					}
				}

			}
		}
	}

	private bool isCheckpoint(float posZ){
		bool passed = false;
		foreach(GameLevel gLevel in gameLevel){
			if (!gLevel.IsPassed() && gLevel.GetDistance () - 4 < posZ & gLevel.GetDistance () + 4 > posZ) {
				passed = true;
				gLevel.SetPassed (true);
				currentGameLevel = gLevel;
			}
		}
		return passed;
	}

	private GameObject createNewChunk(){
		int miniChunkSize = chunkSize / 3;
		//Prefab Floor
		GameObject plane = GameObject.Instantiate(ground);
		GameObject item = null;

		//Floor skaliert für Chunksize
		plane.transform.localScale = Vector3.one * chunkSize / 10f;
		Vector3 planeCoord = plane.transform.position;

		for (int i = -1; i <= 1; i++) {
			for (int j = -1; j <= 1; j++) {
				int percentageObjectrate = Random.Range (0, 100);
				float heightOffset = 1;
				if (percentageObjectrate < (currentGameLevel.GetObjectRate() + offsetObjectrate)) {
					if (Random.Range (0, 2) > 0) {
						item = GameObject.Instantiate (obsticalOne);
					} else {
						item = GameObject.Instantiate (obsticalZero);
					}
					heightOffset = 0.5f;
					int randomScale = Random.Range (0, 100);
					randomScale = randomScale > 94 ? 10 : Mathf.CeilToInt(((randomScale/2) / 10) + 0.1f);
					int randomRotationX = Random.Range (-45,45);
					int randomRotationY = Random.Range (-90,90);
					int randomRotationZ = Random.Range (-45,45);
					randomRotationX = randomRotationX > 40 ? 0 : randomRotationX;
					randomRotationZ = randomRotationZ > 40 ? 0 : randomRotationZ;
					item.transform.localScale = new Vector3 (item.transform.localScale.x*randomScale, item.transform.localScale.y*randomScale, item.transform.localScale.z*randomScale);
					item.transform.Rotate (randomRotationX, randomRotationY, randomRotationZ);
				} else {
					int percentageSpace = Random.Range (0, 100);
					//Wenn Wert kleiner als Spacerate --> Item
					//Spacerate sagt aus viel Prozent Items gespawnt werden
					if (percentageSpace < currentGameLevel.GetSpaceRate()) {
						int percentageItem = Random.Range (0, 100);
						heightOffset = 1f;
						if (percentageItem < currentGameLevel.GetFuelTo() & percentageItem >= currentGameLevel.GetFuelFrom() ) {
							//fuel
							item = GameObject.Instantiate (specialFuel) as GameObject;
							item.transform.position = new Vector3 (item.transform.position.x, 1, item.transform.position.z);
						} else if (percentageItem > currentGameLevel.GetShieldFrom() & percentageItem < currentGameLevel.GetShieldTo()) {
							//shield
							item = GameObject.Instantiate (specialShield) as GameObject;
							item.transform.position = new Vector3 (item.transform.position.x, 1, item.transform.position.z);
						} else if (percentageItem > currentGameLevel.GetReverseFrom() & percentageItem < currentGameLevel.GetReverseTo()) {
							//reverse
							item = GameObject.Instantiate (specialReverse) as GameObject;
						} else {
							item = null;
						}
					} else {
						item = null;
					}
				}
				if (item != null) {
					//Zufallpositionierung innerhalb eines "Minichunks"
					int randomX = Random.Range (-miniChunkSize / 2, miniChunkSize / 2);
					int randomZ = Random.Range (-miniChunkSize / 2, miniChunkSize / 2);

					Vector3 pos = new Vector3 (planeCoord.x + (j * miniChunkSize) - randomX, heightOffset, planeCoord.z + (i * miniChunkSize) - randomZ);
					item.transform.position = pos;
					item.transform.parent = plane.transform;
				}

			}
		}
		return plane;
	}

	public void restGeneration(){
		initGameLevel();
	}

	public void initGameLevel(){
		gameLevel.Clear();
		//Distanz, Obstaclerate (Zero/One), Itemrate (Rest Whitespace), [Fuel, Shield, Reverse] --> Immer 100%  
		/*
		gameLevel.Add (new GameLevel (0,0,0,100,0,0));
		gameLevel.Add (new GameLevel (10,1,0,90,5,5));
		gameLevel.Add (new GameLevel (200,2,0,90,5,5));
		gameLevel.Add (new GameLevel (600,3,0,80,10,10));
		gameLevel.Add (new GameLevel (800,5,0,80,10,10));
		gameLevel.Add (new GameLevel (1000,6,0,70,15,15));
		gameLevel.Add (new GameLevel (1400,8,0,70,15,15));
		gameLevel.Add (new GameLevel (1800,10,0,70,12,18));
		gameLevel.Add (new GameLevel (2500,12,0,70,10,20));
		gameLevel.Add (new GameLevel (3000,14,0,70,10,20));
		*/

		gameLevel.Add (new GameLevel (0,0,1,100,0,0));
		gameLevel.Add (new GameLevel (10,1,1,90,5,5));
		gameLevel.Add (new GameLevel (200,2,1,90,5,5));
		gameLevel.Add (new GameLevel (600,3,1,80,10,10));
		gameLevel.Add (new GameLevel (800,5,2,80,10,10));
		gameLevel.Add (new GameLevel (1000,6,2,70,15,15));
		gameLevel.Add (new GameLevel (1400,8,3,70,15,15));
		gameLevel.Add (new GameLevel (1800,10,3,70,12,18));
		gameLevel.Add (new GameLevel (2500,12,3,70,10,20));
		gameLevel.Add (new GameLevel (3000,14,4,70,10,20));

		currentGameLevel = null;
	}
}

public class TerrainChunk{

	GameObject meshObject;
	Vector2 position;
	Bounds bounds;

	public TerrainChunk(Vector2 coord, int size,GameObject komObject){
		position = coord * size;
		bounds = new Bounds (position, Vector2.one * size);
		Vector3 positionV3 = new Vector3 (position.x, 0, position.y);

		meshObject = komObject;
		meshObject.transform.position = positionV3;
		meshObject.transform.localScale = Vector3.one * size / 10f;
		SetVisible (false);
	}

	public void UpdateTerrainChunk(){
		float viewerDstFromNearstEdge = Mathf.Sqrt (bounds.SqrDistance (EndlessTerrain.viewerPosition));
		bool visible = viewerDstFromNearstEdge <= EndlessTerrain.maxViewDst;
		SetVisible (visible);
	}

	public void SetVisible(bool visible){

		//Damit rotieren sie--> SetActive Bug?
		/*Color currentColor;
		if (visible) {			
			float t = Time.time / Time.time + 5;
			Color startColor = meshObject.GetComponent<MeshRenderer> ().material.color;
			meshObject.GetComponent<MeshRenderer> ().material.color = new Color (startColor.r, startColor.g, startColor.b, 0.0f);
			Color endColor = new Color (startColor.r, startColor.g, startColor.b, 1.0f);
			currentColor = Color.Lerp (startColor, endColor, t);   
		} else {
			float t = Time.time / Time.time + 5;
			Color startColor = meshObject.GetComponent<MeshRenderer> ().material.color;
			meshObject.GetComponent<MeshRenderer> ().material.color = new Color (startColor.r, startColor.g, startColor.b, 1.0f);
			Color endColor = new Color (startColor.r, startColor.g, startColor.b, 0.0f);
			currentColor = Color.Lerp (startColor, endColor, t); 
		}

		meshObject.GetComponent<MeshRenderer> ().material.SetColor("_Color", currentColor);*/    
		meshObject.SetActive (visible);
	}

	public bool IsVisible(){
		return meshObject.activeSelf;
	}

	public void destroyChunk(){
		GameObject.Destroy (meshObject);
	}
}

public class GameLevel{
	private int goalDistance;
	private bool passed;
	private int objectRate;
	private int spaceRate;
	private int itemRateFuel;
	private int itemRateShield;
	private int itemRateReverse;

	//+Die prozentualen Werte geben für Level
	//-Items Abstufung aufteilen (Fuel, Shield, Reverse)

	public GameLevel(int distance, int _percentageObjectrate, int _percentageSpace, int _percentageItemFuel, int _percentageItemShield, int _percentageItemReverse){
		goalDistance = distance;
		passed = false;
		objectRate = _percentageObjectrate;
		spaceRate = _percentageSpace;
		itemRateFuel = _percentageItemFuel;
		itemRateShield = _percentageItemShield;
		itemRateReverse = _percentageItemReverse;
	}

	public bool IsPassed(){
		return passed;
	}
	public void SetPassed(bool _passed){
		passed = _passed;
	}
	public int GetDistance(){
		return goalDistance;
	}
	public int GetObjectRate(){
		return objectRate;
	}
	public int GetSpaceRate(){
		return spaceRate;
	}

	public int GetFuelFrom(){
		return 0;
	}
	public int GetFuelTo(){
		return itemRateFuel;
	}
	public int GetShieldFrom(){
		return itemRateFuel-1;
	}
	public int GetShieldTo(){
		return itemRateFuel + itemRateShield;
	}
	public int GetReverseFrom(){
		return itemRateFuel + itemRateShield - 1;
	}
	public int GetReverseTo(){
		return 100;
	}
}
