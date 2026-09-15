# Name Pending
or, at least, not set in stone. I called the namespace TapSuite, but that's more suitable as an overarching name for the set of projects.

## What is it, and how do I use it?
This here is a Plugin for Warudo. It allows you to send the position of a camera to [OnAirTap](https://github.com/milkydelta/OnAirTap) or [VapourTap](https://github.com/milkydelta/VapourTap), so that your character can appear to be "in" the VR world through a spectator camera.

To use it, drop all 4 .cs files from this repository into `Warudo_Data/StreamingAssets/Playground/`.

Because this project uses `System.IO.MemoryMappedFiles` (and `System.IO.File` on Linux), it cannot be turned into a Plugin Mod, due to the security restictions imposed upon them. This is unfortunate, but entirely reasonable.

You can then add the "VR Sync" asset to your scene.

 - Set VR Camera to the camera you would like to send the details of.
 - Clip Target Location can optionally be used to change the location of the foreground clipping plane.
    - If this is unset, the HMD position is used.
 - Playspace Root is also optional. If the origin of your VR playspace does not align with the world origin at 0,0,0, provide here a GameObjectAsset that represents your playspace origin.
    - The rotation of that GameObjectAsset is also considered.

Once you've configure that correctly, the Enabled toggle will start sending information to OnAirTap. You can then read the OnAirTap wiki for information on the rest of the setup.

