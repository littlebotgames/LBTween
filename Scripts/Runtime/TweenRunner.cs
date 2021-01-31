using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LB.Tween
{
	public readonly struct TweenRef
	{
		public readonly int Id;
		public readonly int Index;

		public static readonly TweenRef Invalid = new TweenRef(TweenRunner.InvalidId, -1);

		public TweenRef(int id, int index)
		{
			Id = id;
			Index = index;			
		}

		public bool IsValid()
		{
			return Id != Invalid.Id;
		}
	}

	public abstract class TweenData
	{
		public abstract void Update(float easeValue);
		public abstract void ReturnToPool();
	}

	public delegate T GetValueFunction<T>(T start, T end, float easeValue) where T : struct;
	public sealed class TweenData<T> : TweenData where T : struct
	{
		public delegate void UpdateCallback(int id, T value, object context);

		private int m_id;
		private T m_start;
		private T m_end;
		private T m_value;
		public T Value => m_value;
		
		private readonly GetValueFunction<T> m_getValue;
		private UpdateCallback m_onUpdate;		
		private object m_context;

		public TweenData(GetValueFunction<T> getValue)
		{
			m_getValue = getValue;
		}

		public void SetData(int id, T start, T end, UpdateCallback onUpdate, object context = null)
		{
			m_id = id;
			m_start = start;
			m_value = m_start;
			m_end = end;
			m_onUpdate = onUpdate;			
			m_context = context;
		}

		public override void Update(float easeValue)
		{
			m_value = m_getValue(m_start, m_end, easeValue);
			m_onUpdate?.Invoke(m_id, m_value, m_context);
		}

		public override void ReturnToPool()
		{
			TweenDataRegistry.ReturnToPool(this);
		}
	}

	public static class TweenDataRegistry
	{
		// The lookup for the GetValue functions, add more functions here if / when required
		private static readonly Dictionary<Type, Delegate> m_getValueFunctions = new Dictionary<Type, Delegate>();
		private static Dictionary<Type, List<(TweenData TweenData, bool IsPooled)>> m_objectPools = new Dictionary<Type, List<(TweenData, bool)>>();

		static TweenDataRegistry()
		{
			RegisterBuiltinTypes();
		}

		private static void RegisterBuiltinTypes()
		{
			RegisterTweenDataType<float>(GetValueFloat);
			RegisterTweenDataType<Vector2>(GetValueVector2);
			RegisterTweenDataType<Vector3>(GetValueVector3);
			RegisterTweenDataType<Quaternion>(GetValueQuaternion);
			RegisterTweenDataType<Color>(GetValueColor);
			RegisterTweenDataType<Color32>(GetValueColor32);
		}

		private static GetValueFunction<T> GetGetValueFunction<T>() where T : struct
		{
			Debug.Assert(m_getValueFunctions.ContainsKey(typeof(T)), $"TweenData type {typeof(T)} has not been registered");
			return (GetValueFunction<T>)m_getValueFunctions[typeof(T)];
		}
		
		// Used to register any additional tween data types with a function to lerp the value based on the ease value
		public static void RegisterTweenDataType<T>(GetValueFunction<T> getValueFunction, int initPoolSize = 10) where T : struct
		{
			var type = typeof(T);
			m_getValueFunctions[type] = getValueFunction;

			var newPool = new List<(TweenData, bool)>(initPoolSize);
			for(int i = 0; i < initPoolSize; ++i)
			{
				newPool.Add((new TweenData<T>(getValueFunction), true));
			}
			m_objectPools[type] = newPool;
		}

		// Get a tween data of type from pool, this will create a new one if the pool has none free
		public static TweenData<T> GetFromPool<T>() where T : struct
		{
			var pool = m_objectPools[typeof(T)];
			for (int i = 0; i < pool.Count; ++i)
			{
				var poolItem = pool[i];
				if(!poolItem.IsPooled)
					continue;

				poolItem.IsPooled = false;
				pool[i] = poolItem;
				return (TweenData<T>)pool[i].TweenData;
			}

			// Failed to find free object so create a new one
			var getValueFunction = GetGetValueFunction<T>();
			var newTweenData = new TweenData<T>(getValueFunction);
			pool.Add((newTweenData, false));
			return newTweenData;
		}

		// Return the TweenData object to the pool
		public static void ReturnToPool<T>(TweenData<T> tweenData) where T : struct
		{
			var pool = m_objectPools[typeof(T)];
			for (int i = 0; i < pool.Count; ++i)
			{
				var poolItem = pool[i];
				if(poolItem.TweenData != tweenData)
					continue;

				poolItem.IsPooled = true;
				pool[i] = poolItem;
			}
		}

		// Builtin GetValue functions
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
			public NativeArray<Tween> Tweens;
			public float DeltaT;

			public void Execute(int index)
			{
				var tween = Tweens[index];

				if(tween.Id == InvalidId)
					return;

				if(tween.IsPaused)
					return;

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

		private const int c_maxTweens = 100;
		private NativeArray<Tween> m_tweens;
		private TweenData[] m_tweenDatas;

		public const int InvalidId = 0;
		private int m_nextId = 1;
		private int m_tweensLength;

		public TweenRunner(int maxTweens = c_maxTweens)
		{
			m_tweens = new NativeArray<Tween>(maxTweens, Allocator.Persistent);
			m_tweenDatas = new TweenData[maxTweens];
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
			job.Run(m_tweensLength);

			for(int i = 0, length = m_tweensLength; i < length; ++i)
			{
				var tween = m_tweens[i];
				if(tween.Id == InvalidId)
					continue;

				var tweenData = m_tweenDatas[i];
				tweenData.Update(tween.Value);
			}
		}

		// Start a tween. This will return the id of the tween which is used to stop it.
		// If you need to support a new type then register it with TweenDataUtils.RegisterTweenDataType
		public TweenRef StartTween<T>(
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
			var index = GetFreeTweenIndex();
			var tween = new Tween(
				id,
				secs,
				ease,
				loop
			);
			m_tweens[index] = tween;
			
			var tweenData = TweenDataRegistry.GetFromPool<T>();
			tweenData.SetData(
				id,
				start,
				end,
				onUpdateCallback,
				context
			);
			m_tweenDatas[index] = tweenData;

			m_tweensLength = Mathf.Max(m_tweensLength, index + 1);

			// Return both the id and index so we can look up the tween by the index but also validate that
			// we are not using stale ids from previously stopped 
			return new TweenRef(id, index);
		}

		// Find the next free tween in the list to use and return it's index
		private int GetFreeTweenIndex()
		{
			for(int i = 0; i < m_tweens.Length; ++i)
			{
				if(m_tweens[i].Id == 0)
					return i;
			}

			Debug.LogAssertion($"Failed to find free tween index, you need to increase the size of maxTween > {m_tweens.Length}");
			return -1;
		}

		public void PauseTween(in TweenRef id)
		{
			SetTweenPaused(id, true);
		}

		public void UnpauseTween(in TweenRef id)
		{
			SetTweenPaused(id, false);
		}

		// Pause / Unpause a tween using it's id
		private void SetTweenPaused(in TweenRef id, bool paused)
		{
			if(!m_tweens.IsCreated)
				return;

			ValidateTweenId(id);

			var tween = m_tweens[id.Index];
			tween.IsPaused = paused;
			m_tweens[id.Index] = tween;
		}

		// Stop a tween using it's id
		public void StopTween(in TweenRef id)
		{
			if(!m_tweens.IsCreated)
				return;

			ValidateTweenId(id);

			var tween = m_tweens[id.Index];
			tween.Id = InvalidId;
			m_tweens[id.Index] = tween;

			m_tweenDatas[id.Index].ReturnToPool();
			m_tweenDatas[id.Index] = null;

			if(id.Index == m_tweensLength - 1)
			{
				// Recalculate the tweens length as the last one has been stopped
				do
				{
					--m_tweensLength;
				}
				while(m_tweensLength > 0 && m_tweens[m_tweensLength - 1].Id != InvalidId);
			}
			
		}

		// Call this function to get the current value of the tween. This will be slower than
		// using an onUpdate callback
		public T GetTweenValue<T>(in TweenRef id) where T : struct
		{
			if(!m_tweens.IsCreated)
				return default;

			ValidateTweenId(id);

			var typedTweenData = (TweenData<T>)m_tweenDatas[id.Index];
			return typedTweenData.Value;
		}

		public int GetNumberOfRunningTweens()
		{
			var numTweens = 0;
			for(var i = 0; i < m_tweensLength; ++i)
			{
				if(m_tweens[i].Id != InvalidId)
				{
					++numTweens;
				}
			}

			return numTweens;
		}

		[Conditional("UNITY_ASSERTIONS")]
		private void ValidateTweenId(in TweenRef id)
		{
			var tween = m_tweens[id.Index];
			Debug.Assert(tween.Id == id.Id, $"Tween Id {id.Id} does not match the tween at index Id {tween.Id}, you are using a old id for a stopped tween");
		}
	}
}
