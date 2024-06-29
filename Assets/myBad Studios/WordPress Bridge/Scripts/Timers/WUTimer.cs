//WordPress For Unity Bridge: Timers Extension © 2024 by Ryunosuke Jansen is licensed under CC BY-ND 4.0.

using UnityEngine;
using System;

namespace MBS {

	public class WUTimer : MonoBehaviour {

		enum WUTActions
		{
			Dummy,
			FetchAllStats,
			FetchStat,
			GetStatStatus,
			SpendPoints,
			GetPoints,
			UpdateMaxPoints,
			UpdateMaxTimer,
			SetMaxPoints,
			SetMaxTimer,
			DeleteTimer,
			Count
		}

        #region internal variables
        const string timer_filepath = "wub_timer/unity_functions.php";
		const string ASSET = "TIMER";
        #endregion

		/// <summary>
		/// Subscribe to this Action to trigger custom code after the server has sent back a response
		/// </summary>
        public Action<CMLData> onContactedServer;

		/// <summary>
		/// Subscribe to this Action to trigger custom code whenever the points value of this timer has changed. 
		/// Normally this would mean the timer has run out and a new value was fetched from the server.
		/// Even if players hack the game to alter their point values, whenever the server is contacted it will send back
		/// the correct value at that time. Hooking into this event means you are working with the latest result recieved
		/// from the server. 
		/// </summary>
		public Action<int> onTimerEvent;

		/// <summary>
		/// The name of the stat that will be stored in the database. 
		/// The value of the stat will be fetched by this value so be sure of the spelling
		/// TIP: For farming sim or other grid type games, name your timer according to the grid location (ie.R001C020)
		///		 This way you can reuse the same timers and avoid leaving unused timers filling up your database
		/// </summary>
		public string field_name = "Energy";

		/// <summary>
		/// If this timer doesn't exist in the database, create it and set it's stat's starting value to this amount.
		/// You can either set this to 0 or set it to match initial_max_value depending on wether you want the stat to
		/// start filled (i.e. first time you play you have 5 of your 5 lives available) or if you want thecounter to
		/// start immediately (i.e. within 24 hours from now you qualify for your first gold coin reward)
		/// </summary>
		public int initial_starting_value = 5;

		/// <summary>
		/// If this timers doesn't exist in the database, create it and set the max value this timer's stat can have to
		/// this amount. (I.e. Lives can be max 5, Stamina can be max 20, etc)
		/// </summary>
		public int initial_max_value = 5;       

		/// <summary>
		/// If this timer doesn't exist in the database, create it and set the duration between stat updates to this value.
		/// in seconds. (i.e. every 60 seconds: gain 1 Stamina, in 600 seconds gain 1 Energy, in 3600 seconds gain 1 life etc)
		/// </summary>
		public int initial_max_timer = 60;
		
		/// <summary>
		/// Should the timer the player sees count down to 0 or should it count up to the timer value. 
		/// I.e. should the player see "5 more seconds till I get a new life" 
		/// or should the player see "My life timer is at 1:55 of 2:00. Almost time for an extra life"
		/// </summary>
		public bool	count_down = false;

		/// <summary>
		/// The raw value in seconds indicating how long till the next update occurson the server. 
		/// This value is mostly internaland as such you should not ever need this. 
		/// It is made public for fringe cases where custom logic could benefit from this value
		/// </summary>
		public int RawTimer { get; private set; } = 0;

		/// <summary>
		/// The current value of the stat linked to this timer. I.e. Timer "Energy" is at 3 out of max 5
		/// </summary>
		public int Value { get; set; } = 0;

		/// <summary>
		/// The max value this timer's stat can have. I.e. Timer "Lives" is capped at 5
		/// </summary>
		public int ValueBounds { get; set; } = 0;// => points_max; 

		/// <summary>
		/// Based on whether you have set your timer to display as a CountDown or CountUp timer this will
		/// return either the amount of seconds this timer is into the delay or how many seconds is left on
		/// the delay before the server is contacted again to fetch the current value of this timer
		/// </summary>
		public int Timer => count_down ? RawTimer : TimerBounds - RawTimer;

		/// <summary>
		/// The delay between server contacts. 
		/// I.e. if the timer is set to update the stat every 600 seconds then this will return 600
		/// </summary>
        public int TimerBounds { get; private set; } = 0;

