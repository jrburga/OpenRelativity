using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class MicroSubControl : MonoBehaviour {

		private const float DEGREE_TO_RADIAN_CONST = 57.2957795f;

		private float targetSpeed = 0.0f;
		public float maxSpeed = 20.0f;

		private float rotationSpeed = 0.0f;
		public float maxRotationSpeed = 40.0f;

		private Vector3 driveVector;
		public GameObject linearMapping;
		private LinearMapping driveMapping;

		private float msRotation = 0.0f;
		private float msTargetRotation = 20.0f;
		public GameObject microSub;
		public GameObject wheel;
		private CircularDrive circularDrive;

		private Rigidbody body;

		private Vector3 deltaVelocity;

		// Use this for initialization

		Vector3 playerVelocityVector;
		GameState state;
		void Start () {
			state = GetComponent<GameState>();
			driveMapping = linearMapping.GetComponent<LinearMapping> ();
			circularDrive = wheel.GetComponent<CircularDrive> ();
			body = gameObject.GetComponent<Rigidbody> ();
		}

		void UpdateControls () {
			UpdateForwardDrive ();
			UpdateTurningDrive ();
		}

		void UpdateForwardDrive () {
			if (driveMapping != null) {
				targetSpeed = driveMapping.value * maxSpeed;
			} else {
				driveMapping = linearMapping.GetComponent<LinearMapping> ();
			}	
		}

		void UpdateTurningDrive () {
			if (circularDrive != null) {
				
				rotationSpeed = circularDrive.linearMapping.value * maxRotationSpeed;
			} else {
				circularDrive = wheel.GetComponent<CircularDrive> ();
			}
		}

		void UpdateMicroSub() {

			// To broken to use currently. The problem is using the right angles and such.
			msRotation = Vector3.Angle (microSub.transform.up, transform.up);
			microSub.transform.Rotate (-(rotationSpeed-msRotation)* rotationSpeed * Vector3.forward * Time.deltaTime);
		}

		// Update is called once per frame
		void LateUpdate () {
			UpdateControls ();
//			UpdateMicroSub ();

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

			deltaVelocity = transform.forward*targetSpeed - body.velocity;

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

			body.velocity = transform.forward * targetSpeed;
		}
	}
}