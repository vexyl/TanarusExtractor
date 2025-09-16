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
	public static List<Fragment> Fragments;
	public static Dictionary<int, string> StringDB;
	public static Queue<int> ContextQueue;

	internal WLD(Stream stream)
	{
		_stream = stream;
		Fragments = new List<Fragment>();
		StringDB = new Dictionary<int, string>();
		ContextQueue = new Queue<int>();

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

	public static string GetNameFromRef(int nameRef)
	{
		nameRef = nameRef - (1 << 32);
		if (nameRef < 0)
		{
			nameRef = -nameRef;
		}

		if (StringDB.ContainsKey(nameRef))
		{
			return StringDB[nameRef];
		}

		return null;
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

			var stringIndex = 0;
			foreach (var stringEntry in stringBlockArray)
			{
				StringDB[stringIndex] = stringEntry;
				//Console.WriteLine($"{stringIndex}: {StringDB[stringIndex]}");

				stringIndex += stringEntry.Length + 1;
			}

			Debug.Assert(stringCount == stringBlockArray.Length - 2); // - 2 because it doesn't count the first and last entries (placeholders/blank)

			Fragments.Add(new Fragment());
			for (int currentFragment = 0; currentFragment < maxFragments; ++currentFragment)
			{
				var fragmentLength = reader.ReadUInt32();
				var fragmentType = reader.ReadUInt32();
				//Console.WriteLine($"Fragment 0x{Convert.ToString(fragmentType, 16)} Length={fragmentLength}");

				int readerOffsetBefore = (int)reader.BaseStream.Position;
				switch (fragmentType)
				{
					case 0x03:
						{
							Fragment03_Frame fragment = new Fragment03_Frame(reader);
							Fragments.Add(fragment);
							break;
						}
					case 0x04:
						{
							Fragment04_SimpleSpriteDef fragment = new Fragment04_SimpleSpriteDef(reader);
							Fragments.Add(fragment);
							break;
						}
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
					case 0x05:
					case 0x07:
					case 0x09:
					case 0xB:
					case 0xD:
					case 0xF:
					case 0x11:
					case 0x16:
					case 0x18:
					case 0x1A:
					case 0x27:
						reader.ReadUInt32(); // Unknown
						var unk = reader.ReadUInt32();
						ContextQueue.Enqueue((int)unk);
						Fragments.Add(new Fragment());
						break;
					default:
						Fragments.Add(new Fragment());
						break;

				}
				int readerOffsetAfter = (int)reader.BaseStream.Position;

				reader.ReadBytes((int)fragmentLength - (readerOffsetAfter - readerOffsetBefore));
			}

			Debug.Assert(reader.BaseStream.Position == reader.BaseStream.Length - 4); // FIXME: Why - 4?
		}
	}
}