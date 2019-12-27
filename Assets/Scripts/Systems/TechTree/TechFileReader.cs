using OP2UtilityDotNet;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OP2MissionEditor.Systems.TechTree
{
	/// <summary>
	/// Represents a technology in a tech file.
	/// </summary>
	public class TechData
	{
		public int techID		{ get; private set; }
		public string techName	{ get; private set; }

		public TechData(int techID, string techName)
		{
			this.techID = techID;
			this.techName = techName;
		}
	}

	public class TechFileReader
	{
		/// <summary>
		/// Gets the technology data from a tech file.
		/// </summary>
		public static TechData[] GetTechnologies(string archiveDirectory, string techFileName)
		{
			// Can't parse files in archive directory if one hasn't been assigned.
			if (string.IsNullOrEmpty(archiveDirectory))
				return new TechData[0];

			List<TechData> technologies = new List<TechData>();

			using (ResourceManager resourceManager = new ResourceManager(archiveDirectory))
			{
				byte[] techSheet = resourceManager.GetResource(techFileName, true);
				if (techSheet == null)
					return new TechData[0];

				using (MemoryStream stream = new MemoryStream(techSheet))
				using (StreamReader reader = new StreamReader(stream))
				{
					while (!reader.EndOfStream)
					{
						string line = reader.ReadLine();

						if (!line.StartsWith("BEGIN_TECH"))
							continue;

						int firstIndexOfQuote = line.IndexOf('"');
						int secondIndexOfQuote = line.IndexOf('"', firstIndexOfQuote+1);

						string techName = line.Substring(firstIndexOfQuote+1, secondIndexOfQuote-firstIndexOfQuote-1);
						string szTechId = line.Substring(secondIndexOfQuote+2);
						int techId;

						if (!int.TryParse(szTechId, out techId))
						{
							Debug.LogWarning("Failed to parse tech id: " + szTechId);
							continue;
						}

						technologies.Add(new TechData(techId, techName));
					}
				}
			}

			return technologies.ToArray();
		}
	}
}
