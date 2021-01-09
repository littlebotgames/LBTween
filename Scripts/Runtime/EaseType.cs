using AOT;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace LB.Tween
{
	public enum EaseType
	{
		Linear,

		OutQuad,
		InQuad,
		InOutQuad,

		InCubic,
		OutCubic,
		InOutCubic,

		InQuart,
		OutQuart,
		InOutQuart,

		InQuint,
		OutQuint,
		InOutQuint,

		InSine,
		OutSine,
		InOutSine,

		InExpo,
		OutExpo,
		InOutExpo,

		InCirc,
		OutCirc,
		InOutCirc,

		InBounce,
		OutBounce,
		InOutBounce,

		InBack,
		OutBack,
		InOutBack,

		InElastic,
		OutElastic,
		InOutElastic
	}

	public delegate float EaseFunction(float start, float end, float value);

	[BurstCompile]
	public static class EaseUtils
	{
		public static FunctionPointer<EaseFunction> GetFunction(EaseType ease)
		{
			switch (ease)
			{
				case EaseType.Linear:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(Linear);
				case EaseType.InQuad:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(InQuad);
				case EaseType.OutQuad:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(OutQuad);
				case EaseType.InOutQuad:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(InOutQuad);
				case EaseType.InCubic:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(InCubic);
				case EaseType.OutCubic:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(OutCubic);
				case EaseType.InOutCubic:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(InOutCubic);
				case EaseType.InQuart:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(InQuart);
				case EaseType.OutQuart:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(OutQuart);
				case EaseType.InOutQuart:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(InOutQuart);
				case EaseType.InQuint:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(InQuint);
				case EaseType.OutQuint:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(OutQuint);
				case EaseType.InOutQuint:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(InOutQuint);
				case EaseType.InSine:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(InSine);
				case EaseType.OutSine:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(OutSine);
				case EaseType.InOutSine:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(InOutSine);
				case EaseType.InExpo:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(InExpo);
				case EaseType.OutExpo:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(OutExpo);
				case EaseType.InOutExpo:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(InOutExpo);
				case EaseType.InCirc:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(InCirc);
				case EaseType.OutCirc:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(OutCirc);
				case EaseType.InOutCirc:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(InOutCirc);
				case EaseType.InBounce:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(InBounce);
				case EaseType.OutBounce:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(OutBounce);
				case EaseType.InOutBounce:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(InOutBounce);
				case EaseType.InBack:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(InBack);
				case EaseType.OutBack:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(OutBack);
				case EaseType.InOutBack:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(InOutBack);
				case EaseType.InElastic:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(InElastic);
				case EaseType.OutElastic:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(OutElastic);
				case EaseType.InOutElastic:
					return BurstCompiler.CompileFunctionPointer<EaseFunction>(InOutElastic);
				default:
					Debug.LogAssertion($"Unhandled ease type {ease}");
					return default;
			}
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float Linear(float start, float end, float value)
		{
			return math.lerp(start, end, value);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float InQuad(float start, float end, float value)
		{
			end -= start;
			return end * value * value + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float OutQuad(float start, float end, float value)
		{
			end -= start;
			return -end * value * (value - 2f) + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float InOutQuad(float start, float end, float value)
		{
			value *= 2f;
			end -= start;
			if (value < 1f)
				return end / 2f * value * value + start;
			value--;
			return -end / 2f * (value * (value - 2f) - 1f) + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float InCubic(float start, float end, float value)
		{
			end -= start;
			return end * value * value * value + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float OutCubic(float start, float end, float value)
		{
			value--;
			end -= start;
			return end * (value * value * value + 1) + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float InOutCubic(float start, float end, float value)
		{
			value /= .5f;
			end -= start;
			if (value < 1) return end / 2 * value * value * value + start;
			value -= 2;
			return end / 2 * (value * value * value + 2) + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float InQuart(float start, float end, float value)
		{
			end -= start;
			return end * value * value * value * value + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float OutQuart(float start, float end, float value)
		{
			value--;
			end -= start;
			return -end * (value * value * value * value - 1) + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float InOutQuart(float start, float end, float value)
		{
			value /= .5f;
			end -= start;
			if (value < 1) return end / 2 * value * value * value * value + start;
			value -= 2;
			return -end / 2 * (value * value * value * value - 2) + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float InQuint(float start, float end, float value)
		{
			end -= start;
			return end * value * value * value * value * value + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float OutQuint(float start, float end, float value)
		{
			value--;
			end -= start;
			return end * (value * value * value * value * value + 1) + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float InOutQuint(float start, float end, float value)
		{
			value /= .5f;
			end -= start;
			if (value < 1) return end / 2 * value * value * value * value * value + start;
			value -= 2;
			return end / 2 * (value * value * value * value * value + 2) + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float InSine(float start, float end, float value)
		{
			end -= start;
			return -end * math.cos(value / 1 * (math.PI * 0.5f)) + end + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float OutSine(float start, float end, float value)
		{
			end -= start;
			return end * math.sin(value / 1 * (math.PI * 0.5f)) + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float InOutSine(float start, float end, float value)
		{
			end -= start;
			return -end / 2 * (math.cos(math.PI * value / 1) - 1) + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float InExpo(float start, float end, float value)
		{
			end -= start;
			return end * math.pow(2, 10 * (value / 1 - 1)) + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float OutExpo(float start, float end, float value)
		{
			end -= start;
			return end * (-math.pow(2, -10 * value / 1) + 1) + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float InOutExpo(float start, float end, float value)
		{
			value /= .5f;
			end -= start;
			if (value < 1) return end / 2 * math.pow(2, 10 * (value - 1)) + start;
			value--;
			return end / 2 * (-math.pow(2, -10 * value) + 2) + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float InCirc(float start, float end, float value)
		{
			end -= start;
			return -end * (math.sqrt(1 - value * value) - 1) + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float OutCirc(float start, float end, float value)
		{
			value--;
			end -= start;
			return end * math.sqrt(1 - value * value) + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float InOutCirc(float start, float end, float value)
		{
			value /= .5f;
			end -= start;
			if (value < 1) return -end / 2 * (math.sqrt(1 - value * value) - 1) + start;
			value -= 2;
			return end / 2 * (math.sqrt(1 - value * value) + 1) + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float InBounce(float start, float end, float value)
		{
			return InBounceInternal(start, end, value);
		}

		private static float InBounceInternal(float start, float end, float value)
		{
			end -= start;
			float d = 1f;
			return end - OutBounceInternal(0, end, d - value) + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float OutBounce(float start, float end, float value)
		{
			return OutBounceInternal(start, end, value);
		}

		private static float OutBounceInternal(float start, float end, float value)
		{
			value /= 1f;
			end -= start;
			if (value < (1 / 2.75f))
			{
				return end * (7.5625f * value * value) + start;
			}
			else if (value < (2 / 2.75f))
			{
				value -= (1.5f / 2.75f);
				return end * (7.5625f * (value) * value + .75f) + start;
			}
			else if (value < (2.5 / 2.75))
			{
				value -= (2.25f / 2.75f);
				return end * (7.5625f * (value) * value + .9375f) + start;
			}
			else
			{
				value -= (2.625f / 2.75f);
				return end * (7.5625f * (value) * value + .984375f) + start;
			}
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float InOutBounce(float start, float end, float value)
		{
			end -= start;
			float d= 1f;
			if (value < d / 2) return InBounceInternal(0, end, value * 2) * 0.5f + start;
			else return OutBounceInternal(0, end, value * 2 - d) * 0.5f + end * 0.5f + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float InBack(float start, float end, float value)
		{
			return InBack(start, end, value, 1f);
		}

		public static float InBack(float start, float end, float value, float overshoot = 1.0f)
		{
			end -= start;
			value /= 1;
			float s= 1.70158f * overshoot;
			return end * (value) * value * ((s + 1) * value - s) + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float OutBack(float start, float end, float value)
		{
			return OutBack(start, end, value, 1f);
		}

		public static float OutBack(float start, float end, float value, float overshoot = 1.0f)
		{
			float s = 1.70158f * overshoot;
			end -= start;
			value = (value / 1) - 1;
			return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float InOutBack(float start, float end, float value)
		{
			return InOutBack(start, end, value, 1f);
		}

		public static float InOutBack(float start, float end, float value, float overshoot = 1.0f)
		{
			float s = 1.70158f * overshoot;
			end -= start;
			value /= .5f;
			if ((value) < 1)
			{
				s *= (1.525f) * overshoot;
				return end / 2 * (value * value * (((s) + 1) * value - s)) + start;
			}
			value -= 2;
			s *= (1.525f) * overshoot;
			return end / 2 * ((value) * value * (((s) + 1) * value + s) + 2) + start;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float InElastic(float start, float end, float value)
		{
			return InElastic(start, end, value, 1f, 0.3f);
		}

		public static float InElastic(float start, float end, float value, float overshoot = 1.0f, float period = 0.3f)
		{
			end -= start;

			float p = period;
			float s = 0f;
			float a = 0f;

			if (value == 0f) return start;

			if (value == 1f) return start + end;

			if (a == 0f || a < math.abs(end))
			{
				a = end;
				s = p / 4f;
			}
			else
			{
				s = p / (2f * math.PI) * math.asin(end / a);
			}

			if (overshoot > 1f && value > 0.6f)
				overshoot = 1f + ((1f - value) / 0.4f * (overshoot - 1f));

			value = value - 1f;
			return start - (a * math.pow(2f, 10f * value) * math.sin((value - s) * (2f * math.PI) / p)) * overshoot;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float OutElastic(float start, float end, float value)
		{
			return OutElastic(start, end, value, 1f, 0.3f);
		}

		public static float OutElastic(float start, float end, float value, float overshoot = 1.0f, float period = 0.3f)
		{
			end -= start;

			float p = period;
			float s = 0f;
			float a = 0f;

			if (value == 0f) return start;

			if (value == 1f) return start + end;

			if (a == 0f || a < math.abs(end))
			{
				a = end;
				s = p / 4f;
			}
			else
			{
				s = p / (2f * math.PI) * math.asin(end / a);
			}
			if (overshoot > 1f && value < 0.4f)
				overshoot = 1f + (value / 0.4f * (overshoot - 1f));

			return start + end + a * math.pow(2f, -10f * value) * math.sin((value - s) * (2f * math.PI) / p) * overshoot;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(EaseFunction))]
		public static float InOutElastic(float start, float end, float value)
		{
			return InOutElastic(start, end, value, 1f, 0.3f);
		}

		public static float InOutElastic(float start, float end, float value, float overshoot = 1.0f, float period = 0.3f)
		{
			end -= start;

			float p = period;
			float s = 0f;
			float a = 0f;

			if (value == 0f) return start;

			value = value / (1f / 2f);
			if (value == 2f) return start + end;

			if (a == 0f || a < math.abs(end))
			{
				a = end;
				s = p / 4f;
			}
			else
			{
				s = p / (2f * math.PI) * math.asin(end / a);
			}

			if (overshoot > 1f)
			{
				if (value < 0.2f)
				{
					overshoot = 1f + (value / 0.2f * (overshoot - 1f));
				}
				else if (value > 0.8f)
				{
					overshoot = 1f + ((1f - value) / 0.2f * (overshoot - 1f));
				}
			}

			if (value < 1f)
			{
				value = value - 1f;
				return start - 0.5f * (a * math.pow(2f, 10f * value) * math.sin((value - s) * (2f * math.PI) / p)) * overshoot;
			}
			value = value - 1f;
			return end + start + a * math.pow(2f, -10f * value) * math.sin((value - s) * (2f * math.PI) / p) * 0.5f * overshoot;
		}

	}
}
