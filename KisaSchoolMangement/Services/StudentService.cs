using System;
using System.Collections.ObjectModel;
using System.Configuration;
using MySql.Data.MySqlClient;
using KisaSchoolMangement.Models;

namespace KisaSchoolMangement.Services
{
    public class StudentService
    {
        private readonly string _connectionString;

        public StudentService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["KisaSchoolDB"].ConnectionString;
        }

        // 🔹 Get all students - درست column names کے ساتھ
        public ObservableCollection<StudentModel> GetAllStudents()
        {
            var students = new ObservableCollection<StudentModel>();

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT s.*, c.name as class_name, sec.name as section_name 
                                    FROM students s
                                    LEFT JOIN classes c ON s.class_id = c.id
                                    LEFT JOIN sections sec ON s.section_id = sec.id
                                    WHERE s.is_deleted = 0
                                    ORDER BY s.id DESC";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            students.Add(new StudentModel
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                GrNo = reader["gr_no"] != DBNull.Value ? reader["gr_no"].ToString() : "",
                                StudentName = reader["student_name"] != DBNull.Value ? reader["student_name"].ToString() : "",
                                DOB = reader["dob"] != DBNull.Value ? Convert.ToDateTime(reader["dob"]).ToString("yyyy-MM-dd") : "",
                                Gender = reader["gender"] != DBNull.Value ? reader["gender"].ToString() : "Male",
                                ClassId = reader["class_id"] != DBNull.Value ? Convert.ToInt32(reader["class_id"]) : 0,
                                SectionId = reader["section_id"] != DBNull.Value ? Convert.ToInt32(reader["section_id"]) : 0,
                                AdmissionDate = reader["admission_date"] != DBNull.Value ? Convert.ToDateTime(reader["admission_date"]).ToString("yyyy-MM-dd") : "",
                                Photo = reader["photo"] != DBNull.Value ? reader["photo"].ToString() : "",
                                GuardianName = reader["guardian_name"] != DBNull.Value ? reader["guardian_name"].ToString() : "",
                                GuardianPhone = reader["guardian_phone"] != DBNull.Value ? reader["guardian_phone"].ToString() : "",
                                Address = reader["address"] != DBNull.Value ? reader["address"].ToString() : "",
                                IsActive = reader["is_active"] != DBNull.Value ? Convert.ToBoolean(reader["is_active"]) : true,
                                CreatedAt = reader["created_at"] != DBNull.Value ? Convert.ToDateTime(reader["created_at"]).ToString("yyyy-MM-dd HH:mm:ss") : "",
                                UpdatedAt = reader["updated_at"] != DBNull.Value ? Convert.ToDateTime(reader["updated_at"]).ToString("yyyy-MM-dd HH:mm:ss") : "",

                                // نئے fields - درست column names کے ساتھ
                                FamilyCode = reader["family_code"] != DBNull.Value ? Convert.ToInt32(reader["family_code"]) : 0,
                                DistrictCode = reader["district_code"] != DBNull.Value ? Convert.ToInt32(reader["district_code"]) : 0,
                                FatherName = reader["father_name"] != DBNull.Value ? reader["father_name"].ToString() : "",
                                IsSyed = reader["is_syed"] != DBNull.Value ? Convert.ToBoolean(reader["is_syed"]) : false,
                                FamilyMembers = reader["family_members"] != DBNull.Value ? Convert.ToInt32(reader["family_members"]) : 0,
                                ChildPosition = reader["child_position"] != DBNull.Value ? Convert.ToInt32(reader["child_position"]) : 0,
                                StudentCategory = reader["student_category"] != DBNull.Value ? reader["student_category"].ToString() : "",
                                AdmissionYear = reader["admission_year"] != DBNull.Value ? Convert.ToInt32(reader["admission_year"]) : DateTime.Now.Year,
                                AdmissionClass = reader["admission_class"] != DBNull.Value ? reader["admission_class"].ToString() : "",
                                Status = reader["status"] != DBNull.Value ? reader["status"].ToString() : "active",

                                // Additional fields from your database
                                ChildCNICNumber = reader["child_cnic_number"] != DBNull.Value ? reader["child_cnic_number"].ToString() : "",
                                CNICImage = reader["cnic_image"] != DBNull.Value ? reader["cnic_image"].ToString() : "",
                                FatherCNIC = reader["father_cnic"] != DBNull.Value ? reader["father_cnic"].ToString() : "",
                                FatherCNICPhoto = reader["father_cnic_photo"] != DBNull.Value ? reader["father_cnic_photo"].ToString() : "",
                                FatherOccupation = reader["father_occupation"] != DBNull.Value ? reader["father_occupation"].ToString() : "",
                                FatherMonthlyIncome = reader["father_monthly_income"] != DBNull.Value ? Convert.ToDecimal(reader["father_monthly_income"]) : (decimal?)null,
                                MotherName = reader["mother_name"] != DBNull.Value ? reader["mother_name"].ToString() : "",
                                MotherCNICNumber = reader["mother_cnic_number"] != DBNull.Value ? reader["mother_cnic_number"].ToString() : "",
                                MotherCNICPhoto = reader["mother_cnic_photo"] != DBNull.Value ? reader["mother_cnic_photo"].ToString() : "",
                                MotherMonthlyIncome = reader["mother_monthly_income"] != DBNull.Value ? Convert.ToDecimal(reader["mother_monthly_income"]) : (decimal?)null,
                                HomeStatus = reader["home_status"] != DBNull.Value ? reader["home_status"].ToString() : "",
                                ReasonOfScholarship = reader["reason_of_scholarship"] != DBNull.Value ? reader["reason_of_scholarship"].ToString() : "",
                                MonthlyFee = reader["monthly_fee"] != DBNull.Value ? Convert.ToDecimal(reader["monthly_fee"]) : (decimal?)null,
                                MotherPhone = reader["Mother_Phone"] != DBNull.Value ? reader["Mother_Phone"].ToString() : "",
                                MotherWhatsapp = reader["Mother_Whatsapp"] != DBNull.Value ? reader["Mother_Whatsapp"].ToString() : "",
                                LeftMonth = reader["left_month"] != DBNull.Value ? reader["left_month"].ToString() : "",
                                ReasonOfSchoolLeft = reader["reason_of_school_left"] != DBNull.Value ? reader["reason_of_school_left"].ToString() : "",
                                LeftDate = reader["left_date"] != DBNull.Value ? Convert.ToDateTime(reader["left_date"]).ToString("yyyy-MM-dd") : "",

                                ClassName = reader["class_name"] != DBNull.Value ? reader["class_name"].ToString() : "",
                                SectionName = reader["section_name"] != DBNull.Value ? reader["section_name"].ToString() : ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading students: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            return students;
        }


