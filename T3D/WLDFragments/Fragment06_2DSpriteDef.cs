// TODO

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using static Fragment22_Region;

public class Fragment06_2DSpriteDef : Fragment
{
	public uint NameRef;
	public uint Flag;
	public RenderInfo RenderInfo;
	public List<Vertex> vertices = new List<Vertex>();
	public List<int> VertexList = new List<int>();
	public List<Normal> Normals = new List<Normal>();
	public List<Fragment03_Frame> Frames = new List<Fragment03_Frame>();

	internal Fragment06_2DSpriteDef(BinaryReader reader)
	{
		NameRef = reader.ReadUInt32() + 1;

		// Flag
		//	0x80
		Flag = reader.ReadUInt32();

		var numFrames = reader.ReadUInt32();
		var numPitches = reader.ReadUInt32();
		var spriteSizeX = reader.ReadSingle();
		var spriteSizeY = reader.ReadSingle();
		var unk0 = reader.ReadUInt32(); // Maybe fragment ref for collision info

		Name = WLD.GetNameFromRef((int)NameRef);

		Console.WriteLine($"\n2DSPRITEDEF {Name} Flag={Flag} numFrames={numFrames} numPitches={numPitches} spriteSizeX={spriteSizeX} spriteSizeY={spriteSizeY} unk0={unk0}");

		if (unk0 != 0)
		{
			WLD.FragmentContextQueue.Dequeue();
		}

		// Flag
		// 0x80 - DEPTHSCALE
		// 0x1 - CENTEROFFSET
		// 0x2 - BOUNDINGRADIUS
		// 0x4 - CURRENTFRAME
		// 0x8 - SLEEP
		// 0x40 - SKIPFRAMESON
		if ((Flag & 0x80) != 0)
		{
			var depthScale = reader.ReadSingle();
			//if (v23 != 1.0)
			//	sub_40B960(a1, "DEPTHSCALE %f\n", v23);
			if (depthScale != 1.0f)
			{
				Console.WriteLine($"DEPTHSCALE {depthScale}");
			}
		}
		if ((Flag & 0x1) != 0)
		{
			var centerX = reader.ReadSingle();
			var centerY = reader.ReadSingle();
			var centerZ = reader.ReadSingle();
			Console.WriteLine($"CENTEROFFSET {centerX} {centerY} {centerZ}");
		}
		if ((Flag & 0x2) != 0)
		{
			var boundingRadius = reader.ReadSingle();
			Console.WriteLine($"BOUNDINGRADIUS {boundingRadius}");
		}
		if ((Flag & 0x4) != 0)
		{
			var currentFrame = reader.ReadUInt32();
			Console.WriteLine($"CURRENTFRAME {currentFrame}");
		}
		if ((Flag & 0x8) != 0)
		{
			var sleep = reader.ReadUInt32();
			Console.WriteLine($"SLEEP {sleep}");
		}
		if ((Flag & 0x40) != 0)
		{
			Console.WriteLine("SKIPFRAMESON");
		}

		Console.WriteLine($"NUMPITCHES {numPitches}");
		for (int i = 0; i < numPitches; i++)
		{
			Console.WriteLine("PITCH");
			var pitchCap = reader.ReadUInt32();
			Console.WriteLine($"PITCHCAP {pitchCap}");

			//if (v14 < 0)
			//{
				//v14 &= ~0x80000000;
				//sub_40B960(a1, aToporbottomvie);
			//}
			var numHeadings = reader.ReadUInt32();
			if ((int)numHeadings < 0)
			{
				numHeadings &= ~0x80000000;
				Console.WriteLine("TOPORBOTTOMVIEW");
			}

			Console.WriteLine($"NUMHEADINGS {numHeadings}");

			// if ( v14 ) do ... while
			for (int j = 0; j < numHeadings; ++j)
			{
				Console.WriteLine("HEADING");
				var headingCap = reader.ReadUInt32();
				Console.WriteLine($"HEADINGCAP {headingCap}");
				for (int h = 0; h < numFrames; ++h)
				{
					var frameRef = reader.ReadUInt32();
					var frameFragment = (Fragment03_Frame)WLD.Fragments[(int)frameRef];

					Frames.Add(frameFragment);
					Console.WriteLine($"FRAME {frameFragment.Name}");
				}
				Console.WriteLine("ENDHEADING");
			}
		}
		Console.WriteLine("ENDPITCH");

		RenderInfo = RenderInfo.ReadRenderMethod(reader);
	}
}