		/// <summary>
		/// The value before the server response set Value to it's current value. 
		/// Useful during the onContactedServer Action to determine if the server actually changed the value
		/// you had in the game or if there was some timing issue that caused the server to be contacted prematurely.
		/// If PreviousValue matches Value then you know something went wrong and can hande the in-game response accordingly
		/// Alternatively, if you put the game into the background and made it active after multiple points were added on
		/// the server side you can use this to determine how many points just got added to / subtracted to get to Value
		/// </summary>
		public int PreviousValue { get; private set; } = 0;

		/// <summary>
		/// Thenumber of seconds left before the server is contacted will be formatted into a string resembling a
		/// digital clock and stored in here. In order for you to display your timers on screen all you have to do
		/// is write a coroutine that displays this value every second. Job done.
		/// Examples: Text.text = $"{FormattedTimer}/{TimerBounds}" or Text.text = FormattedTimer;
		/// </summary>
        public string FormattedTimer { get; private set; }

        Action<CML> _onContactedServer;
		
		void Start () {
			_onContactedServer = __onContactedServer;
			
			//set the values to default values before the server is contacted to fetch the actual values
			Value = initial_starting_value;
			ValueBounds = initial_max_value;
			TimerBounds = initial_max_timer;
			
			///then get the actual values from the server immediately 
			FetchStat();
		}

		void OnDestroy()
		{
			CancelInvoke ();
			Value = ValueBounds = RawTimer = TimerBounds = 0;
			FormattedTimer = ConvertToTimer (0);
		}

		/// <summary>
		/// Tell the server to send you all of the timer's details including stat and timer bounds.
		/// If the server fails to find this timer it will create it using the values passed by this function
		/// Example use: When a prefab is first instantiated and a timer by it's name may or may not exist on the server, yet
		/// </summary>
		public void FetchStat()
		{
			CMLData data = new CMLData();
			data.Set ("fid", field_name);
			data.Seti("tx" , initial_max_timer);
			data.Seti("vx" , initial_max_value);
			data.Seti("v"  , initial_starting_value);
			
			CancelInvoke ();
			WPServer.ContactServer(WUTActions.FetchStat, timer_filepath, ASSET, data, _onContactedServer);
		}

		/// <summary>
		/// Tell the server to send you the timer's current details excluding stat and timer bounds.
		/// Use this when you are sure the timer exists and you just want it's values. 
		/// If you are not sure that the timer exists on the server, use FetchStat() instead
		/// Example use: Whenever the timer reaches 0 and you want to get the new current value from the server
		/// </summary>
		public void UpdateStat()
		{
			CMLData data = new CMLData();
			data.Set ("fid", field_name);
			
			//Set X to anything other than 0 if the timer already exists otherwise pass along everything from
			//FetchStat()..... in which case, just call FetchStat() instead of UpdateStat()
			data.Seti("x", 1); 
			
			CancelInvoke ();
			WPServer.ContactServer(WUTActions.GetStatStatus, timer_filepath, ASSET, data, _onContactedServer);
		}

		/// <summary>
		/// when force_to_0 is true, you can spend more points than you have and the resulting balance will be 0
		/// When it is false and you do not have enough points the call fails and nothing happens.
		/// It will be as if the function just wasn't even called....
		/// Examples: Do an attack that requires 5 energy but you only have 3
		///		With true: Server reports success and energy is now 0
		///		With false: Server reports failure and the timer continues uninterrupted. Energy is still 3
		/// </summary>
		/// <param name="amount">How many points to try and spend</param>
		/// <param name="force_to_0">Spend more than what we have and set value to 0... or reject the transaction if the points value is too low?</param>
		public void SpendPoints(int amount, bool force_to_0)
		{
			CMLData data = new CMLData();
			data.Set  ("fid"  , field_name);
			data.Seti ("amt"  , amount);
			data.Seti ("force", force_to_0 ? 1 : 0);
			
			CancelInvoke ();
			WPServer.ContactServer(WUTActions.SpendPoints, timer_filepath, ASSET, data, _onContactedServer);
		}
		/// <summary>
		/// Copy of SpendPoints(int, bool) created so it can be called from UGUI Button components
		/// </summary>
		/// <param name="amount">How many points to try and spend</param>
		public void SpendPoints(int amount) => SpendPoints(amount, false);

