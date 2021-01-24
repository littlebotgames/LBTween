/* 
 * A MonoBehvaiour that can be used to tween a Kinematic Rigidbody2D's rotation using the Example TweenManager
*/

using UnityEngine;

namespace LB.Tween.Example
{
	[AddComponentMenu("LBTween/ExampleTweenRigidbody2DRotation")]
	public class TweenRigidbody2DRotation : MonoBehaviour
	{
		[SerializeField]
		private Rigidbody2D m_rigidbody;
		[SerializeField]
		private float m_target;
		[SerializeField]
		private TweenParams m_tweenParams;

		private TweenRef m_tweenRef;
		private float m_startRotation;

		public Vector2 Velocity => Vector2.zero;
		private float m_angularVelocity;
		public float AngularVelocity => m_angularVelocity;

		private void OnEnable()
		{
			// Tween is a displacement not world posiation so store start pos to add to it
			m_startRotation = m_rigidbody.rotation;
			var tweenManager = TweenManager.Instance;
			tweenManager.StartPhysicsTween(
				0f,
				m_target,
				m_tweenParams.Secs,
				m_tweenParams.Ease,
				m_tweenParams.Loop,
				OnTweenFixedUpdate
			);
		}

		private void OnDisable()
		{
			var tweenManager = TweenManager.Instance;
			tweenManager.StopPhysicsTween(m_tweenRef);
		}

		private void OnTweenFixedUpdate(int id, float value, object context)
		{
			var nextRot = m_startRotation + value;
			m_angularVelocity = (nextRot - m_rigidbody.rotation) / Time.deltaTime;
			m_rigidbody.MoveRotation(nextRot);
		}
	}
}
