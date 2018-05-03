using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class MicroSubControl : MonoBehaviour {

		private const float DEGREE_TO_RADIAN_CONST = 57.2957795f;

		private int speedOfLightTarget;
		private float speedOfLightStep;

		private float targetSpeed = 0.0f;

		private float rotationSpeed = 0.0f;
		public float maxRotationSpeed;

		private Vector3 driveVector;
		public GameObject linearMapping;
		public GameObject lightSpeedMapping;
		public GameObject turnLinearMapping;
		private LinearMapping lightMapping;
		private LinearMapping driveMapping;
		private LinearMapping turnMapping;

		private float msRotation = 0.0f;
		private float msTargetRotation = 20.0f;
		public GameObject microSub;

		private Rigidbody body;

		private Vector3 deltaVelocity;

		public GameObject playerObject;

		// Use this for initialization

		Vector3 playerVelocityVector;
		GameState state;
		void Start () {
			state = playerObject.GetComponent<GameState>();
			driveMapping = linearMapping.GetComponent<LinearMapping> ();
			turnMapping = turnLinearMapping.GetComponent<LinearMapping> ();
			lightMapping = lightSpeedMapping.GetComponent<LinearMapping> ();
			body = gameObject.GetComponent<Rigidbody> ();

			speedOfLightTarget = (int)state.SpeedOfLight;
		}

		void UpdateControls () {
			UpdateForwardDrive ();
			UpdateTurningDrive ();
			UpdateLightSpeedControl ();
		}

		void UpdateForwardDrive () {
			if (driveMapping != null) {
				targetSpeed = driveMapping.value * (float)state.maxPlayerSpeed;
			} else {
				driveMapping = linearMapping.GetComponent<LinearMapping> ();
			}	
		}

		void UpdateTurningDrive () {
			if (turnMapping != null) {
				rotationSpeed = (turnMapping.value-.5f)*2f * maxRotationSpeed;
			} else {
				Debug.Log ("No turn mapping");
			}
		}

		void UpdateLightSpeedControl () {
			if (lightMapping != null) {
				speedOfLightTarget = (int) (state.totalC * (1.0f-lightMapping.value));
			} else {
				lightMapping = lightSpeedMapping.GetComponent<LinearMapping> ();
			}
		}

		void UpdateSpeedOfLight () {
			speedOfLightStep = Mathf.Abs((float)(state.SpeedOfLight - speedOfLightTarget) / 20);
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

			//Send current speed of light to the shader
			Debug.Log(state.SpeedOfLight);
			Shader.SetGlobalFloat("_spdOfLight", (float)state.SpeedOfLight);

		}

		void UpdateMovement () {
			transform.Rotate (rotationSpeed*Vector3.up * Time.deltaTime);

			Vector3 playerVelocityVector = state.PlayerVelocityVector;
			// Cache the player velocity vector (but not in a new variable)
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

			//Turn our camera rotation into a Quaternion. This allows us to make where we're pointing the direction of our added velocity.
			//If you want to constrain the player to just x/z movement, with no Y direction movement, comment out the next two lines
			//and uncomment the line below that is marked
			float rotationAngle = -DEGREE_TO_RADIAN_CONST * Mathf.Acos(Vector3.Dot(transform.forward, Vector3.forward));
			Quaternion rotation = Quaternion.AngleAxis(rotationAngle, Vector3.Cross(transform.forward, Vector3.forward).normalized);

			deltaVelocity = getDeltaVelocity();

			//Add the velocities here. remember, this is the equation:
			//vNew = (1/(1+vOld*vAddx/cSqrd))*(Vector3(vAdd.x+vOld.x,vAdd.y/Gamma,vAdd.z/Gamma))
			if (deltaVelocity.sqrMagnitude != 0)
			{
				//Rotate our velocity Vector    
				Vector3 rotatedVelocity = rotateX * playerVelocityVector;
				//Rotate our added Velocity
				deltaVelocity = rotateX * deltaVelocity;

				//get gamma so we don't have to bother getting it every time
				float gamma = (float)state.SqrtOneMinusVSquaredCWDividedByCSquared;
				//Do relativistic velocity addition as described by the above equation.
				rotatedVelocity = (1 / (1 + (rotatedVelocity.x * deltaVelocity.x) / (float)state.SpeedOfLightSqrd)) *
					(new Vector3(deltaVelocity.x + rotatedVelocity.x, deltaVelocity.y * gamma, gamma * deltaVelocity.z));

				//Unrotate our new total velocity
				rotatedVelocity = unRotateX * rotatedVelocity;
				//Set it
				state.PlayerVelocityVector = rotatedVelocity;

			}
		}

		void UpdateMicroSub() {

			// To broken to use currently. The problem is using the right angles and such.
			msRotation = Vector3.Angle (microSub.transform.up, transform.up);
			microSub.transform.Rotate (-(rotationSpeed-msRotation)* rotationSpeed * Vector3.forward * Time.deltaTime);
		}

		Vector3 getDeltaVelocity() {
			return transform.forward*targetSpeed - body.velocity;
		}

		// Update is called once per frame
		void LateUpdate () {
			UpdateControls ();
			UpdateMovement ();
			UpdateSpeedOfLight ();
//			UpdateMicroSub ();
			if (Camera.main)
			{
				Shader.SetGlobalFloat("xyr", (float)Camera.main.pixelWidth / Camera.main.pixelHeight);
				Shader.SetGlobalFloat("xs", (float)Mathf.Tan(Mathf.Deg2Rad * Camera.main.fieldOfView / 2f));

				//Don't cull because at high speeds, things come into view that are not visible normally
				//This is due to the lorenz transformation, which is pretty cool but means that basic culling will not work.
				Camera.main.layerCullSpherical = true; 
				Camera.main.useOcclusionCulling = false;
			}

//			body.velocity = transform.forward * targetSpeed;
		}
	}
}