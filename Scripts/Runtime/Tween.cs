using System.Runtime.InteropServices;
using Unity.Burst;

namespace LB.Tween
{
	public struct Tween
	{
		public float TotalSecs;
		public FunctionPointer<EaseFunction> TweenFunc;
		public FunctionPointer<LoopFunction> LoopFunc;
		public float CurrentSecs;
		public float Value;
		[MarshalAs(UnmanagedType.U1)]
		public bool IsFinished;
		public float Direction;
		public float Overshoot;
		[MarshalAs(UnmanagedType.U1)]
		public bool Invert;

		public float NormalisedTime => CurrentSecs / TotalSecs;
		public float Start => Invert ? 1f : 0f;
		public float End => Invert ? 0f : 1f;

		public Tween(float secs, 
			FunctionPointer<EaseFunction> tweenFunc, FunctionPointer<LoopFunction> loopFunc)
		{
			TotalSecs = secs;
			TweenFunc = tweenFunc;
			LoopFunc = loopFunc;
			CurrentSecs = 0f;
			Value = 0f;
			IsFinished = false;
			Direction = 1f;
			Overshoot = 0f;
			Invert = false;
		}

		public Tween(float secs, EaseType ease, LoopType loop)
			: this(secs, EaseUtils.GetFunction(ease), LoopUtils.GetFunction(loop))
		{
		}
	}
}
