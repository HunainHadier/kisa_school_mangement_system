namespace KisaSchoolMangement.Models
{
    public class StudentMarkModel
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int ExamId { get; set; }
        public int SubjectId { get; set; }
        public decimal MarksObtained { get; set; }
        public string Grade { get; set; }
        public string Remarks { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }

        // For display
        public string StudentName { get; set; }
        public string StudentAdmissionNo { get; set; }
        public string StudentClassName { get; set; }
        public string SubjectName { get; set; }
        public string ExamName { get; set; }
        public decimal MaxMarks { get; set; }
        public decimal PassMarks { get; set; }

        // ✅ Calculated properties (read-only) - YEH RAHNE DO
        public decimal Percentage => MaxMarks > 0 ? (MarksObtained / MaxMarks) * 100 : 0;
        public string Status => MarksObtained >= PassMarks ? "Pass" : "Fail";

        // ✅ NAYE METHODS ADD KAREIN JINSE HUM CALCULATIONS TRIGGER KAR SAKEIN
        public void CalculateResults()
        {
            // Yeh method automatically call hoga jab MarksObtained change hoga
            // Percentage aur Status automatically calculate ho jayenge
        }
    }
}