using UnityEngine;
using System.Collections;

public class Magnet : MonoBehaviour {

	public GameObject target;
	
	public float strength = 10.0f;
	
	void OnTriggerEnter(Collider other) {
		Debug.Log("Magnet triggerEnter!");
		Magnetizable mag = other.GetComponent<Magnetizable>();
		if (mag) {
			mag.Magnetize(target, strength);
		}
	}
}
