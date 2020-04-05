using DotNetMissionSDK.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OP2MissionEditor.Systems
{
	public class PluginExporter
	{
		// AIModDesc default values in template
		private static byte[] modDescTemplateValues = new byte[]
		{
			0xFF, 0xFF, 0xFF, 0xFF,		// Mission Type: Colony
			0x06, 0x00, 0x00, 0x00,		// Players: 6
			0x0C, 0x00, 0x00, 0x00,		// Max Tech: 12
			0x00, 0x00, 0x00, 0x00		// Unit Mission: False
		};


		public static void ExportPlugin(string path, string sdkVersion, LevelDetails details)
		{
			string templatePath = Path.Combine(Application.streamingAssetsPath, "PluginTemplate.dll");

			// Convert AIModDesc values from the editor to a byte array
			List<byte> modDescList = new List<byte>(modDescTemplateValues.Length);

			modDescList.AddRange(BitConverter.GetBytes((int)details.missionType));
			modDescList.AddRange(BitConverter.GetBytes(details.numPlayers));
			modDescList.AddRange(BitConverter.GetBytes(details.maxTechLevel));
			modDescList.AddRange(BitConverter.GetBytes(details.unitOnlyMission ? 1 : 0));

			byte[] newModDesc = modDescList.ToArray();

			// Open plugin destination
			using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
			using (BinaryWriter writer = new BinaryWriter(fs))
			{
				// Read plugin template
				using (FileStream templateFS = new FileStream(templatePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (BinaryReader templateReader = new BinaryReader(templateFS))
				{
					// If you recompile the template, you will need to double check that the order has not changed.
					ReplaceSection(templateReader, writer, "MapFile", details.mapName);
					ReplaceSection(templateReader, writer, "TechFile", details.techTreeName);
					ReplaceSection(templateReader, writer, modDescTemplateValues, newModDesc);
					ReplaceSection(templateReader, writer, "LevelDesc", details.description);
					ReplaceSection(templateReader, writer, "DotNetMissionSDK", "DotNetMissionSDK_v" + sdkVersion.Replace(".", "_") + ".dll");

					// Write rest of file
					writer.Write(templateReader.ReadBytes((int)(templateReader.BaseStream.Length - templateReader.BaseStream.Position)));
				}
			}
		}

		private static int GetIndexOfKey(BinaryReader reader, string key)
		{
			List<char> buffer = new List<char>(key.Length);

			while (reader.BaseStream.Position != reader.BaseStream.Length)
			{
				// If our buffer is full, remove the oldest character to make room for the next one
				if (buffer.Count == key.Length)
					buffer.RemoveAt(0);

				byte val = reader.ReadByte();

				if (!char.IsLetter((char)val))
				{
					buffer.Clear();
					continue;
				}

				buffer.Add((char)val);

				string compareKey = string.Concat(buffer);

				// If our buffer matches the key, we found it! Return the index
				if (compareKey == key)
					return (int)reader.BaseStream.Position - key.Length;
			}

			throw new Exception("PluginExporter: Could not find key: " + key);
		}

		private static int GetLastIndexOfChar(BinaryReader reader, char skipChar)
		{
			while (reader.BaseStream.Position != reader.BaseStream.Length)
			{
				if (reader.ReadChar() != skipChar)
					return (int)reader.BaseStream.Position-1;
			}

			return (int)reader.BaseStream.Length-1;
		}

		private static void ReplaceSection(BinaryReader templateReader, BinaryWriter writer, string key, string value)
		{
			// Get level desc section
			long startPosition = templateReader.BaseStream.Position;
			int startIndex = GetIndexOfKey(templateReader, key);
			int endIndex = GetLastIndexOfChar(templateReader, 'X');
					
			// Write data before section
			long endPosition = templateReader.BaseStream.Position;
			templateReader.BaseStream.Seek(startPosition, SeekOrigin.Begin);
			writer.Write(templateReader.ReadBytes(startIndex - (int)templateReader.BaseStream.Position));
					
			// Write section
			for (int i=0; i < value.Length && i <= endIndex; ++i)
				writer.Write(value[i]);

			// Fill excess with zeroes
			for (int i=(int)writer.BaseStream.Position; i <= endIndex; ++i)
				writer.Write('\0');

			// Go back to where we left off
			templateReader.BaseStream.Seek(endPosition, SeekOrigin.Begin);
		}

		private static int GetIndexOfKey(BinaryReader reader, byte[] key)
		{
			List<byte> buffer = new List<byte>(key.Length);

			while (reader.BaseStream.Position != reader.BaseStream.Length)
			{
				// If our buffer is full, remove the oldest byte to make room for the next one
				if (buffer.Count == key.Length)
					buffer.RemoveAt(0);

				buffer.Add(reader.ReadByte());

				// If our buffer matches the key, we found it! Return the index				
				if (AreArraysEqual(buffer.ToArray(), key))
					return (int)reader.BaseStream.Position - key.Length;
			}

			throw new Exception("PluginExporter: Could not find key: " + key);
		}

		private static bool AreArraysEqual(byte[] arr1, byte[] arr2)
		{
			if (arr1.Length != arr2.Length)
				return false;

			for (int i=0; i < arr1.Length; ++i)
			{
				if (arr1[i] != arr2[i])
					return false;
			}

			return true;
		}

		private static void ReplaceSection(BinaryReader templateReader, BinaryWriter writer, byte[] key, byte[] value)
		{
			// Get level desc section
			long startPosition = templateReader.BaseStream.Position;
			int startIndex = GetIndexOfKey(templateReader, key);
					
			// Write data before section
			long endPosition = templateReader.BaseStream.Position;
			templateReader.BaseStream.Seek(startPosition, SeekOrigin.Begin);
			writer.Write(templateReader.ReadBytes(startIndex - (int)templateReader.BaseStream.Position));
					
			// Write section
			for (int i=0; i < value.Length; ++i)
				writer.Write(value[i]);

			// Go back to where we left off
			templateReader.BaseStream.Seek(endPosition, SeekOrigin.Begin);
		}
	}
}
