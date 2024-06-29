ABOUT THIS DEMO
===============

This demo demonstrates how to create 3 different timers, each with a different duration between updates
One shows acountdown timer, theother counts up towards it's final value.
The demo also contains buttons to deplete and restore values to the timer's underlaying values

This demo demonstrates that you do not need to do anything at all on the server side to create your timers.
Everythign is done in the inspector.

THE THEORY BEHIND IT ALL
========================

The logic is really very straightforward. 

You have a stat and that stat gets updated once every X number of seconds. You can specify the max value of the stat
and the interval between stat updates. Once the timer is created onthe server it will maintain itself.
You can create as many timers as you want to per player per game.

In order to make them work, though, it is very important that each timer has a unique name as they are identified
by their names. In this demo we create three timers called Lives, Stamina and Energy. If you are creating a game
that uses a grid a good convention would be to name your timers by their grid coordinate like, for example, X10Y15.

To create a timer simply add a WUTimer component to an object and specify the following values:
1. field_name - The name as explained above
2. initial_starting_value - Do you want your stat to start at 0? at max value? Somewhere in between? 
3. initial_max_value - What is the max value this stat can have? I.e. 5 lives, 20 energy, 50 Stamina etc
4. initial_max_timer - Specify the update interval in seconds

If you create a timer with a name that does not exist on your server then it will be created using the values
you specified above. If a timer by that name is found then it will ignore the values you just passed and use the
values from the server instead. 

You may have noticed that some fields start with "initial" values. That is the value you assign the field when creating
the timer but you are provided with functions to alter this over time. For instance, you could tell your player that
for only $5 they can upgrade their max Stamina by 5 points or for only 20,000 Gold they can upgrade their Energy, etc.

As stated before, the timers maintain themselves and operate passively. This means that if you have a million players
with a hundred timers each you won't have to worry about how much data this game is gonna use over the course of an hour
or a day etc since it doesn't check the data all the time. It only checks the database when you tell it to and it only
updates the database when it has to. Data use is kept to an absolute minimum.

To that end you will see in the demo that I fetch the three timers from the server and ask them each how many seconds
they have left before they are due an update. I then run a local clock to say "Contact the server after that much time".
Locally I just update the timer and display the value on screen so the player can know what is going on but if a timer
has a 5 minute update interval, or an hour or 3 day update interval then that timer is only polled every 5 minutes,
every hour or every 3 days, respectively. 

You could contact the server every second if you wanted to but there would be no point as the value will not have changed.
If you stop playing and then return a few hours or weeks later it will have kept track of how many points you are due and
update the stat(s) as required when you do the query. 

As I said, it maintains itself so all you have to do is create a coroutine that places the formatted timer value on screen
every second for players to see and then, when the timer reaches 0, you contact the server and ask it what the current
value is. Whenever you get a response from the server you will be notified that a timer has been updated 
(if it has been updated) and you can respond to the change in the timer however you need to. 

Any attempts to hack the local game client will be pointless since the next time the server is polled it will return the
actual stat value. If they hack the timer they will contact the server more often and have their stat be set to the same
value more often. If they hack the stat itself then it will just get reset to the real value the next time you poll
the server. 

TLDR SUMMARY
============
Add a WUTimer component to an object and give it a unique name. if the timer doesn't exist it will be created with the
initial values you provide. If the timer does exist then WUTimer will tell you how long you need to wait locally
before you contact the server again for an update. It will give you a string formatted to look like a digital time
that you can display for your players as is. All you need to do now is write a function to run whenever the timer
triggersthe event to tell you the value has changed on the server. 

That is all there is to it...