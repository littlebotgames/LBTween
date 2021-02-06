using System;
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
				var normT = tween.NormalisedTime;
				tweenData.Update(tween.Value);
			}
		}

		// Start a tween. This will return the id of the tween which is used to stop it.
		// If you need to support a new type then register it with TweenDataUtils.RegisterTweenDataType
		public TweenRef StartTween<T>(
			T start, 
			T end, 
			in TweenParams tweenParams, 
			TweenData<T>.UpdateCallback onUpdateCallback, 
			object context = null)
			where T : struct
		{
			if(tweenParams.TweenSecs <= 0f)
			{
				return TweenRef.Invalid;
			}

			var id = m_nextId++;
			var index = GetFreeTweenIndex();
			var tween = new Tween(id, tweenParams);
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
