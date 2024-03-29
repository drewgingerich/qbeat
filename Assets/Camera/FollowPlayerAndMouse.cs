﻿using UnityEngine;

public class FollowPlayerAndMouse : MonoBehaviour
{
	[SerializeField]
	Transform player;

	[Range(0, 1)]
	[SerializeField]
	private float mouseImportance = 0.5f;

	private Vector2 aimInput = Vector2.zero;

	void Awake()
	{
		MusicStartAnnouncer.OnStartLate += Initialize;
		PlayerController.OnPlayerDying += HandlePlayerDying;
	}

	void Initialize(double startTime)
	{
		MusicStartAnnouncer.OnStartLate -= Initialize;

		PlayerController.controls.Player.Aim.performed += ctx =>
		{
			aimInput = ctx.ReadValue<Vector2>();
		};
	}

	void Update()
	{
		var mouseWorldPosition = Camera.main.ScreenToWorldPoint(aimInput);
		var diff = (Vector3)mouseWorldPosition - player.position;
		transform.position = player.position + diff * mouseImportance;
	}

	void HandlePlayerDying()
	{
		PlayerController.OnPlayerDying -= HandlePlayerDying;
		mouseImportance = 0;
	}
}
