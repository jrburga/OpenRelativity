using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRCamCalibration : MonoBehaviour {

	public Camera VRCam;
	public Transform Player;
	public Vector3 Offset;
	// Use this for initialization


	void CalibrateCam() {
		Player.parent = null;
		transform.localPosition = VRCam.transform.localPosition + Offset;
		Player.parent = transform;
//		transform.localPosition = VRCam.transform.localPosition;
	}
	void Start () {
		CalibrateCam ();
	}

}
