# Do Not Disturb Fix
If you have Windows set to automatically turn on Do Not Disturb mode when you're running a fullscreen program, you may have noticed Do Not Disturb activating when you're staring at an empty desktop, and turning back off as soon as you open a window. It seems the most common cause (and the reason I wrote this program) is the NVIDIA Overlay; it seems that Windows will identify it as a fullscreen program, so unless there are other non-fullscreen programs running "in front of" it, Windows will turn on Do Not Disturb. The only solutions I've been able to find are to disable either automatic Do Not Disturb or the NVIDIA Overlay; heaven forbid someone should want to use both. This problem stretches back years and it seems neither Microsoft nor NVIDIA are in any rush to fix it, so I decided to take matters into my own hands.

This should work for any program that Windows is improperly detecting as a fullscreen program on an empty desktop.

This program works by detecting when Do Not Disturb is activated due to a fullscreen program, then checks all currently open windows to see if you have anything open that would possibly be legitimately causing Do Not Disturb to activate. If not, it'll shut Do Not Disturb off. You may see Do Not Disturb turn on for a second before Windows catches up.

## Setup
1. Download and extract the zip file to the directory of your choosing.
2. You can run DoNotDisturbFix.exe now if you choose. It will silently run in the background and constantly check your Do Not Disturb status.
3. You'll probably want the program to run every time you start your computer. The easiest way to set this up is to right-click DoNotDisturbFix.exe, click Show more options (if needed), then click Create shortcut. Cut the shortcut, click on the folder bar in File Explorer, and type in `shell:startup`, then paste the shortcut in that folder.
4. By default, the program will delay 1 millsecond after every time it checks your Do Not Disturb status. You can modify this by right-clicking the shortcut, clicking Properties, and then in the window that pops up, under Target, at the very end of that text box (after the quotation marks, if they're there), add a space, then your preferred number of milliseconds. You can also type 0 for no delay whatsoever, although this may cause the program to use a lot more CPU for a completely unnoticeable benefit.

## Disclaimer
This program relies heavily on undocumented Windows APIs that could potentially break with any Windows update.

## Credits
I created this program by starting with [SharpWnfSuite by daem0nc0re](https://github.com/daem0nc0re/SharpWnfSuite) and hacking out absolutely everything I didn't need. SharpWnfSuite, in turn, is a C# port of [WNFUN by Alex Ionescu and Gabrielle Viala](https://github.com/ionescu007/wnfun).
