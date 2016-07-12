using UnityEngine;
using System.Collections;

[System.Serializable]
public struct Arrows
{
	public GameObject up;
	public GameObject down;
	public GameObject right;
	public GameObject left;
}

[RequireComponent(typeof(SteamVR_ControllerEvents))]
[RequireComponent(typeof(SteamVR_ControllerActions))]
public class ArrowManager : MonoBehaviour {
	
	public GameObject arrowPrefab;
	
	// The "ideal arrow" in fully notched position
	public Transform notchedArrow;
	// The point by which the butt of the arrow is held
	public Transform heldArrowButt;
	// The point on the bow string where the tail of the arrow is notched
	public Transform notchPoint;
	
	// Inside of this distance the arrow is always notched
	public float snapDeadZoneSize = 0.2f;
	// The distance at which the "snap" interpolation starts
	public float snapStartDistance = 0.4f;
	
	// The maximum distance the arrow can be pulled back
	public float maxDrawDistance = 0.3f;
	
	// Dirty scale factor - try to get rid of this
	public float bowScale = 10.0f;
	
	public Transform stringBone;
	
	// The arrows available to equip
	public Arrows arrows;
	
	// The currently held arrow
	GameObject heldArrow;
	
	float drawFrac;
	
	bool notched = false;
	bool locked = false;
	
	bool triggerDown = false;
	bool touchpadDown = false;
	Vector2 touchpadAxis;
	
	float lastPulseTime;
	
	SteamVR_ControllerActions controllerActions;
	
	Vector3 originalStringBonePosition;
	
	// The transform of the arrow butt point (relative to the notch point) when either
	// a) the trigger was pressed while the arrow was notched, or 
	// b) the arrow was notched while the trigger was pressed 
	// whatever happened last
	Vector3 initialDrawRelativePosition;
	
	Transform originalParent;

	// Use this for initialization
	void Start () {
		// Register for controller events
		SteamVR_ControllerEvents controllerEvents = GetComponent<SteamVR_ControllerEvents>();
		if (controllerEvents == null) {
            Debug.LogError("SteamVR_ControllerEvents_ListenerExample is required to be attached to a SteamVR Controller that has the SteamVR_ControllerEvents script attached to it");
            return;
        }
		controllerEvents.TriggerClicked += new ControllerClickedEventHandler(DoTriggerClicked);
        controllerEvents.TriggerUnclicked += new ControllerClickedEventHandler(DoTriggerUnclicked);
		controllerEvents.TouchpadAxisChanged += new ControllerClickedEventHandler(TouchpadAxisChanged);
		controllerEvents.TouchpadClicked += new ControllerClickedEventHandler(TouchpadClicked);
		controllerEvents.TouchpadUnclicked += new ControllerClickedEventHandler(TouchpadUnclicked);
		
		controllerActions = GetComponent<SteamVR_ControllerActions>();
		
		// Store the original string bone position, because we'll modify it later when the arrow is drawn
		originalStringBonePosition = stringBone.localPosition;
		
		// Instantiate an arrow and stick it to the controller
		SpawnArrow();
	}
	
