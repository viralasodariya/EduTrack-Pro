using System.Data;
using Microsoft.AspNetCore.Http;
using MyProject.Core.Interface;
using MyProject.Core.Models;
using Npgsql;
using NpgsqlTypes;

namespace MyProject.Core.Implementation
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly NpgsqlConnection _conn;

        public TeacherRepository(NpgsqlConnection conn)
        {
            _conn = conn;
        }


        public async Task<int> GetTeacherIdByUserId(int userId)
        {
            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                    await _conn.OpenAsync();

                var query = "SELECT c_teacher_id FROM t_teachers WHERE c_user_id = @userId";

                using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@userId", userId);

                var result = await cmd.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching teacher ID: {ex.Message}");
                return 0;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }
        public async Task<bool> AddTeacherProfile(int userId, TeacherProfileRequest model)
        {
            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                    await _conn.OpenAsync();

                var query = @"
                INSERT INTO t_teachers (c_user_id, c_full_name, c_phone_number, c_date_of_birth, 
                                        c_qualification, c_experience_years, c_created_at)
                VALUES (@userId, @fullName, @phoneNumber, @dateOfBirth, @qualification, @experienceYears, NOW())";

                using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@fullName", model.FullName);
                cmd.Parameters.AddWithValue("@phoneNumber", (object?)model.PhoneNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dateOfBirth", model.DateOfBirth);
                cmd.Parameters.AddWithValue("@qualification", model.Qualification);
                cmd.Parameters.AddWithValue("@experienceYears", model.ExperienceYears);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding profile: {ex.Message}");
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return false;
        }
        public async Task<bool> UpdateTeacherProfile(int userId, TeacherProfileRequest model)
        {
            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                    await _conn.OpenAsync();

                var query = @"
                UPDATE t_teachers
                SET c_full_name = @fullName, 
                    c_phone_number = @phoneNumber, 
                    c_date_of_birth = @dateOfBirth, 
                    c_qualification = @qualification, 
                    c_experience_years = @experienceYears
                WHERE c_user_id = @userId";

                using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@fullName", model.FullName);
                cmd.Parameters.AddWithValue("@phoneNumber", (object?)model.PhoneNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dateOfBirth", model.DateOfBirth);
                cmd.Parameters.AddWithValue("@qualification", model.Qualification);
                cmd.Parameters.AddWithValue("@experienceYears", model.ExperienceYears);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating profile: {ex.Message}");
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return false;
        }
        public async Task<object?> GetTeacherProfile(int userId)
        {
            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                    await _conn.OpenAsync();

                var query = @"
            SELECT c_teacher_id, c_full_name, c_phone_number, c_date_of_birth, 
                   c_qualification, c_experience_years
            FROM t_teachers 
            WHERE c_user_id = @userId";

                using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@userId", userId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new
                    {
                        TeacherId = reader.GetInt32(0),
                        FullName = reader.GetString(1),
                        PhoneNumber = reader.IsDBNull(2) ? null : reader.GetString(2),
                        DateOfBirth = reader.GetDateTime(3),
                        Qualification = reader.GetString(4),
                        ExperienceYears = reader.GetInt32(5)
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching profile: {ex.Message}");
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return null;
        }
        public async Task<List<object>> GetAssignedSubjects(int teacherId)
        {
            var subjectList = new List<object>();

            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                    await _conn.OpenAsync();

                var query = @"
            SELECT 
                s.c_subject_id, 
                s.c_subject_name, 
                ts.c_start_date, 
                ts.c_end_date, 
                ts.c_completion_percentage,
                ts.c_assignment_id
            FROM t_teacher_subjects ts
            JOIN t_subjects s ON ts.c_subject_id = s.c_subject_id
            WHERE ts.c_teacher_id = @teacherId";

                using (var cmd = new NpgsqlCommand(query, _conn))
                {
                    cmd.Parameters.AddWithValue("@teacherId", teacherId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var subject = new
                            {
                                SubjectId = reader.GetInt32(0),
                                SubjectName = reader.GetString(1),
                                StartDate = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
                                EndDate = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                                CompletionPercentage = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4),
                                AssignmentId = reader.GetInt32(5)
                            };

                            subjectList.Add(subject);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching subjects: {ex.Message}");
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return subjectList;
        }

        public async Task<object?> GetAssignedSubjectById(int assignmentId)
        {
            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                    await _conn.OpenAsync();

                string query = @"
            SELECT c_assignment_id, c_teacher_id, c_subject_id, c_start_date, c_end_date, c_completion_percentage 
            FROM t_teacher_subjects 
            WHERE c_assignment_id = @assignmentId";

                using (var cmd = new NpgsqlCommand(query, _conn))
                {
                    cmd.Parameters.AddWithValue("@assignmentId", assignmentId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new
                            {
                                AssignmentId = reader.GetInt32(0),
                                TeacherId = reader.GetInt32(1),
                                SubjectId = reader.GetInt32(2),
                                StartDate = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                                EndDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                CompletionPercentage = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching assignment: {ex.Message}");
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return null;
        }

        public async Task<(bool success, string message)> UpdateSubjectProgress(UpdateSubjectProgressRequest request)
        {
            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                    await _conn.OpenAsync();

                using (var cmd = new NpgsqlCommand(@"
            UPDATE t_teacher_subjects
            SET c_start_date = @StartDate, 
                c_end_date = @EndDate, 
                c_completion_percentage = @CompletionPercentage
            WHERE c_assignment_id = @AssignmentId", _conn))
                {
                    cmd.Parameters.AddWithValue("@StartDate", (object?)request.StartDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EndDate", (object?)request.EndDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CompletionPercentage", request.CompletionPercentage);
                    cmd.Parameters.AddWithValue("@AssignmentId", request.AssignmentId);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                        return (true, "Progress updated successfully.");
                    else
                        return (false, "No records were updated. Invalid assignment ID.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in updating subject progress: " + ex.Message);
                return (false, "Error occurred while updating progress.");
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<List<object>> GetAssignedStudentsByTeacherId(int teacherId)
        {
            var studentList = new List<object>();

            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }

                var query = @"
        SELECT 
            s.c_student_id, 
            s.c_full_name, 
            s.c_date_of_birth, 
            s.c_gender, 
            s.c_class_id, 
            c.c_class_name, 
            s.c_enrollment_date, 
            s.c_profile_picture, 
            s.c_status, 
            s.c_created_at, 
            s.c_user_id
        FROM t_student_teacher st
        INNER JOIN t_students s ON st.c_student_id = s.c_student_id
        LEFT JOIN t_classes c ON s.c_class_id = c.c_class_id
        WHERE st.c_teacher_id = @teacherId;";

                using (var cmd = new NpgsqlCommand(query, _conn))
                {
                    cmd.Parameters.AddWithValue("@teacherId", NpgsqlDbType.Integer, teacherId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            studentList.Add(new
                            {
                                StudentId = reader.GetInt32(0),
                                FullName = reader.GetString(1),
                                DateOfBirth = reader.IsDBNull(2) ? (DateTime?)null : reader.GetFieldValue<DateTime>(2),
                                Gender = reader.GetString(3),
                                ClassId = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                                ClassName = reader.IsDBNull(5) ? "Unassigned" : reader.GetString(5),
                                EnrollmentDate = reader.IsDBNull(6) ? (DateTime?)null : reader.GetFieldValue<DateTime>(6),
                                ProfilePicture = reader.IsDBNull(7) ? null : reader.GetString(7),
                                Status = reader.GetBoolean(8),
                                CreatedAt = reader.IsDBNull(9) ? (DateTime?)null : reader.GetFieldValue<DateTime>(9),
                                UserId = reader.GetInt32(10)

                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching assigned students: {ex.Message}");
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return studentList;
        }

        public async Task<(bool success, string message, List<object> data)> GetUpcomingClasses(int teacherId)
        {
            List<object> schedules = new List<object>();

            try
            {
                if (_conn.State == ConnectionState.Closed)
                    await _conn.OpenAsync();

                string query = @"
            SELECT t.c_timetable_id, c.c_class_name, s.c_subject_name, t.c_time_slot, t.c_day_of_week
            FROM t_timetable t
            JOIN t_classes c ON t.c_class_id = c.c_class_id
            JOIN t_subjects s ON t.c_subject_id = s.c_subject_id
            WHERE t.c_teacher_id = @TeacherId
            ORDER BY t.c_day_of_week, t.c_time_slot";

                using (var cmd = new NpgsqlCommand(query, _conn))
                {
                    cmd.Parameters.AddWithValue("@TeacherId", teacherId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            schedules.Add(new
                            {
                                TimetableId = reader.GetInt32(0),
                                ClassName = reader.GetString(1),
                                SubjectName = reader.GetString(2),
                                TimeSlot = reader.GetTimeSpan(3),
                                DayOfWeek = reader.GetString(4)
                            });
                        }
                    }
                }

                if (schedules.Count == 0)
                {
                    return (false, "No upcoming classes found.", schedules);
                }

                return (true, "Upcoming classes retrieved successfully.", schedules);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching upcoming classes: {ex.Message}");
                return (false, "Error retrieving upcoming classes.", new List<object>());
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<(bool success, string message)> UploadMaterial(int teacherId, int subjectId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return (false, "Invalid file. Please select a valid file.");
                }

                if (_conn.State == ConnectionState.Closed)
                    await _conn.OpenAsync();

                // Allowed file types
                var allowedExtensions = new List<string> { ".pdf", ".docx", ".pptx", ".xlsx" };
                string fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return (false, "Invalid file format. Allowed formats: PDF, DOCX, PPTX, XLSX.");
                }

                // **Find the path to the MVC project's `wwwroot/materials/` folder**
                string mvcProjectPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "MyProject.MVC");
                string uploadsFolder = Path.Combine(mvcProjectPath, "wwwroot", "materials");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // **Save the file in the MVC project's folder**
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // **Store the relative file path in the database**
                string relativeFilePath = $"/materials/{uniqueFileName}";

                string insertQuery = @"
            INSERT INTO t_materials (c_teacher_id, c_subject_id, c_file_name, c_file_type, c_upload_date, c_file_path) 
            VALUES (@TeacherId, @SubjectId, @FileName, @FileType, CURRENT_DATE, @FilePath)";

                using (var cmd = new NpgsqlCommand(insertQuery, _conn))
                {
                    cmd.Parameters.AddWithValue("@TeacherId", teacherId);
                    cmd.Parameters.AddWithValue("@SubjectId", subjectId);
                    cmd.Parameters.AddWithValue("@FileName", file.FileName);
                    cmd.Parameters.AddWithValue("@FileType", fileExtension);
                    cmd.Parameters.AddWithValue("@FilePath", relativeFilePath);

                    await cmd.ExecuteNonQueryAsync();
                }

                // Create an entry in the notification table
                string notificationQuery = @"
                    INSERT INTO t_notifications_material (c_subject_id, c_message) 
                    VALUES (@SubjectId, @Message)";

                using (var notifyCmd = new NpgsqlCommand(notificationQuery, _conn))
                {
                    notifyCmd.Parameters.AddWithValue("@SubjectId", subjectId);
                    notifyCmd.Parameters.AddWithValue("@Message", $"New material for {file.FileName} has been uploaded");
                    await notifyCmd.ExecuteNonQueryAsync();
                }


                return (true, "File uploaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
                return (false, "Error uploading file.");
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<(bool success, string message, List<object> materials)> GetMaterialsByTeacher(int teacherId)
        {
            try
            {
                if (_conn.State == ConnectionState.Closed)
                    await _conn.OpenAsync();

                string query = @"
            SELECT m.c_material_id, m.c_subject_id, m.c_file_name, m.c_file_type, m.c_upload_date, m.c_file_path, s.c_subject_name 
            FROM t_materials m
            JOIN t_subjects s ON m.c_subject_id = s.c_subject_id
            WHERE c_teacher_id = @TeacherId";

                using (var cmd = new NpgsqlCommand(query, _conn))
                {
                    cmd.Parameters.AddWithValue("@TeacherId", teacherId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        var materials = new List<object>();
                        while (await reader.ReadAsync())
                        {
                            materials.Add(new
                            {
                                MaterialId = reader.GetInt32(0),
                                SubjectId = reader.GetInt32(1),
                                FileName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                FileType = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                UploadDate = reader.IsDBNull(4) ? "" : reader.GetDateTime(4).ToString("yyyy-MM-dd"),
                                FilePath = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                SubjectName = reader.IsDBNull(6) ? "" : reader.GetString(6)
                            });
                        }

                        return (true, "Materials retrieved successfully!", materials);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching materials: {ex.Message}");
                return (false, "Error fetching materials.", new List<object>());
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }
        public async Task<(bool success, string message)> DeleteMaterial(int materialId)
        {
            try
            {
                if (_conn.State == ConnectionState.Closed)
                    await _conn.OpenAsync();

                string filePath = null;

                // Retrieve file path from database before deleting
                string selectQuery = "SELECT c_file_path FROM t_materials WHERE c_material_id = @MaterialId";
                using (var cmd = new NpgsqlCommand(selectQuery, _conn))
                {
                    cmd.Parameters.AddWithValue("@MaterialId", materialId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            filePath = reader.IsDBNull(0) ? null : reader.GetString(0);
                        }
                    }
                }

                if (string.IsNullOrEmpty(filePath))
                {
                    return (false, "Material not found.");
                }

                // **Find the path to the MVC project's `wwwroot/materials/` folder**
                string mvcProjectPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "MyProject.MVC");
                string fullPath = Path.Combine(mvcProjectPath, "wwwroot", filePath.TrimStart('/'));

                // Delete the file from storage
                if (File.Exists(fullPath))
                {
                    try
                    {
                        File.Delete(fullPath);
                    }
                    catch (Exception fileEx)
                    {
                        Console.WriteLine($"Error deleting file: {fileEx.Message}");
                        return (false, "Failed to delete file from storage.");
                    }
                }

                // Delete entry from database
                string deleteQuery = "DELETE FROM t_materials WHERE c_material_id = @MaterialId";
                using (var cmd = new NpgsqlCommand(deleteQuery, _conn))
                {
                    cmd.Parameters.AddWithValue("@MaterialId", materialId);
                    await cmd.ExecuteNonQueryAsync();
                }

                return (true, "Material deleted successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting material: {ex.Message}");
                return (false, "Error deleting material.");
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }


        public async Task<List<object>> GetTeacherSubjectProgress(int teacherId)
        {
            var progressList = new List<object>();

            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }

                var query = @"
        SELECT 
            ts.c_assignment_id,
            s.c_subject_id, 
            s.c_subject_name, 
            COALESCE(ts.c_completion_percentage, 0) AS completion_percentage, 
            t.c_teacher_id, 
            t.c_full_name AS teacher_name
        FROM t_teacher_subjects ts
        INNER JOIN t_subjects s ON ts.c_subject_id = s.c_subject_id
        INNER JOIN t_teachers t ON ts.c_teacher_id = t.c_teacher_id
        WHERE ts.c_teacher_id = @teacherId";

                using (var cmd = new NpgsqlCommand(query, _conn))
                {
                    cmd.Parameters.AddWithValue("@teacherId", NpgsqlDbType.Integer, teacherId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            progressList.Add(new
                            {
                                AssignmentId = reader.GetInt32(0),  // Unique assignment ID
                                SubjectId = reader.GetInt32(1),
                                SubjectName = reader.GetString(2),
                                CompletionPercentage = reader.GetDecimal(3),
                                TeacherId = reader.GetInt32(4),
                                TeacherName = reader.GetString(5)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching progress: {ex.Message}");
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return progressList;
        }


        public async Task<(bool success,string message, string className)> GetTeacherClass(int teacherId)
        {
            try
            {
                if (_conn.State == ConnectionState.Closed)
                    await _conn.OpenAsync();

                string query = @"
            SELECT c.c_class_name
            FROM t_classes c
            JOIN t_class_teachers t ON c.c_class_id = t.c_class_id
            WHERE t.c_teacher_id = @TeacherId";

                using (var cmd = new NpgsqlCommand(query, _conn))
                {
                    cmd.Parameters.AddWithValue("@TeacherId", teacherId);
                    var className = await cmd.ExecuteScalarAsync();
                    return (true, "Class retrieved successfully.", className.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching class: {ex.Message}");
                return (false, "Error fetching class.", "");
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }



    }

}