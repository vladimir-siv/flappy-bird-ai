﻿using System;
using UnityEngine;

public class Bird : MonoBehaviour
{
	[SerializeField] private float JumpForce = 5f;
	[SerializeField] private double JumpDelta = 250.0;

	public Rigidbody body => GetComponent<Rigidbody>(); //{ get; private set; }

	private DateTime jumpTime;

	public event Action<Bird> Termination;
	public event Action Terminated;

	private void Start()
	{
		//body = GetComponent<Rigidbody>();
		jumpTime = DateTime.Now.AddMilliseconds(-JumpDelta);
	}

	public void Jump()
	{
		if ((DateTime.Now - jumpTime).TotalMilliseconds < JumpDelta) return;
		body.velocity = new Vector3(0f, JumpForce, 0f);
		jumpTime = DateTime.Now;
	}

	public void Terminate()
	{
		Termination?.Invoke(this);
		Destroy(gameObject);
		Terminated?.Invoke();
	}
}
