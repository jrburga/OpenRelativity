using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRMovementScript : MonoBehaviour {
	
	//Consts 
	private const int INIT_FRAME_WAIT = 5;
	private const float DEGREE_TO_RADIAN_CONST = 57.2957795f;
	public float controllerBoost=6000;
	//Affect our rotation speed
	public float rotSpeed;
	//Keep track of the camera transform
	public Transform camTransform;
	//Just turn this negative when they press the Y button for inversion.
	private int inverted;
	//What is our current target for the speed of light?
	public int speedOfLightTarget;
	//What is each step we take to reach that target?
	private float speedOfLightStep;
	//For now, you can change this how you like.
	public float mouseSensitivity;
	//So we can use getAxis as keyHit function
	public bool invertKeyDown = false;    
	//Keep track of total frames passed
	int frames;    
	//How fast are we going to shoot the bullets?
	public float viwMax = 3;
	//Gamestate reference for quick access
	GameState state;

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
