namespace KisaSchoolMangement.Models
{
    public class SubjectModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}