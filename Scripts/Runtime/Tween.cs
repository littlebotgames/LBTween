using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Mathematics;

namespace LB.Tween
{
	public struct Tween
	{
		public int Id;
		public float TweenSecs;
		public float StartDelaySecs;
		public float EndDelaySecs;
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

		public float TotalSecs => StartDelaySecs + TweenSecs + EndDelaySecs;
		public float NormalisedTime
		{
			get
			{
				// NormalisedTime will give us 0 in all the start delay and 1 in all the end delay 
				// and between 0 - 1 during the tween section
				var clampedSecs = math.clamp(CurrentSecs, StartDelaySecs, TotalSecs - EndDelaySecs) - StartDelaySecs;
				return clampedSecs / TweenSecs;
			}
		}
		public float Start => Invert ? 1f : 0f;
		public float End => Invert ? 0f : 1f;

		public Tween(int id, in TweenParams tweenParams)
		{
			Id = id;
			TweenSecs = tweenParams.TweenSecs;
			StartDelaySecs = tweenParams.StartDelaySecs;
			EndDelaySecs = tweenParams.EndDelaySecs;
			TweenFunc = EaseUtils.GetFunction(tweenParams.Ease);
			LoopFunc = LoopUtils.GetFunction(tweenParams.Loop);
			CurrentSecs = 0f;
			Value = 0f;
			IsFinished = false;
			Direction = 1f;
			Overshoot = 0f;
			Invert = false;
			IsPaused = false;
		}
	}
}