        // 🔹 Add student - تمام fields کے ساتھ
        public bool AddStudent(StudentModel student)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO students 
                            (gr_no, student_name, dob, gender, class_id, section_id, 
                             admission_date, admission_class, guardian_name, guardian_phone, address, is_active, 
                             family_code, district_code, father_name, is_syed, family_members, child_position,
                             student_category, admission_year, child_cnic_number, father_cnic, father_occupation, 
                             father_monthly_income, mother_name, mother_cnic_number, mother_monthly_income,
                             home_status, reason_of_scholarship, monthly_fee, Mother_Phone, Mother_Whatsapp,
                             status, created_at, updated_at) 
                            VALUES 
                            (@GrNo, @StudentName, @DOB, @Gender, @ClassId, @SectionId, 
                             @AdmissionDate, @AdmissionClass, @GuardianName, @GuardianPhone, @Address, @IsActive,
                             @FamilyCode, @DistrictCode, @FatherName, @IsSyed, @FamilyMembers, @ChildPosition,
                             @StudentCategory, @AdmissionYear, @ChildCNICNumber, @FatherCNIC, @FatherOccupation, 
                             @FatherMonthlyIncome, @MotherName, @MotherCNICNumber, @MotherMonthlyIncome,
                             @HomeStatus, @ReasonOfScholarship, @MonthlyFee, @MotherPhone, @MotherWhatsapp,
                             @Status, @CreatedAt, @UpdatedAt)";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        // بنیادی معلومات
                        cmd.Parameters.AddWithValue("@GrNo", student.GrNo ?? "");
                        cmd.Parameters.AddWithValue("@StudentName", student.StudentName ?? "");
                        cmd.Parameters.AddWithValue("@DOB", string.IsNullOrEmpty(student.DOB) ? DBNull.Value : (object)student.DOB);
                        cmd.Parameters.AddWithValue("@Gender", student.Gender ?? "Male");
                        cmd.Parameters.AddWithValue("@ClassId", student.ClassId.HasValue ? student.ClassId.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@SectionId", student.SectionId.HasValue ? student.SectionId.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@AdmissionDate", string.IsNullOrEmpty(student.AdmissionDate) ? DBNull.Value : (object)student.AdmissionDate);
                        cmd.Parameters.AddWithValue("@AdmissionClass", string.IsNullOrEmpty(student.AdmissionClass) ? DBNull.Value : (object)student.AdmissionClass);
                        cmd.Parameters.AddWithValue("@GuardianName", student.GuardianName ?? "");
                        cmd.Parameters.AddWithValue("@GuardianPhone", student.GuardianPhone ?? "");
                        cmd.Parameters.AddWithValue("@Address", student.Address ?? "");
                        cmd.Parameters.AddWithValue("@IsActive", student.IsActive);

