﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BeamController : MonoBehaviour
{
	[SerializeField]
	private Note preFireNoteTime = Note.Quarter;
	[SerializeField]
	private int preFireNoteOffset = 3;
	[SerializeField]
	private float aimBeamWidth = 0.05f;
	[SerializeField]
	private float fireBeamWidth = 0.15f;

	[SerializeField]
	private LineRenderer beam;
	[SerializeField]
	private Transform ring;

	private ContactFilter2D hitFilter = new ContactFilter2D();
	private RaycastHit2D[] hitBuffer = new RaycastHit2D[1];

	private Vector2 direction;
	private AudioSource audioSource;

	private void Awake()
	{
		// Logic below assumes this
		Debug.Assert(preFireNoteOffset > 2);

		var mask = LayerMask.GetMask(new string[] {
			LayerMask.LayerToName(PhysicsLayers.Enemy),
			LayerMask.LayerToName(PhysicsLayers.Stage),
		});
		hitFilter.SetLayerMask(mask);
	}

	public void Activate(Vector2 direction, AudioSource audioSource)
	{
		this.direction = direction;
		this.audioSource = audioSource;
		StartCoroutine(ActivateRoutine());
	}

	private IEnumerator ActivateRoutine()
	{
		beam.startWidth = beam.endWidth = aimBeamWidth;
		beam.enabled = true;

		var fireTime = Conductor.GetNextNote(preFireNoteTime, preFireNoteOffset);
		StartCoroutine(Aim(fireTime));
		StartCoroutine(Spin(fireTime));
		yield return new WaitForNote(preFireNoteTime, preFireNoteOffset - 1);

		var waitTime = Conductor.GetNextNote(preFireNoteTime) - Conductor.noteDurations[Note.Thirtysecond];
		yield return new WaitForDspTime(waitTime);
		PlayerSfx.PlayShootSfx();

		yield return Fire();

		Destroy(gameObject);
		// if (hits > 0)
		// {
		// 	var hitDetector = hitBuffer[0].collider.gameObject.GetComponent<HitDetector>();
		// 	if (hitDetector != null) hitDetector.Hit(Hit.Shot, direction);
		// }

		GameObject.Destroy(gameObject);
	}

	private IEnumerator Aim(double targetTime)
	{
		while (targetTime > AudioSettings.dspTime)
		{
			var hits = Physics2D.Raycast(transform.position, direction, hitFilter, hitBuffer);
			if (hits > 0)
			{
				GameObject other = hitBuffer[0].collider.gameObject;
				if (other.layer == PhysicsLayers.Enemy)
				{
					var diff = (Vector2)(other.transform.position - transform.position);
					direction = diff.normalized;
				}
			}

			var endPoint = hits > 0
				? (Vector3)hitBuffer[0].point
				: transform.position + (Vector3)direction * Mathf.Infinity;

			var beamPositions = new Vector3[]{
				transform.position,
				endPoint
			};

			beam.SetPositions(beamPositions);

			yield return null;
		}
	}

	private IEnumerator Spin(double targetTime)
	{
		var targetDuration = targetTime - AudioSettings.dspTime;

		var rotateDuration = (float)(targetDuration - Conductor.noteDurations[Note.Quarter]);
		var rotTween = ring.DOLocalRotate(new Vector3(0, 0, 45 * 17), rotateDuration, RotateMode.FastBeyond360);
		yield return rotTween.WaitForCompletion();

		yield return new WaitForNote(Note.Sixteenth);

		var reverseDuration = (float)Conductor.noteDurations[Note.Sixteenth];
		var revTween = ring.DOLocalRotate(new Vector3(0, 0, 0), reverseDuration, RotateMode.FastBeyond360);
		yield return revTween.WaitForCompletion();
	}

	private IEnumerator Fire()
	{
		var hits = Physics2D.Raycast(transform.position, direction, hitFilter, hitBuffer);
		if (hits > 0)
		{
			var hitDetector = hitBuffer[0].collider.gameObject.GetComponent<HitDetector>();
			if (hitDetector != null) hitDetector.Hit(Hit.Shot, direction);
		}

		beam.startWidth = beam.endWidth = fireBeamWidth;

		CameraJuicer.Kick(direction);
		CameraJuicer.Shake();

		yield return new WaitForSeconds(0.1f);
		beam.enabled = false;
	}
}
