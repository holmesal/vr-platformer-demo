using UnityEngine;
using System.Collections;

public class ArrowTeleport : ArrowBase {
	
	protected override void OnHit(Collision col) {
		// If the object we collided with is a teleport target, teleport there
		// TODO - interfaces might be a better fit here?
		TeleportTarget tel = col.gameObject.GetComponent<TeleportTarget>();
		if (tel != null) {
			tel.TeleportPlayer();
		}
		
		StickIntoTarget(col);
		// Destroy(gameObject);
	}
}
