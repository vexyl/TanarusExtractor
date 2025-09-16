using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using static Fragment22_Region;

public class VertexUVPair
{
    public int vertexIdx;
    public float u, v;
    public int normalIdx;

    public VertexUVPair(int vertexIdx, float u, float v, int normalIdx)
	{
		this.vertexIdx = vertexIdx;
		this.u = u;
        this.v = v;
        this.normalIdx = normalIdx;
	}

	public override int GetHashCode()
	{
		return (vertexIdx, u, v).GetHashCode();
	}

	public override bool Equals(object obj)
	{
        var pair = obj as VertexUVPair;
        return (vertexIdx == pair.vertexIdx) && ( u == pair.u) && (v == pair.v);
	}
}

namespace T3D
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var programName = Assembly.GetEntryAssembly()?.GetName().Name;
            var extractObj = false;

            Console.WriteLine("=================");
            Console.WriteLine($"{programName} {Assembly.GetEntryAssembly()?.GetName().Version}");
            Console.WriteLine("=================");

            if (args.Length < 2)
            {
                Console.WriteLine($"Usage: {programName} <archive.t3d> <output folder> [-obj]");

                return -1;
            }

            if (args.Length == 3 && args[2] == "-obj")
            {
                extractObj = true;
            }

            Directory.CreateDirectory(args[1]);

            try
            {
                using (var t3d = T3DArchive.OpenRead(args[0]))
                {
                    foreach (var entry in t3d.Entries)
                    {
                        Console.Write($"Extracting {Path.Combine(args[1], entry.FileName)} ... ");
                        entry.ExtractToDirectory(args[1]);

                        if (extractObj && entry.FileName.ToLower().EndsWith(".wld"))
                        {
                            Console.Write("Converting WLD to OBJ ... ");
                            var wld = new WLD(entry.GetStreamCopy());
                            wld.Parse();

                            var filename = entry.FileName.Replace(".wld", "");
                            StreamWriter objWriter = new StreamWriter(Path.Combine(args[1], filename + ".obj"));
                            StreamWriter mtlWriter = new StreamWriter(Path.Combine(args[1], filename + ".mtl"));

                            var worldVerticesFragmentList = WLD.Fragments.OfType<Fragment2c_WorldVertices>().ToList();
                            var worldVertices = worldVerticesFragmentList[0].vertices;
                            var regionFragmentList = WLD.Fragments.OfType<Fragment22_Region>().ToList();
                            var simpleSpriteDefs = WLD.Fragments.OfType<Fragment04_SimpleSpriteDef>().ToList();

                            objWriter.WriteLine("# Exported with TanarusExtractor");
                            objWriter.WriteLine($"mtllib {filename}.mtl");

							foreach (var simpleSpriteDef in simpleSpriteDefs)
							{
								mtlWriter.WriteLine($"newmtl {simpleSpriteDef.Name}");
								foreach (var frame in simpleSpriteDef.Frames)
								{
									mtlWriter.WriteLine($"map_Kd {frame.Name}");
								}
                                mtlWriter.WriteLine();
                                mtlWriter.Flush();
							}

                            var vertexUVToIdx = new Dictionary<VertexUVPair, int>();
                            var uniqueVertices = new List<Vertex>();
                            var uniqueUVs = new List<UV>();
                            var normals = new List<Normal>();
                            
                            int nextIndex = 1;
							foreach (var fragment in regionFragmentList)
							{
								foreach (var wall in fragment.Walls)
								{
                                    if (wall.Normals.Count > 0)
                                        normals.AddRange(wall.Normals);

									if (wall.UVs.Count == 0)
										continue;

									for (int i = 0; i < wall.VertexList.Count; i++)
                                    {
                                        int vIndex = wall.VertexList[i] - 1;
                                        var uv = wall.UVs[i];

                                        var pair = new VertexUVPair(vIndex, uv.x, uv.y, i);
                                        if (!vertexUVToIdx.ContainsKey(pair))
                                        {
                                            vertexUVToIdx[pair] = nextIndex++;
                                            uniqueVertices.Add(worldVertices[vIndex]);
                                            uniqueUVs.Add(uv);
                                        }
									}
								}
							}

							foreach (var v in uniqueVertices)
							{
								objWriter.WriteLine($"v {v.x} {v.z} {v.y}");
							}

							foreach (var vn in normals)
							{
								objWriter.WriteLine($"vn {vn.a} {vn.b} {vn.c}");
							}

							foreach (var uv in uniqueUVs)
							{
								objWriter.WriteLine($"vt {uv.x} {uv.y}");
							}

							string newMaterial = "";
							string currentMaterial = "";
							int uvIndex = 1;
                            int normalIndex = 0;
							List<UV> wallUVs = new List<UV>();
							foreach (var fragment in regionFragmentList)
							{
								foreach (var wall in fragment.Walls)
								{
                                    if (wall.Normals.Count > 0)
                                    {
                                        normalIndex++;
                                    }

									if (wall.UVs.Count == 0)
										continue;

									foreach (var simpleSpriteInst in wall.SimpleSpriteDefs)
									{
										newMaterial = simpleSpriteInst.Name;
										break;
									}

									if (newMaterial != currentMaterial)
									{
										currentMaterial = newMaterial;
										objWriter.WriteLine($"usemtl {currentMaterial}");
									}

                                    objWriter.Write("f");
									for (int i = 0; i < wall.VertexList.Count; i++)
									{
										int vIndex = wall.VertexList[i] - 1;
										var uv = wall.UVs[i];

										var pair = new VertexUVPair(vIndex, uv.x, uv.y, i);
                                        int objIndex = vertexUVToIdx[pair];

										objWriter.Write($" {objIndex}/{objIndex}/{normalIndex}");
									}
									objWriter.WriteLine();
								}
							}

							objWriter.Flush();
						}
                        Console.WriteLine("ok!");
                    }
                }

                return 0;
            }

            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
            }

            return -1;
        }
    }
}
