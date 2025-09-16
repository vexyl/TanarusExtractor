using System.Reflection.PortableExecutable;
using System;
using System.IO;
using System.Collections.Generic;
using static Fragment22_Region;
using T3D;

public class UV
{
	public float x, y;
}

public class Normal
{
	public float a, b, c, d;
}

public class Fragment22_Region : Fragment
{
	public List<Wall> Walls = new List<Wall>();

	public struct RegionHeader
	{
		public uint NameRef;
		public uint RegionFlag;
		public uint AmbientLightDef;
		public uint NumRegionVertex;
		public uint NumProximalRegions;
		public uint NumRenderVertices;
		public uint NumWalls;
		public uint NumObstacles;
		public uint NumCuttingObstacles;
		public uint NumVisNode;
		public uint NumVisList;

		public RegionHeader(BinaryReader reader)
		{
			// region static-size header (11 * 4 bytes)
			NameRef = reader.ReadUInt32() + 1;
			RegionFlag = reader.ReadUInt32();
			AmbientLightDef = reader.ReadUInt32();
			NumRegionVertex = reader.ReadUInt32();
			NumProximalRegions = reader.ReadUInt32();
			NumRenderVertices = reader.ReadUInt32();
			NumWalls = reader.ReadUInt32();
			NumObstacles = reader.ReadUInt32();
			NumCuttingObstacles = reader.ReadUInt32();
			NumVisNode = reader.ReadUInt32();
			NumVisList = reader.ReadUInt32();
		}

		public override string ToString()
		{
			return
				$"NameRef: {NameRef} " +
				$"RegionFlag: {RegionFlag} " +
				$"AmbientLightDef: {AmbientLightDef} " +
				$"NumRegionVertex: {NumRegionVertex} " +
				$"NumProximalRegions: {NumProximalRegions} " +
				$"NumRenderVertices: {NumRenderVertices} " +
				$"NumWalls: {NumWalls} " +
				$"NumObstacles: {NumObstacles} " +
				$"NumCuttingObstacles: {NumCuttingObstacles} " +
				$"NumVisNode: {NumVisNode} " +
				$"NumVisList: {NumVisList}";
		}
	}

	public class Wall
	{
		public uint RenderMethod = 0;
		public List<Fragment04_SimpleSpriteDef> SimpleSpriteDefs = new List<Fragment04_SimpleSpriteDef>();
		public List<int> VertexList = new List<int>();
		public List<UV> UVs = new List<UV>();
		public List<Normal> Normals = new List<Normal>();
	}

	internal Fragment22_Region(BinaryReader reader)
	{
		RegionHeader header = new RegionHeader(reader);

		var name = WLD.GetNameFromRef((int)header.NameRef);

		// untested
		// 3 floats XYZ vertex each num_region
		for (int i = 0; i < header.NumRegionVertex; ++i)
		{
			var vx = reader.ReadSingle();
			var vy = reader.ReadSingle();
			var vz = reader.ReadSingle();

			Console.WriteLine("WARNING: NUMREGIONVERTEX untested");
		}

		// PROXIMALREGION %d %f (v11, v10)
		for (int i = 0; i < header.NumProximalRegions; ++i)
		{
			var v11 = reader.ReadUInt32() + 1; // v11 = *(_DWORD *)v4 + 1;
			var v10 = reader.ReadSingle(); // v10 = v4[1];

			Console.WriteLine("WARNING: NUMPROXIMALREGIONS untested");
		}

		for (int i = 0; i < header.NumWalls; ++i)
		{
			var wall = ReadWall(reader);
			Walls.Add(wall);
		}

		//Console.WriteLine(header.ToString());
	}

	public Wall ReadWall(BinaryReader reader)
	{
		Wall wall = new Wall();

		// flag | 0x1 == FLOOR
		// flag | 0x2 == RENDERMETHOD
		var wallFlag = reader.ReadUInt32();
		var numVertices = reader.ReadUInt32();
		//Console.Write("VERTEXLIST");
		for (int j = 0; j < numVertices; ++j)
		{
			var v = reader.ReadUInt32() + 1; // non-zero vertex index
			wall.VertexList.Add((int)v);
			//Console.Write($" {v}");
		}
		//Console.WriteLine();

		wall.RenderMethod = reader.ReadUInt32();

		// If highest bit isn't set
		if ((wall.RenderMethod & 0x80000000) == 0)
		{
			//Console.WriteLine("WARNING: TODO Transparency/Wireframe");
		}
		else
		{
			// (v5 ^ 0x80000000) + 1);
			wall.RenderMethod = (wall.RenderMethod ^ 0x80000000) + 1;
			//Console.WriteLine($"USERDEFINED {wall.RenderMethod}");
		}

		// 0x01 = PEN %d
		// 0x02 = BRIGHTNESS %f
		// 0x04 = SCALEDAMBIENT %f
		// 0x08 = SIMPLESPRITEINST/DEFINITION %s/SKIPFRAMESON?/ENDSIMPLESPRITEINST
		// 0x10 = UVORIGIN %f %f %f/UAXIS 1.0 %f %f %f/VAXIS 1.0 %f %f %f
		// 0x20 = UV %f %f
		// 0x40 = TWOSIDED
		var renderMethodFlag = reader.ReadUInt32();

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
			var context = WLD.ContextQueue.Dequeue();
			//Console.WriteLine($"SimpleSpriteInst context={context}");

			var simpleSpriteDefFragment = (Fragment04_SimpleSpriteDef)WLD.Fragments[context];
			wall.SimpleSpriteDefs.Add(simpleSpriteDefFragment);
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
				wall.UVs.Add(uv);
				//Console.WriteLine($"\t\tUV {uvx} {uvy}");
			}
		}

		if ((renderMethodFlag & 0x40) != 0)
		{
			//Console.WriteLine("\t\tTWOSIDED");
		}

		if ((wallFlag & 0x2) != 0)
		{
			float na = reader.ReadSingle(); // FIXME: output incorrectly negative vs ascii -- maybe if -0 -> 0 for all 4?
			float nb = reader.ReadSingle();
			float nc = reader.ReadSingle();
			float nd = reader.ReadSingle();
			Normal normal = new Normal();
			normal.a = na; normal.b = nb; normal.c = nc; normal.d = nd;
			wall.Normals.Add(normal);
			//Console.WriteLine($"\tNORMALABCD {na} {nb} {nc} {nd}");
		}

		return wall;
	}
}