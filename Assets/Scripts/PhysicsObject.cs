﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour {
	public LayerMask RayMask;

	Vector2 vectorToPlanet; 
	GravField currentGravField = null;
	float planetDistane { get {return vectorToPlanet.magnitude;}}
	public Vector2 PlanetDirection { get {return vectorToPlanet.normalized;}}
	Vector2 PlanetTangentLeft { 
		get {
				Vector2 dir = PlanetDirection;
				return new Vector2(dir.y, -dir.x);
			}
		}
	Vector2 PlanetTangentRight{ get {return -PlanetTangentLeft;}}
	private Rigidbody2D body;
	float objectRadius;
	
	Vector2 velocity = Vector2.zero;
	float gravity;
	float planetFriction;
	float atmosphereFriction;
	float planetRadius;
	public bool IsGrounded { get; private set; }
	Transform targetPlanet;

	public bool InGravField { get {return targetPlanet != null; }}

	// Use this for initialization
	void Start () {
		body = GetComponent<Rigidbody2D>();
		float objectColliderRadius = transform.GetComponent<CircleCollider2D>().radius;
		float objectScale = transform.localScale.x;
		objectRadius = objectScale * objectColliderRadius;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if(InGravField){
			
			vectorToPlanet = targetPlanet.position - transform.position;
			velocity += PlanetDirection * gravity * Time.deltaTime;
			Vector2 xVector = Vector2.Dot(velocity, PlanetTangentRight)*PlanetTangentRight; 
			Vector2 yVector = velocity - xVector; 

			//Collison detection with the planet.
			float raycastDistance = objectRadius + yVector.magnitude * Time.deltaTime;
			RaycastHit2D hit = Physics2D.Raycast(transform.position, PlanetDirection, raycastDistance, RayMask);
			Debug.DrawRay(body.position, PlanetDirection *raycastDistance, Color.yellow);
			IsGrounded = hit;
			if(hit){
				print("HIT!");
				velocity = xVector * planetFriction +  (hit.distance - objectRadius)  * PlanetDirection   ;
			}
			else{
				velocity = xVector * atmosphereFriction + yVector;
			}
			
			float groundDistance = planetDistane - planetRadius - objectRadius;
			float zeroToOne = Mathf.Clamp(1 - (groundDistance - (groundDistance * 0.935f)), 0, 1);
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(PlanetDirection.y, PlanetDirection.x) + 90), zeroToOne);		

			body.velocity = velocity;
			//Debug.DrawRay(body.position, body.velocity, Color.green);
		}	
	}
	public void addVelocityLeft(float velocity){
		this.velocity += velocity * PlanetTangentLeft * Time.deltaTime;
	}
	public void addVelocityRight(float velocity){
		this.velocity += velocity * PlanetTangentRight * Time.deltaTime;
	}
	public void addVelocityUp(float velocity){
		this.velocity += velocity * -PlanetDirection * Time.deltaTime;
	}
	public void SetTargetPlanet(GravField gravField ){

		if(gravField != null){
			targetPlanet = gravField.transform.parent;
			this.gravity = gravField.gravity;
			this.planetFriction = gravField.planetFriction;
			this.atmosphereFriction = gravField.atmosphereFriction;
			planetRadius = targetPlanet.transform.localScale.x * targetPlanet.GetComponent<CircleCollider2D>().radius;
		}
	}
}
