using System;
using System.Text;

namespace OP2MissionEditor.Utility
{
	/// <summary>
	/// Makes a JSON string look pretty.
	/// </summary>
	public class JsonFormatter
	{
		public static string Format(string jsonString)
		{
			var stringBuilder = new StringBuilder();

			bool escaping = false;
			bool inQuotes = false;
			int indentation = 0;

			foreach (char character in jsonString)
			{
				if (escaping)
				{
					escaping = false;
					stringBuilder.Append(character);
				}
				else
				{
					if (character == '\\')
					{
						escaping = true;
						stringBuilder.Append(character);
					}
					else if (character == '\"')
					{
						inQuotes = !inQuotes;
						stringBuilder.Append(character);
					}
					else if (!inQuotes)
					{
						if (character == ',')
						{
							stringBuilder.Append(character);
							stringBuilder.Append("\r\n");
							stringBuilder.Append('\t', indentation);
						}
						else if (character == '[' || character == '{')
						{
							stringBuilder.Append("\r\n");
							stringBuilder.Append('\t', indentation);
							stringBuilder.Append(character);
							stringBuilder.Append("\r\n");
							stringBuilder.Append('\t', ++indentation);
						}
						else if (character == ']' || character == '}')
						{
							stringBuilder.Append("\r\n");
							stringBuilder.Append('\t', --indentation);
							stringBuilder.Append(character);
						}
						else if (character == ':')
						{
							stringBuilder.Append(character);
							//stringBuilder.Append('\t');
						}
						else if (!Char.IsWhiteSpace(character))
						{
							stringBuilder.Append(character);
						}
					}
					else
					{
						stringBuilder.Append(character);
					}
				}
			}

			return stringBuilder.ToString();
		}
	}
}
