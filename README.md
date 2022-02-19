# Stream Views Plugin

This unofficial TaleSpire allows the GM to send cut-scene views to individual players. Includes support for addressing
multiple client from a single account which is ideal for streaming GMs who want additional views of the game action for
their stream. Video Demo: https://youtu.be/GHj0RL1QdBc

This plugin, like all others, is free but if you want to donate, use: http://198.91.243.185/TalespireDonate/Donate.php

## Change Log

```
1.1.0: Reworked plugin to work with BR updates
1.0.2: Added missing CustomData folder with border and background.
1.0.1: Fixed manifest with correct description. No plugin change.
1.0.0: Initial release
```

## Install

Use R2ModMan or similar installer to install this plugin.

Use R2ModMan to configure optional settings for the plugin.

## Usage

### Basic Usage (For Sending Individual Cut-Scene Views To Players)

As GM, add cut-scene views to the selection of view using the regular core Cutscene functionality.

To send a cut-scene view to a player, select the eye icon beside the disired cutscene (to select the cutscene without
sending it). Then press the Send keyboard combination (default RCTRL+S for send). This opens the player menu. Click on
the desired player and the corresponding cut-scene will be sent to that player.

When a cutscene is active pressing any key will end the cutscene and return the camera to the position and orientation
it was in before the cutscene was activated.

### Streaming Usage (For GMs That Want Additional Views Of The Action For Their Streams)

As GM, make multiple connections to Talespire using your talespire account. You can run multiple modded instances of
Talespire by running the following command from the game folder:

``TaleSpire.exe --doorstop-enable true --doorstop-target "C:\Users\%USERNAME%\AppData\Roaming\r2modmanPlus-local\TaleSpire\profiles\default\BepInEx\core\BepInEx.Preloader.dll"``

Update the above if you are using different R2ModMan profile than "default".

Once you have established multiple connection, set the identity for each connection that will be used for a stream
view (i.e. all connections except the one connection used by the GM). To do this press the keyboard shortcut associated
with each camera person (default RCTRL+1 or RCTRL+2). Each connection being used for a stream view should be assigned
a unique camera identity.

Send cutscenes to the stream views cameras just like you would send cutscene views to any player except select the
desired camera from the player menu as opposed to choosing a player name. Since stream view sessions are typically
unattended, the view will remain in the cutscene view until replaced by a different cutscene view.

### Custom Settings

The plugin comes with a bunch of custom settings that can be set using the R2ModMan configuration for the plugin.

The keyboard shortcuts for Send Cutscene To Player, Claim Camera 1 Identity and Claim Camera 2 Identity can be
changed using the configruation.

The menu font color, size and the spacing between entries can be configured.

The Close Player Menu After One Selection allows configruation if the player menu should close as soon as the cutscene
has been sent to one player or if it should remain open (until the close menu option is used) so that the same cutscene
can be sent to multiple players.

The background image can be customized be editing Images/org.lordashes.plugins.streamviews.menu.png. This image is scaled
based on the number of entries in the list and the width of the menu buttons (see below). Borders of about 30 pixels are
added in each direction.

The button image can be customized be editing Images/org.lordashes.plugins.streamviews.button.png. This image is not scaled
and is used to determine the size of the background image.
