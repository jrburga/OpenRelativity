using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class MicroSubControl : MonoBehaviour {

		private float targetSpeed = 0.0f;
		public float maxSpeed = 20.0f;

		private float rotationSpeed = 0.0f;
		public float maxRotationSpeed = 20.0f;

		private Vector3 driveVector;
		public GameObject linearMapping;
		private LinearMapping driveMapping;

		private float msRotation = 0.0f;
		private float msTargetRotation = 20.0f;
		public GameObject microSub;
		public GameObject wheel;
		private CircularDrive circularDrive;

		private Rigidbody body;

		public int speedOfLightTarget;

		GameState state;
		// Use this for initialization
		void Start () {
			state = GetComponent<GameState> ();
			driveMapping = linearMapping.GetComponent<LinearMapping> ();
			circularDrive = wheel.GetComponent<CircularDrive> ();

			body = gameObject.GetComponent<Rigidbody> ();

			speedOfLightTarget = (int)state.SpeedOfLight;
		}

		void UpdateControls () {
			if (driveMapping != null) {
				targetSpeed = driveMapping.value * maxSpeed;
			} else {
				driveMapping = linearMapping.GetComponent<LinearMapping> ();
			}

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

			body.velocity = transform.forward * targetSpeed;

		}
	}
}