using UnityEngine;
using System.Collections;

namespace ThinRL.Core
{
	public class CoroutineManager : MonoSingleton<CoroutineManager>
	{
		/// <summary>
		/// 全局协程入口
		/// </summary>
		public new Coroutine StartCoroutine(IEnumerator routine)
		{
			return StartCoroutine(routine, this);
		}

		/// <summary>
		/// 协程（该函数可根据需要传入特定的MonoBehaviour）
		/// </summary>
		public Coroutine StartCoroutine(IEnumerator routine, MonoBehaviour mono)
		{
			return mono.StartCoroutine(routine);
		}

		public new void StopCoroutine(Coroutine coroutine)
        {
			StopCoroutine(coroutine, this);
        }

		public void StopCoroutine(Coroutine coroutine, MonoBehaviour mono)
        {
			mono.StopCoroutine(coroutine);
        }
	}
}
