using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

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
                            Console.Write("\nConverting WLD to OBJ ... ");
                            var wld = new WLD(entry.GetStreamCopy());
                            wld.Parse();

                            StreamWriter objWriter = new StreamWriter(Path.Combine(args[1], entry.FileName.Replace(".wld", "") + ".obj"));
                            var worldVerticesFragmentList = wld.Fragments.OfType<Fragment2c_WorldVertices>().ToList();
                            foreach (var fragment in worldVerticesFragmentList)
                            {
                                foreach (var vertex in fragment.vertices)
                                {
                                    objWriter.WriteLine($"v {vertex.x} {vertex.z} {vertex.y}"); // Write vertices (swap Y and Z for Y-up)
                                }
                            }

							var regionFragmentList = wld.Fragments.OfType<Fragment22_Region>().ToList();
							foreach (var fragment in regionFragmentList)
								foreach (var wall in fragment.Walls)
							{
								// Skip if RenderMethod 1, which removes the exterior box
								if (wall.VertexList.Count < 3 || wall.RenderMethod == 1)
									continue;

								objWriter.Write("f");
								foreach (var v in wall.VertexList)
								{
									objWriter.Write($" {v}");
								}
								objWriter.WriteLine();
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
