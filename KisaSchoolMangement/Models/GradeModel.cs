namespace KisaSchoolMangement.Models
{
    public class GradeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal MinMarks { get; set; }
        public decimal MaxMarks { get; set; }
        public decimal Points { get; set; }
        public string Description { get; set; }
    }
}