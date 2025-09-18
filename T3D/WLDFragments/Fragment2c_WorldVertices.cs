using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

public class Vertex
{
	public float x, y, z;
	internal Vertex(float x, float y, float z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}
}

public class Fragment2c_WorldVertices : Fragment
{
	public List<Vertex> vertices = new List<Vertex>();
	internal Fragment2c_WorldVertices(BinaryReader reader)
	{
		var numVertices = reader.ReadUInt32();

		for (int i = 0; i < numVertices; ++i)
		{
			var vx = reader.ReadSingle() * -1; // Flip mesh to fix mirror issue
			var vy = reader.ReadSingle();
			var vz = reader.ReadSingle();

			vertices.Add(new Vertex(vx, vy, vz));
		}
	}

	public string ToASCII(int indentation = 0)
	{
		Debug.Assert(indentation >= 0);

		StringBuilder stringBuilder = new StringBuilder();

		stringBuilder.Append('\t', indentation);
		stringBuilder.AppendLine("WORLDVERTICES");
		indentation += 1;
		stringBuilder.Append('\t', indentation);
		stringBuilder.AppendLine($"NUMVERTICES {vertices.Count}");
		foreach (var vertex in vertices)
		{
			stringBuilder.Append('\t', indentation);
			stringBuilder.AppendLine($"XYZ {vertex.x} {vertex.y} {vertex.z}");
		}
		indentation -= 1;
		stringBuilder.AppendLine("ENDWORLDVERTICES");

		return stringBuilder.ToString();
	}
}