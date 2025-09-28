namespace TechWebSol.DTOs.Map
{
    public class MapSaveDto
    {
        public string Name { get; set; } = "Default Map";
        public object Regions { get; set; }
        public object Obstacles { get; set; }
        public object Safe { get; set; }
    }
    public class MapLoadDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public object Regions { get; set; }
        public object Obstacles { get; set; }
        public object Safe { get; set; }
    }

    public class MapEditDto
    {
        public int Id { get; set; }            // which record to update
        public string Name { get; set; }       // optional new name
        public object Regions { get; set; }    // optional updated regions
        public object Obstacles { get; set; }  // optional updated obstacles
        public object Safe { get; set; }       // optional updated safe area
    }

    public class MapDeleteDto
    {
        public int Id { get; set; }   // which record to delete
    }
}
