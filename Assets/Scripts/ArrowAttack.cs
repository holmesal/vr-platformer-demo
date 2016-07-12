using UnityEngine;
using System.Collections;

public class ArrowAttack : ArrowBase {
	
	public float arrowDamage = 10.0f;
	
	protected override void OnHit(Collision col) {
		// If the object we collided with is implements the Idamageable interface, have it take some damage
		IDamageable dam = col.gameObject.GetComponent<IDamageable>();
		if (dam != null) {
			dam.TakeDamage(arrowDamage, col.contacts[0].point);
		}
		
		StickIntoTarget(col);
	}
}
