using AOT;
using Unity.Burst;
using UnityEngine;

namespace LB.Tween
{
	public enum LoopType
	{
		Once,
		PingPong,
		PingPongInvert
	}

	public delegate void LoopFunction(ref Tween tween);

	[BurstCompile]
	public static class LoopUtils
	{
		public static FunctionPointer<LoopFunction> GetFunction(LoopType loop)
		{
			switch(loop)
			{
				case LoopType.Once:
					return BurstCompiler.CompileFunctionPointer<LoopFunction>(Once);
				case LoopType.PingPong:
					return BurstCompiler.CompileFunctionPointer<LoopFunction>(PingPong);
				case LoopType.PingPongInvert:
					return BurstCompiler.CompileFunctionPointer<LoopFunction>(PingPongInvert);
				default:
					Debug.LogAssertion($"Unhandled loop type {loop}");
					return default;
			}
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(LoopFunction))]
		public static void Once(ref Tween tween)
		{
			// Once means no looping happens so tween has now finished
			tween.Overshoot = tween.End - tween.Value;
			tween.Value = tween.End;
			tween.CurrentSecs = tween.TotalSecs;
			tween.IsFinished = true;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(LoopFunction))]
		public static void PingPong(ref Tween tween)
		{
			// Flip the direction we are travelling in so time will move in the oppsite direction
			if(tween.Direction >= 0f)
			{
				tween.Overshoot = tween.CurrentSecs - tween.TotalSecs;
				tween.CurrentSecs = tween.TotalSecs - tween.Overshoot;
				tween.Value = tween.TweenFunc.Invoke(tween.Start, tween.End, tween.NormalisedTime);
			}
			else
			{
				tween.Overshoot = tween.CurrentSecs;
				tween.CurrentSecs = -tween.Overshoot;
				tween.Value = tween.TweenFunc.Invoke(tween.Start, tween.End, tween.NormalisedTime);
			}
			tween.Direction = -tween.Direction;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(LoopFunction))]
		public static void PingPongInvert(ref Tween tween)
		{
			// Flip invert which will flip the start and end values but don't change direction
			// as we are always increasing time but with flipped start and end
			tween.Invert = !tween.Invert;
			tween.Overshoot = tween.CurrentSecs - tween.TotalSecs;
			tween.CurrentSecs = tween.Overshoot;
			tween.Value = tween.TweenFunc.Invoke(tween.Start, tween.End, tween.NormalisedTime);			
		}
	}
}
