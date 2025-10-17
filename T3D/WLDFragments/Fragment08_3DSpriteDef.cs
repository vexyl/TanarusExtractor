using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using static Fragment22_Region;

public class BSPNode
{
	public List<int> VertexList = new List<int>();
	public List<Normal> Normals = new List<Normal>();
	public RenderInfo RenderInfo;
}

public class Fragment08_3DSpriteDef : Fragment
{
	public uint NameRef;
	public uint Flag;
	public List<Vertex> vertices = new List<Vertex>();
	public List<BSPNode> BSPNodes = new List<BSPNode>();

	internal Fragment08_3DSpriteDef(BinaryReader reader)
	{
		NameRef = reader.ReadUInt32() + 1;

		// Flag
		//	0x20 - ENABLEGOURAUD2
		//	0x1	 - CENTEROFFSET %f %f %f
		//	0x2  - BOUNDINGRADIUS %f
		//  0x40 - NORMALABCD
		Flag = reader.ReadUInt32();

		var numVertices = reader.ReadUInt32();
		var numBSPNodes = reader.ReadUInt32();
		var shape = reader.ReadUInt32();

		//Console.WriteLine($"NUMVERTICES {numVertices}");

		Name = WLD.GetNameFromRef((int)NameRef);

		//Console.WriteLine($"\n3DSPRITEDEF {Name} {Flag} {numBSPNodes} {shape}");

		if ((Flag & 0x20) != 0)
		{
			//Console.WriteLine("ENABLEGOURAUD2");
		}
		if ((Flag & 0x1) != 0)
		{
			var x = reader.ReadSingle();
			var y = reader.ReadSingle();
			var z = reader.ReadSingle();
			//Console.WriteLine($"CENTEROFFSET {x} {y} {z}");
		}
		if ((Flag & 0x2) != 0)
		{
			var boundingRadius = reader.ReadSingle();
			//Console.WriteLine($"BOUNDINGRADIUS {boundingRadius}");
		}

		for (int i = 0; i < numVertices; ++i)
		{
			var vx = reader.ReadSingle();
			var vy = reader.ReadSingle();
			var vz = reader.ReadSingle();

			vertices.Add(new Vertex(vx, vy, vz));

			//Console.WriteLine($"XYZ {vx} {vy} {vz}");
		}

		//Console.WriteLine($"NUMBSPNODES {numBSPNodes}");
		for (int i = 0; i < numBSPNodes; ++i)
		{
			BSPNode bspNode = new BSPNode();

			//Console.WriteLine("BSPNODE");

			var numVertices2 = reader.ReadUInt32();
			reader.ReadUInt32(); // Unknown
			reader.ReadUInt32(); // Unknown
			//Console.WriteLine($"NUMVERTICES {numVertices2}");
			//Console.Write("VERTEXLIST");
			var vertexList = new List<int>();
			for (int j = 0; j < numVertices2; ++j)
			{
				var v = reader.ReadUInt32() + 1; // non-zero vertex index
				bspNode.VertexList.Add((int)v);
				//Console.Write($" {v}");
			}
			//Console.WriteLine();

			bspNode.RenderInfo = RenderInfo.ReadRenderMethod(reader);

			if ((Flag & 0x40) != 0)
			{
				float na = reader.ReadSingle();
				float nb = reader.ReadSingle();
				float nc = reader.ReadSingle();
				float nd = reader.ReadSingle();
				Normal normal = new Normal();
				normal.a = na; normal.b = nb; normal.c = nc; normal.d = nd;
				bspNode.Normals.Add(normal);
				//Console.WriteLine($"\tNORMALABCD {na} {nb} {nc} {nd}");
			}


			//Console.WriteLine("ENDBSPNODE");
			BSPNodes.Add(bspNode);
		}
	}
}