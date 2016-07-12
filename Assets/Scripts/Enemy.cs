using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour, IDamageable {
	
	public Transform playerHead;
	public GameObject projectilePrefab;
	public float timeBetweenShots = 3.0f;
	
	public float health = 100.0f;
	
	public GameObject drop;
	
	bool canSeePlayer = false;
	float lastShootTime = 0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
		// Check if we can see the player
		RaycastHit hit;
		if (Physics.Linecast(transform.position, playerHead.position, out hit)) {
			// If we hit a MainCamera, we hit the player
			if (hit.collider.gameObject.CompareTag("MainCamera")) {
				canSeePlayer = true;
				transform.LookAt(hit.collider.gameObject.transform);
				// This lags the look rotation
				// Thy only problem is that we still shoot right away, so the first shot usually goes wild...
				// transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.LookRotation(hit.collider.gameObject.transform.position - transform.position), 0.5f);
				Debug.DrawLine(transform.position, playerHead.position, Color.green);
			} else {
				canSeePlayer = false;
				Debug.DrawLine(transform.position, playerHead.position, Color.red);
				// Debug.Log("did not hit player but hit: " + hit.collider.gameObject.name);
			}
		} else {
			// Debug.Log("linecast did not hit anything");
		}
		
		// If we can see the player, shoot a projectile at them
		if(canSeePlayer && Time.time - lastShootTime > timeBetweenShots) {
			GameObject proj = Instantiate(projectilePrefab, transform.position + transform.forward * 4.0f, transform.rotation) as GameObject;
			proj.GetComponent<Rigidbody>().AddForce(proj.transform.forward * 13.0f, ForceMode.VelocityChange);
			lastShootTime = Time.time;
			
		}
	}
	
	public void TakeDamage(float damage, Vector3 hitPoint) {
		health -= damage;
		Debug.Log(gameObject.name + " took " + damage + " damage - health is now " + health);
		if (health <= 0) {
			Die();
		}
	}
	
	void Die() {
		Debug.Log(gameObject.name + " was slain!");
		// TODO - emit event to notify player
		DropItems();
		// TODO - death animation
		Destroy(gameObject);
	}
	
	void DropItems() {
		// TODO - drop items
		Instantiate(drop, transform.position, transform.rotation);
	}
}
