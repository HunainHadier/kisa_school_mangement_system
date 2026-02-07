using System;

namespace KisaSchoolMangement.Models
{
    public class TeacherSubjectAssignment
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public int SubjectId { get; set; }
        public int ClassId { get; set; }
        public string AcademicYear { get; set; } = "2024-2025";
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public string TeacherName { get; set; }
        public string SubjectName { get; set; }
        public string ClassName { get; set; }
        public string SubjectCode { get; set; }
    }
}