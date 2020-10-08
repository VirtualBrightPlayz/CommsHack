# CommsHack ASS

CommsHack Audio Sub-System

## !!!WARNING!!!

There isn't any maximum to the volume of the audio played. Loud audio can/will/does break the SCP:SL VSR (https://scpslgame.com/Verified_server_rules.pdf). Please be careful.

There is also no security measures as to what can be played. Don't give Remote Admin to randoms.

## Commands

`audio <volume between 0-1> <file path>`

Please note that spaces and other special chars. don't work in file names.

## FFmpeg

There isn't a builtin converter for the plugin. Audio comes in different formats and as such can be a pain to decode. So we added ffmpeg support. Downloads for the binaries can be found at: https://ffmpeg.org/download.html

If you would like to use this without ffmpeg, read up on how to convert the files beforehand https://dissonance.readthedocs.io/en/latest/Tutorials/Custom-Microphone-Capture/index.html (Step 4)

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
