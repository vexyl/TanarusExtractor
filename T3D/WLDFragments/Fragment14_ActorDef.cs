using System.IO;
using System;
using System.Collections.Generic;
using T3D;
using System.Linq;
using static Fragment22_Region;

public class Fragment14_ActorDef : Fragment
{
	public List<Fragment03_Frame> Frames = new List<Fragment03_Frame>();
	public List<int> Sprite3DRefs = new List<int>();

	internal Fragment14_ActorDef(BinaryReader reader)
	{
		var nameRef = reader.ReadUInt32() + 1;
		// Flags
		// 0x1	- CURRENTACTION %d
		// 0x2	- LOCATION %d %f %f %f %d %d %d
		// 0x40 - ACTIVEGEOMETRY
		// 0x80	- SPRITEVOLUMEONLY
		var Flag = reader.ReadUInt32();
		var callback = reader.ReadUInt32();
		var numActions = reader.ReadUInt32();
		var unk0 = reader.ReadUInt32();
		var unk1 = reader.ReadUInt32(); // Maybe fragment ref for collision info

		Name = WLD.GetNameFromRef((int)nameRef);
		//Console.WriteLine($"ACTORDEF: ACTORTAG Name={Name}, numActions={numActions}");

		if (unk1 != 0)
		{
			WLD.FragmentContextQueue.Dequeue();
		}

		if ((Flag & 0x1) != 0)
		{
			var v32 = reader.ReadSingle();
			// TODO: *(_DWORD *)v32 = LODWORD(v8) + 1 == CURRENTACTION
		}
		if ((Flag & 0x2) != 0)
		{
			var v25 = reader.ReadSingle();
			var v26 = reader.ReadSingle();
			var v27 = reader.ReadSingle();
			var v28 = reader.ReadUInt32();
			var v29 = reader.ReadUInt32();
			var v30 = reader.ReadUInt32();
			var v31 = reader.ReadUInt32();
			// "LOCATION {v31} {v25} {v26} {v27} {%d:v28} {%d:v29} {%d:v30}\n",
		}

		for (int i = 0; i < numActions; ++i)
		{
			var numLevelsOfDetail = reader.ReadUInt32();
			//Console.WriteLine($"NUMLEVELSOFDETAIL {(int)numLevelsOfDetail}");
			for (int j = 0; j < numLevelsOfDetail; ++j)
			{
				var minDistance = reader.ReadSingle();
				var maxDistance = reader.ReadSingle();
				// TODO
			}
		}
		for (int i = 0; i < numActions; ++i)
		{
			var spriteInst = reader.ReadUInt32();
			var fragmentRef = WLD.FragmentContextQueue.Dequeue();
			Sprite3DRefs.Add(fragmentRef);
			// TODO
		}

		if (callback != 0)
		{
			var callbackName = WLD.GetNameFromRef((int)nameRef);
		}

		//throw new Exception("ACTORDEF Test");
	}
}