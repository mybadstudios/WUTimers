using UnityEngine;
using UnityEngine.UI;
using MBS;

/// <summary>
/// A very simple class to show how to display the current values of timers as well as the amount of time left
/// before the value(s) are updated
/// 
/// WUTimer sends us updates whenever it has made contact with the server and received a response.
/// Usually that means a timer has reached 0 and we should check to see what the new value is on the server
/// and if the timer really WAS 0 or if someone just tried to hack the game.
/// 
/// In this script, though, I am using the very first server response just to trigger displaying all stats
/// and then to poll the various timers each second to ask each timer "How long before you contact the server?"
/// I then display the current timer's value as well as the remaining time before it contacts the server again.
/// 
/// The timers maintain themselves so there is nothiing more to do other than to display what it tells us to display
/// </summary>
public class WUTDemo : MonoBehaviour {
	
	public WUTimer
		Energy,
		Stamina,
		Lives;

	public Text
		energy_text,
		stamina_text,
		lives_text;

	void Start () => Energy.onTimerEvent += OnTimerResponse;	
	void OnTimerResponse(int points)
	{
		Energy.onTimerEvent -= OnTimerResponse;
		InvokeRepeating (nameof(ShowValues), 0f, 1f);
	}

	void ShowValues()
	{
		energy_text.text = $"{Energy.Value}/{Energy.ValueBounds}\n({Energy.FormattedTimer})";
		stamina_text.text = $"{Stamina.Value}/{Stamina.ValueBounds}\n({Stamina.FormattedTimer})";
		lives_text.text = $"{Lives.Value}/{Lives.ValueBounds}";
	}

	/// <summary>
	/// Deplete the Energy timer's associated value by 1. Value will not go below 0
	/// </summary>
	public void DepleteEnergy() => Energy.SpendPoints(1);

	/// <summary>
	/// Increase the Energy timer's associated value by 1. Value will not exceed it's max value
	/// </summary>
	public void GainEnergy() => Energy.GivePoints(1);

	/// <summary>
	/// Deplete the Stamina timer's associated value by 1
	/// </summary>
	public void DepleteStamina() => Stamina.SpendPoints(1);

	/// <summary>
	/// Increase the max value that the Stamina timer can have. 
	/// If the value was at max this will trigger the timer to start up again.
	/// If the value was not at max then the total will be updated but the timer will continue unhindered
	/// </summary>
	public void IncreaseMaxEnergy() => Energy.UpdateMaxPoints(1); 
}
