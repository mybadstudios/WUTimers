### WUTimers
If you want to add time delayed actions (like growing crops, gaining extra lives / health / gold) and want to be sure that players can't cheat by just changing the system clock then this is what you need. 

It takes seconds to setup in Unity, no configuration required on the server and manages all time based events on the server. Now changing the time on their device has absolutely no impact at all.

Requires [WordPress For Unity Bridge](https://mybadstudios.com/product/wordpress-bridge/).

If You enjoy my work, please consider buying me a coffee...

[<img src="bmcbutton.png">](https://www.buymeacoffee.com/mybad)


# High level overview:

- Server hosted timers mean non-cheatable timers
- Add any number of stats (HP, Energy, Gold, etc)
- Each stat has an independent timer
- Each timer has its own duration
- Can count down or count up
- No configuration required on the server
- Takes seconds to implement
- Works on all platforms
- Fully GUI agnostic
- Features callbacks to attach custom behavior
