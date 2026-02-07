using System;

namespace KisaSchoolMangement.Models
{
    public class ExamModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ClassId { get; set; }
        public int ExamTypeId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string AcademicYear { get; set; }
        public bool IsActive { get; set; }
        public string CreatedAt { get; set; }

        // For display
        public string ClassName { get; set; }
        public string ExamTypeName { get; set; }
        public string Duration => $"{StartDate} to {EndDate}";
    }
}