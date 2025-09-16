# TanarusExtractor

## Info
This repository adds basic WLD support to the T3D extractor. At the moment, it will convert the WLD inside the T3D to a .obj/.mtl. It still needs a ton of polish, so feel free to make contributions.

## New features
Basic WLD support

Export WLD to OBJ

## Running
If running as an executable: ```./T3D.exe <archive.t3d> <output folder> [-obj]```

If running as a DLL: ```dotnet T3D.dll <archive.t3d> <output folder> [-obj]```

The command line option ```-obj``` will convert the WLD to OBJ as a basic untextured map. Leaving this out it will function as the original T3D and only extract the contents of the T3D archive.

## References
https://github.com/MrPnut/T3D

https://wld-doc.github.io/

https://docs.eqemu.io/client/guides/file-formats/wld-by-windcatcher/ (EverQuest specific but still helpful)

https://github.com/LanternEQ/LanternExtractor (EverQuest specific)

WLDCOM.EXE from the Tanarus MapEditor (Using IDA Free + Decompiler)

# T3D (Original README)
This is a program to extract the contents of .t3d files belonging to the defunct 1997 Tank game called [Tanarus](https://en.wikipedia.org/wiki/Tanarus_(video_game))

## Why bother?
Tanarus was a fun game and this file format is dead simple.  Having the original assets might spark interest in recreating the game for another engine?  I don't know..  This could be part of a larger effort of reverse engineering the other file types that the engine uses "for fun".

## This program only extracts .t3d files
I don't think that's a problem since the original engine is defunct anyway.  Noone is creating any new t3d archives.  If you do need to, there's always the Tanarus map editor which has ffcreate (which creates the t3d files)
