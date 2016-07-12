using UnityEngine;
using System.Collections;

public class ArrowBoomerang : ArrowBase {
	
	public float returnForce = 10.0f;
	
	bool seekingPlayer = false;
	GameObject playerHead;
	
	protected override void Start() {
		base.Start();
		playerHead = GameObject.Find("Camera (head)");
		Debug.Assert(playerHead, "Boomerang arrow could not find player head!");
	}
	
	protected override void FixedUpdate() {
		base.FixedUpdate();
		
		if (seekingPlayer) {
			// Add a velocity towards the player
			Vector3 playerSeekPoint = playerHead.transform.position + (Vector3.down * 0.5f) + (playerHead.transform.forward * 0.2f);
			Vector3 playerHeadDirection = (playerSeekPoint - transform.position).normalized;
			Vector3 playerSeekForce = playerHeadDirection * returnForce;
			// Also counteract gravity
			Vector3 antigravity = Vector3.up * gravity;
			antigravity = Vector3.zero;
			// body.AddForce(playerSeekForce, ForceMode.Acceleration);
			body.velocity = playerSeekForce;
			Debug.Log("Seeking player!");
		}
	}
	
	protected override void OnHit(Collision col) {
		// If we hit the player, we're done
		if (col.gameObject.name != "Camera (head)") {
			Debug.Log("Boomerang arrow hit something else... " + col.gameObject.name);
			// If we hit something besides the player, turn around and head back to the player
			SeekPlayer();
		}
	}
	
	void OnTriggerEnter(Collider other) {
		// If we hit the player, we're done
		if (shot && other.gameObject.name == "Camera (head)") {
			Debug.Log("Returned to player!");
			Destroy(gameObject);
		}
	}
	
	void SeekPlayer() {
		seekingPlayer = true;
	}
}
