namespace LB.Tween
{
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
}