﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamController : MonoBehaviour
{
	[SerializeField]
	private LineRenderer beam;
	[SerializeField]
	private GameObject beamMech;

	private ContactFilter2D hitFilter = new ContactFilter2D();
	private RaycastHit2D[] hitBuffer = new RaycastHit2D[1];

	private Vector2 direction;
	private AudioSource audioSource;

	public void Fire(Vector2 direction, AudioSource audioSource)
	{
		this.direction = direction;
		this.audioSource = audioSource;
		StartCoroutine(Activate());
	}

	private IEnumerator Activate()
	{
		yield return new WaitForNote(Note.Eighth);
		yield return new WaitForNote(Note.Sixteenth);
		var hits = Physics2D.Raycast(transform.position, direction, hitFilter, hitBuffer);

		if (hits > 0)
		{
			var hitDetector = hitBuffer[0].collider.gameObject.GetComponent<HitDetector>();
			if (hitDetector != null) hitDetector.Hit(Hit.Tag, direction);
		}

		var endPoint = hits > 0
			? (Vector3)hitBuffer[0].point
			: transform.position + (Vector3)direction * 100;
		var beamPositions = new Vector3[]{
				transform.position,
				endPoint
			};

		beam.SetPositions(beamPositions);
		beam.startWidth = beam.endWidth = 0.05f;
		beam.enabled = true;

		audioSource.PlayScheduled(Conductor.GetNextNote(Note.Quarter, 1));
		yield return new WaitForNote(Note.Quarter, 1);
		beam.startWidth = beam.endWidth = 0.15f;
		CameraJuicer.Kick(direction);
		CameraJuicer.Shake();

		yield return new WaitForSeconds(0.1f);
		beam.enabled = false;

		if (hits > 0)
		{
			var hitDetector = hitBuffer[0].collider.gameObject.GetComponent<HitDetector>();
			if (hitDetector != null) hitDetector.Hit(Hit.Shot, direction);
		}

		GameObject.Destroy(gameObject);
	}

	private IEnumerator Fire()
	{
		yield return new WaitForNote(Note.Quarter, 1);
		beam.startWidth = beam.endWidth = 0.15f;
		// shootAudioSource.Play();
		yield return new WaitForSeconds(0.1f);
		beam.enabled = false;
		GameObject.Destroy(gameObject);
	}
}
