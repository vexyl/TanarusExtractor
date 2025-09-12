using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using T3D;

public class WLD
{
	private static readonly byte[] MagicBytes = { 0x02, 0x3D, 0x50, 0x54 };
	private static readonly byte[] VersionBytes = { 0x00, 0x55, 0x01, 0x00 };
	private static readonly byte[] XORTable = { 0x95, 0x3A, 0xC5, 0x2A, 0x95, 0x7A, 0x95, 0x6A };
	private readonly Stream _stream;
	public List<Fragment> Fragments = new List<Fragment>();

	internal WLD(Stream stream)
	{
		_stream = stream;
	}
	private static bool VerifyAgainstBytes(BinaryReader reader, byte[] compareBytes)
	{
		var bytes = reader.ReadBytes(compareBytes.Length);
		return !bytes.Where((t, i) => t != compareBytes[i]).Any();
	}

	private static void Verify(BinaryReader reader)
	{
		if (!VerifyAgainstBytes(reader, MagicBytes))
		{
			throw new T3DArchiveException("Not a valid WLD file");
		}

		if (!VerifyAgainstBytes(reader, VersionBytes))
		{
			throw new T3DArchiveException("WLD version mistmatch");
		}
	}

	public static string DecodeStringXOR(byte[] encodedBytes)
	{
		StringBuilder decodedStringBuilder = new StringBuilder(encodedBytes.Length);
		for (int i = 0; i < encodedBytes.Length; ++i)
		{
			char decodedChar = (char)(XORTable[i % XORTable.Length] ^ encodedBytes[i]);
			decodedStringBuilder.Append(decodedChar);
		}
		return decodedStringBuilder.ToString();
	}

	public void Parse()
	{
		_stream.Seek(0, SeekOrigin.Begin);
		using (var reader = new BinaryReader(_stream, Encoding.ASCII, false))
		{
			Console.WriteLine();

			Verify(reader);

			var maxFragments = reader.ReadUInt32();
			reader.ReadBytes(8); // Unknown
			var stringBlockLen = reader.ReadUInt32();
			var stringCount = reader.ReadUInt32();

			//Console.WriteLine($"maxFragments: {maxFragments} stringBlockLen: {stringBlockLen} stringCount: {stringCount}");

			var encodedStringBlock = reader.ReadBytes((int)stringBlockLen);

			Debug.Assert(stringBlockLen == encodedStringBlock.Length);

			var decodedStringBlock = DecodeStringXOR(encodedStringBlock);
			var stringBlockArray = decodedStringBlock.Split('\0');

			Debug.Assert(stringCount == stringBlockArray.Length - 2); // FIXME: Why - 2?

			List<string> stringDatabase = new List<string>();

			for (int currentFragment = 0; currentFragment < maxFragments; ++currentFragment)
			{
				var fragmentLength = reader.ReadUInt32();
				var fragmentType = reader.ReadUInt32();

				//Console.WriteLine($"Fragment 0x{Convert.ToString(fragmentType, 16)} Length={fragmentLength}");

				int readerOffsetBefore = (int)reader.BaseStream.Position;
				switch (fragmentType)
				{
					case 0x2c:
						{
							Fragment2c_WorldVertices fragment = new Fragment2c_WorldVertices(reader);
							Fragments.Add(fragment);
							//Console.WriteLine(fragment.ToASCII());
						}
						break;
					case 0x22:
						{
							Fragment22_Region fragment = new Fragment22_Region(reader);
							Fragments.Add(fragment);
						}
						break;
					default:
						break;

				}
				int readerOffsetAfter = (int)reader.BaseStream.Position;

				reader.ReadBytes((int)fragmentLength - (readerOffsetAfter - readerOffsetBefore));
			}

			Debug.Assert(reader.BaseStream.Position == reader.BaseStream.Length - 4); // FIXME: Why - 4?
		}
	}
}