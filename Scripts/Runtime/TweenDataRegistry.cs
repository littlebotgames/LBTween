using System;
using System.Collections.Generic;
using UnityEngine;

namespace LB.Tween
{
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
			for (int i = 0; i < initPoolSize; ++i)
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
				if (!poolItem.IsPooled)
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
				if (poolItem.TweenData != tweenData)
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
}
