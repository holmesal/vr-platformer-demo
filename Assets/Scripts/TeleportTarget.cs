using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class TeleportTarget : MonoBehaviour {
	
	public bool animateTeleport = true;
	public float teleportDuration = 1.0f;
	
	public Transform cameraRig;
	
	public Transform target;
	
	SteamVR_ControllerActions leftControllerActions;
	SteamVR_ControllerActions rightControllerActions;

	public void TeleportPlayer() {
		GetComponent<AudioSource>().Play();
		if (leftControllerActions == null) {
			leftControllerActions = GameObject.Find("Controller (left)").GetComponent<SteamVR_ControllerActions>();
		}
		if (rightControllerActions == null) {
			rightControllerActions = GameObject.Find("Controller (right)").GetComponent<SteamVR_ControllerActions>();
		}
		if (animateTeleport) {
			StartCoroutine(AnimateToTarget());
		} else {
			cameraRig.localPosition = target.localPosition;
			cameraRig.localRotation = target.localRotation;
		}
		
		leftControllerActions.TriggerHapticPulse(1, 3000);
		rightControllerActions.TriggerHapticPulse(1, 3000);
	}
	
	IEnumerator AnimateToTarget() {
		float startTime = Time.time;
		while (Time.time < startTime + teleportDuration) {
			float frac = (Time.time - startTime)/teleportDuration;
			cameraRig.localPosition = Vector3.Lerp(cameraRig.localPosition, target.localPosition, frac);
			cameraRig.localRotation = Quaternion.Lerp(cameraRig.localRotation, target.localRotation, frac);
			// Trigger a sweet-feeling haptic pulse
			// leftControllerActions.TriggerHapticPulse(1, (ushort)Mathf.Lerp(1000, 200, frac));
			yield return null;
		}
	}
}
