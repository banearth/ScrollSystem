using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TestDragEvent : MonoBehaviour
{

	public ScrollRect scrollRect;
	public GameObject button;

	void Start()
	{

		var listener1 = scrollRect.gameObject.AddComponent<EventTriggerListener>();
		listener1.onDrag += go =>
		{
			Debug.Log("onDrag EventTriggerListener");
		};

		listener1.onClick += go =>
		{
			Debug.Log("onClick EventTriggerListener");
		};

		//var listener2 = scrollRect.gameObject.AddComponent<DragListener>();
		//listener2.onDrag += go =>
		//{
		//	Debug.Log("onDrag DragListener");
		//};

		button.AddComponent<ClickListener>().onClick += go =>
		{
			Debug.Log("onClick button");
		};

	}

	// Update is called once per frame
	void Update()
	{

	}
}

public class EventTriggerListener : EventTrigger
{

	public Action<GameObject> onDrag;
	public Action<GameObject> onClick;
	public override void OnDrag(PointerEventData eventData)
	{
		onDrag?.Invoke(this.gameObject);
	}
	public override void OnPointerClick(PointerEventData eventData) {
		onClick?.Invoke(this.gameObject);
	}
}


public class DragListener : MonoBehaviour, IDragHandler
{
	public delegate void UIDelegate(GameObject go);
	public UIDelegate onDrag;

	public void OnDrag(PointerEventData eventData)
	{
		onDrag?.Invoke(gameObject);
	}
}

public class ClickListener : MonoBehaviour, IPointerClickHandler
{
	public Action<GameObject> onClick;

	public void OnPointerClick(PointerEventData eventData)
	{
		onClick?.Invoke(gameObject);
	}

}