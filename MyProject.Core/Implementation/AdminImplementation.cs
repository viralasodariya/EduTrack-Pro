using System.Data;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MyProject.Core.Helper;
using MyProject.Core.Interface;
using MyProject.Core.Models;
using Npgsql;
using NpgsqlTypes;

namespace MyProject.Core.Implementation
{
    public class AdminImplementation : IadminInterface
    {

        private readonly NpgsqlConnection _conn;
        public AdminImplementation(NpgsqlConnection npgsqlConnection)
        {
            _conn = npgsqlConnection;
        }

        // create class 
        public async Task<(bool success, string message)> CreateClass(string className)
        {
            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }

                // Check if class already exists
                using (var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM t_classes WHERE c_class_name = @c_class_name", _conn))
                {
                    checkCmd.Parameters.AddWithValue("@c_class_name", className);
                    var count = (long)await checkCmd.ExecuteScalarAsync();

                    if (count > 0)
                    {
                        return (false, "Class already exists.");
                    }
                }

                // Insert new class
                using (var cmd = new NpgsqlCommand("INSERT INTO t_classes (c_class_name) VALUES (@c_class_name)", _conn))
                {
                    cmd.Parameters.AddWithValue("@c_class_name", className);
                    await cmd.ExecuteNonQueryAsync();
                }

                return (true, "Class created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateClass: {ex.Message}");
                return (false, "Error occurred while creating class.");
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        // get class
        public async Task<(bool success, string message, List<object>)> GetAllClasses()
        {
            try
            {
                if (_conn.State == ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }

                var classList = new List<object>();

                using (var cmd = new NpgsqlCommand("SELECT c_class_id, c_class_name FROM t_classes ORDER BY c_class_name ASC", _conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        classList.Add(new
                        {
                            ClassId = reader.GetInt32(0),
                            ClassName = reader.GetString(1)
                        });
                    }
                }

                return (true, "Classes retrieved successfully.", classList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllClasses: {ex.Message}");
                return (false, "Error occurred while fetching classes.", null);
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        // GetUnassignedTeachersAndClasses
        public async Task<(bool success, string message, List<object>, List<object>)> GetUnassignedTeachersAndClasses()
        {
            try
            {
                if (_conn.State == ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }

                var unassignedTeachers = new List<object>();
                var unassignedClasses = new List<object>();

                using (var cmd = new NpgsqlCommand(@"
           -- Get unassigned teachers
            SELECT t.c_teacher_id, t.c_full_name 
            FROM t_teachers t
            LEFT JOIN t_class_teachers ct ON t.c_teacher_id = ct.c_teacher_id
            WHERE ct.c_teacher_id IS NULL;
            
            -- Get classes with no assigned teacher
            SELECT c.c_class_id, c.c_class_name
            FROM t_classes c
            LEFT JOIN t_class_teachers ct ON c.c_class_id = ct.c_class_id
            WHERE ct.c_teacher_id IS NULL;", _conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    // Read unassigned teachers
                    while (await reader.ReadAsync())
                    {
                        unassignedTeachers.Add(new
                        {
                            TeacherId = reader.GetInt32(0),
                            FullName = reader.GetString(1)
                        });
                    }

                    // Move to next result set
                    await reader.NextResultAsync();

                    // Read unassigned classes
                    while (await reader.ReadAsync())
                    {
                        unassignedClasses.Add(new
                        {
                            ClassId = reader.GetInt32(0),
                            ClassName = reader.GetString(1)
                        });
                    }
                }

                return (true, "Unassigned teachers retrieved successfully.", unassignedTeachers, unassignedClasses);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUnassignedTeachers: {ex.Message}");
                return (false, "Error occurred while fetching unassigned teachers.", null, null);
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        // AssignClassToTeacher
        public async Task<(bool success, string message)> AssignClassToTeacher(int classId, int teacherId)
        {
            try
            {
                if (_conn.State == ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }

                // Check if teacher is already assigned to a class
                using (var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM t_class_teachers WHERE c_teacher_id = @teacherId", _conn))
                {
                    checkCmd.Parameters.AddWithValue("@teacherId", teacherId);
                    int count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                    if (count > 0)
                    {
                        return (false, "This teacher is already assigned to a class.");
                    }
                }

                // Check if class already has a teacher
                using (var checkClassCmd = new NpgsqlCommand("SELECT COUNT(*) FROM t_class_teachers WHERE c_class_id = @classId", _conn))
                {
                    checkClassCmd.Parameters.AddWithValue("@classId", classId);
                    int classCount = Convert.ToInt32(await checkClassCmd.ExecuteScalarAsync());

                    if (classCount > 0)
                    {
                        return (false, "This class already has a teacher assigned.");
                    }
                }

                // Assign class to teacher
                using (var cmd = new NpgsqlCommand("INSERT INTO t_class_teachers (c_class_id, c_teacher_id) VALUES (@classId, @teacherId)", _conn))
                {
                    cmd.Parameters.AddWithValue("@classId", classId);
                    cmd.Parameters.AddWithValue("@teacherId", teacherId);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        return (true, "Class assigned to teacher successfully.");
                    }
                    else
                    {
                        return (false, "Failed to assign class to teacher.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AssignClassToTeacher: {ex.Message}");
                return (false, "Error occurred while assigning class.");
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        // GetAssignedClassesAndTeachers
        public async Task<(bool success, string message, List<object>)> GetAssignedClassesAndTeachers()
        {
            try
            {
                if (_conn.State == ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }

                var assignedData = new List<object>();

                using (var cmd = new NpgsqlCommand(@"
            SELECT c.c_class_id, c.c_class_name, t.c_teacher_id, t.c_full_name
            FROM t_class_teachers ct
            JOIN t_classes c ON ct.c_class_id = c.c_class_id
            JOIN t_teachers t ON ct.c_teacher_id = t.c_teacher_id;", _conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        assignedData.Add(new
                        {
                            ClassId = reader.GetInt32(0),
                            ClassName = reader.GetString(1),
                            TeacherId = reader.GetInt32(2),
                            TeacherName = reader.GetString(3)
                        });
                    }
                }

                if (assignedData.Count == 0)
                {
                    return (false, "No assigned classes found.", null);
                }

                return (true, "Assigned classes retrieved successfully.", assignedData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAssignedClassesAndTeachers: {ex.Message}");
                return (false, "Error occurred while fetching assigned classes.", null);
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }


        //add subject
        public async Task<(bool success, string message)> AddSubject(string subjectName)
        {
            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }

                using (var cmd = new NpgsqlCommand("INSERT INTO t_subjects (c_subject_name) VALUES (@c_subject_name)", _conn))
                {
                    cmd.Parameters.AddWithValue("@c_subject_name", subjectName);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        return (true, "Subject added successfully");
                    }
                    else
                    {
                        return (false, "Failed to add subject");
                    }
                }
            }
            catch (PostgresException ex) when (ex.SqlState == "23505") // Unique constraint violation
            {
                return (false, "Subject name already exists.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return (false, "Error adding subject.");
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }


        // GetAllTeachersWithUnassignedSubjects
        public async Task<(bool success, string message, List<object> teacherSubjectData)> GetAllTeachersWithUnassignedSubjects()
        {
            try
            {
                if (_conn.State == ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }

                var teacherSubjectData = new List<object>();

                using (var cmd = new NpgsqlCommand(@"
            SELECT t.c_teacher_id, t.c_full_name, s.c_subject_id, s.c_subject_name 
            FROM t_teachers t
            CROSS JOIN t_subjects s
            LEFT JOIN t_teacher_subjects ts 
            ON t.c_teacher_id = ts.c_teacher_id AND s.c_subject_id = ts.c_subject_id
            WHERE ts.c_teacher_id IS NULL
            ORDER BY t.c_teacher_id;", _conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        Dictionary<int, (string teacherName, List<object> subjects)> teacherSubjectsMap = new();

                        while (await reader.ReadAsync())
                        {
                            int teacherId = reader.GetInt32(0);
                            string teacherName = reader.GetString(1);
                            int subjectId = reader.GetInt32(2);
                            string subjectName = reader.GetString(3);

                            if (!teacherSubjectsMap.ContainsKey(teacherId))
                            {
                                teacherSubjectsMap[teacherId] = (teacherName, new List<object>());
                            }

                            teacherSubjectsMap[teacherId].subjects.Add(new { SubjectId = subjectId, SubjectName = subjectName });
                        }

                        foreach (var entry in teacherSubjectsMap)
                        {
                            teacherSubjectData.Add(new
                            {
                                TeacherId = entry.Key,
                                FullName = entry.Value.teacherName,  // ✅ Corrected
                                UnassignedSubjects = entry.Value.subjects
                            });
                        }
                    }
                }

                return (true, "Teachers with unassigned subjects retrieved successfully.", teacherSubjectData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllTeachersWithUnassignedSubjects: {ex.Message}");
                return (false, "Error occurred while fetching unassigned subjects for teachers.", null);
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }
        // AssignSubjectToTeacher
        public async Task<(bool success, string message)> AssignSubjectToTeacher(int teacherId, int subjectId)
        {
            try
            {
                if (_conn.State == ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }

                // Check if the teacher exists
                using (var checkTeacherCmd = new NpgsqlCommand("SELECT COUNT(*) FROM t_teachers WHERE c_teacher_id = @teacherId", _conn))
                {
                    checkTeacherCmd.Parameters.AddWithValue("@teacherId", teacherId);
                    int teacherCount = Convert.ToInt32(await checkTeacherCmd.ExecuteScalarAsync());
                    if (teacherCount == 0)
                    {
                        return (false, "Teacher not found.");
                    }
                }

                // Check if the subject exists
                using (var checkSubjectCmd = new NpgsqlCommand("SELECT COUNT(*) FROM t_subjects WHERE c_subject_id = @subjectId", _conn))
                {
                    checkSubjectCmd.Parameters.AddWithValue("@subjectId", subjectId);
                    int subjectCount = Convert.ToInt32(await checkSubjectCmd.ExecuteScalarAsync());
                    if (subjectCount == 0)
                    {
                        return (false, "Subject not found.");
                    }
                }

                // Check if subject is already assigned to this teacher
                using (var checkAssignmentCmd = new NpgsqlCommand("SELECT COUNT(*) FROM t_teacher_subjects WHERE c_teacher_id = @teacherId AND c_subject_id = @subjectId", _conn))
                {
                    checkAssignmentCmd.Parameters.AddWithValue("@teacherId", teacherId);
                    checkAssignmentCmd.Parameters.AddWithValue("@subjectId", subjectId);
                    int existingAssignment = Convert.ToInt32(await checkAssignmentCmd.ExecuteScalarAsync());
                    if (existingAssignment > 0)
                    {
                        return (false, "This subject is already assigned to the teacher.");
                    }
                }

                // Assign subject to teacher
                using (var cmd = new NpgsqlCommand("INSERT INTO t_teacher_subjects (c_teacher_id, c_subject_id) VALUES (@teacherId, @subjectId)", _conn))
                {
                    cmd.Parameters.AddWithValue("@teacherId", teacherId);
                    cmd.Parameters.AddWithValue("@subjectId", subjectId);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        return (true, "Subject assigned to teacher successfully.");
                    }
                    else
                    {
                        return (false, "Failed to assign subject to teacher.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AssignSubjectToTeacher: {ex.Message}");
                return (false, "An error occurred while assigning subject to teacher.");
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }
        // GetAllTeachersWithAssignedSubjects
        public async Task<(bool success, string message, List<object> teacherSubjectData)> GetAllTeachersWithAssignedSubjects()
        {
            try
            {
                if (_conn.State == ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }

                var teacherSubjectData = new List<object>();

                using (var cmd = new NpgsqlCommand(@"
            SELECT t.c_teacher_id, t.c_full_name, s.c_subject_id, s.c_subject_name 
            FROM t_teachers t
            JOIN t_teacher_subjects ts ON t.c_teacher_id = ts.c_teacher_id
            JOIN t_subjects s ON ts.c_subject_id = s.c_subject_id
            ORDER BY t.c_teacher_id;", _conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        Dictionary<int, (string fullName, List<object> subjects)> teacherSubjectsMap =
                            new Dictionary<int, (string, List<object>)>();

                        while (await reader.ReadAsync())
                        {
                            int teacherId = reader.GetInt32(0);
                            string teacherName = reader.GetString(1);
                            int subjectId = reader.GetInt32(2);
                            string subjectName = reader.GetString(3);

                            if (!teacherSubjectsMap.ContainsKey(teacherId))
                            {
                                teacherSubjectsMap[teacherId] = (teacherName, new List<object>());
                            }

                            teacherSubjectsMap[teacherId].subjects.Add(new
                            {
                                SubjectId = subjectId,
                                SubjectName = subjectName
                            });
                        }

                        foreach (var entry in teacherSubjectsMap)
                        {
                            teacherSubjectData.Add(new
                            {
                                TeacherId = entry.Key,
                                FullName = entry.Value.fullName,
                                AssignedSubjects = entry.Value.subjects
                            });
                        }
                    }
                }

                return (true, "Teachers with assigned subjects retrieved successfully.", teacherSubjectData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllTeachersWithAssignedSubjects: {ex.Message}");
                return (false, "Error occurred while fetching assigned subjects for teachers.", null);
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }



        // AssignStudentToTeacher 
        public async Task<(bool success, string message)> AssignStudentToTeacher(int studentId, int classId)
        {
            const int maxStudentsPerClass = 30; // Change this as needed

            try
            {
                if (_conn.State == ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }

                // Get the number of students already in this class
                int studentCount;
                using (var cmd = new NpgsqlCommand(@"
            SELECT COUNT(*) FROM t_students WHERE c_class_id = @classId", _conn))
                {
                    cmd.Parameters.AddWithValue("@classId", classId);
                    studentCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                // Check if the class is full
                if (studentCount >= maxStudentsPerClass)
                {
                    return (false, $"Class {classId} has reached the student limit ({maxStudentsPerClass}).");
                }


                int teacherId;
                using (var cmd = new NpgsqlCommand(@"
            SELECT c_teacher_id FROM t_class_teachers WHERE c_class_id = @classId", _conn))
                {
                    cmd.Parameters.AddWithValue("@classId", classId);
                    object result = await cmd.ExecuteScalarAsync();
                    if (result == null || result == DBNull.Value)
                    {
                        return (false, "No teacher assigned to this class.");
                    }
                    teacherId = (int)result;
                }

                // ✅ Assign student to the teacher
                using (var cmd = new NpgsqlCommand(@"
            INSERT INTO t_student_teacher (c_student_id, c_teacher_id)
            VALUES (@studentId, @teacherId)", _conn))
                {
                    cmd.Parameters.AddWithValue("@studentId", studentId);
                    cmd.Parameters.AddWithValue("@teacherId", teacherId);
                    await cmd.ExecuteNonQueryAsync();
                }

                return (true, "Student assigned to teacher successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AssignStudentToTeacher: {ex.Message}");
                return (false, "Error assigning student to teacher.");
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }
        // add student
        public async Task<(bool success, string message, StudentModel)> AddStudent(StudentModel studentModel)
        {
            try
            {
                if (_conn.State == ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }

                string generatedEmail = await HelperClass.GenerateUniqueStudentEmail(_conn, studentModel.FullName);
                string generatedPassword = HelperClass.GenerateRandomPassword();

                int userId;
                int studentId;

                // ✅ Insert into t_users table
                using (var cmd = new NpgsqlCommand(@"
            INSERT INTO t_users (c_username, c_password_hash, c_role, c_email)
            VALUES (@c_username, @c_password_hash, 'student', @c_email)
            RETURNING c_user_id", _conn))
                {
                    cmd.Parameters.AddWithValue("@c_username", studentModel.FullName);
                    cmd.Parameters.AddWithValue("@c_password_hash", generatedPassword);
                    cmd.Parameters.AddWithValue("@c_email", generatedEmail);

                    userId = (int)await cmd.ExecuteScalarAsync();
                }

                // ✅ Insert into t_students table (Removed `c_section_id`)
                using (var cmd = new NpgsqlCommand(@"
            INSERT INTO t_students (c_full_name, c_date_of_birth, c_gender, c_class_id, 
                                    c_enrollment_date, c_profile_picture, c_status, 
                                    c_created_at, c_user_id)
            VALUES (@c_full_name, @c_date_of_birth, @c_gender, @c_class_id, 
                    @c_enrollment_date, @c_profile_picture, @c_status, CURRENT_TIMESTAMP, @c_user_id) 
            RETURNING c_student_id", _conn))
                {
                    cmd.Parameters.AddWithValue("@c_full_name", studentModel.FullName);
                    cmd.Parameters.AddWithValue("@c_date_of_birth", studentModel.DateOfBirth);
                    cmd.Parameters.AddWithValue("@c_gender", studentModel.Gender);
                    cmd.Parameters.AddWithValue("@c_class_id", studentModel.ClassId);
                    cmd.Parameters.AddWithValue("@c_enrollment_date", studentModel.EnrollmentDate);
                    cmd.Parameters.AddWithValue("@c_profile_picture", studentModel.ProfilePicture ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@c_status", studentModel.Status);
                    cmd.Parameters.AddWithValue("@c_user_id", userId);

                    studentId = (int)await cmd.ExecuteScalarAsync();
                }

                // ✅ Assign student to teacher based on `t_class_teachers` table
                var (success, message) = await AssignStudentToTeacher(studentId, (int)studentModel.ClassId);
                if (!success)
                {
                    return (false, message, null);
                }


                using (var cmd = new NpgsqlCommand(@"
            INSERT INTO t_notifications (c_message, c_type, c_student_id)
            VALUES (@message, 'StudentApproval', @studentId)", _conn))
                {
                    cmd.Parameters.AddWithValue("@message", $"Student {studentModel.FullName} added successfully.");
                    cmd.Parameters.AddWithValue("@studentId", studentId);
                    await _conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }

                studentModel.StudentId = studentId;
                studentModel.Password = generatedPassword;
                studentModel.UserId = userId;
                studentModel.Email = generatedEmail;

                return (true, "Student created and assigned to a teacher.", studentModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddStudent: {ex.Message}");
                return (false, "Error during student creation.", null);
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }
        // getallStudent
        public async Task<(bool success, string message, List<object>)> GetAllStudent()
        {
            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }
                var students = new List<object>();

                using (var cmd = new NpgsqlCommand(@"
            SELECT 
                s.c_student_id, s.c_full_name, s.c_date_of_birth, s.c_gender,
                c.c_class_name, s.c_enrollment_date,
                s.c_profile_picture, s.c_status, s.c_created_at, u.c_email, 
                s.c_class_id, s.c_user_id
            FROM t_students s 
            LEFT JOIN t_users u ON s.c_user_id = u.c_user_id
            LEFT JOIN t_classes c ON s.c_class_id = c.c_class_id", _conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var student = new
                            {
                                StudentId = reader.GetInt32(0),
                                FullName = reader.GetString(1),
                                DateOfBirth = reader.GetDateTime(2),
                                Gender = reader.GetString(3),
                                ClassName = reader.IsDBNull(4) ? null : reader.GetString(4),
                                EnrollmentDate = reader.GetDateTime(5),
                                ProfilePicture = reader.IsDBNull(6) ? null : reader.GetString(6),
                                Status = reader.GetBoolean(7),
                                CreatedAt = reader.GetDateTime(8),
                                Email = reader.IsDBNull(9) ? null : reader.GetString(9),
                                ClassId = reader.GetInt32(10),
                                UserId = reader.GetInt32(11)
                            };
                            students.Add(student);
                        }
                        return (true, "Students retrieved successfully", students);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error in GetAllStudent: {ex.Message}");
                return (false, "Error in GetAllStudent", null);
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }
        // update student
        public async Task<(bool success, string message)> UpdateStudent(StudentModel studentModel)
        {
            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }

                var query = @"
            UPDATE t_students 
            SET c_full_name = @fullName, 
                c_date_of_birth = @dateOfBirth,
                c_gender = @gender,
                c_profile_picture = @profilePicture,
                c_status = @status
            WHERE c_student_id = @studentId";

                using (var cmd = new NpgsqlCommand(query, _conn))
                {
                    cmd.Parameters.AddWithValue("@fullName", studentModel.FullName);
                    cmd.Parameters.AddWithValue("@dateOfBirth", studentModel.DateOfBirth);
                    cmd.Parameters.AddWithValue("@gender", studentModel.Gender);
                    cmd.Parameters.AddWithValue("@profilePicture", studentModel.ProfilePicture ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@status", studentModel.Status);
                    cmd.Parameters.AddWithValue("@studentId", studentModel.StudentId);
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected == 0)
                        return (false, "Student not found or no changes made");

                }
                using (var userCmd = new NpgsqlCommand(@"
                    UPDATE t_users 
                    SET c_username = @username
                    WHERE c_user_id = (SELECT c_user_id FROM t_students WHERE c_student_id = @studentId)", _conn))
                {
                    userCmd.Parameters.AddWithValue("@username", studentModel.FullName);
                    userCmd.Parameters.AddWithValue("@studentId", studentModel.StudentId);

                    await userCmd.ExecuteNonQueryAsync();
                }


                string notificationQuery = @"
            INSERT INTO t_notifications (c_message, c_type, c_student_id)
            VALUES (@message, 'StudentUpdate', @studentId);
            ";

                using (var cmd = new NpgsqlCommand(notificationQuery, _conn))
                {
                    cmd.Parameters.AddWithValue("@message", $"Student {studentModel.FullName} details have been updated.");
                    cmd.Parameters.AddWithValue("@studentId", studentModel.StudentId);
                    await cmd.ExecuteNonQueryAsync();
                }

                return (true, "Student updated successfully");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateStudent: {ex.Message}");
                return (false, "Error updating student");
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }
        // getstudentByIdd
        public async Task<(bool success, string message, object)> GetStudentById(int Id)
        {
            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }

                var query = @"
        SELECT 
            s.c_student_id, s.c_full_name, s.c_date_of_birth, s.c_gender,
            s.c_class_id, c.c_class_name,
            s.c_enrollment_date, s.c_profile_picture, s.c_status, s.c_created_at,
            u.c_user_id  
        FROM t_students s
        LEFT JOIN t_users u ON s.c_user_id = u.c_user_id
        LEFT JOIN t_classes c ON s.c_class_id = c.c_class_id
        WHERE s.c_student_id = @studentId";

                using (var cmd = new NpgsqlCommand(query, _conn))
                {
                    cmd.Parameters.AddWithValue("@studentId", Id);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var student = new
                            {
                                StudentId = reader.GetInt32(0),
                                FullName = reader.GetString(1),
                                DateOfBirth = reader.GetDateTime(2),
                                Gender = reader.GetString(3),
                                ClassId = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                                ClassName = reader.IsDBNull(5) ? null : reader.GetString(5),
                                EnrollmentDate = reader.GetDateTime(6),
                                ProfilePicture = reader.IsDBNull(7) ? null : reader.GetString(7),
                                Status = reader.GetBoolean(8),
                                CreatedAt = reader.GetDateTime(9),
                                UserId = reader.GetInt32(10)
                            };
                            return (true, "Student retrieved successfully", student);
                        }
                    }
                }
                return (false, "Student not found", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetStudentById: {ex.Message}");
                return (false, "Error retrieving student", null);
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }
        // deletestudentbyid
        public async Task<(bool success, string message)> DeleteStudent(int Id)
        {

            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }

                int UserId = 0;
                var query = @"
                    SELECT c_user_id FROM t_students
                    WHERE c_student_id=@studentId
                ";
                using (var cmd = new NpgsqlCommand(query, _conn))
                {
                    cmd.Parameters.AddWithValue("@studentId", Id);
                    var result = await cmd.ExecuteScalarAsync();

                    if (result == null)
                        return (false, "Student not found");

                    UserId = Convert.ToInt32(result);
                }

                // delete from t_students
                var query1 = @"
                    DELETE FROM t_students 
                    WHERE c_student_id=@studentId
                ";
                using (var cmd = new NpgsqlCommand(query1, _conn))
                {
                    cmd.Parameters.AddWithValue("@studentId", Id);

                    await cmd.ExecuteNonQueryAsync();
                }

                // delete from user
                var query2 = @"
                    DELETE FROM t_users
                    WHERE c_user_id=@userID
                ";
                using (var cmd = new NpgsqlCommand(query2, _conn))
                {
                    cmd.Parameters.AddWithValue("@userID", UserId);
                    await cmd.ExecuteNonQueryAsync();
                }

                return (true, "student Delete successfully");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteStudent: {ex.Message}");
                return (false, "Error deleting student");
            }
            finally
            {
                await _conn.CloseAsync();
            }

        }
        // aprove student
        public async Task<bool> ApproveStudent(int studentId, bool newStatus)
        {
            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }

                // Check if student exists
                string checkStudentQuery = "SELECT COUNT(*) FROM t_students WHERE c_student_id = @studentId";
                using (var checkCmd = new NpgsqlCommand(checkStudentQuery, _conn))
                {
                    checkCmd.Parameters.AddWithValue("@studentId", studentId);
                    var count = (long)await checkCmd.ExecuteScalarAsync();

                    if (count == 0)
                    {
                        return false; // Student not found
                    }
                }

                // Update student status
                string updateQuery = "UPDATE t_students SET c_status = @newStatus WHERE c_student_id = @studentId";
                using (var updateCmd = new NpgsqlCommand(updateQuery, _conn))
                {
                    updateCmd.Parameters.AddWithValue("@newStatus", newStatus);
                    updateCmd.Parameters.AddWithValue("@studentId", studentId);

                    int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0; // Return true if update successful
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ApproveStudent: {ex.Message}");
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }


        // dashboard
        public async Task<(bool succes, object principalName)> GetPrincipalName()
        {
            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }
                object principal = null;
                var query = @"SELECT c_user_id,c_username  FROM t_users where c_role='admin' LIMIT 1";
                await using (var cmd = new NpgsqlCommand(query, _conn))
                {
                    await using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            principal = new { PrincipalId = reader.GetInt32(0), PrincipalName = reader.GetString(1) };
                            return (true, principal);
                        }
                    }
                }
                return (false, "No principal found");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPrincipalName: {ex.Message}");
                return (false, "error in retriving principleName");
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }
        public async Task<(bool success, string message, object data)> GetSchoolHierarchy()
        {
            try
            {

                var (principalFound, principal) = await GetPrincipalName();
                if (principalFound == null)
                {
                    return (false, "No principal found.", null);
                }

                if (_conn.State == System.Data.ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }
                var hierarchy = new List<object>();

                string teacherQuery = "SELECT t.c_teacher_id, u.c_username FROM t_teachers t JOIN t_users u ON t.c_user_id = u.c_user_id";
                await using (var teacherCmd = new NpgsqlCommand(teacherQuery, _conn))
                await using (var reader = await teacherCmd.ExecuteReaderAsync())
                {
                    var teachers = new List<(int TeacherId, string TeacherName)>();

                    while (await reader.ReadAsync())
                    {
                        teachers.Add((reader.GetInt32(0), reader.GetString(1)));
                    }

                    reader.Close();

                    foreach (var teacher in teachers)
                    {
                        var studentList = new List<object>();

                        string studentQuery = @"
                    SELECT s.c_student_id, s.c_full_name
                    FROM t_students s
                    JOIN t_student_teacher st ON s.c_student_id = st.c_student_id
                    WHERE st.c_teacher_id = @teacherId";

                        await using (var studentCmd = new NpgsqlCommand(studentQuery, _conn))
                        {
                            studentCmd.Parameters.AddWithValue("@teacherId", teacher.TeacherId);
                            await using (var studentReader = await studentCmd.ExecuteReaderAsync())
                            {
                                while (await studentReader.ReadAsync())
                                {
                                    studentList.Add(new
                                    {
                                        StudentId = studentReader.GetInt32(0),
                                        FullName = studentReader.GetString(1)
                                    });
                                }
                            }
                        }

                        hierarchy.Add(new
                        {
                            TeacherId = teacher.TeacherId,
                            TeacherName = teacher.TeacherName,
                            Students = studentList
                        });
                    }
                }

                return (true, "School hierarchy retrieved successfully.", new { Principal = principal, Teachers = hierarchy });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSchoolHierarchy: {ex.Message}");
                return (false, "An error occurred while retrieving the school hierarchy.", null);
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                {
                    await _conn.CloseAsync();
                }
            }
        }
        public async Task<(bool success, List<object> studentCounts)> GetStudentCountPerClass()
        {
            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }

                var studentCounts = new List<object>();
                string query = @"
            SELECT c.c_class_name, COUNT(s.c_student_id) AS student_count
            FROM t_classes c
            LEFT JOIN t_students s ON c.c_class_id = s.c_class_id
            GROUP BY c.c_class_name
            ORDER BY c.c_class_name;
        ";

                await using (var cmd = new NpgsqlCommand(query, _conn))
                {
                    await using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            studentCounts.Add(new
                            {
                                ClassName = reader.GetString(0),
                                StudentCount = reader.GetInt32(1)
                            });
                        }
                    }
                }

                return (true, studentCounts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetStudentCountPerClass: {ex.Message}");
                return (false, null);
            }

        }

        public async Task<(bool success, List<object> notifications)> GetUnreadNotifications()
        {
            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }

                var notifications = new List<object>();

                string query = @"
            SELECT c_notification_id, c_message, c_type, c_created_at, c_status
            FROM t_notifications 
            where c_status = false
            ORDER BY c_created_at DESC;
        ";

                await using (var cmd = new NpgsqlCommand(query, _conn))
                {
                   await  using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            notifications.Add(new
                            {
                                NotificationId = reader.GetInt32(0),
                                Message = reader.GetString(1),
                                Type = reader.GetString(2),
                                CreatedAt = reader.GetDateTime(3),
                                Status=reader.GetBoolean(4)
                            });
                        }
                    }
                }

                return (true, notifications);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUnreadNotifications: {ex.Message}");
                return (false, null);
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        


        public async Task<bool> MarkNotificationAsRead(int notificationId)
        {
            try
            {
                if (_conn.State != System.Data.ConnectionState.Open)
                {
                    await _conn.OpenAsync();
                }

                string query = "UPDATE t_notifications SET c_status = TRUE WHERE c_notification_id = @notificationId AND c_status = FALSE";

                await using (var cmd = new NpgsqlCommand(query, _conn))
                {
                    cmd.Parameters.AddWithValue("@notificationId", notificationId);
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0; // Return true if any row was updated
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MarkNotificationAsRead: {ex.Message}");
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }
        public async Task<(bool success, List<object> notifications)> GetreadNotifications()
        {
                try
                {
                    if (_conn.State == System.Data.ConnectionState.Closed)
                    {
                        await _conn.OpenAsync();
                    }

                    var notifications = new List<object>();

                    string query = @"
                SELECT c_notification_id, c_message, c_type, c_created_at, c_status
                FROM t_notifications
                where c_status = true 
                ORDER BY c_created_at DESC;
            ";

                    await using (var cmd = new NpgsqlCommand(query, _conn))
                    {
                        await using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                notifications.Add(new
                                {
                                    NotificationId = reader.GetInt32(0),
                                    Message = reader.GetString(1),
                                    Type = reader.GetString(2),
                                    CreatedAt = reader.GetDateTime(3),
                                    Status=reader.GetBoolean(4)
                                });
                            }
                        }
                    }

                    return (true, notifications);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in GetUnreadNotifications: {ex.Message}");
                    return (false, null);
                }
                finally
                {
                    await _conn.CloseAsync();
                }
        }


        public async Task<byte[]> ExportStudentsToPDF()
{
    try
    {
        if (_conn.State == System.Data.ConnectionState.Closed)
            await _conn.OpenAsync();

        string query = @"
            SELECT s.c_full_name, u.c_email, s.c_date_of_birth, s.c_gender, 
                c.c_class_name
            FROM t_students s
            LEFT JOIN t_users u ON s.c_user_id = u.c_user_id  
            LEFT JOIN t_classes c ON s.c_class_id = c.c_class_id;
        ";

        var students = new List<StudentModel>();

        using (var cmd = new NpgsqlCommand(query, _conn))
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                students.Add(new StudentModel
                {
                    FullName = reader.GetString(0),
                    Email = reader.IsDBNull(1) ? "N/A" : reader.GetString(1),
                    DateOfBirth = reader.GetDateTime(2),
                    Gender = reader.GetString(3),
                    ClassName = reader.IsDBNull(4) ? "N/A" : reader.GetString(4)
                });
            }
        }

        if (students.Count == 0)
        {
            throw new Exception("No students found to export.");
        }

        using (MemoryStream ms = new MemoryStream())
        {
            Document doc = new Document(PageSize.A4);
            PdfWriter.GetInstance(doc, ms);
            doc.Open();

            Font font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            doc.Add(new Paragraph("Student List", font));
            doc.Add(new Paragraph("\n"));

            PdfPTable table = new PdfPTable(5) { WidthPercentage = 100 };
            table.AddCell("Full Name");
            table.AddCell("Email");
            table.AddCell("Date of Birth");
            table.AddCell("Gender");
            table.AddCell("Class");

            foreach (var student in students)
            {
                table.AddCell(student.FullName);
                table.AddCell(student.Email);
                table.AddCell(student.DateOfBirth.ToShortDateString());
                table.AddCell(student.Gender);
                table.AddCell(student.ClassName);
            }

            doc.Add(table);
            doc.Close();

            return ms.ToArray();
        }
    }
    catch (Exception ex)
    {
        // Log the error (replace with your logging framework)
        Console.WriteLine($"Error in ExportStudentsToPDF: {ex.Message}");
        throw; // Re-throw the exception to be handled by the controller
    }
    finally
    {
        if (_conn.State == System.Data.ConnectionState.Open)
            await _conn.CloseAsync();
    }
}
        public async Task<byte[]> ExportStudentsToExcel()
        {
            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                    await _conn.OpenAsync();

                string query = @"
        SELECT s.c_full_name, u.c_email, s.c_date_of_birth, s.c_gender, 
               c.c_class_name
        FROM t_students s
        LEFT JOIN t_users u ON s.c_user_id = u.c_user_id  
        LEFT JOIN t_classes c ON s.c_class_id = c.c_class_id";

                var students = new List<StudentModel>();

                using (var cmd = new NpgsqlCommand(query, _conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        students.Add(new StudentModel
                        {
                            FullName = reader.GetString(0),
                            Email = reader.IsDBNull(1) ? "N/A" : reader.GetString(1),
                            DateOfBirth = reader.GetDateTime(2),
                            Gender = reader.GetString(3),
                            ClassName = reader.IsDBNull(4) ? "N/A" : reader.GetString(4)

                        });
                    }
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Students");

                    worksheet.Cell(1, 1).Value = "Full Name";
                    worksheet.Cell(1, 2).Value = "Email";
                    worksheet.Cell(1, 3).Value = "Date of Birth";
                    worksheet.Cell(1, 4).Value = "Gender";
                    worksheet.Cell(1, 5).Value = "Class";


                    int row = 2;

                    foreach (var student in students)
                    {
                        worksheet.Cell(row, 1).Value = student.FullName;
                        worksheet.Cell(row, 2).Value = student.Email;
                        worksheet.Cell(row, 3).Value = student.DateOfBirth.ToShortDateString();
                        worksheet.Cell(row, 4).Value = student.Gender;
                        worksheet.Cell(row, 5).Value = student.ClassName;
                        row++;
                    }

                    // ✅ Auto Adjust Column Widths
                    worksheet.Columns().AdjustToContents();

                    using (MemoryStream ms = new MemoryStream())
                    {
                        workbook.SaveAs(ms);
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ExportStudentsToExcel: {ex.Message}");
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }


        public async Task<ThemeModel> GetTheme(int adminId)
        {
            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                    await _conn.OpenAsync();

                string query = "SELECT c_theme_name FROM t_theme_settings WHERE c_admin_id = @adminId";

                using (var cmd = new NpgsqlCommand(query, _conn))
                {
                    cmd.Parameters.AddWithValue("@adminId", adminId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ThemeModel
                            {
                                AdminId = adminId,
                                ThemeName = reader.GetString(0)
                            };
                        }
                    }
                }

                return new ThemeModel { AdminId = adminId, ThemeName = "Light" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTheme: {ex.Message}");
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }
        public async Task<bool> ToggleTheme(ThemeModel theme)
        {
            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                    await _conn.OpenAsync();


                string checkQuery = "SELECT c_theme_name FROM t_theme_settings WHERE c_admin_id = @adminId";
                string currentTheme = "Light";

                using (var cmd = new NpgsqlCommand(checkQuery, _conn))
                {
                    cmd.Parameters.AddWithValue("@adminId", theme.AdminId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            currentTheme = reader.GetString(0);
                        }
                    }
                }

                string newTheme = currentTheme == "Light" ? "Dark" : "Light";


                string updateQuery = @"
            INSERT INTO t_theme_settings (c_admin_id, c_theme_name)
            VALUES (@adminId, @themeName)
            ON CONFLICT (c_admin_id) DO UPDATE
            SET c_theme_name = @themeName, c_updated_at = CURRENT_TIMESTAMP;
        ";

                using (var cmd = new NpgsqlCommand(updateQuery, _conn))
                {
                    cmd.Parameters.AddWithValue("@adminId", theme.AdminId);
                    cmd.Parameters.AddWithValue("@themeName", newTheme);
                    await cmd.ExecuteNonQueryAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ToggleTheme: {ex.Message}");
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }


        public async Task<List<object>> GetSubjectProgress()
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
                ts.c_teacher_id, 
                t.c_full_name AS teacher_name,
                ts.c_subject_id,
                s.c_subject_name,
                ts.c_start_date,
                ts.c_end_date,
                ts.c_completion_percentage
            FROM t_teacher_subjects ts
            JOIN t_teachers t ON ts.c_teacher_id = t.c_teacher_id
            JOIN t_subjects s ON ts.c_subject_id = s.c_subject_id
            ORDER BY ts.c_completion_percentage DESC";

                using (var cmd = new NpgsqlCommand(query, _conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            progressList.Add(new
                            {
                                AssignmentId = reader.GetInt32(0),
                                TeacherId = reader.GetInt32(1),
                                TeacherName = reader.GetString(2),
                                SubjectId = reader.GetInt32(3),
                                SubjectName = reader.GetString(4),
                                StartDate = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                                EndDate = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                                CompletionPercentage = reader.GetDecimal(7)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching subject progress: {ex.Message}");
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return progressList;
        }


        // time table
        public async Task<(bool succes, string message)> CreateTimetable(int classId, int subjectId, int teacherId, TimeSpan timeSlot, string dayOfWeek)
        {
            try
            {
                if (_conn.State == System.Data.ConnectionState.Closed)
                    await _conn.OpenAsync();

                // 1️⃣ **Check if the teacher is assigned to the subject**
                string checkAssignmentQuery = @"
        SELECT COUNT(*) FROM t_teacher_subjects 
        WHERE c_teacher_id = @TeacherId AND c_subject_id = @SubjectId";

                using (var cmd = new NpgsqlCommand(checkAssignmentQuery, _conn))
                {
                    cmd.Parameters.AddWithValue("@TeacherId", teacherId);
                    cmd.Parameters.AddWithValue("@SubjectId", subjectId);

                    int assignedCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    if (assignedCount == 0)
                    {
                        return (false, "This teacher is not assigned to this subject.");
                    }
                }

                // 2️⃣ **Check for existing timetable conflicts**
                string checkConflictQuery = @"
        SELECT COUNT(*) FROM t_timetable 
        WHERE c_class_id = @ClassId AND c_time_slot = @TimeSlot AND c_day_of_week = @DayOfWeek";

                using (var cmd = new NpgsqlCommand(checkConflictQuery, _conn))
                {
                    cmd.Parameters.AddWithValue("@ClassId", classId);
                    cmd.Parameters.AddWithValue("@TimeSlot", timeSlot);
                    cmd.Parameters.AddWithValue("@DayOfWeek", dayOfWeek);

                    int conflictCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    if (conflictCount > 0)
                    {
                        return (false, "A subject is already scheduled for this class at this time.");
                    }
                }

                // 3️⃣ **Check if the teacher is already teaching another class at the same time**
                string checkTeacherConflictQuery = @"
        SELECT COUNT(*) FROM t_timetable 
        WHERE c_teacher_id = @TeacherId AND c_time_slot = @TimeSlot AND c_day_of_week = @DayOfWeek";

                using (var cmd = new NpgsqlCommand(checkTeacherConflictQuery, _conn))
                {
                    cmd.Parameters.AddWithValue("@TeacherId", teacherId);
                    cmd.Parameters.AddWithValue("@TimeSlot", timeSlot);
                    cmd.Parameters.AddWithValue("@DayOfWeek", dayOfWeek);

                    int teacherConflictCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    if (teacherConflictCount > 0)
                    {
                        return (false, "This teacher is already assigned to another class at this time.");
                    }
                }

                // 4️⃣ **Insert into timetable**
                string insertQuery = @"
        INSERT INTO t_timetable (c_class_id, c_subject_id, c_teacher_id, c_time_slot, c_day_of_week) 
        VALUES (@ClassId, @SubjectId, @TeacherId, @TimeSlot, @DayOfWeek)";

                using (var cmd = new NpgsqlCommand(insertQuery, _conn))
                {
                    cmd.Parameters.AddWithValue("@ClassId", classId);
                    cmd.Parameters.AddWithValue("@SubjectId", subjectId);
                    cmd.Parameters.AddWithValue("@TeacherId", teacherId);
                    cmd.Parameters.AddWithValue("@TimeSlot", timeSlot);
                    cmd.Parameters.AddWithValue("@DayOfWeek", dayOfWeek);

                    await cmd.ExecuteNonQueryAsync();
                }

                return (true, "Timetable entry successfully created!");
            }
            catch (PostgresException pgEx) when (pgEx.SqlState == "23505") // Unique violation
            {
                return (false, "Duplicate timetable entry detected.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateTimetable: {ex.Message}");
                return (false, "Could not create timetable entry due to an unexpected error.");
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<(bool succes, string message, List<object>)> GetTimetableByClass(int classId)
        {
            try
            {
                if (_conn.State == ConnectionState.Closed)
                {
                    await _conn.OpenAsync();
                }

                string query = @"
                    SELECT t.c_timetable_id,
                            t.c_class_id,
                            c.c_class_name,
                            t.c_teacher_id,
                            te.c_full_name,
                            t.c_subject_id,
                            s.c_subject_name,
                            t.c_time_slot,
                            t.c_day_of_week
                    FROM t_timetable t 
                    JOIN t_classes c ON t.c_class_id=c.c_class_id
                    JOIN t_subjects s ON t.c_subject_id=s.c_subject_id
                    LEFT JOIN t_teachers te ON t.c_teacher_id=te.c_teacher_id
                    WHERE t.c_class_id=@classID
                ";

                var TimeTableList = new List<object>();

                using (var cmd = new NpgsqlCommand(query, _conn))
                {
                    cmd.Parameters.AddWithValue("@classID", classId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            TimeTableList.Add(new
                            {
                                TimeTableId = reader.GetInt32(0),
                                classId = reader.GetInt32(1),
                                className = reader.GetString(2),
                                TeacherId = reader.GetInt32(3),
                                TeacherName = reader.GetString(4),
                                SubjectId = reader.GetInt32(5),
                                SubjectName = reader.GetString(6),
                                TimeSlot = reader.GetTimeSpan(7),
                                DayOfWeek = reader.GetString(8)
                            });
                        }
                    }
                }
                return (true, "timtable retrive successFully", TimeTableList);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return (false, "Error retrieving timetable.", null);
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<(bool success, string message)> UpdateTimetable(int timetableId, int classId, int subjectId, int teacherId, TimeSpan timeSlot, string dayOfWeek)
        {
            try
            {
                if (_conn.State == ConnectionState.Closed)
                    await _conn.OpenAsync();

                // 1️⃣ **Check if teacher is assigned to subject**
                string checkAssignmentQuery = @"
            SELECT COUNT(*) FROM t_teacher_subjects 
            WHERE c_teacher_id = @TeacherId AND c_subject_id = @SubjectId";

                using (var cmd = new NpgsqlCommand(checkAssignmentQuery, _conn))
                {
                    cmd.Parameters.AddWithValue("@TeacherId", teacherId);
                    cmd.Parameters.AddWithValue("@SubjectId", subjectId);

                    int assignedCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    if (assignedCount == 0)
                    {
                        return (false, "This teacher is not assigned to this subject.");
                    }
                }

                // 2️⃣ **Check for timetable conflicts**
                string checkConflictQuery = @"
            SELECT COUNT(*) FROM t_timetable 
            WHERE c_class_id = @ClassId AND c_time_slot = @TimeSlot 
            AND c_day_of_week = @DayOfWeek AND c_timetable_id != @TimetableId";

                using (var cmd = new NpgsqlCommand(checkConflictQuery, _conn))
                {
                    cmd.Parameters.AddWithValue("@ClassId", classId);
                    cmd.Parameters.AddWithValue("@TimeSlot", timeSlot);
                    cmd.Parameters.AddWithValue("@DayOfWeek", dayOfWeek);
                    cmd.Parameters.AddWithValue("@TimetableId", timetableId);

                    int conflictCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    if (conflictCount > 0)
                    {
                        return (false, "Another subject is already scheduled for this class at the same time.");
                    }
                }

                // 3️⃣ **Update Timetable Entry**
                string updateQuery = @"
            UPDATE t_timetable 
            SET c_class_id = @ClassId, c_subject_id = @SubjectId, 
                c_teacher_id = @TeacherId, c_time_slot = @TimeSlot, 
                c_day_of_week = @DayOfWeek 
            WHERE c_timetable_id = @TimetableId";

                using (var cmd = new NpgsqlCommand(updateQuery, _conn))
                {
                    cmd.Parameters.AddWithValue("@ClassId", classId);
                    cmd.Parameters.AddWithValue("@SubjectId", subjectId);
                    cmd.Parameters.AddWithValue("@TeacherId", teacherId);
                    cmd.Parameters.AddWithValue("@TimeSlot", timeSlot);
                    cmd.Parameters.AddWithValue("@DayOfWeek", dayOfWeek);
                    cmd.Parameters.AddWithValue("@TimetableId", timetableId);

                    await cmd.ExecuteNonQueryAsync();
                }

                return (true, "Timetable updated successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating timetable: {ex.Message}");
                return (false, "Error updating timetable entry.");
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<(bool success, string message)> DeleteTimetable(int timetableId)
        {
            try
            {
                if (_conn.State == ConnectionState.Closed)
                    await _conn.OpenAsync();

                // Check if the timetable entry exists
                string checkQuery = "SELECT COUNT(*) FROM t_timetable WHERE c_timetable_id = @TimetableId";
                using (var cmd = new NpgsqlCommand(checkQuery, _conn))
                {
                    cmd.Parameters.AddWithValue("@TimetableId", timetableId);
                    int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    if (count == 0)
                    {
                        return (false, "Timetable entry not found.");
                    }
                }

                // Delete the timetable entry
                string deleteQuery = "DELETE FROM t_timetable WHERE c_timetable_id = @TimetableId";
                using (var cmd = new NpgsqlCommand(deleteQuery, _conn))
                {
                    cmd.Parameters.AddWithValue("@TimetableId", timetableId);
                    await cmd.ExecuteNonQueryAsync();
                }

                return (true, "Timetable entry deleted successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting timetable: {ex.Message}");
                return (false, "Error deleting timetable entry.");
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<(bool success, string message, List<object>)> GetAllTimetable(){
            try{
                if(_conn.State == ConnectionState.Closed){
                    await _conn.OpenAsync();
                }
                var query = @"
                SELECT t.c_timetable_id, t.c_class_id, c.c_class_name, t.c_teacher_id, te.c_full_name, t.c_subject_id, s.c_subject_name, t.c_time_slot, t.c_day_of_week
                FROM t_timetable t
                JOIN t_classes c ON t.c_class_id = c.c_class_id
                JOIN t_teachers te ON t.c_teacher_id = te.c_teacher_id
                JOIN t_subjects s ON t.c_subject_id = s.c_subject_id
                ORDER BY t.c_timetable_id";
                var timetableList = new List<object>();
                using(var cmd = new NpgsqlCommand(query, _conn)){
                    using(var reader = await cmd.ExecuteReaderAsync()){
                        while(await reader.ReadAsync()){
                            timetableList.Add(new{
                                TimetableId = reader.GetInt32(0),
                                ClassId = reader.GetInt32(1),
                                ClassName = reader.GetString(2),
                                TeacherId = reader.GetInt32(3),
                                TeacherName = reader.GetString(4),
                                SubjectId = reader.GetInt32(5),
                                SubjectName = reader.GetString(6),
                                TimeSlot = reader.GetTimeSpan(7),
                                DayOfWeek = reader.GetString(8)
                            });
                        }
                    }
                }
                return (true, "Timetable retrieved successfully", timetableList);

            }catch(Exception ex){
                Console.WriteLine($"Error in GetAllTimetable: {ex.Message}");
                return (false, "Error retrieving timetable.", null);
                }
                finally{
                    await _conn.CloseAsync();
                }
        }
        
    }
}