	// Update is called once per frame
	void Update () {
		
		// If our touchpad is down, swap out the arrow that we're holding
		if (touchpadDown) {
			UpdateCurrentArrow();
		}
		
		// If we're notched (arrow close to notch point) and locked (trigger down), then use the z-displacement of the controller to calculate the pullback
		if (notched && triggerDown) {
			// Debug.Log("Arrow is drawing");
			Vector3 currentDrawRelativePosition = notchPoint.position - heldArrowButt.position;
			Vector3 relativePositionDiff = currentDrawRelativePosition - initialDrawRelativePosition;
			Debug.DrawLine(notchPoint.position + initialDrawRelativePosition, notchPoint.position + currentDrawRelativePosition, Color.red);
			// Vector3 displacement = heldArrowButt.position - initialDrawPosition;
			// Get component of displacement in z-direction of bow
			float bowZDisp = Vector3.Dot(relativePositionDiff, notchPoint.forward);
			// Debug.Log("drawing w/ distance: " + bowZDisp);
			
			bowZDisp = Mathf.Clamp(bowZDisp, 0, maxDrawDistance);
			
			// Update the draw amount
			drawFrac = bowZDisp / maxDrawDistance;
			float pulseLength = Mathf.Lerp(100, 200, drawFrac);
			float intensity = Mathf.Lerp(500, 1000, drawFrac);
			// controllerActions.TriggerHapticPulse((int)pulseLength, (ushort)intensity);
			if (Time.time - lastPulseTime > 0.05) {
				controllerActions.TriggerHapticPulse(1, (ushort)intensity);
				lastPulseTime = Time.time;
			}
			
			// Draw the displacement
			// Debug.DrawLine(initialDrawPosition, initialDrawPosition - notchPoint.forward * bowZDisp, Color.yellow);
			
			// Move the arrow and bow back
			stringBone.localPosition = originalStringBonePosition + Vector3.right * bowZDisp * bowScale;
			heldArrow.transform.localPosition = Vector3.back * bowZDisp * bowScale;
		} else {
			// We're not locked
			// TODO - don't run these calculations if we're not near the notch point
			
			// Calculate the distance away from the ideal arrow position
			float dist = Vector3.Distance(heldArrowButt.position, notchPoint.position);
			// Debug.Log("Distance to notch point: " + dist);
			Debug.DrawLine(heldArrowButt.position, notchPoint.position, Color.green);
			
			if (dist < snapDeadZoneSize) {
				// Debug.Log("Arrow is notched");
				heldArrow.transform.position = notchPoint.position;
				heldArrow.transform.rotation = notchPoint.rotation;
				heldArrow.transform.parent = notchPoint;
				if (!notched) {
					controllerActions.TriggerHapticPulse(1, 1200);
				}
				notched = true;
				if (triggerDown) {
					BeginDrawing();
				}
			} else if (dist > snapStartDistance) {
				// Debug.Log("Arrow is unnotched");
				heldArrow.transform.position = heldArrowButt.position;
				heldArrow.transform.rotation = heldArrowButt.rotation;
				heldArrow.transform.parent = heldArrowButt;
				notched = false;
			} else {
				float relativeDistance = (snapStartDistance - dist) / (snapStartDistance - snapDeadZoneSize);
				// Debug.Log("Relative distance: " + relativeDistance);
				heldArrow.transform.position = Vector3.Lerp(heldArrowButt.position, notchPoint.position, relativeDistance);
				heldArrow.transform.rotation = Quaternion.Lerp(heldArrowButt.rotation, notchPoint.rotation, relativeDistance);
				heldArrow.transform.parent = notchPoint;
				notched = false;
			}
			
			stringBone.localPosition = originalStringBonePosition;
			
		}
	}
	
	void UpdateCurrentArrow() {
		// Debug.Log(touchpadAxis);
		// Up is teleport
		if (Mathf.Abs(touchpadAxis[0]) > Mathf.Abs(touchpadAxis[1])) {
			// It's horizontal
			if (touchpadAxis[0] < 0) {
				EquipArrow(arrows.left);
			} else {
				EquipArrow(arrows.right);
			}
		} else {
			if (touchpadAxis[1] < 0) {
				EquipArrow(arrows.down);
			} else {
				EquipArrow(arrows.up);
			}
		}
	}
	
	void EquipArrow(GameObject _arrowPrefab) {
		if (_arrowPrefab) {
			arrowPrefab = _arrowPrefab;
			ReplaceArrow();
		}
	}
	
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.name == "IdealArrow") {
			// Trying to implement a "magnetic" effect, like in the lab demo
			// Debug.Log("Entered Ideal arrow!");
			locked = true;
		}
	}
	
	void BeginDrawing() {
		initialDrawRelativePosition = notchPoint.position - heldArrowButt.position;
	}
	
	void SpawnArrow() {
		heldArrow = Instantiate(arrowPrefab);
		heldArrow.transform.parent = heldArrowButt;
		heldArrow.transform.localPosition = Vector3.zero;
		heldArrow.transform.localRotation = Quaternion.identity;
	}
	
	void ReplaceArrow() {
		GameObject newHeldArrow = Instantiate(arrowPrefab, heldArrow.transform.localPosition, heldArrow.transform.localRotation) as GameObject;
		newHeldArrow.transform.parent = heldArrow.transform.parent;
		newHeldArrow.transform.localPosition = heldArrow.transform.localPosition;
		newHeldArrow.transform.localRotation = heldArrow.transform.localRotation;
		Destroy(heldArrow);
		heldArrow = newHeldArrow;
	}
	
	void Shoot() {
		heldArrow.transform.parent = null;
		heldArrow.GetComponent<ArrowBase>().Shoot(drawFrac);
		
		// Reset everything
		drawFrac = 0;
		stringBone.localPosition = originalStringBonePosition;
		
		// Spawn a new arrow
		SpawnArrow();
	}
	
	
	// Steam controller event listeners
	void DoTriggerClicked(object sender, ControllerClickedEventArgs e) {
		triggerDown = true;
		if (notched) {
			BeginDrawing();
		}
	}
	
	void DoTriggerUnclicked(object sender, ControllerClickedEventArgs e) {
		triggerDown = false;
		if (drawFrac > 0.1) {
			Shoot();
		}
	}
	
	void TouchpadAxisChanged(object sender, ControllerClickedEventArgs e) {
		touchpadAxis = e.touchpadAxis;
	}
	
	void TouchpadClicked(object sender, ControllerClickedEventArgs e) {
		touchpadDown = true;
	}
	
	void TouchpadUnclicked(object sender, ControllerClickedEventArgs e) {
		touchpadDown = false;
	}
}
