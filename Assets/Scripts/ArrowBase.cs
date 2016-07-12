using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public abstract class ArrowBase : MonoBehaviour {
	
	// The arrow will be shot with a velocity between min and max
	// determined by bow draw length
	public float minVelocity = 0;
	public float maxVelocity = 30;
	
	// The velocity required for the arrow to "look at" the velocity direction
	public float minLookAtZVelocity = 1.0f;
	
	// The range of possible penetration depths
	public float maxPenetrationDepth = 0.4f;
	public float minPenetrationDepth = 0.1f;
	
	// The effective "gravity" for this arrow
	public float gravity = 5.0f;
	
	// Below this velocity, the arrow will simply bounce off of the target
	public float minVelocityToPenetrate = 0.5f;
	
	// The attached rigid body and collider
	protected Rigidbody body;
	protected Collider collider;
	
	// Whether the arrow has been shot
	protected bool shot = false;
	
	// Every subclass needs to implement onHit
	protected abstract void OnHit(Collision col);

	protected virtual void Start () {
		// Get the attached body and collider
		body = GetComponent<Rigidbody>();
		collider = GetComponent<Collider>();
	}
	
	protected virtual void FixedUpdate () {
		// If we've been shot and are going fast enough
		if (shot == true && body.velocity.sqrMagnitude > minLookAtZVelocity * minLookAtZVelocity) {
			// Rotate to point the same direction as the velocity
			transform.LookAt(transform.position + body.velocity);
		}
		
		// TODO - if you shoot an arrow upwards slowly, this trick is very visible as the arrow falls unrealistically
		// so figure out how to only apply this acceleration in the right situations
		// Add an acceleration to counteract gravity, to make our arrows float more and simulate lift
		float antigravity = 9.8f - gravity;
		body.AddForce(Vector3.up * antigravity, ForceMode.Acceleration);
	}
	
	public void Shoot(float power) {
		PlayShootSound();
		DefaultShoot(power);
		shot = true;
	}
	
	// Default shoot behavior
	protected virtual void DefaultShoot(float power) {
		float velocity = Mathf.Lerp(minVelocity, maxVelocity, power);
		body.velocity = transform.forward * velocity;
		body.isKinematic = false;
	}
	
	protected virtual void PlayShootSound() {
		GetComponent<AudioSource>().Play();
	}
	
	void OnCollisionEnter(Collision col) {
		Debug.Log("Arrow collider enter! " + col.gameObject.name);
		// Ignore collisions with other arrows
		if (col.gameObject.CompareTag("Arrow")) {
			// We hit an arrow, so ignore this collision
			Debug.Log("Ignoring col with another arrow");
			return;
		}
		
		OnHit(col);
	}
	
	// Default OnHit behavior is just to stick into the target object
	protected void StickIntoTarget(Collision col) {
		float mag = col.relativeVelocity.magnitude;
		if (mag > minVelocityToPenetrate) {
			// Move forward a bit
			transform.localPosition = transform.localPosition + transform.forward * Mathf.Lerp(minPenetrationDepth, maxPenetrationDepth, mag / maxVelocity);
			// Remove forces on the arrow
			body.isKinematic = true;
			// Stop the arrow
			body.velocity = Vector3.zero;
			// Disable the arrow's collider
			collider.enabled = false;
			// Parent the arrow to the thing we just hit
			transform.parent = col.transform;
		}
	}
}
