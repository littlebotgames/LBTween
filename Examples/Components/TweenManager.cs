﻿/* 
 * An example of a Singleton MonoBehaviour containing 2 separate TweenRunners, one for
 * Update and one for FixedUpdate so we can tween Rigidbodies correctly
*/

using UnityEngine;

namespace LB.Tween.Example
{
	[AddComponentMenu("LBTween/ExampleTweenManager")]
	public class TweenManager : MonoBehaviour
	{
		private TweenRunner m_tweenManager;
		private TweenRunner m_physicsTweenManager;

		public static TweenManager Instance { get; private set; }

		protected void Awake()
		{
			// Check if instance already exists
			if (Instance == null)
			{
				//if not, set instance to this
				Instance = this;
			}
			else if (Instance != this)
			{
				// If instance already exists and it's not this then destroy this. 
				// This enforces our singleton pattern, meaning there can only ever be one instance.
				Debug.LogWarning("There is already an Instance for this Singleton so destroying this", gameObject);
				Destroy(gameObject);
				return;
			}

			m_tweenManager = new TweenRunner();
			m_physicsTweenManager = new TweenRunner();
		}

		protected void OnDestroy()
		{
			m_tweenManager.Dispose();
			m_physicsTweenManager.Dispose();
		}

		private void Update()
		{
			m_tweenManager.Update(Time.deltaTime);
		}

		private void FixedUpdate()
		{
			m_physicsTweenManager.Update(Time.deltaTime);
		}

		public TweenRef StartTween<T>(
			T start, 
			T end, 
			in TweenParams tweenParams, 
			TweenData<T>.OnUpdateCallback onUpdateCallback,
			TweenData<T>.OnFinishedCallback onFinishedCallback = null)
			where T : struct
		{
			return m_tweenManager.StartTween(start, end, tweenParams, onUpdateCallback, onFinishedCallback);
		}

		public void StopTween(TweenRef id)
		{
			m_tweenManager.StopTween(id);
		}

		public TweenRef StartPhysicsTween<T>(
			T start, 
			T end, 
			in TweenParams tweenParams, 
			TweenData<T>.OnUpdateCallback onUpdateCallback,
			TweenData<T>.OnFinishedCallback onFinishedCallback = null)
			where T : struct
		{
			return m_physicsTweenManager.StartTween(start, end, tweenParams, onUpdateCallback, onFinishedCallback);
		}

		public void StopPhysicsTween(TweenRef id)
		{
			m_physicsTweenManager.StopTween(id);
		}
	}
}
