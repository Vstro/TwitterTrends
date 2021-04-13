using TwitterTrends.Entities;

namespace TwitterTrends.Services
{
    public static class CoordinatesService
    {
        /// <param name="textCoords">Coordinates as string in format "[lat, lon]"</param>
        public static Coordinates ParseCoordinates(string textCoords)
        {
            Coordinates coords = new Coordinates();
            int coordStart = textCoords.IndexOf('[');
            int coordSeparator = textCoords.IndexOf(',');
            int coordEnd = textCoords.IndexOf(']');
            coords.Lon = double.Parse(textCoords[(coordStart + 1)..coordSeparator].Replace('.', ','));
            coords.Lat = double.Parse(textCoords[(coordSeparator + 2)..coordEnd].Replace('.', ','));
            return coords;
        }
    }
}
