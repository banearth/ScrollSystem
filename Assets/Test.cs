using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnRectTransformDimensionsChange()
	{
        Debug.Log("OnRectTransformDimensionsChange");
	}

	private void OnTransformChildrenChanged()
    {
		Debug.Log("OnTransformChildrenChanged");
	}

}
