using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Valve.VR.InteractionSystem
{
	public class RecenteringCircularDrive : CircularDrive {

		[Range(0.0f, 0.05f)]
		public float proportionalRecentingControl = 0.02f;
		void Update () {
			if (!steering ) {
				outAngle = outAngle + (startAngle - outAngle) * proportionalRecentingControl;
				UpdateAll ();
			}

		}
	}
}