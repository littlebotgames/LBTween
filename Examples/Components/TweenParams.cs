using System;

namespace LB.Tween.Example
{
	[Serializable]
	public struct TweenParams
	{
		public float Secs;
		public EaseType Ease;
		public LoopType Loop;
	}
}
