using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BanSupport.ScrollSystem
{
	public class PrefabGroup
	{
		public string prefabName { get; private set; }
		public GameObject origin;
		public GameObject bindOrigin;
		public GameObjectPool pool;
		public ScrollLayout.NewLine newLine { private set; get; }

		public float prefabWidth { private set; get; }

		public float prefabHeight { private set; get; }

		private ScrollSystem scrollSystem;

		public PrefabGroup(GameObject origin, ScrollSystem scrollSystem, int registPoolCount)
		{
			this.prefabName = origin.name;
			this.origin = origin;
			this.scrollSystem = scrollSystem;
			var prefabRectTransform = origin.transform as RectTransform;
			this.prefabWidth = prefabRectTransform.sizeDelta.x;
			this.prefabHeight = prefabRectTransform.sizeDelta.y;
			var layout = origin.GetComponent<ScrollLayout>();
			if (layout != null)
			{
				this.newLine = layout.newLine;
			}
			else
			{
				this.newLine = ScrollLayout.NewLine.None;
			}
			this.pool = new GameObjectPool(origin, scrollSystem.contentTrans, registPoolCount);
		}

		public GameObject Get()
		{
			return this.pool.Get();
		}

		public void Release(GameObject obj)
		{
			this.pool.Release(obj);
		}

		public void BindScript(Action<string, GameObject> bindFunc)
		{
			if (bindOrigin == null)
			{
				bindOrigin = Instantiate(origin, scrollSystem.transform);
				bindOrigin.name = bindOrigin.name.Substring(0, bindOrigin.name.Length - 7) + "_BindScript";
				bindOrigin.SetActive(true);
				bindOrigin.SetActive(false);
			}
			bindFunc(prefabName, bindOrigin);
		}

	}

}