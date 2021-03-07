using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{
	public class UIExtensionManager : MonoBehaviour
	{

		private static UIExtensionManager Instance;

		private List<ExBase> updateDatas;

		private void Awake()
		{
			this.gameObject.hideFlags = HideFlags.HideInHierarchy;
			DontDestroyOnLoad(this.gameObject);
			updateDatas = ListPool<ExBase>.Get();
		}

		private void OnDestroy()
		{
			ListPool<ExBase>.Release(updateDatas);
			updateDatas = null;
		}

		private void LateUpdate()
		{
			for (int i = updateDatas.Count - 1; i >= 0; i--)
			{
				var curBase = updateDatas[i];
				if (curBase == null)
				{
					updateDatas.RemoveAt(i);
					continue;
				}
				if (!curBase.DoUpdate())
				{
					updateDatas.RemoveAt(i);
				}
			}
		}

		public static void Add(ExBase exBase)
		{
			if (Instance != null)
			{
				if (!Instance.updateDatas.Contains(exBase))
				{
					Instance.updateDatas.Add(exBase);
				}
			}
		}

		public static void Remove(ExBase exBase)
		{
			if (Instance != null)
			{
				if (Instance.updateDatas.Contains(exBase))
				{
					Instance.updateDatas.Remove(exBase);
				}
			}
		}

		public static void Init()
		{
			if (Instance == null)
			{
				Instance = new GameObject("UIExtensionManager").AddComponent<UIExtensionManager>();
			}
		}


	}

}