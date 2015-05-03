using UnityEngine;
using System.Collections;

public class DisplayLives : MonoBehaviour {
	
	public Sprite[] sprites;
	private SpriteRenderer spriteRenderer;
	
	void Start () {
		spriteRenderer = GetComponent<Renderer>() as SpriteRenderer;
		Set (3);
	}
	
	public void Set (int number) {
		switch (number)
		{
		case 3:
			spriteRenderer.sprite = sprites[0];
			break;
		case 2:
			spriteRenderer.sprite = sprites[1];
			break;
		case 1:
			spriteRenderer.sprite = sprites[2];
			break;
		case 0:
			spriteRenderer.sprite = sprites[3];
			break;
		}
	}
	
	void Update () {
		Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.93f, Screen.height * 0.93f, Camera.main.nearClipPlane));
		transform.position = pos;
	}
}