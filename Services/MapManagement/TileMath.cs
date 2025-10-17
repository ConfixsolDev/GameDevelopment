namespace TechWebSol.Services.MapManagement
{
	public static class TileMath
	{
		private const double EarthRadiusKm = 6371.0;

		public static (int x, int y) Deg2Num(double latDeg, double lonDeg, int zoom)
		{
			var latRad = latDeg * Math.PI / 180.0;
			var n = Math.Pow(2.0, zoom);
			var x = (int)Math.Floor((lonDeg + 180.0) / 360.0 * n);
			var y = (int)Math.Floor((1.0 - Math.Log(Math.Tan(latRad) + 1 / Math.Cos(latRad)) / Math.PI) / 2.0 * n);
			return (x, y);
		}

		/// <summary>
		/// Calculate the geographic area of a bounding box in square kilometers
		/// Uses the Haversine formula to account for Earth's curvature
		/// </summary>
		public static double CalculateAreaKm2(double north, double south, double east, double west)
		{
			// Convert degrees to radians
			double lat1 = south * Math.PI / 180.0;
			double lat2 = north * Math.PI / 180.0;
			double lon1 = west * Math.PI / 180.0;
			double lon2 = east * Math.PI / 180.0;

			// Calculate the width at the northern and southern edges
			double widthNorth = EarthRadiusKm * Math.Cos(lat2) * Math.Abs(lon2 - lon1);
			double widthSouth = EarthRadiusKm * Math.Cos(lat1) * Math.Abs(lon2 - lon1);
			double avgWidth = (widthNorth + widthSouth) / 2.0;

			// Calculate the height using Haversine formula
			double dLat = lat2 - lat1;
			double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2);
			double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
			double height = EarthRadiusKm * c;

			return avgWidth * height;
		}

		/// <summary>
		/// Calculate the geographic area of a bounding box in square miles
		/// </summary>
		public static double CalculateAreaMi2(double north, double south, double east, double west)
		{
			const double km2ToMi2 = 0.386102;
			return CalculateAreaKm2(north, south, east, west) * km2ToMi2;
		}
	}
}
