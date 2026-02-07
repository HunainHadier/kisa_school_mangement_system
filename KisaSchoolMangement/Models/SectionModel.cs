namespace KisaSchoolMangement.Models
{
    public class SectionModel
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
        public string CreatedAt { get; set; }

        // For display
        public string ClassName { get; set; }
    }
}