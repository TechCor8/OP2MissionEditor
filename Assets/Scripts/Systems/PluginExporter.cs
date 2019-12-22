using DotNetMissionSDK.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OP2MissionEditor.Systems
{
	public class PluginExporter
	{
		public static void ExportPlugin(string path, LevelDetails details)
		{
			string templatePath = Path.Combine(Application.streamingAssetsPath, "PluginTemplate.dll");

			// Open plugin destination
			using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
			using (BinaryWriter writer = new BinaryWriter(fs))
			{
				// Read plugin template
				using (FileStream templateFS = new FileStream(templatePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (BinaryReader templateReader = new BinaryReader(templateFS))
				{
					ReplaceSection(templateReader, writer, "MapFile", details.mapName);
					ReplaceSection(templateReader, writer, "LevelDesc", details.description);
					ReplaceSection(templateReader, writer, "TechFile", details.techTreeName);

					// Write null-terminator of last section
					writer.Write('\0');
					templateReader.BaseStream.Seek(1, SeekOrigin.Current);

					// Write AIModDesc
					writer.Write((int)details.missionType);
					writer.Write(details.numPlayers);
					writer.Write(details.maxTechLevel);
					writer.Write(details.unitOnlyMission ? 1 : 0);

					templateReader.BaseStream.Seek(16, SeekOrigin.Current);

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

			throw new System.Exception("PluginExporter: Could not find key: " + key);
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
	}
}
