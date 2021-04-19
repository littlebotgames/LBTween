using System;

namespace LB.Tween
{
	[Serializable]
	public struct TweenParams
	{
		public float TweenSecs;
		public EaseType Ease;
		public LoopType Loop;
		public float StartDelaySecs;
		public float EndDelaySecs;
		public float StartOffsetSecs;

		public TweenParams(float tweenSecs, EaseType ease, LoopType loop,
			float startDelaySecs = 0f, float endDelaySecs = 0f, float startOffsetSecs = 0f)
		{
			TweenSecs = tweenSecs;
			StartDelaySecs = startDelaySecs;
			EndDelaySecs = endDelaySecs;
			Ease = ease;
			Loop = loop;
			StartOffsetSecs = startOffsetSecs;
		}
	}
}
