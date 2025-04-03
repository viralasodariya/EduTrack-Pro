using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MyProject.Core.Interface;
using MyProject.Core.Models;
using Npgsql;

namespace MyProject.Core.Implementation
{
    public class StudentServices : IStudentServices
    {
        private readonly NpgsqlConnection _connection;
        public StudentServices(NpgsqlConnection npgsqlConnection)
        {
            _connection = npgsqlConnection;
        }


        public async Task<int> GetStudentIdByUserId(int userId)
        {
            try
            {
                if (_connection.State == System.Data.ConnectionState.Closed)
                    await _connection.OpenAsync();

                var query = "SELECT c_student_id FROM t_students WHERE c_user_id = @userId";

                using var cmd = new NpgsqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@userId", userId);

                var result = await cmd.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching student ID: {ex.Message}");
                return 0;
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }

        public async Task<(bool success, string message, object)> GetStudentByIdAsync(int studentId)
        {
            try
            {
                if (_connection.State == System.Data.ConnectionState.Closed)
                    await _connection.OpenAsync();

                var query = @"
            SELECT s.*, c.c_class_name, u.c_email
            FROM t_students s
            LEFT JOIN t_classes c ON s.c_class_id = c.c_class_id
            LEFT JOIN t_users u ON s.c_user_id = u.c_user_id
            WHERE s.c_student_id = @StudentId";

                using var cmd = new NpgsqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (!reader.HasRows)
                {
                    return (false, "Student not found.", null);
                }

                await reader.ReadAsync();

                var student = new
                {
                    StudentId = reader.GetInt32(0),  // c_student_id
                    FullName = reader.GetString(1), // c_full_name
                    DateOfBirth = reader.GetDateTime(2), // c_date_of_birth
                    Gender = reader.GetString(3), // c_gender
                    ClassId = !reader.IsDBNull(4) ? reader.GetInt32(4) : (int?)null, // c_class_id (nullable)
                    EnrollmentDate = reader.GetDateTime(5), // c_enrollment_date
                    ProfilePicture = !reader.IsDBNull(6) ? reader.GetString(6) : null, // c_profile_picture (nullable)
                    Status = reader.GetBoolean(7), // c_status
                    CreatedAt = reader.GetDateTime(8), // c_created_at
                    UserId = reader.GetInt32(9), // c_user_id
                    ClassName = !reader.IsDBNull(10) ? reader.GetString(10) : "Not Assigned", // c_class_name (nullable)
                    Email = !reader.IsDBNull(11) ? reader.GetString(11) : "No Email" // c_email (nullable)
                };

                return (true, "Student retrieved successfully.", student);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching student details: {ex.Message}");
                return (false, "Error occurred while fetching student details.", null);
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }

        public async Task<int> GetStudentClassIdByUserId(int userId)
        {
            try
            {
                if (_connection.State == System.Data.ConnectionState.Closed)
                    await _connection.OpenAsync();

                var query = "SELECT c_class_id FROM t_students WHERE c_user_id = @UserId";

                using var cmd = new NpgsqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@UserId", userId);

                var result = await cmd.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching class ID: {ex.Message}");
                return 0;
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }
        public async Task<object> GetClassTeacherByClassId(int classId)
        {
            try
            {
                if (_connection.State == System.Data.ConnectionState.Closed)
                    await _connection.OpenAsync();

                var query = @"
        SELECT 
            t.c_teacher_id, 
            t.c_full_name, 
            t.c_phone_number, 
            t.c_date_of_birth, 
            t.c_qualification, 
            t.c_experience_years, 
            u.c_email
        FROM t_teachers t
        JOIN t_users u ON t.c_user_id = u.c_user_id
        JOIN t_class_teachers ct ON ct.c_teacher_id = t.c_teacher_id
        WHERE ct.c_class_id = @ClassId";

                using var cmd = new NpgsqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@ClassId", classId);

                using var reader = await cmd.ExecuteReaderAsync();

                if (!reader.HasRows)
                    return null;

                await reader.ReadAsync();

                var teacher = new
                {
                    TeacherId = reader.GetInt32(0),  // c_teacher_id
                    FullName = reader.GetString(1),  // c_full_name
                    PhoneNumber = !reader.IsDBNull(2) ? reader.GetString(2) : null, // c_phone_number (nullable)
                    DateOfBirth = reader.GetDateTime(3), // c_date_of_birth
                    Qualification = reader.GetString(4), // c_qualification
                    ExperienceYears = reader.GetInt32(5), // c_experience_years
                    Email = reader.GetString(6) // c_email
                };

                return teacher;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching class teacher: {ex.Message}");
                return null;
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }


        public async Task<(bool success, string message)> EditStudentProfileAsync(EditStudentDto model)
        {
            try
            {
                if (_connection.State == System.Data.ConnectionState.Closed)
                    await _connection.OpenAsync();

                var query = @"
            UPDATE t_students
            SET 
                c_full_name = @FullName, 
                c_date_of_birth = @DateOfBirth, 
                c_gender = @Gender, 
                c_profile_picture = @ProfilePicture
            WHERE c_student_id = @StudentId";

                using var cmd = new NpgsqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@FullName", model.FullName);
                cmd.Parameters.AddWithValue("@DateOfBirth", model.DateOfBirth);
                cmd.Parameters.AddWithValue("@Gender", model.Gender);
                cmd.Parameters.AddWithValue("@ProfilePicture", (object)model.ProfilePicture ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@StudentId", model.StudentId);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                    return (false, "Student not found or no changes made.");

                return (true, "Student profile updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating student profile: {ex.Message}");
                return (false, "Error occurred while updating student profile.");
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }

        public async Task<(bool success, string message)> AddGuardianAsync(int studentId, AddGuardianDto guardianDto)
        {
            try
            {
                if (_connection.State == System.Data.ConnectionState.Closed)
                    await _connection.OpenAsync();

                // Validation: Check if a father or mother already exists
                if (guardianDto.Relationship == "Father" || guardianDto.Relationship == "Mother")
                {
                    var checkQuery = @"
                SELECT COUNT(*) FROM t_guardians 
                WHERE c_student_id = @StudentId AND c_relationship = @Relationship";

                    using var checkCmd = new NpgsqlCommand(checkQuery, _connection);
                    checkCmd.Parameters.AddWithValue("@StudentId", studentId);
                    checkCmd.Parameters.AddWithValue("@Relationship", guardianDto.Relationship);

                    int existingCount = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                    if (existingCount > 0)
                    {
                        return (false, $"{guardianDto.Relationship} already exists for this student.");
                    }
                }

                // Insert the new guardian
                var query = @"
            INSERT INTO t_guardians (c_student_id, c_full_name, c_relationship, c_phone_number, c_email, c_address) 
            VALUES (@StudentId, @FullName, @Relationship, @PhoneNumber, @Email, @Address)";

                using var cmd = new NpgsqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@StudentId", studentId);
                cmd.Parameters.AddWithValue("@FullName", guardianDto.FullName);
                cmd.Parameters.AddWithValue("@Relationship", guardianDto.Relationship);
                cmd.Parameters.AddWithValue("@PhoneNumber", guardianDto.PhoneNumber);
                cmd.Parameters.AddWithValue("@Email", (object)guardianDto.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object)guardianDto.Address ?? DBNull.Value);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0 ? (true, "Guardian added successfully.") : (false, "Failed to add guardian.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding guardian: {ex.Message}");
                return (false, "An error occurred while adding guardian.");
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }

        public async Task<(bool success, string message, object guardians)> GetGuardiansByStudentIdAsync(int studentId)
        {
            try
            {
                if (_connection.State == System.Data.ConnectionState.Closed)
                    await _connection.OpenAsync();

                var query = @"
            SELECT c_guardian_id, c_full_name, c_relationship, c_phone_number, c_email, c_address
            FROM t_guardians
            WHERE c_student_id = @StudentId";

                using var cmd = new NpgsqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                using var reader = await cmd.ExecuteReaderAsync();

                var guardians = new List<object>();

                while (await reader.ReadAsync())
                {
                    guardians.Add(new
                    {
                        GuardianId = reader.GetInt32(0),
                        FullName = reader.GetString(1),
                        Relationship = reader.GetString(2),
                        PhoneNumber = !reader.IsDBNull(3) ? reader.GetString(3) : null,
                        Email = !reader.IsDBNull(4) ? reader.GetString(4) : null,
                        Address = !reader.IsDBNull(5) ? reader.GetString(5) : null
                    });
                }

                if (guardians.Count == 0)
                    return (false, "No guardians found for this student.", null);

                return (true, "Guardians retrieved successfully.", guardians);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching guardians: {ex.Message}");
                return (false, "An error occurred while fetching guardians.", null);
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }

        public async Task<(bool success, string message)> UpdateGuardianAsync(int guardianId, AddGuardianDto guardianDto)
        {
            try
            {
                if (_connection.State == System.Data.ConnectionState.Closed)
                    await _connection.OpenAsync();

                // Update the guardian's details
                var query = @"
            UPDATE t_guardians
            SET c_full_name = @FullName, 
                c_relationship = @Relationship, 
                c_phone_number = @PhoneNumber, 
                c_email = @Email, 
                c_address = @Address
            WHERE c_guardian_id = @GuardianId";

                using var cmd = new NpgsqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@GuardianId", guardianId);
                cmd.Parameters.AddWithValue("@FullName", guardianDto.FullName);
                cmd.Parameters.AddWithValue("@Relationship", guardianDto.Relationship);
                cmd.Parameters.AddWithValue("@PhoneNumber", guardianDto.PhoneNumber);
                cmd.Parameters.AddWithValue("@Email", (object)guardianDto.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object)guardianDto.Address ?? DBNull.Value);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0 ? (true, "Guardian updated successfully.") : (false, "No changes made to the guardian.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating guardian: {ex.Message}");
                return (false, "An error occurred while updating guardian.");
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }

        public async Task<List<object>> GetTeachersForFeedbackAsync(int classId)
        {
            try
            {
                if (_connection.State == System.Data.ConnectionState.Closed)
                    await _connection.OpenAsync();

                var query = @"
        SELECT 
            tt.c_teacher_id, 
            te.c_full_name AS teacher_name, 
            tt.c_subject_id, 
            s.c_subject_name
        FROM t_timetable tt
        JOIN t_teachers te ON tt.c_teacher_id = te.c_teacher_id
        JOIN t_subjects s ON tt.c_subject_id = s.c_subject_id
        WHERE tt.c_class_id = @ClassId
        ORDER BY tt.c_teacher_id;";

                using var cmd = new NpgsqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@ClassId", classId);

                using var reader = await cmd.ExecuteReaderAsync();

                var feedbackTeachers = new List<object>();

                while (await reader.ReadAsync())
                {
                    feedbackTeachers.Add(new
                    {
                        TeacherId = reader.GetInt32(0),
                        TeacherName = reader.GetString(1),
                        SubjectId = reader.GetInt32(2),
                        SubjectName = reader.GetString(3)
                    });
                }

                return feedbackTeachers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching teachers for feedback: {ex.Message}");
                return new List<object>();
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }


        public async Task<(bool success, string message)> SubmitTeacherFeedbackAsync(int studentId, SubmitFeedbackDto feedbackDto)
        {
            try
            {
                if (_connection.State == ConnectionState.Closed)
                    await _connection.OpenAsync();

                using var transaction = await _connection.BeginTransactionAsync();

                // 1️⃣ Check if the student has already given feedback
                var checkQuery = @"
            SELECT COUNT(*) 
            FROM t_teacher_ratings 
            WHERE c_student_id = @StudentId AND c_teacher_id = @TeacherId";

                using var checkCmd = new NpgsqlCommand(checkQuery, _connection, (NpgsqlTransaction)transaction);
                checkCmd.Parameters.AddWithValue("@StudentId", studentId);
                checkCmd.Parameters.AddWithValue("@TeacherId", feedbackDto.TeacherId);

                int feedbackCount = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (feedbackCount > 0)
                {
                    await transaction.RollbackAsync();
                    return (false, "You have already submitted feedback for this teacher.");
                }

                // 2️⃣ Insert feedback into the database
                var insertQuery = @"
            INSERT INTO t_teacher_ratings 
            (c_student_id, c_teacher_id, c_experience_rating, c_feedback, c_rating_date) 
            VALUES (@StudentId, @TeacherId, @ExperienceRating, @Feedback, CURRENT_DATE)";

                using var insertCmd = new NpgsqlCommand(insertQuery, _connection, (NpgsqlTransaction)transaction);
                insertCmd.Parameters.AddWithValue("@StudentId", studentId);
                insertCmd.Parameters.AddWithValue("@TeacherId", feedbackDto.TeacherId);
                insertCmd.Parameters.AddWithValue("@ExperienceRating", feedbackDto.ExperienceRating);
                insertCmd.Parameters.AddWithValue("@Feedback", (object)feedbackDto.Feedback ?? DBNull.Value);

                int rowsAffected = await insertCmd.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    await transaction.CommitAsync();
                    return (true, "Feedback submitted successfully.");
                }

                await transaction.RollbackAsync();
                return (false, "Failed to submit feedback.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SubmitTeacherFeedbackAsync: {ex.Message}");
                return (false, "An error occurred while submitting feedback.");
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }

        public async Task<List<object>> GetMaterialNotificationsAsync(int subjectId = 0)
        {
            try
            {
                if (_connection.State == ConnectionState.Closed)
                    await _connection.OpenAsync();

                string query = @"
                    SELECT nm.c_notification_id, nm.c_subject_id, s.c_subject_name, 
                           nm.c_message, nm.c_created_at
                    FROM t_notifications_material nm
                    JOIN t_subjects s ON nm.c_subject_id = s.c_subject_id";

                // Add filter for specific subject if provided
                if (subjectId > 0)
                {
                    query += " WHERE nm.c_subject_id = @SubjectId";
                }

                query += " ORDER BY nm.c_created_at DESC";

                using var cmd = new NpgsqlCommand(query, _connection);

                if (subjectId > 0)
                {
                    cmd.Parameters.AddWithValue("@SubjectId", subjectId);
                }

                using var reader = await cmd.ExecuteReaderAsync();

                var notifications = new List<object>();

                while (await reader.ReadAsync())
                {
                    notifications.Add(new
                    {
                        NotificationId = reader.GetInt32(0),
                        SubjectId = reader.GetInt32(1),
                        SubjectName = reader.GetString(2),
                        Message = reader.GetString(3),
                        CreatedAt = reader.GetDateTime(4)
                    });
                }

                return notifications;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching material notifications: {ex.Message}");
                return new List<object>();
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }

        public async Task<List<object>> GetAllMaterial()
        {
            try
            {
                if (_connection.State == ConnectionState.Closed)
                    await _connection.OpenAsync();

                string query = @"
                        SELECT m.c_material_id, m.c_teacher_id, m.c_subject_id, m.c_file_name,
                               m.c_file_type, m.c_upload_date, m.c_file_path,
                               t.c_full_name AS teacher_name, s.c_subject_name
                        FROM t_materials m
                        JOIN t_teachers t ON m.c_teacher_id = t.c_teacher_id
                        JOIN t_subjects s ON m.c_subject_id = s.c_subject_id
                        ORDER BY m.c_upload_date DESC";

                using var cmd = new NpgsqlCommand(query, _connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var materials = new List<object>();

                while (await reader.ReadAsync())
                {
                    materials.Add(new
                    {
                        MaterialId = reader.GetInt32(0),
                        TeacherId = reader.GetInt32(1),
                        SubjectId = reader.GetInt32(2),
                        FileName = reader.GetString(3),
                        FileType = reader.GetString(4),
                        UploadDate = reader.GetDateTime(5),
                        FilePath = reader.GetString(6),
                        TeacherName = reader.GetString(7),
                        SubjectName = reader.GetString(8)
                    });
                }

                return materials;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching all materials: {ex.Message}");
                return new List<object>();
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }


    }
}