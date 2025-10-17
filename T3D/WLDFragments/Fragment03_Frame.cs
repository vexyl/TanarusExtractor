using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;

// FIXME
// FRAME "file?" "name?"
public class Fragment03_Frame : Fragment
{
	internal Fragment03_Frame(BinaryReader reader)
	{
		var nameRef = reader.ReadUInt32() + 1;
		reader.ReadBytes(4); // Unknown (Flag?)
		var length = reader.ReadUInt16();
		var s = WLD.DecodeStringXOR(reader.ReadBytes((int)length));
		Name = s.Replace("\0", "");
		//Console.WriteLine($"FRAME {s} {Name}");
	}
}