# Stream Views Plugin

This unofficial TaleSpire allows the GM to send cut-scene views to individual players. Includes support for addressing
multiple client from a single account which is ideal for streaming GMs who want additional views of the game action for
their stream. Video Demo: https://youtu.be/GHj0RL1QdBc

This plugin, like all others, is free but if you want to donate, use: http://198.91.243.185/TalespireDonate/Donate.php

## Change Log

```
1.0.1: Fixed manifest with correct description. No plugin change.
1.0.0: Initial release
```

## Install

Use R2ModMan or similar installer to install this plugin.

Use R2ModMan to configure optional settings for the plugin.

## Usage

### Basic Usage (For Sending Individual Cut-Scene Views To Players)

As GM, add cut-scene views to the selection of view by using the keyboard shortcut (default LCTRL+P).

To send a cut-scene view to a player, open the stream view menu using the keyboard shortcut (default LSHIFT+P).

This displays all of the cut-scene views available and highights the selected one with a fire border.

Select the desired cut-scene and use the buttons on the right to send it to one of the listed players. To send it to
multipole players just click the button beside each player. Use the "Delete View" button to remove the cut-scene from
the cut-scene list (making room for new cut-scenes).

When a player send a cut-scene mode, their camera will show the cut-scene. Pressing the mouse button or any key exits
cut-scene mode and returns the view backt to the view before the cut-scene was activated.

### Streaming Usage (For GMs That Want Additional Views Of The Action For Their Streams)

As GM, make multiple connections to Talespire using your talespire account. You can run multiple modded instances of
Talespire by running the following command from the game folder:

``TaleSpire.exe --doorstop-enable true --doorstop-target "C:\Users\%USERNAME%\AppData\Roaming\r2modmanPlus-local\TaleSpire\profiles\default\BepInEx\core\BepInEx.Preloader.dll"``

Update the above if you are using different R2ModMan profile than "default".

Once you have established multiple connection, set the identity for each connection that will be used for a stream
view (i.e. all connections except the one connection used by the GM). To do this press the keyboard shortcut associated
with each camera person (default RCTRL+1 to RCTRL+3). Each connection being used for a stream view should be assigned
a unique camera identity.

To create cut-scene views, see the instructions under Basic Usage.
To send a cut-scene view to the camera person, see the instructions under Basic Usage but instead of selecting a player
name, select one of the three camera person buttons (to send it to the corresponding camera person).

In your streaming software, you can now use the instances of running Talespire associated with the camera persons as
views into the action without needing any player or the GM to be looking that way. 

## Limitation

Currently cut-scene views change the position and rotation of the camera but do not adjust the focus point. Hopefully
this will be addressed in a future update but currently that means that in many cases the cut-scene view can be out of
focus. To compensate for this, the plugin allows you to turn the post processing off (which is the default). This fixes
the focus issue but does not have all of the visual effect of the scene with post processing. Once the cut-scene view
ends, the post processing setting is restored. 
