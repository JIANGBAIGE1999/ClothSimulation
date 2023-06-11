using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothSpriteBallScript:MonoBehaviour{
	private Rigidbody2D rb;
	public float maxVelocity=20f;
	private void Awake(){
		rb=GetComponent<Rigidbody2D>();
	}
	void Update(){
		//Get cursor's position
		Vector3 dragPosition=Camera.main.ScreenToWorldPoint(Input.mousePosition);
		dragPosition.z=transform.position.z;
		Vector2 force=(Vector2)(dragPosition-transform.position);
		rb.velocity+=force*2f;
		//Limiting maximum velocity so the object doesn't move unnaturally fast
		if(rb.velocity.magnitude>maxVelocity) rb.velocity*=maxVelocity/rb.velocity.magnitude;
		//Damping the velocity
		rb.velocity*=0.91f;
	}
}