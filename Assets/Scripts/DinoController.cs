﻿using UnityEngine;
using System.Collections;

public class DinoController : MonoBehaviour
{
	private Dinosaur me;
	private CharacterMotor motor;
	private Camera cam;

	#region Zoom Variables
	private bool zooming_enabled = true;
	public float zoomTime = 0.35f;
	private float normalFOV;
	private float minFOV;
	private float maxFOV;
	private float zoomCount = 0f;
	private float zoomInc;
	private int zoomState = 0;
	private int nextZoomState = 0;
	private bool zooming = false;
	#endregion
	#region Attack Variables
	private bool hit_attack_key = false;
	private bool attack_is_cooling_down = false;
	private bool has_enemy_in_range;
	private bool is_in_enemys_range;
	private float attack_cooldown = 0.5f;
	private float attack_timer = 0f;
	#endregion
	
	void Start ()
	{
		me = new Dinosaur ();
		motor = GetComponent<CharacterMotor> ();
		cam = GameObject.FindWithTag ("MainCamera").GetComponent<Camera>();
		normalFOV = cam.fieldOfView;

		//******************
		//TODO placeholder
		{
			//me.AddPointsTo_Agility (10);
			//me.AddPointsTo_Sensory (5);
			me.AddPointsTo_Intelligence (10);
		}
		//******************

		update_speed ();
		update_visibility ();
	}

	void GatherInput ()
	{
		if (zooming_enabled && !zooming) {
			setZoomState ();
		}
		checkForAttack ();
	}

	void UpdateGameLogic (float delta)
	{
		if (zooming_enabled && zooming) {
			inc_zoom (delta);
		}
		if (hit_attack_key) {
			hit_attack_key = false;
			Attack ();
		}
		if (attack_is_cooling_down) {
			attack_timer += delta;
			if (attack_timer > attack_cooldown) {
				attack_timer = 0f;
				attack_is_cooling_down = false;
			}
		}
		me.Heal (delta);
	}

	void Render ()
	{
		if (zooming_enabled) {
			cam.fieldOfView += zoomInc;
		}
	}

	// Update is called once per frame
	void Update ()
	{
		GatherInput ();
		UpdateGameLogic (Time.deltaTime);
		Render ();

	}

	#region Camera and Motor Update functions

	void update_speed ()
	{
		float speed = me.Movespeed ();
		motor.movement.maxForwardSpeed = speed;
		motor.movement.maxSidewaysSpeed = speed * 0.85f;
		motor.movement.maxBackwardsSpeed = speed * 0.75f;
	}

	void update_visibility ()
	{
		minFOV = me.MinFieldOfView ();
		maxFOV = me.MaxFieldOfView ();
		cam.farClipPlane = me.VisibilityDistance ();
	}

	#endregion

	#region Zoom Functions 

	void setZoomState ()
	{
		float scroll = Input.GetAxis ("Mouse ScrollWheel");
		if (scroll != 0f) {
			if (scroll > 0f) {
				if (zoomState == 0) 
					nextZoomState = 1;
				else if (zoomState == -1) 
					nextZoomState = 0;
			} else {
				if (zoomState == 0) 
					nextZoomState = -1;
				else if (zoomState == 1)
					nextZoomState = 0;
			}
			zooming = true;
		}
	}


	void inc_zoom (float delta)
	{
		if (zoomState == -1) {
			if (nextZoomState == 0) {
				zoomInc = delta * (normalFOV - maxFOV) / zoomTime;
			}
		} else if (zoomState == 0) {
			if (nextZoomState == -1) {
				zoomInc = delta * (maxFOV - normalFOV) / zoomTime;
			} else if (nextZoomState == 1) {
				zoomInc = delta * (minFOV - normalFOV) / zoomTime;
			}
		} else if (nextZoomState == 0) {
			zoomInc = delta * (normalFOV - minFOV) / zoomTime;
		}
		if ((zoomCount += delta) > zoomTime) {
			resetZoom ();
		}
	}

	void resetZoom ()
	{
		zoomCount = 0f;
		zoomInc = 0f;
		zooming = false;
		zoomState = nextZoomState;
		
		if (zoomState == 1)
			cam.fieldOfView = minFOV;
		else if (zoomState == 0)
			cam.fieldOfView = normalFOV;
		else if (zoomState == -1)
			cam.fieldOfView = maxFOV;
	}

	#endregion

	#region Attack Functions

	private void checkForAttack ()
	{
		if (Input.GetButton ("Fire1")) {
			hit_attack_key = true;
		}
	}

	private void Attack ()
	{
		if (!attack_is_cooling_down) {
			int layer = 1;
			layer <<= 8; //Dinosaur is layer 8
			Collider[] colliders = Physics.OverlapSphere (motor.transform.position, me.Attack_Radius (), layer);
			foreach (Collider c in colliders) {
				cameron_AI_Behavior enemy = c.GetComponent ("cameron_AI_Behavior") as cameron_AI_Behavior;
				if (enemy != null) {
					me.Attack (enemy.GetDinosaur ());
					break;
				}
			}
			attack_is_cooling_down = true;
		}
	}

	#endregion

    public Dinosaur GetDinosaur()
    {
        return me;
    }
}