using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BanFPS : MonoBehaviour {
	/// <summary>
	/// 每次刷新计算的时间      帧/秒
	/// </summary>
	public float updateInterval = 0.5f;
	/// <summary>
	/// 最后间隔结束时间
	/// </summary>
	private double lastInterval;
	private int frames = 0;
	private float currFPS;

	public Text label_Text;
	
	void Start() {
		Application.targetFrameRate = 60;
		lastInterval = Time.realtimeSinceStartup;
		frames = 0;
	}
	
	void Update() {
		++frames;
		float timeNow = Time.realtimeSinceStartup;
		if (timeNow > lastInterval + updateInterval) {
			currFPS = (float)(frames / (timeNow - lastInterval));
			frames = 0;
			lastInterval = timeNow;
			if(label_Text != null){
				label_Text.text = "FPS:" + currFPS.ToString("f1");	
			}
		}
	}

}