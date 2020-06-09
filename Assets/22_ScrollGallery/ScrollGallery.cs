using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BanSupport
{
	public class ScrollGallery : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{

		/// <summary>
		/// 滚动方向
		/// </summary>
		public enum ScrollDirection { Vertical, Horizontal }
		[SerializeField]
		private ScrollDirection scrollDirection = ScrollDirection.Vertical;

		/// <summary>
		/// 跳转的速度
		/// </summary>
		[Tooltip("跳转的速度")]
		[SerializeField]
		private float jumpToSpeed = 10;

		/// <summary>
		/// 注册数量
		/// </summary>
		[Tooltip("注册数量")]
		[SerializeField]
		private int registPoolCount = 3;

		public float width
		{
			get
			{
				return rectTransform.sizeDelta.x;
			}
		}

		public float height
		{
			get
			{
				return rectTransform.sizeDelta.y;
			}
		}

		private RectTransform _rectTransform = null;
		public RectTransform rectTransform
		{
			get
			{
				if (_rectTransform == null)
				{
					_rectTransform = this.transform as RectTransform;
				}
				return _rectTransform;
			}
		}

		public RectTransform[] splits;

		public int splitCount = 5;

		[ContextMenu("Create Split")]
		public void Split()
		{
			foreach (var split in splits)
			{
				if (split != null)
				{
					DestroyImmediate(split.gameObject);
				}
			}
			splits = new RectTransform[splitCount];
			if (scrollDirection == ScrollDirection.Vertical)
			{
				var splitWidth = width;
				var splitHeight = height / 5;
				for (int i = 0; i < splitCount; i++)
				{
					var split = new GameObject("split_" + i, typeof(RectTransform)).transform as RectTransform;
					split.SetParent(this.transform);
					split.sizeDelta = new Vector2(splitWidth, splitHeight);
					split.anchoredPosition = new Vector2(0, -i * splitHeight + height / 2 - splitHeight / 2);
					splits[i] = split;
				}
			}
			else
			{
				var splitWidth = width / 5;
				var splitHeight = height;
				for (int i = 0; i < splitCount; i++)
				{
					var split = new GameObject("split_" + i, typeof(RectTransform)).transform as RectTransform;
					split.SetParent(this.transform);
					split.sizeDelta = new Vector2(splitWidth, splitHeight);
					split.anchoredPosition = new Vector2(i * splitWidth - width / 2 + splitWidth / 2, 0);
					splits[i] = split;
				}
			}

		}

		private void OnDrawGizmos()
		{
			Gizmos.color = new Color(0, 1, 0, 0.2f);
			foreach (var split in splits)
			{
				if (split != null) {
					var size = new Vector3(split.lossyScale.x * split.sizeDelta.x, split.lossyScale.y * split.sizeDelta.y, 0);
					Gizmos.DrawWireCube(split.position, size);
				}
			}
		}

		public enum State { None, Drag, Move, Return }
		private State state = State.None;
		private int curPointerId = int.MinValue;

		public void OnPointerDown(PointerEventData eventData)
		{
			Debug.Log("OnPointerDown:" + eventData.position);
			if (curPointerId != int.MinValue) { return; }
			curPointerId = eventData.pointerId;
			state = State.Drag;
			listData.ForEach(temp=> temp.RecordPos());

		}

		public void OnPointerUp(PointerEventData eventData)
		{
			Debug.Log("OnPointerUp:" + eventData.position);
			if (curPointerId != eventData.pointerId) { return; }
			state = State.None;
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (curPointerId != eventData.pointerId) { return; }
			Debug.Log("OnDrag:" + eventData.position);
		}

		private void OnDisable()
		{
			state = State.None;
			curPointerId = int.MinValue;
		}

		private void LateUpdate()
		{
			switch (state){
				case State.Drag:
					
					break;
			}

		}

		private List<SubData> listData = new List<SubData>();

		public class SubData
		{
			private float normalizedPos;
			private float recordNormalizedPos;
			public SubData() { 
				
			}

			public void RecordPos() {
				this.recordNormalizedPos = this.normalizedPos;
			}

			public void Move(float normalizedOffset)
			{
				this.normalizedPos += normalizedOffset;
			}

		}

	}
}