using System;

namespace KisaSchoolMangement.Models
{
    public class StudentDonorAssignment
    {
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string StudentPhone { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }

        public int DonorId { get; set; }
        public string DonorName { get; set; }
        public string DonorEmail { get; set; }
        public string DonorPhone { get; set; }
        public string DonationType { get; set; }
        public decimal DonationAmount { get; set; }

        public DateTime AssignedDate { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; } // Active, Inactive, Pending
    }
}