using System;

namespace KisaSchoolMangement.Models
{
    public class StudentModel
    {
        // Basic Information
        public int Id { get; set; }
        public string GrNo { get; set; }
        public string StudentName { get; set; }
        public string DOB { get; set; }
        public string Gender { get; set; }
        public int? ClassId { get; set; }
        public int? SectionId { get; set; }
        public string AdmissionDate { get; set; }
        public string Photo { get; set; }
        public string GuardianName { get; set; }
        public string GuardianPhone { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // Family Information
        public int FamilyCode { get; set; }
        public int DistrictCode { get; set; }
        public string FatherName { get; set; }
        public bool IsSyed { get; set; }
        public int FamilyMembers { get; set; }
        public int ChildPosition { get; set; }
        public string StudentCategory { get; set; }
        public int AdmissionYear { get; set; }
        public string AdmissionClass { get; set; }

        // CNIC Information
        public string ChildCNICNumber { get; set; }
        public string CNICImage { get; set; }
        public string FatherCNIC { get; set; }
        public string FatherCNICPhoto { get; set; }
        public string FatherOccupation { get; set; }
        public decimal? FatherMonthlyIncome { get; set; }

        // Mother Information
        public string MotherName { get; set; }
        public string MotherCNICNumber { get; set; }
        public string MotherCNICPhoto { get; set; }
        public decimal? MotherMonthlyIncome { get; set; }

        // Residential Information
        public string HomeStatus { get; set; }
        public string ReasonOfScholarship { get; set; }
        public decimal? MonthlyFee { get; set; }

        // Contact Information
        public string MotherPhone { get; set; }
        public string MotherWhatsapp { get; set; }

        // Leaving Information
        public string LeftMonth { get; set; }
        public string ReasonOfSchoolLeft { get; set; }
        public string LeftDate { get; set; }
        public string Status { get; set; }

        // For display purposes
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string StudentPhotoPath { get; set; }
        public string CnicPhotoPath { get; set; }


        // Compatibility properties
        public string FullName => StudentName;
        public string Name => StudentName;
        public string RollNumber => GrNo;

        public string Age
        {
            get
            {
                if (DateTime.TryParse(DOB, out DateTime dob))
                {
                    int age = DateTime.Today.Year - dob.Year;
                    if (DateTime.Today < dob.AddYears(age)) age--;
                    return age.ToString();
                }
                return "N/A";
            }
        }

        public string ActiveStatus => IsActive ? "Active" : "Inactive";

        // Additional properties for better data binding
        public string IsSyedDisplay => IsSyed ? "Yes" : "No";
        public string FatherIncomeDisplay => FatherMonthlyIncome.HasValue ? FatherMonthlyIncome.Value.ToString("N2") : "N/A";
        public string MotherIncomeDisplay => MotherMonthlyIncome.HasValue ? MotherMonthlyIncome.Value.ToString("N2") : "N/A";
        public string MonthlyFeeDisplay => MonthlyFee.HasValue ? MonthlyFee.Value.ToString("N2") : "N/A";
    }
}