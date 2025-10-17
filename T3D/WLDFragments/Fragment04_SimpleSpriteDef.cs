using System.IO;
using System;
using System.Collections.Generic;
using T3D;

public class Fragment04_SimpleSpriteDef : Fragment
{
	public List<Fragment03_Frame> Frames = new List<Fragment03_Frame>();

	internal Fragment04_SimpleSpriteDef(BinaryReader reader)
	{
		var nameRef = reader.ReadUInt32() + 1;
		var flag = reader.ReadUInt32();
		var numFrames = reader.ReadUInt32();

		// Flags:
		//	0x04 = CURRENTFRAME
		//	0x08 = SLEEP
		//	0x40 = SKIPFRAMES ON
		if ((flag & 0x04) != 0) // CURRENTFRAME
		{
			// CURRENTFRAME %d
			var currentFrame = reader.ReadUInt32();
		}

		if ((flag & 0x08) != 0) // SLEEP
		{
			// SLEEP %d
			var sleep = reader.ReadUInt32();
		}

		for (int i = 0; i < numFrames; ++i)
		{
			var frameRef = reader.ReadUInt32();
			var frameFragment = (Fragment03_Frame)WLD.Fragments[(int)frameRef];

			Frames.Add(frameFragment);
		}

		Name = WLD.GetNameFromRef((int)nameRef);
	}
}