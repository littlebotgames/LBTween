﻿namespace LB.Tween
{
	public abstract class TweenData
	{
		public abstract void Update(in Tween tween);
		public abstract void ReturnToPool();
	}

	public delegate T GetValueFunction<T>(T start, T end, float easeValue) where T : struct;
	public sealed class TweenData<T> : TweenData where T : struct
	{
		public delegate void OnUpdateCallback(int id, T value, object context);
		public delegate void OnFinishedCallback(int id, T value, object context);

		private int m_id;
		private T m_start;
		private T m_end;
		private T m_value;
		public T Value => m_value;

		private readonly GetValueFunction<T> m_getValue;
		private OnUpdateCallback m_onUpdate;
		private OnFinishedCallback m_onFinished;
		private object m_context;

		public TweenData(GetValueFunction<T> getValue)
		{
			m_getValue = getValue;
		}

		public void SetData(int id, T start, T end, OnUpdateCallback onUpdate, OnFinishedCallback onFinished = null, object context = null)
		{
			m_id = id;
			m_start = start;
			m_value = m_start;
			m_end = end;
			m_onUpdate = onUpdate;
			m_onFinished = onFinished;
			m_context = context;
		}

		public override void Update(in Tween tween)
		{
			m_value = m_getValue(m_start, m_end, tween.Value);
			m_onUpdate?.Invoke(m_id, m_value, m_context);
			if(tween.IsFinished)
			{
				m_onFinished?.Invoke(m_id, m_value, m_context);
			}
		}

		public override void ReturnToPool()
		{
			TweenDataRegistry.ReturnToPool(this);
		}
	}
}