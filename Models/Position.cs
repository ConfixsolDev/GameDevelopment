namespace TechWebSol.Models
{
    /// <summary>
    /// Represents a geographic position with latitude and longitude
    /// </summary>
    public class Position
    {
        public double Lat { get; set; }
        public double Lng { get; set; }

        public Position()
        {
        }

        public Position(double lat, double lng)
        {
            Lat = lat;
            Lng = lng;
        }

        public override string ToString()
        {
            return $"[{Lat}, {Lng}]";
        }
    }
}

