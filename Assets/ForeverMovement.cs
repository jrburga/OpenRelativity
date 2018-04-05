﻿using UnityEngine;
using System.Collections;

public class ForeverMovement : MonoBehaviour
{
	//Consts 
	private const float ACCEL_RATE = 2f;
	private const int INIT_FRAME_WAIT = 5;
	private const float DEGREE_TO_RADIAN_CONST = 57.2957795f;
	public float controllerBoost=6000;
	//Affect our rotation speed
	public bool constraintXZ = false;
	public float rotSpeed;
	//Keep track of the camera transform
	public Transform camTransform;
	//What is our current target for the speed of light?
	public int speedOfLightTarget;
	//What is each step we take to reach that target?
	private float speedOfLightStep;
	//For now, you can change this how you like.
	public float mouseSensitivity;  
	//Keep track of total frames passed
	int frames;    
	//How fast are we going to shoot the bullets?
	public float viwMax = 3;
	//Gamestate reference for quick access
	GameState state;

	void Start()
	{
		//grab Game State, we need it for many actions
		state = GetComponent<GameState>();
		//Lock and hide cursor
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		//Set the speed of light to the starting speed of light in GameState
		speedOfLightTarget = (int)state.SpeedOfLight;

		viwMax = Mathf.Min(viwMax,(float)GameObject.FindGameObjectWithTag(Tags.player).GetComponent<GameState>().MaxSpeed);

		frames = 0;
	}
	//Again, use LateUpdate to solve some collision issues.
	void LateUpdate()
	{
		if(true)
		{
			float viewRotX = 0;
			//If we're not paused, update speed and rotation using player input.
			if(!state.MovementFrozen)
			{
				state.deltaRotation = Vector3.zero;

				#region ControlScheme

				//PLAYER MOVEMENT

				//If we press W, move forward, if S, backwards.
				//A adds speed to the left, D to the right. We're using FPS style controls
				//Here's hoping they work.

				//The acceleration relation is defined by the following equation
				//vNew = (v+uParallel+ (uPerpendicular/gamma))/(1+(v*u)/c^2)

				//Okay, so Gerd found a good equation that doesn't break when your velocity is zero, BUT your velocity has to be in the x direction.
				//So, we're gonna reuse some code from our relativisticObject component, and rotate everything to be at the X axis.

				//Cache our velocity
				Vector3 playerVelocityVector = state.PlayerVelocityVector;

				//Get our angle between the velocity and the X axis. Get the angle in degrees (radians suck)
				float rotationAroundX = DEGREE_TO_RADIAN_CONST * Mathf.Acos(Vector3.Dot(playerVelocityVector, Vector3.right) / playerVelocityVector.magnitude);



				//Make a Quaternion from the angle, one to rotate, one to rotate back. 
				Quaternion rotateX = Quaternion.AngleAxis(rotationAroundX, Vector3.Cross(playerVelocityVector, Vector3.right).normalized);
				Quaternion unRotateX = Quaternion.AngleAxis(rotationAroundX, Vector3.Cross(Vector3.right,playerVelocityVector).normalized);


				//If the magnitude's zero just make these angles zero and the Quaternions identity Q's
				if (playerVelocityVector.sqrMagnitude == 0)
				{
					rotationAroundX = 0;
					rotateX = Quaternion.identity;
					unRotateX = Quaternion.identity;
				}

				//Store our added velocity into temporary variable addedVelocity
				Vector3 addedVelocity = Vector3.zero;

				//Turn our camera rotation into a Quaternion. This allows us to make where we're pointing the direction of our added velocity.
				//If you want to constrain the player to just x/z movement, with no Y direction movement, comment out the next two lines
				//and uncomment the line below that is marked
				float cameraRotationAngle = -DEGREE_TO_RADIAN_CONST * Mathf.Acos(Vector3.Dot(camTransform.forward, Vector3.forward));
				Quaternion cameraRotation = Quaternion.AngleAxis(cameraRotationAngle, Vector3.Cross(camTransform.forward, Vector3.forward).normalized);

				//UNCOMMENT THIS LINE if you would like to constrain the player to just x/z movement.
				if (constraintXZ)
					cameraRotation = Quaternion.AngleAxis(camTransform.eulerAngles.y, Vector3.up);

				addedVelocity -= Vector3.forward*ACCEL_RATE*(float)Time.deltaTime;
//				state.keyHit = true;

				//Add the velocities here. remember, this is the equation:
				//vNew = (1/(1+vOld*vAddx/cSqrd))*(Vector3(vAdd.x+vOld.x,vAdd.y/Gamma,vAdd.z/Gamma))
				if (addedVelocity.sqrMagnitude != 0)
				{
					//Rotate our velocity Vector    
					Vector3 rotatedVelocity = rotateX * playerVelocityVector;
					//Rotate our added Velocity
					addedVelocity = rotateX * addedVelocity;

					//get gamma so we don't have to bother getting it every time
					float gamma = (float)state.SqrtOneMinusVSquaredCWDividedByCSquared;
					//Do relativistic velocity addition as described by the above equation.
					rotatedVelocity = (1 / (1 + (rotatedVelocity.x * addedVelocity.x) / (float)state.SpeedOfLightSqrd)) *
						(new Vector3(addedVelocity.x + rotatedVelocity.x, addedVelocity.y * gamma, gamma * addedVelocity.z));

					//Unrotate our new total velocity
					rotatedVelocity = unRotateX * rotatedVelocity;
					//Set it
					state.PlayerVelocityVector = rotatedVelocity;

				}
				//CHANGE the speed of light

				//Get our input axis (DEFAULT N, M) value to determine how much to change the speed of light
				int temp2 = (int)(Input.GetAxis("Speed of Light"));
				//If it's too low, don't subtract from the speed of light, and reset the speed of light
				if(temp2<0 && speedOfLightTarget<=state.MaxSpeed)
				{
					temp2 = 0;
					speedOfLightTarget = (int)state.MaxSpeed;
				}
				if(temp2!=0)
				{
					speedOfLightTarget += temp2;		

					speedOfLightStep = Mathf.Abs((float)(state.SpeedOfLight - speedOfLightTarget) / 20);
				}
				//Now, if we're not at our target, move towards the target speed that we're hoping for
				if (state.SpeedOfLight < speedOfLightTarget * .995)
				{
					//Then we changege the speed of light, so that we get a smooth change from one speed of light to the next.
					state.SpeedOfLight += speedOfLightStep;
				}
				else if (state.SpeedOfLight > speedOfLightTarget * 1.005)
				{
					//See above
					state.SpeedOfLight -= speedOfLightStep;
				}
				//If we're within a +-.05 distance of our target, just set it to be our target.
				else if (state.SpeedOfLight != speedOfLightTarget)
				{
					state.SpeedOfLight = speedOfLightTarget;
				}

				//If we have a speed of light less than max speed, fix it.
				//This should never happen
				if (state.SpeedOfLight < state.MaxSpeed)
				{
					state.SpeedOfLight = state.MaxSpeed;
				}


				#endregion

				//Send current speed of light to the shader
				Shader.SetGlobalFloat("_spdOfLight", (float)state.SpeedOfLight);

				if (Camera.main)
				{
					Shader.SetGlobalFloat("xyr", (float)Camera.main.pixelWidth / Camera.main.pixelHeight);
					Shader.SetGlobalFloat("xs", (float)Mathf.Tan(Mathf.Deg2Rad * Camera.main.fieldOfView / 2f));

					//Don't cull because at high speeds, things come into view that are not visible normally
					//This is due to the lorenz transformation, which is pretty cool but means that basic culling will not work.
					Camera.main.layerCullSpherical = true; 
					Camera.main.useOcclusionCulling = false;
				}


			}
		}



	}
}