                        // نئی معلومات
                        cmd.Parameters.AddWithValue("@FamilyCode", student.FamilyCode);
                        cmd.Parameters.AddWithValue("@DistrictCode", student.DistrictCode);
                        cmd.Parameters.AddWithValue("@FatherName", student.FatherName ?? "");
                        cmd.Parameters.AddWithValue("@IsSyed", student.IsSyed);
                        cmd.Parameters.AddWithValue("@FamilyMembers", student.FamilyMembers);
                        cmd.Parameters.AddWithValue("@ChildPosition", student.ChildPosition);
                        cmd.Parameters.AddWithValue("@StudentCategory", string.IsNullOrEmpty(student.StudentCategory) ? DBNull.Value : (object)student.StudentCategory);
                        cmd.Parameters.AddWithValue("@AdmissionYear", student.AdmissionYear);

                        // CNIC Information
                        cmd.Parameters.AddWithValue("@ChildCNICNumber", string.IsNullOrEmpty(student.ChildCNICNumber) ? DBNull.Value : (object)student.ChildCNICNumber);
                        cmd.Parameters.AddWithValue("@FatherCNIC", string.IsNullOrEmpty(student.FatherCNIC) ? DBNull.Value : (object)student.FatherCNIC);
                        cmd.Parameters.AddWithValue("@FatherOccupation", string.IsNullOrEmpty(student.FatherOccupation) ? DBNull.Value : (object)student.FatherOccupation);
                        cmd.Parameters.AddWithValue("@FatherMonthlyIncome", student.FatherMonthlyIncome.HasValue ? student.FatherMonthlyIncome.Value : DBNull.Value);

                        // Mother Information
                        cmd.Parameters.AddWithValue("@MotherName", string.IsNullOrEmpty(student.MotherName) ? DBNull.Value : (object)student.MotherName);
                        cmd.Parameters.AddWithValue("@MotherCNICNumber", string.IsNullOrEmpty(student.MotherCNICNumber) ? DBNull.Value : (object)student.MotherCNICNumber);
                        cmd.Parameters.AddWithValue("@MotherMonthlyIncome", student.MotherMonthlyIncome.HasValue ? student.MotherMonthlyIncome.Value : DBNull.Value);

                        // Residential Information
                        cmd.Parameters.AddWithValue("@HomeStatus", string.IsNullOrEmpty(student.HomeStatus) ? DBNull.Value : (object)student.HomeStatus);
                        cmd.Parameters.AddWithValue("@ReasonOfScholarship", string.IsNullOrEmpty(student.ReasonOfScholarship) ? DBNull.Value : (object)student.ReasonOfScholarship);
                        cmd.Parameters.AddWithValue("@MonthlyFee", student.MonthlyFee.HasValue ? student.MonthlyFee.Value : DBNull.Value);

                        // Contact Information
                        cmd.Parameters.AddWithValue("@MotherPhone", string.IsNullOrEmpty(student.MotherPhone) ? DBNull.Value : (object)student.MotherPhone);
                        cmd.Parameters.AddWithValue("@MotherWhatsapp", string.IsNullOrEmpty(student.MotherWhatsapp) ? DBNull.Value : (object)student.MotherWhatsapp);

                        // Status
                        cmd.Parameters.AddWithValue("@Status", student.Status ?? "active");

                        // Timestamps
                        cmd.Parameters.AddWithValue("@CreatedAt", student.CreatedAt ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@UpdatedAt", student.UpdatedAt ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error adding student: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        // 🔹 Update student - تمام fields کے ساتھ
        // 🔹 Update student - تمام fields کے ساتھ
        public bool UpdateStudent(StudentModel student)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"UPDATE students 
                    SET gr_no = @GrNo,
                        student_name = @StudentName,
                        dob = @DOB,
                        gender = @Gender,
                        class_id = @ClassId,
                        section_id = @SectionId,
                        admission_date = @AdmissionDate,
                        admission_class = @AdmissionClass,
                        guardian_name = @GuardianName,
                        guardian_phone = @GuardianPhone,
                        address = @Address,
                        is_active = @IsActive,
                        family_code = @FamilyCode,
                        district_code = @DistrictCode,
                        father_name = @FatherName,
                        is_syed = @IsSyed,
                        family_members = @FamilyMembers,
                        child_position = @ChildPosition,
                        student_category = @StudentCategory,
                        admission_year = @AdmissionYear,
                        child_cnic_number = @ChildCNICNumber,
                        father_cnic = @FatherCNIC,
                        father_occupation = @FatherOccupation,
                        father_monthly_income = @FatherMonthlyIncome,
                        mother_name = @MotherName,
                        mother_cnic_number = @MotherCNICNumber,
                        mother_monthly_income = @MotherMonthlyIncome,
                        home_status = @HomeStatus,
                        reason_of_scholarship = @ReasonOfScholarship,
                        monthly_fee = @MonthlyFee,
                        Mother_Phone = @MotherPhone,
                        Mother_Whatsapp = @MotherWhatsapp,
                        left_month = @LeftMonth,
                        reason_of_school_left = @LeftReason,
                        left_date = @LeftDate,
                        status = @Status,
                        updated_at = @UpdatedAt
                    WHERE id = @Id";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        // Your existing parameters...

                        // New Left Information parameters
                        cmd.Parameters.AddWithValue("@LeftMonth", string.IsNullOrEmpty(student.LeftMonth) ? DBNull.Value : (object)student.LeftMonth);
                        cmd.Parameters.AddWithValue("@LeftReason", string.IsNullOrEmpty(student.ReasonOfSchoolLeft) ? DBNull.Value : (object)student.ReasonOfSchoolLeft);
                        cmd.Parameters.AddWithValue("@LeftDate", string.IsNullOrEmpty(student.LeftDate) ? DBNull.Value : (object)student.LeftDate);
                        cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(student.Status) ? "active" : student.Status);

