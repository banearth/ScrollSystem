using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultipleItemA : MonoBehaviour
{

	public Text label1;
	public Text label2;

	public void OnRefresh(MultipleDataA data)
	{
		label1.text = data.a.ToString();
		label2.text = data.b.ToString();
	}

}

public class MultipleDataA
{
	public int a;
	public int b;
}