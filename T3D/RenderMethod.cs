using static Fragment22_Region;
using System.Reflection.PortableExecutable;
using System.IO;
using System.Collections.Generic;
using System;

public class RenderInfo
{
	//public List<Fragment04_SimpleSpriteDef> SimpleSpriteDefs = new List<Fragment04_SimpleSpriteDef>();
	public List<UV> UVs = new List<UV>();
	public uint RenderMethod;
	public uint RenderMethodFlag;
	public int SimpleSpriteDef = -1;
	uint Pen;
	float Brightness;
	float ScaledAmbient;

	public static RenderInfo ReadRenderMethod(BinaryReader reader)
	{
		var renderInfo = new RenderInfo();
		renderInfo.RenderMethod = reader.ReadUInt32();

		// If highest bit isn't set
		if ((renderInfo.RenderMethod & 0x80000000) == 0)
		{
			//Console.WriteLine("WARNING: TODO Transparency/Wireframe");
		}
		else
		{
			// (v5 ^ 0x80000000) + 1);
			renderInfo.RenderMethod = (renderInfo.RenderMethod ^ 0x80000000) + 1;
			//Console.WriteLine($"USERDEFINED {renderInfo.RenderMethod}");
		}

		// 0x01 = PEN %d
		// 0x02 = BRIGHTNESS %f
		// 0x04 = SCALEDAMBIENT %f
		// 0x08 = SIMPLESPRITEINST/DEFINITION %s/SKIPFRAMESON?/ENDSIMPLESPRITEINST
		// 0x10 = UVORIGIN %f %f %f/UAXIS 1.0 %f %f %f/VAXIS 1.0 %f %f %f
		// 0x20 = UV %f %f
		// 0x40 = TWOSIDED
		var renderMethodFlag = reader.ReadUInt32();
		renderInfo.RenderMethodFlag = renderMethodFlag;

		if ((renderMethodFlag & 0x1) != 0)
		{
			uint pen = reader.ReadUInt32();
			//Console.WriteLine($"\t\tPEN {pen}");
		}

		if ((renderMethodFlag & 0x2) != 0)
		{
			float brightness = reader.ReadSingle();
			//Console.WriteLine($"\t\tBRIGHTNESS {brightness}");
		}

		if ((renderMethodFlag & 0x4) != 0)
		{
			float scaledAmbient = reader.ReadSingle();
			//Console.WriteLine($"\t\tSCALEDAMBIENT {scaledAmbient}");
		}

		if ((renderMethodFlag & 0x8) != 0)
		{
			uint simpleSpriteInstRef = reader.ReadUInt32();
			var fragmentRef = WLD.FragmentContextQueue.Dequeue();

			//Console.WriteLine($"SimpleSpriteInst context (fragmentRef)={fragmentRef}");

			renderInfo.SimpleSpriteDef = fragmentRef;
		}

		if ((renderMethodFlag & 0x10) != 0)
		{
			float uvox = reader.ReadSingle();
			float uvoy = reader.ReadSingle();
			float uvoz = reader.ReadSingle();
			//Console.WriteLine($"\t\tUVORIGIN {uvox} {uvoy} {uvoz}");

			float uax = reader.ReadSingle();
			float uay = reader.ReadSingle();
			float uaz = reader.ReadSingle(); // FIXME: incorrect in output vs ascii
			//Console.WriteLine($"\t\tUAXIS 1.0 {uax} {uay} {uaz}");

			float vax = reader.ReadSingle();
			float vay = reader.ReadSingle();
			float vaz = reader.ReadSingle();
			//Console.WriteLine($"\t\tVAXIS 1.0 {vax} {vay} {vaz}");
		}

		if ((renderMethodFlag & 0x20) != 0)
		{
			uint numUV = reader.ReadUInt32();

			for (int currentUV = 0; currentUV < numUV; currentUV++)
			{
				float uvx = reader.ReadSingle();
				float uvy = reader.ReadSingle();

				UV uv = new UV();
				uv.x = uvx;
				uv.y = uvy;
				renderInfo.UVs.Add(uv);
				////Console.WriteLine($"\t\tUV {uvx} {uvy}");
			}
		}

		if ((renderMethodFlag & 0x40) != 0)
		{
			//Console.WriteLine("\t\tTWOSIDED");
		}

		return renderInfo;
	}
}