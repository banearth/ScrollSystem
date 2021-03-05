using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanSupport;

public class MultipleManager : MonoBehaviour
{

	public ScrollSystem scrollSystem;
	private int a;

	void Start()
	{

		scrollSystem.SetItemRefresh((prefabName, root, data) =>
		{
			switch (prefabName)
			{
				case "A":
					{
						root.GetComponent<MultipleItemA>().OnRefresh(data as MultipleDataA);
					}
					break;
			}
		});
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.A))
		{
			scrollSystem.Add("A", new MultipleDataA { a = ++a, b = 2 });
		}

	}

}
