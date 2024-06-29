using UnityEngine;
using MBS;

/// <summary>
/// WUTimer needs the player to be logged in before it can do anything.
/// This script just tells this temporary game object to wait around until the
/// player has loged in and then spawn the actual demo prefab before destroying itself.
/// </summary>
public class WUTLoginScanner : MonoBehaviour {

	/// <summary>
	/// The demo prefab to load once the player has logged in
	/// </summary>
	public GameObject DemoTimerPrefab;

	/// <summary>
	/// The parent canvas to instantiatet the demo prefab under
	/// </summary>
	public Canvas canvas;

	void Start() =>	WULogin.OnLoggedIn += StartTimerDemo;
	
	void StartTimerDemo(object data)
	{
		GameObject go = Instantiate(DemoTimerPrefab);
		go.transform.SetParent (canvas.transform, false);
		Destroy (gameObject);
	}

}