                        // Your existing code for other parameters...

                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error updating student: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }
        // 🔹 Delete student (soft delete)
        public bool DeleteStudent(int id)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "UPDATE students SET is_deleted = 1, updated_at = @UpdatedAt WHERE id = @Id";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error deleting student: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        // 🔹 Get student by ID
        public StudentModel GetStudentById(int id)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT s.*, c.name as class_name, sec.name as section_name 
                                    FROM students s
                                    LEFT JOIN classes c ON s.class_id = c.id
                                    LEFT JOIN sections sec ON s.section_id = sec.id
                                    WHERE s.id = @Id AND s.is_deleted = 0";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new StudentModel
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    GrNo = reader["gr_no"] != DBNull.Value ? reader["gr_no"].ToString() : "",
                                    StudentName = reader["student_name"] != DBNull.Value ? reader["student_name"].ToString() : "",
                                    DOB = reader["dob"] != DBNull.Value ? Convert.ToDateTime(reader["dob"]).ToString("yyyy-MM-dd") : "",
                                    Gender = reader["gender"] != DBNull.Value ? reader["gender"].ToString() : "Male",
                                    ClassId = reader["class_id"] != DBNull.Value ? Convert.ToInt32(reader["class_id"]) : 0,
                                    SectionId = reader["section_id"] != DBNull.Value ? Convert.ToInt32(reader["section_id"]) : 0,
                                    AdmissionDate = reader["admission_date"] != DBNull.Value ? Convert.ToDateTime(reader["admission_date"]).ToString("yyyy-MM-dd") : "",
                                    Photo = reader["photo"] != DBNull.Value ? reader["photo"].ToString() : "",
                                    GuardianName = reader["guardian_name"] != DBNull.Value ? reader["guardian_name"].ToString() : "",
                                    GuardianPhone = reader["guardian_phone"] != DBNull.Value ? reader["guardian_phone"].ToString() : "",
                                    Address = reader["address"] != DBNull.Value ? reader["address"].ToString() : "",
                                    IsActive = reader["is_active"] != DBNull.Value ? Convert.ToBoolean(reader["is_active"]) : true,

                                    FamilyCode = reader["family_code"] != DBNull.Value ? Convert.ToInt32(reader["family_code"]) : 0,
                                    DistrictCode = reader["district_code"] != DBNull.Value ? Convert.ToInt32(reader["district_code"]) : 0,
                                    FatherName = reader["father_name"] != DBNull.Value ? reader["father_name"].ToString() : "",
                                    IsSyed = reader["is_syed"] != DBNull.Value ? Convert.ToBoolean(reader["is_syed"]) : false,
                                    FamilyMembers = reader["family_members"] != DBNull.Value ? Convert.ToInt32(reader["family_members"]) : 0,
                                    ChildPosition = reader["child_position"] != DBNull.Value ? Convert.ToInt32(reader["child_position"]) : 0,
                                    StudentCategory = reader["student_category"] != DBNull.Value ? reader["student_category"].ToString() : "",
                                    AdmissionYear = reader["admission_year"] != DBNull.Value ? Convert.ToInt32(reader["admission_year"]) : DateTime.Now.Year,
                                    AdmissionClass = reader["admission_class"] != DBNull.Value ? reader["admission_class"].ToString() : "",
                                    Status = reader["status"] != DBNull.Value ? reader["status"].ToString() : "active",

                                    ClassName = reader["class_name"] != DBNull.Value ? reader["class_name"].ToString() : "",
                                    SectionName = reader["section_name"] != DBNull.Value ? reader["section_name"].ToString() : ""
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error getting student: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            return null;
        }
    }
}