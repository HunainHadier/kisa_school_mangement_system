namespace KisaSchoolMangement.Models
{
    public class ExamSubjectModel
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public int SubjectId { get; set; }
        public decimal MaxMarks { get; set; }
        public decimal PassMarks { get; set; }

        // For display
        public string SubjectName { get; set; }
        public string SubjectCode { get; set; }
        public string ExamName { get; set; }
    }
}