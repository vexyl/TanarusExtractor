using System.Reflection.PortableExecutable;
using System;
using System.IO;
using System.Collections.Generic;
using static Fragment22_Region;
using T3D;
using System.Text.RegularExpressions;

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
		public RenderInfo RenderInfo;
		public List<int> VertexList = new List<int>();
		public List<Normal> Normals = new List<Normal>();
		public bool IsFloor = false;
	}

	internal Fragment22_Region(BinaryReader reader)
	{
		if (reader == null)
			return;

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

		wall.RenderInfo = RenderInfo.ReadRenderMethod(reader);

		if ((wallFlag & 0x1) != 0)
		{
			wall.IsFloor = true;
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