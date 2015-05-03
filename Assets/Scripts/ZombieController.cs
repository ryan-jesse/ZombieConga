using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ZombieController : MonoBehaviour {

	public float moveSpeed;
	public float turnSpeed;

	private Vector3 moveDirection;

	[SerializeField]
	private PolygonCollider2D[] colliders;
	private int currentColliderIndex = 0;

	private List<Transform> congaLine = new List<Transform>();

	private bool isInvincible = false;
	private float timeSpentInvincible;

	private int lives = 3;
	private int level = 1;

	public AudioClip enemyContactSound;
	public AudioClip catContactSound;

	private Text scoreLabel;
	private Text livesLabel;
	private Text levelLabel;

	// Use this for initialization
	void Start () {
		moveDirection = Vector3.right;
		scoreLabel = GameObject.Find ("Canvas").GetComponentsInChildren<Text>()[0];
		livesLabel = GameObject.Find ("Canvas").GetComponentsInChildren<Text>()[1];
		levelLabel = GameObject.Find ("Canvas").GetComponentsInChildren<Text>()[2];
	}
	
	// Update is called once per frame
	void Update () {

		// 1
		Vector3 currentPosition = transform.position;
		// 2
		if( Input.GetButton("Fire1") ) {
			// 3
			Vector3 moveToward = Camera.main.ScreenToWorldPoint( Input.mousePosition );
			// 4
			moveDirection = moveToward - currentPosition;
			moveDirection.z = 0; 
			moveDirection.Normalize();
		}

		Vector3 target = moveDirection * moveSpeed + currentPosition;
		transform.position = Vector3.Lerp( currentPosition, target, Time.deltaTime );

		float targetAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
		transform.rotation = 
			Quaternion.Slerp( transform.rotation, 
			                 Quaternion.Euler( 0, 0, targetAngle ), 
			                 turnSpeed * Time.deltaTime );

		EnforceBounds();

		//1
		if (isInvincible)
		{
			//2
			timeSpentInvincible += Time.deltaTime;
			
			//3
			if (timeSpentInvincible < 3f) {
				float remainder = timeSpentInvincible % .3f;
				GetComponent<Renderer>().enabled = remainder > .15f; 
			}
			//4
			else {
				GetComponent<Renderer>().enabled = true;
				isInvincible = false;
			}
		}

		//Updating GUI Elements
		GameObject.Find ("number").GetComponent<DisplayLives>().Set (lives);
		scoreLabel.text = "Score: " + congaLine.Count.ToString ();
		levelLabel.text = "Level " + level;
		
		Vector3 scorePos = new Vector3(Screen.width * 0.16f, Screen.height * 0.92f, Camera.main.nearClipPlane);
		scoreLabel.transform.position = scorePos;
		
		Vector3 livesPos = new Vector3(Screen.width * 0.93f, Screen.height * 0.92f, Camera.main.nearClipPlane);
		livesLabel.transform.position = livesPos;

		Vector3 levelPos = new Vector3(Screen.width * 0.55f, Screen.height * 0.92f, Camera.main.nearClipPlane);
		levelLabel.transform.position = levelPos;
	}

	public void SetColliderForSprite( int spriteNum )
	{
		colliders[currentColliderIndex].enabled = false;
		currentColliderIndex = spriteNum;
		colliders[currentColliderIndex].enabled = true;
	}

	void OnTriggerEnter2D( Collider2D other )
	{
		if(other.CompareTag("cat")) {
			GetComponent<AudioSource>().PlayOneShot(catContactSound);

			Transform followTarget = congaLine.Count == 0 ? transform : congaLine[congaLine.Count-1];
			other.transform.parent.GetComponent<CatController>().JoinConga( followTarget, moveSpeed, turnSpeed );

			congaLine.Add( other.transform );

			if (congaLine.Count >= 5) {
				Application.LoadLevel("WinScene");
			}
		}
		else if(!isInvincible && other.CompareTag("enemy")) {
			GetComponent<AudioSource>().PlayOneShot(enemyContactSound);

			isInvincible = true;
			timeSpentInvincible = 0;

			for( int i = 0; i < 2 && congaLine.Count > 0; i++ )
			{
				int lastIdx = congaLine.Count-1;
				Transform cat = congaLine[ lastIdx ];
				congaLine.RemoveAt(lastIdx);
				cat.parent.GetComponent<CatController>().ExitConga();
			}

			if (--lives <= 0) {
				Application.LoadLevel("LoseScene");
			}
		}
	}
	
	private void EnforceBounds()
	{
		// 1
		Vector3 newPosition = transform.position; 
		Camera mainCamera = Camera.main;
		Vector3 cameraPosition = mainCamera.transform.position;
		
		// 2
		float xDist = mainCamera.aspect * mainCamera.orthographicSize; 
		float xMax = cameraPosition.x + xDist;
		float xMin = cameraPosition.x - xDist;
		
		// 3
		if ( newPosition.x < xMin || newPosition.x > xMax ) {
			newPosition.x = Mathf.Clamp( newPosition.x, xMin, xMax );
			moveDirection.x = -moveDirection.x;
		}
		// TODO vertical bounds
		float yMax = mainCamera.orthographicSize;
		
		if (newPosition.y < -yMax || newPosition.y > yMax) {
			newPosition.y = Mathf.Clamp( newPosition.y, -yMax, yMax );
			moveDirection.y = -moveDirection.y;
		}

		// 4
		transform.position = newPosition;
	}
}
