using LB.Tween;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

class EditorTests
{
	[Test]
	public void StartAndStopTween()
	{
		const int numTweens = 50;

		var runner = new TweenRunner();
		var tweenRefs = new List<TweenRef>(numTweens);

		// Start a bunch of tweens running
		for(var i = 0; i < numTweens; ++i)
		{
			var tweenRef = runner.StartTween(
				0f,
				10f,
				10f,
				EaseType.Linear,
				LoopType.PingPong,
				null
			);
			tweenRefs.Add(tweenRef);
		}

		// Make sure the correct amount started
		var numRunningTweens = runner.GetNumberOfRunningTweens();
		Assert.IsTrue(numRunningTweens == numTweens, $"Only {numRunningTweens} tweens running. We should have started {numTweens}");

		// Stop all tweens
		for(var i = 0; i < tweenRefs.Count; ++i)
		{
			runner.StopTween(tweenRefs[i]);
		}

		// Make sure they all stopped
		numRunningTweens = runner.GetNumberOfRunningTweens();
		Assert.IsTrue(numRunningTweens == 0, $"There are still {numRunningTweens} tweens running. All should have been stopped");
	}

	public struct TestType
	{
		public float Value;
		public static implicit operator float(TestType test)
		{
			return test.Value;
		}
		public static implicit operator TestType(float test)
		{
			return new TestType { Value = test };
		}
		public override string ToString()
		{
			return Value.ToString();
		}
	}
	public static TestType GetValueTestType(TestType start, TestType end, float easeValue)
	{
		return Mathf.LerpUnclamped(start, end, easeValue);
	}

	[Test]
	public void RegisterTweenValueType()
	{
		var runner = new TweenRunner();
		// Register a test type
		TweenDataRegistry.RegisterTweenDataType<TestType>(GetValueTestType);

		// Make sure a tween of that type can be started and updated
		var startValue = (TestType)1f;
		var tweenRef = runner.StartTween<TestType>(
			startValue,
			10f,
			10f,
			EaseType.Linear,
			LoopType.PingPong,
			null
		);

		// Make sure the starting value is correct
		var value = runner.GetTweenValue<TestType>(tweenRef);
		Assert.IsTrue(value == startValue, $"TestType pre update, got value {value} should be {startValue}");

		runner.Update(1f / 60f);

		// Make sure the value has changed after updating
		value = runner.GetTweenValue<TestType>(tweenRef);
		Assert.IsTrue(value != startValue, $"TestType post update, got value {value} should be different from start value {startValue}");

		runner.StopTween(tweenRef);
	}
}