        public void GivePoints( int amount ) => _multiPurpose( WUTActions.GetPoints, amount );
        public void UpdateMaxPoints( int amount ) => _multiPurpose( WUTActions.UpdateMaxPoints, amount );
        public void SetMaxPoints( int amount ) => _multiPurpose( WUTActions.SetMaxPoints, amount );
        public void UpdateMaxTimer( int amount ) => _multiPurpose( WUTActions.UpdateMaxTimer, amount );
        public void SetMaxTimer( int amount ) => _multiPurpose( WUTActions.SetMaxTimer, amount );

		/// <summary>
		/// Completely remove this timer from the database
		/// </summary>
		/// <param name="onSuccess">Trigger custom code once the timer has been successfully removed from the database</param>
		/// <param name="onError">OPTIONAL: Trigger custom code if removing the timer failed to be removed from the database</param>
		public void DeleteTimer(Action<CML> onSuccess, Action<CMLData> onError = null) => _multiPurpose(WUTActions.DeleteTimer, 0, onSuccess, onError);

        #region Utility function
        /// <summary>
        /// Turn number of seconds into a formatted string breaking it up into a human readable timer value
        /// </summary>
        /// <param name="amount">How many seconds to format into the timer text</param>
        /// <returns>A formatted string showing days, hours, minutes and seconds. If days or hours are not applicable they are omitted from the timer string</returns>
        static public string ConvertToTimer(int amount)
		{
			int seconds = amount % 60;
			amount -= seconds;

			int minutes = amount / 60;
			int hours = 0;
			int days = 0;

			if (minutes > 59)
			{
				minutes %= 60;
				hours = ((amount / 60) - minutes) / 24;
			}

			if (hours > 23)
			{
				int h = hours % 24;
				days = (hours - h) / 24;
				hours = h;
			}

			if (days > 0)
				return $"{days:D2}:{hours:D2}:{minutes:D2}:{seconds:D2}";
			if (hours > 0)
				return $"{hours:D2}:{minutes:D2}:{seconds:D2}";

			return $"{minutes:D2}:{seconds:D2}";
		}
        #endregion

        #region private functions
        void _multiPurpose(WUTActions action, int amount, Action<CML> onSuccess = null, Action<CMLData> onError = null)
		{
			CMLData data = new CMLData();
			data.Set("fid", field_name);
			data.Seti("amt", amount);

			CancelInvoke();
			WPServer.ContactServer(action, timer_filepath, ASSET, data, onSuccess ?? _onContactedServer, onError);
		}

		void __onContactedServer(CML response)
		{
			CMLData _timer = response.GetFirstNodeOfType("timer");
			if (null != _timer)
			{
				int 
					max			= _timer.Int ("px"),
					max_timer	= _timer.Int ("tx"),
					pts			= _timer.Int ("p"),
					ttu			= _timer.Int ("t");
				
				
				if (max > 0) ValueBounds = max;
				if (max_timer > 0 ) TimerBounds = max_timer;
				
				if (Value != pts)
					PreviousValue = Value;
				
				Value = pts;
				this.RawTimer = ttu;
				
				if (Value > ValueBounds)
				{
					Value = ValueBounds;
					this.RawTimer = -1;
				}
				
				//format output text...
				FormatOutPutText();
				
				//allow external kits to plug into this action...
				onContactedServer?.Invoke(_timer);
				
				if (Value != PreviousValue)
					onTimerEvent?.Invoke(Value);

				if (this.RawTimer > 0)
                    Invoke(nameof(UpdateTimer), 1);
			}
		}

		void FormatOutPutText()
		{
			//the server returns how many seconds are left and we keep taking seconds off that value
			//so let's see how many more seconds are left on the timer
			int difference = TimerBounds - RawTimer;
			if (difference > TimerBounds)
				difference = TimerBounds;
			
			//difference will go from 0 to timer_max but to count down we want it to go the other way
			//so subtract the difference from timer_max again
			if (count_down)
				difference = TimerBounds - difference;

			//now set the string value
			FormattedTimer = ConvertToTimer(difference);
		}
		
		void UpdateTimer () {
			if (RawTimer >= 0)
				RawTimer--;
			
			FormatOutPutText();
			
			switch(RawTimer)
			{
			case 0:
				UpdateStat();
				break;
			case -1:
				Invoke(nameof(UpdateStat), TimerBounds);
				break;
			default:
				Invoke(nameof(UpdateTimer), 1);
				break;
			}
		}
        #endregion
    }
}