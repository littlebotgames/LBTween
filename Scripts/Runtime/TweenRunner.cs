using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace LB.Tween
{
	public abstract class TweenData
	{
		public int Id { get; private set; }
		public abstract void Update(float easeValue);

		public TweenData(int id)
		{
			Id = id;
		}
	}

	public sealed class TweenData<T> : TweenData where T : struct
	{
		public delegate void UpdateCallback(int id, T value, object context);
		public delegate T GetValueFunction(T start, T end, float easeValue);

		private T m_start;
		private T m_end;
		private T m_value;
		public T Value => m_value;
		
		private GetValueFunction m_getValue;
		private UpdateCallback m_onUpdate;		
		private object m_context;

		public TweenData(int id, T start, T end, GetValueFunction getValue, UpdateCallback onUpdate, object context = null)
			: base(id)
		{
			m_start = start;
			m_end = end;
			m_getValue = getValue;
			m_onUpdate = onUpdate;			
			m_context = context;
		}

		public override void Update(float easeValue)
		{
			m_value = m_getValue(m_start, m_end, easeValue);
			m_onUpdate?.Invoke(Id, m_value, m_context);
		}
	}

	// The lookup for the GetValue functions, add more functions here if / when required
	public static class GetValueFunctions
	{
		public static readonly Dictionary<Type, Delegate> Lookup = new Dictionary<Type, Delegate>
		{
			{ typeof(float), new TweenData<float>.GetValueFunction(GetValueFloat) },
			{ typeof(Vector2), new TweenData<Vector2>.GetValueFunction(GetValueVector2) },
			{ typeof(Vector3), new TweenData<Vector3>.GetValueFunction(GetValueVector3) },
			{ typeof(Quaternion), new TweenData<Quaternion>.GetValueFunction(GetValueQuaternion) },
			{ typeof(Color), new TweenData<Color>.GetValueFunction(GetValueColor) },
			{ typeof(Color32), new TweenData<Color32>.GetValueFunction(GetValueColor32) },
		};

		public static float GetValueFloat(float start, float end, float easeValue)
		{
			return Mathf.LerpUnclamped(start, end, easeValue);
		}

		public static Vector2 GetValueVector2(Vector2 start, Vector2 end, float easeValue)
		{
			return Vector2.LerpUnclamped(start, end, easeValue);
		}

		public static Vector3 GetValueVector3(Vector3 start, Vector3 end, float easeValue)
		{
			return Vector3.LerpUnclamped(start, end, easeValue);
		}

		public static Quaternion GetValueQuaternion(Quaternion start, Quaternion end, float easeValue)
		{
			return Quaternion.SlerpUnclamped(start, end, easeValue);
		}

		public static Color GetValueColor(Color start, Color end, float easeValue)
		{
			return Color.LerpUnclamped(start, end, easeValue);
		}

		public static Color32 GetValueColor32(Color32 start, Color32 end, float easeValue)
		{
			return Color32.LerpUnclamped(start, end, easeValue);
		}
	}

	public class TweenRunner : IDisposable
	{
		[BurstCompile]
		private struct UpdateEaseValuesJob : IJobParallelFor
		{
			[NativeDisableParallelForRestriction]
			public NativeList<Tween> Tweens;
			public float DeltaT;

			public void Execute(int index)
			{
				var tween = Tweens[index];
				var direction = tween.Direction;
				tween.CurrentSecs += direction * DeltaT;
				tween.Value = tween.TweenFunc.Invoke(tween.Start, tween.End, tween.NormalisedTime);
				if(direction > 0f && tween.CurrentSecs >= tween.TotalSecs
					|| direction < 0f && tween.CurrentSecs <= 0f)
				{
					tween.LoopFunc.Invoke(ref tween);
				}
				Tweens[index] = tween;
			}
		}

		private NativeList<Tween> m_tweens;
		private List<TweenData> m_tweenDatas = new List<TweenData>();

		private Dictionary<int, int> m_tweenIdToIndex = new Dictionary<int, int>();
		private int m_nextId;

		public TweenRunner()
		{
			m_tweens = new NativeList<Tween>(Allocator.Persistent);
		}

		public void Dispose()
		{
			m_tweens.Dispose();
		}

		// Update all the tween ease values in parallel using the Unity job system and then loop through
		// and resolve the actual tween values and call the onUpdate callbacks
		public void Update(float deltaT)
		{
			var job = new UpdateEaseValuesJob
			{
				Tweens = m_tweens,
				DeltaT = deltaT
			};
			job.Run(m_tweens.Length);

			for(int i = 0, length = m_tweens.Length; i < length; ++i)
			{
				var tween = m_tweens[i];
				var tweenData = m_tweenDatas[i];
				tweenData.Update(tween.Value);
			}
		}

		// Start a tween. This will return the id of the tween which is used to stop it.
		// If you need to support a new type then add it to the GetValueFunctions lookup and functions.
		public int StartTween<T>(
			T start, 
			T end, 
			float secs, 
			EaseType ease, 
			LoopType loop, 
			TweenData<T>.UpdateCallback onUpdateCallback, 
			object context = null)
			where T : struct
		{
			var id = m_nextId++;

			var tweenIndex = m_tweens.Length;
			var tween = new Tween(
				secs,
				ease,
				loop
			);
			m_tweens.Add(tween);
			
			var tweenData = new TweenData<T>(
				id,
				start,
				end,
				(TweenData<T>.GetValueFunction)GetValueFunctions.Lookup[typeof(T)],
				onUpdateCallback,
				context
			);
			m_tweenDatas.Add(tweenData);
			m_tweenIdToIndex[id] = tweenIndex;

			return id;
		}

		// Stop a tween using it's id
		public void StopTween(int id)
		{
			if(!m_tweenIdToIndex.TryGetValue(id, out var index))
				return;

			m_tweenIdToIndex.Remove(id);
			m_tweens.RemoveAt(index);
			m_tweenDatas.RemoveAt(index);
		}

		// Call this function to get the current value of the tween. This will be slower than
		// using an onUpdate callback as it will have to look up the tween by id
		public T GetTweenValue<T>(int id) where T : struct
		{
			if(!m_tweenIdToIndex.TryGetValue(id, out var index))
				return default;

			var typedTweenData = (TweenData<T>)m_tweenDatas[index];
			return typedTweenData.Value;
		}
	}
}
