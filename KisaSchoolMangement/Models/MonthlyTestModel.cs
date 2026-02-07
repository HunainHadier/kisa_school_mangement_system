namespace KisaSchoolMangement.Models
{
    public class MonthlyTestModel
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public int SubjectId { get; set; }
        public int Month { get; set; } // 1-12
        public string MonthName { get; set; }
        public decimal MaxMarks { get; set; }
        public decimal ObtainedMarks { get; set; }
        public DateTime TestDate { get; set; }

        // Navigation properties
        public string SubjectName { get; set; }
        public string ExamName { get; set; }
        public string StudentName { get; set; }
        public int StudentId { get; set; }
    }
}
