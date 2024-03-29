﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhysicsObject : MonoBehaviour {
	//All the enemies, projectiles and the player has a physics object.
	//It handles the collision with the planet and the movement and rotation of the object.
	
	public LayerMask RayMask;
	public Vector2 VectorToPlanet; 
	float planetDistane { get {return VectorToPlanet.magnitude;}}
	public Vector2 PlanetDirection { get {return VectorToPlanet.normalized;}}
	public Vector2 PlanetTangentLeft { 
		get {
				Vector2 dir = PlanetDirection;
				return new Vector2(dir.y, -dir.x);
			}
		}
	public Vector2 PlanetTangentRight{ get {return -PlanetTangentLeft;}}
	
	float objectRadius;
	
	public Vector2 Velocity = Vector2.zero;
	float gravity;
	float planetFriction;
	float atmosphereFriction;
	public float planetRadius;
	public bool IsGrounded { get; private set; }
	public Transform TargetPlanet;
	GameObject camera;
	public float FrictionMultiplier = 1;
	public Vector2 xVector;

	public bool InGravField { get {return TargetPlanet != null; }}
	Rigidbody2D body; 
	public UnityEvent HitEvenet = new UnityEvent();
	// Use this for initialization
	void Awake () {
		body = GetComponent<Rigidbody2D>();
		float objectColliderRadius = transform.GetComponent<CircleCollider2D>().radius;
		float objectScale = transform.localScale.x;
		objectRadius = objectScale * objectColliderRadius;
		camera = GameObject.FindGameObjectWithTag("MainCamera");
	}
	
	// UpdatePhysics is called by PlayerMovement after all the player input is done at the end of every FixedUpdate 
	public void UpdateVelocity() {
		if(InGravField){
			VectorToPlanet = TargetPlanet.position - transform.position;
			Velocity += PlanetDirection * gravity * Time.deltaTime;
			xVector = Vector2.Dot(Velocity, PlanetTangentRight)*PlanetTangentRight; 
			Vector2 yVector = Velocity - xVector; 

			float raycastDistance = 0f;
			if(Vector2.Dot(yVector, PlanetDirection) >= 0){
				raycastDistance = objectRadius + yVector.magnitude * Time.deltaTime;
			}

			//Collison detection with the planet.
			RaycastHit2D hit = Physics2D.Raycast(transform.position, PlanetDirection, raycastDistance, RayMask);
			IsGrounded = hit;
			
			if(hit){
				if(TargetPlanet.GetComponentInChildren<GravField>().PlanetBeanified == false && gameObject.name == "Player"){
					camera.transform.GetComponent<CameraMovement>().PlayerView = true;
				}
				Velocity = xVector * (1 - planetFriction * FrictionMultiplier) + (hit.distance - objectRadius) * PlanetDirection;
				HitEvenet.Invoke();
			}
			else{	
				Velocity = xVector * (1 - atmosphereFriction * FrictionMultiplier)  + yVector;
			}
		}	
		body.velocity = Velocity;
	}

	public void UpdateRotation(string type){
		if(type == "standard"){
			float groundDistance = planetDistane - planetRadius - objectRadius;
			float zeroToOne = Mathf.Clamp(1 - (groundDistance - (groundDistance * 0.935f)), 0, 1);
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(PlanetDirection.y, PlanetDirection.x) + 90), zeroToOne);
		}
		else if(type == "projectile"){
			float groundDistance = planetDistane - planetRadius - objectRadius;
			float zeroToOne = Mathf.Clamp(1 - (groundDistance - (groundDistance * 0.935f)), 0, 1);
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(PlanetDirection.y, PlanetDirection.x)), zeroToOne) ;
		}
	}

	public void addVelocityLeft(float velocity){
		this.Velocity += velocity * PlanetTangentLeft * Time.deltaTime;
		UpdateVelocity();
	}
	public void addVelocityRight(float velocity){
		this.Velocity += velocity * PlanetTangentRight * Time.deltaTime;
		UpdateVelocity();
	}
	public void addVelocityUp(float velocity){
		this.Velocity += velocity * -PlanetDirection * Time.deltaTime;
		UpdateVelocity();
	}

	public void addVelocityVector(Vector2 velocityVector){
		this.Velocity += velocityVector;
		UpdateVelocity();
	}
	public void SetTargetPlanet(GravField gravField ){
		//Sets the target planet for collison detection and movement
		if(gravField != null){
			TargetPlanet = gravField.transform.parent;
			this.gravity = gravField.gravity;
			this.planetFriction = gravField.planetFriction;
			this.atmosphereFriction = gravField.atmosphereFriction;
			planetRadius = TargetPlanet.transform.localScale.x * TargetPlanet.GetComponent<CircleCollider2D>().radius;
		}
	}
}
