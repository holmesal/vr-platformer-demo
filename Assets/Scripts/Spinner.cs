using UnityEngine;
using System.Collections;

public class Spinner : MonoBehaviour {
	
	public float spinSpeed = 10;

	void FixedUpdate() {
		transform.Rotate(spinSpeed * new Vector3(1,0.2f,0.1f));
	}
}
