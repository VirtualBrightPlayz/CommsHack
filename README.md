# CommsHack ASS

CommsHack Audio Sub-System

## !!!WARNING!!!

There isn't any maximum to the volume of the audio played. Loud audio can/will/does break the SCP:SL VSR (https://scpslgame.com/Verified_server_rules.pdf). Please be careful.

There is also no security measures as to what can be played. Don't give Remote Admin to randoms.

## Commands

`audio <volume between 0-1> <file path>`

## FFmpeg

There isn't a builtin converter for the plugin. Audio comes in different formats and as such can be a pain to decode. So we added ffmpeg support. Downloads for the binaries can be found at: https://ffmpeg.org/download.html

## Permissions

### `ass.playra`

Let people with RA play sounds.

## Configs

### `f_f_m_p_e_g`

Default Value: `ffmpeg`

Type: `string`

The path to the ffmpeg executable. On Linux, make sure the execute permission is allowed.

### `comms_file`

Default Value: `N/A`

Type: `string`

This is unused.


## Thanks RogerFK for the name `ASS`
