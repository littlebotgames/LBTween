/* 
 * A MonoBehvaiour that can be used to tween a Kinematic Rigidbody2D's position using the Example TweenManager
*/

using UnityEngine;

namespace LB.Tween.Example
{
	[AddComponentMenu("LBTween/ExampleTweenRigidbody2DPosition")]
	public class TweenRigidbody2DPosition : MonoBehaviour
	{
		[SerializeField]
		private Rigidbody2D m_rigidbody;
		[SerializeField]
		private Vector2 m_target;
		[SerializeField]
		private TweenParams m_tweenParams;

		private TweenRef m_tweenRef;
		private Vector2 m_startPos;

		private Vector2 m_velocity;
		public Vector2 Velocity => m_velocity;
		public float AngularVelocity => 0f;

		private void OnEnable()
		{
			// Tween is a displacement not world posiation so store start pos to add to it
			m_startPos = m_rigidbody.position;
			var tweenManager = TweenManager.Instance;
			tweenManager.StartPhysicsTween(
				Vector2.zero,
				m_target,
				m_tweenParams,
				OnTweenFixedUpdate
			);
		}

		private void OnDisable()
		{
			var tweenManager = TweenManager.Instance;
			tweenManager.StopPhysicsTween(m_tweenRef);
		}

		private void OnTweenFixedUpdate(int id, Vector2 value, object context)
		{
			var nextPos = m_startPos + value;
			m_velocity = (nextPos - m_rigidbody.position) / Time.deltaTime;
			m_rigidbody.MovePosition(nextPos);
		}
	}
}
