using UnityEngine;

namespace RTS
{
	public static class TeamColorManager
	{
		private static int colorIndex = 0;

		private static Color[] colors = {
			Color.red,
			Color.blue,
			Color.green,
			Color.yellow
		};

		public static Color GetUniqueColor()
		{
			Color color = Color.black;

			if (colorIndex < colors.Length)
			{
				color = colors[colorIndex];
				colorIndex++;
			}

			return color;
		}
	}
}
