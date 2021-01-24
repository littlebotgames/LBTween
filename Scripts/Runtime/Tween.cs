using System.Runtime.InteropServices;
using Unity.Burst;

namespace LB.Tween
{
	public struct Tween
	{
		public int Id;
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
		[MarshalAs(UnmanagedType.U1)]
		public bool IsPaused;

		public float NormalisedTime => CurrentSecs / TotalSecs;
		public float Start => Invert ? 1f : 0f;
		public float End => Invert ? 0f : 1f;

		public Tween(int id, float secs, 
			FunctionPointer<EaseFunction> tweenFunc, FunctionPointer<LoopFunction> loopFunc)
		{
			Id = id;
			TotalSecs = secs;
			TweenFunc = tweenFunc;
			LoopFunc = loopFunc;
			CurrentSecs = 0f;
			Value = 0f;
			IsFinished = false;
			Direction = 1f;
			Overshoot = 0f;
			Invert = false;
			IsPaused = false;
		}

		public Tween(int id, float secs, EaseType ease, LoopType loop)
			: this(id, secs, EaseUtils.GetFunction(ease), LoopUtils.GetFunction(loop))
		{
		}
	}
}
