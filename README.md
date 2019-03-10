# Dolphin-TAStudio
A small GUI used to display input data from a lua file and allow a user to modify input data, add new frames of input data, and save to a file.
___
## About
This GUI aims to mimic that of TAStudio from the multi-system emulator BizHawk, which can be found [here.](https://github.com/TASVideos/BizHawk) One issue that arises, however, is that due to the current state of how Dolphin handles input data polling, input data is duplicated a variable number of times in the native Dolphin recording file. Rather than writing input data once for every frame that the game polls for input, Dolphin writes input data to a Dolphin TAS Movie (.dtm) file for every time the emulator itself needs an input. Due to this variability and unpredictability, it is unfeasible at this time to create a Dolphin .dtm editor.

However, I created a small lua script to circumvent this. Using functions provided with [SwareJonge's custom Lua version of Dolphin](https://github.com/SwareJonge/Dolphin-Lua-Core) I created an input writer script that interprets input data from a Gamecube controller and formats it into an input file. This input file contains rows, each representing a frame of input, with attributes in every row that specify the analog stick horizontal and vertical input as well as boolean values for A, B, L, and the D-Pad buttons.

Regarding the analog stick horizontal and vertical inputs, a value of 0 represents the leftmost horizontal value or the downmost vertical value, whereas a value of 14 represents the rightmost horizontal value or the upmost vertical value. The reasoning for this funky way of handling analog inputs is due to the nature of Mario Kart Wii's code, wherein analog values normally between 0-255 are translated into values between 0-14. These 15 different values are called the different turning radii.

Dolphin-TAStudio allows the user to open said input file and display the input data in a neatly formatted table. The user can modify cell values (except frame count directly) and changes will be reflected when the user saves the file. In addition, the user can add new frames of input by typing in any cell in the empty bottommost row. Default values will be set to save a bit of time when adding several frames at a time. Default values are set as follows:

```Horizontal Input: 7 (Neutral)
Vertical Input: 7 (Neutral)
A: 1 (Pressed)
B: 0 (Not Pressed)
L: 0
DU: 0
DD: 0
DL: 0
DR: 0
```
![Screenshot](https://i.gyazo.com/bb3ec9f6c43c301e242a506fc17bf6e5.png)

## Functionality
First download [SwareJonge's custom Lua version of Dolphin](https://github.com/SwareJonge/Dolphin-Lua-Core). **MAKE SURE** that you go to Config and Disable Dual Core Speedup as well as Idle Core Skipping. Next navigate to DSP and change the audio engine to DSP LLE Recompiler. Failure to correctly change these settings will result in desynchronization of your inputs on writing to the file as well as playback in-game. Next, place `MKW_Core.lua` and `mkw_input_file_output.lua` in the root Dolphin folder. Place `input_writer_with_savestate_support.lua` and `input_reader_with_savestate_support.lua` in the scripts folder. When you'd like to begin recording your inputs to a file, in Dolphin go to Tools>Execute Script, and in the drop down menu select `input_writer_with_savestate_support.lua`. Press Start Script, and your inputs will begin to write to `mkw_input_file_output.lua`. Note that, as the name implies, you can load savestates and any inputs that you rewrite will be overwritten in the input file accordingly. When you would like to stop writing to file, press Cancel Script.

Run `Dolphin TAStudio.exe` and use File>Open to open `mkw_input_file_output.lua`. Make changes to your input file as desired. When finished, use File>Save to save changes. Then to play back your newly modified input file, go back in Dolphin and in the drop down menu select `input_reader_with_savestate_support.lua`. Press Start Script and the inputs stored in the newly modified input file will be played back.

## Features
### Editing Frames
You can change the analog stick inputs by either double clicking on the cell or simply by typing while selecting a cell. For button presses, simply check the box to represent the button being pressed.

### Copy-Pasting Framedata
Select a cell or a group of cells and go to Edit>Copy or just press Ctrl + C. Then simply paste overtop of another cell.
WARNING: If you paste a group of cells several columns wide, do not try to paste it if there isn't enough room to the right. The program will crash.

### Inserting New Frames w/ Default Values
Simply type a value into the last blank row and a new row will be generated with default values. Horizontal and vertical analog stick values are neutral values, A is defaulted to on, and all other buttons are defaulted to off.

## Recent Updates
#v1.3
Added undo+redo feature

Represent button inputs as checkbox booleans rather than integers.
