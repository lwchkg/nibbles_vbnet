# Nibbles for VB.NET

This is a remake of [QBasic Nibbles](https://en.wikipedia.org/wiki/Nibbles_(video_game)) in VB.NET.
QBasic Nibbles was published with MS-DOS version 5.0 or above, and served as an example of what QBasic can do.

Nibbles for VB.NET is intended to be an example program for beginners in their first year of programming.

![Title Screen](screenshot/title_screen.png?raw=true)
![Gameplay](screenshot/gameplay.png?raw=true)

## Playing the game

If you just want to try the game, go to [the latest release](https://github.com/lwchkg/nibbles_vbnet/releases/latest), download the binary and just run it.
Then follow the in-game instructions.

But as an example program, it is much more useful to download the source instead by the following instructions:

1. Install Visual Studio if you did not already install it.
   The Community Version of Visual Studio is free to use for open-source projects including this one.
   _Warning: installation takes several hours._

1. Click the green “Clone or download” button, then press “Download ZIP”.

1. Right-click on the downloaded file, click “Extract All...”, then click “Extract”.

1. Open `nibbles\_vbnet.sln`.
   After that you are free to tinker with the source.

_Note: exact wordings are different if your Windows installation is not in American English. The same applies for all instructions below._

## Known problems with display

**The squares appears to be malformed.**

This is a problems using a non-English font, such as MS Gothic or MingLiU.
To solve the problem, right-click the title bar, click “Properties”, go to the “Font” tab, and then select the font “Consolas”.

If you are still unsure what to do, see [this tutorial by iSunShare Studio](http://www.isunshare.com/windows-10/change-font-and-font-size-in-windows-10-command-prompt.html).

**There are some one-pixel wide lines in the display.**

This happens with Windows console if a TrueType font is used. To solve this, either:

* Change to “Raster Fonts” in the font settings.
  However, even the largest raster font available is likely to be too small for you.
  Also, the arrows in the game introduction will not show properly.

* Play the game using [ConEmu](https://conemu.github.io/), that disables ClearType on the half-block characters.
  You should set the console to something larger than 80×25 if you choose this solution.

## Programming styles

Considering this is an example for a first year coder, only a limited set of features are used.
Here is a list of trade-offs in the program:

* Classes are not used, except for existing classes in the .NET platform.

* Lambda functions are not used.

* Most of the code are put into a single file.
  _(Note: this turns out to be **really bad** for readability, because tens of unrelated procedures are put in the same file.)_

* The data structure `Queue (Of Point)` is used.
  It is easier to learn `Queue` and `Point` than to read the ring buffer implementation in the original QBasic implementation.

* The usual 80-column limit is ignored in a few statements because it is infeasible to observe the 80-column limit with the long strings.

* The separation of user interface and the business logic is not complete, and some strings are hard-coded.
  Complete separation is infeasible for a programmer in his or her first year.

And here are some non-tradeoffs.

* Using `AndAlso` and `OrElse` instead of `And` and `Or`.
  The use of short-circuiting operatiors (`AndAlso` and `OrElse`) is a must to learn, because logical operators of most computer languages are short-circuiting.

* Not observing the single-exit rule in structured programming practice.
  This rule helped in the old days because it was easy to forget deallocating resources (e.g. memory, file handle).
  Things work differently in VB.NET with destructors and exception handling, and single-exit does not help anymore.
  Instead, the single-exit rule often make control flows difficult to read.

## Level set, sound, and copyright issues

As a VB.NET reincarnation of Nibbles, the original level set and sound are used in this program, sadly without authorization from Microsoft.

If the game gains momentum, then I will be discussing with Microsoft about the copyright issues.

Anyway, the sound is created with [LMMS](https://lmms.io/).
To build the sound:

1. Install [Chocolatey](https://chocolatey.org/install).

1. Install LMMS and ffmpeg via Chocolatey: `choco install lmms ffmpeg -y`

1. Execute `audio\makeaudio.bat`.

## Difference from original QBasic game

* The code is completely rewritten.

* Levels are made more symmetric.
  Both players have equivalent start positions at all times.
  In particular, if not moving, both players will die at the same time.

* If player’s head collide, both die and they do not score even if they hit the number.
  In the QBasic game, Sammy (player 1) gets the score. The players still die unless they are hitting a “9”.

* If both players are hitting the number at the same time, a random player gets the score if their heads don’t collide.
  In the QBasic game, Sammy (player 1) always get the score.

* It is possible to have one player levelling up and the other dying at the same time.
  In the QBasic game, levlling up makes the other snake not to die.

* The bug of the rotating stars in the introduction screen is fixed.
