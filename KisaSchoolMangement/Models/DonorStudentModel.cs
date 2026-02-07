////using System;

////namespace KisaSchoolMangement.Models
////{
////    public class DonorStudentModel
////    {
////        public int StudentId { get; set; }
////        public int DonorId { get; set; }
////        public decimal Amount { get; set; }
////        public string StartDate { get; set; }
////        public string EndDate { get; set; }
////        public string Note { get; set; }

////        // Navigation properties (optional)
////        public string StudentName { get; set; }
////        public string DonorName { get; set; }
////        public string StudentRollNumber { get; set; }
////        public string StudentClassName { get; set; }
////    }
////}

//using System;

//namespace KisaSchoolMangement.Models
//{
//    public class DonorStudentModel
//    {
//        public int StudentId { get; set; }
//        public int DonorId { get; set; }
//        public decimal Amount { get; set; }
//        public string StartDate { get; set; }
//        public string EndDate { get; set; }
//        public string Note { get; set; }

//        // Student Details
//        public string StudentName { get; set; }
//        public string StudentAdmissionNo { get; set; }
//        public string StudentClassName { get; set; }
//        public string StudentSectionName { get; set; }
//        public string StudentGuardianName { get; set; }
//        public string StudentGuardianPhone { get; set; }
//        public string StudentDOB { get; set; }
//        public string StudentGender { get; set; }

//        // Donor Details
//        public string DonorName { get; set; }
//        public string DonorContactPerson { get; set; }
//        public string DonorEmail { get; set; }
//        public string DonorPhone { get; set; }
//        public string DonorAddress { get; set; }
//        public string DonorNotes { get; set; }

//        // Calculated Properties
//        public string Duration
//        {
//            get
//            {
//                if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
//                    return $"{StartDate} to {EndDate}";
//                return StartDate ?? "N/A";
//            }
//        }

//        public string Status
//        {
//            get
//            {
//                if (string.IsNullOrEmpty(EndDate)) return "Active";

//                if (DateTime.TryParse(EndDate, out DateTime endDate))
//                {
//                    return endDate >= DateTime.Today ? "Active" : "Expired";
//                }
//                return "Active";
//            }
//        }

//        public string AmountFormatted => $"{Amount:C}";
//    }
//}