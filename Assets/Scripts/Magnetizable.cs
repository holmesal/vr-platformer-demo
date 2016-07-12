using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Magnetizable : MonoBehaviour {
	
	public GameObject target;
	
	public float strength;
	
	Rigidbody body;
	
	void Start() {
		body = GetComponent<Rigidbody>();
	}

	public void Magnetize(GameObject _target, float _strength) {
		target = _target;
		strength = _strength;
	}
	
	void FixedUpdate() {
		if (target) {
			Debug.Log("Magnetized seeking target!");
			Vector3 targetDirection = (target.transform.position - transform.position).normalized;
			Vector3 targetSeekForce = targetDirection * strength;
			
			body.useGravity = false;
			body.isKinematic = false;
			body.velocity = targetSeekForce;
			// body.AddForce(targetSeekForce, ForceMode.Force);
		}
	}
}
