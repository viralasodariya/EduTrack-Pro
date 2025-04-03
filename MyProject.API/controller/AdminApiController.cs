using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using MyProject.Core.Interface;
using MyProject.Core.Models;

namespace MyProject.API.controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminApiController : ControllerBase
    {
        private readonly IadminInterface _IadminInterface;
        public AdminApiController(IadminInterface iadminInterface)
        {
            _IadminInterface = iadminInterface;
        }

        // class add get
        [Authorize(Roles = "admin")]
        [HttpPost("createClass")]
        public async Task<IActionResult> CreateClass([FromBody] string className)
        {
            try
            {
                var classname = className.ToLower();
                var (success, message) = await _IadminInterface.CreateClass(classname);
                if (success)
                {
                    return Ok(new { success, message });
                }
                return BadRequest(new { success, message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while creating class." });
            }
        }

        [HttpGet("get-class")]
        public async Task<IActionResult> GetAllClasses()
        {
            try
            {
                var (success, message, classes) = await _IadminInterface.GetAllClasses();
                if (success)
                {
                    return Ok(new { success, message, classes });
                }
                return BadRequest(new { success, message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving classes." });
            }
        }


        // assignclasstoTeacher
        [HttpPost("AssignClassToTeacher")]
        public async Task<IActionResult> AssignClassToTeacher([FromBody] AssignClassModel model)
        {
            try
            {
                var (success, message) = await _IadminInterface.AssignClassToTeacher(model.ClassId, model.TeacherId);
                if (success)
                {
                    return Ok(new { success, message });
                }
                else
                {
                    return BadRequest(new { success, message });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while assigning the class." });
            }
        }

        [HttpGet("UnassignedTeachersAndClasses")]
        public async Task<IActionResult> GetUnassignedTeachersAndClasses()
        {
            try
            {
                var (success, message, teachers, classes) = await _IadminInterface.GetUnassignedTeachersAndClasses();
                if (success)
                {
                    return Ok(new { success, message, teachers, classes });
                }
                return BadRequest(new { success, message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving unassigned teachers." });
            }
        }

        [HttpGet("GetAssignedClassesAndTeachers")]
        public async Task<IActionResult> GetAssignedClassesAndTeachers()
        {
            try
            {
                var (success, message, data) = await _IadminInterface.GetAssignedClassesAndTeachers();
                if (success)
                {
                    return Ok(new { success, message, data });
                }
                else
                {
                    return NotFound(new { success, message });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while fetching assigned classes." });
            }
        }


        // add subject
        [HttpPost("addSubject")]
        public async Task<IActionResult> AddSubject([FromBody] SubjectRequestModel model)
        {
            if (string.IsNullOrWhiteSpace(model.SubjectName))
            {
                return BadRequest(new { success = false, message = "Subject name is required" });
            }
            string subjectNameLower = model.SubjectName.ToLower();
            var (success, message) = await _IadminInterface.AddSubject(subjectNameLower);

            if (success)
            {
                return Ok(new { success, message });
            }
            return BadRequest(new { success, message });
        }

        [HttpPost("assign-subject")]
        public async Task<IActionResult> AssignSubject([FromBody] AssignSubjectModel request)
        {
            var result = await _IadminInterface.AssignSubjectToTeacher(request.TeacherId, request.SubjectId);

            if (result.success)
            {
                return Ok(new { success = true, message = result.message });
            }
            return BadRequest(new { success = false, message = result.message });
        }

        [HttpGet("GetAllTeachersWithUnassignedSubjects")]
        public async Task<IActionResult> GetAllTeachersWithUnassignedSubjects()
        {
            try
            {
                var (success, message, teacherSubjectData) = await _IadminInterface.GetAllTeachersWithUnassignedSubjects();
                if (success)
                {
                    return Ok(new { success, message, teacherSubjectData });
                }
                else
                {
                    return BadRequest(new { success, message });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while fetching unassigned subjects for teachers." });
            }
        }

        [HttpGet("GetAllTeachersWithSubjects")]
        public async Task<IActionResult> GetAllTeachersWithSubjects()
        {
            var result = await _IadminInterface.GetAllTeachersWithAssignedSubjects();

            if (result.success)
            {
                return Ok(new { success = true, message = result.message, teacherSubjectData = result.teacherSubjectData });
            }
            return BadRequest(new { success = false, message = result.message });
        }



        
        [HttpPost("CreateStudent")]
        public async Task<IActionResult> create([FromBody] StudentModel studentModel)
        {
            Console.WriteLine("CreateStudent");
            try
            {
                var (success, message, student) = await _IadminInterface.AddStudent(studentModel);
                if (success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Student created successfully",
                        student,
                        loginDetails = new
                        {
                            email = student.Email,
                            password = student.Password
                        }
                    });
                }
                else
                {
                    return BadRequest(new { success = false, message });

                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while creating the student." });
            }
        }

        // [Authorize(Roles = "admin")]
        [HttpGet("GetAllStudent")]
        public async Task<IActionResult> GetAllStudent()
        {
            try
            {
                var (succes, message, students) = await _IadminInterface.GetAllStudent();
                if (succes)
                {
                    return Ok(new { succes, message, students });
                }
                else
                {
                    return BadRequest(new { succes, message, students });
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return StatusCode(500, new { succes = false, message = "An error occurred while fetching the student." });
            }
        }

        // [Authorize(Roles = "admin")]
        [HttpPut("students/{id}")]
        public async Task<IActionResult> EditStudent(int id, [FromBody] StudentModel studentModel)
        {
            try
            {
                studentModel.StudentId = id;
                var (success, message) = await _IadminInterface.UpdateStudent(studentModel);
                if (success)
                    return Ok(new { success = true, message });
                return BadRequest(new { success = false, message });

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateStudent: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while updating student details." });
            }
        }

        // [Authorize(Roles = "admin")]
        [HttpGet("students/{id}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            try
            {
                var (success, message, student) = await _IadminInterface.GetStudentById(id);

                if (!success)
                    return NotFound(new { success = false, message });

                return Ok(new { success = true, student });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetStudentById: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while fetching student details." });
            }
        }

        // [Authorize(Roles = "admin")]
        [HttpDelete("students/{id}")]
        public async Task<IActionResult> DeleteStudentById(int id)
        {
            try
            {
                var (success, message) = await _IadminInterface.DeleteStudent(id);

                if (success)
                    return Ok(new { success = true, message });

                return NotFound(new { success = false, message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteStudent: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the student." });
            }
        }

        // [Authorize(Roles = "admin")]
        [HttpPut("dashboard/approve-student")]
        public async Task<IActionResult> ApproveStudent([FromBody] StudentStatusUpdateModel model)
        {
            try
            {
                var success = await _IadminInterface.ApproveStudent(model.StudentId, model.NewStatus);

                if (!success)
                    return BadRequest(new { success = false, message = "Failed to approve student." });

                return Ok(new { success = true, message = "Student approved successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ApproveStudent: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while approving student." });
            }
        }




        // [Authorize(Roles = "admin")]
        [HttpGet("dashboard/hierarchy")]
        public async Task<IActionResult> GetSchoolHierarchy()
        {
            try
            {
                var (success, message, hierarchy) = await _IadminInterface.GetSchoolHierarchy();

                if (!success)
                    return NotFound(new { success = false, message = "No hierarchy data available." });

                return Ok(new { success = true, hierarchy });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSchoolHierarchy: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while fetching school hierarchy." });
            }
        }

        // [Authorize(Roles = "admin")]
        [HttpGet("dashboard/student-count")]
        public async Task<IActionResult> GetStudentCountPerClass()
        {
            try
            {
                var (success, studentCounts) = await _IadminInterface.GetStudentCountPerClass();

                if (!success)
                    return NotFound(new { success = false, message = "No student data available." });

                return Ok(new { success = true, studentCounts });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetStudentCountPerClass: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while fetching student counts." });
            }
        }



        // [Authorize(Roles = "admin")]
        [HttpGet("dashboard/GetUnreadNotifications")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            try
            {
                var (success, notifications) = await _IadminInterface.GetUnreadNotifications();

                if (!success)
                    return NotFound(new { success = false, message = "No new notifications available." });

                return Ok(new { success = true, notifications });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUnreadNotifications: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while fetching notifications." });
            }
        }


        // [Authorize(Roles = "admin")]
        [HttpGet("dashboard/GetreadNotifications")]
        public async Task<IActionResult> GetreadNotifications()
        {
            try
            {
                var (success, notifications) = await _IadminInterface.GetreadNotifications();

                if (!success)
                    return NotFound(new { success = false, message = "No new notifications available." });

                return Ok(new { success = true, notifications });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetreadNotifications: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while fetching notifications." });
            }
        }


        // [Authorize(Roles = "admin")]
        [HttpPut("dashboard/notifications/mark-read/{notificationId}")]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            try
            {
                var success = await _IadminInterface.MarkNotificationAsRead(notificationId);

                if (!success)
                    return BadRequest(new { success = false, message = "Failed to mark the notification as read." });

                return Ok(new { success = true, message = "Notification marked as read." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MarkNotificationAsRead: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while updating the notification." });
            }
        }



        // [Authorize(Roles = "admin")]
        [HttpGet("export/students/pdf")]
        public async Task<IActionResult> ExportStudentsToPDF()
        {
                    try
            {
                var pdfBytes = await _IadminInterface.ExportStudentsToPDF();

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return NotFound("No data available to export.");
                }

                // Set the Content-Disposition header to force download with the correct filename
                var contentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = "Students.pdf"
                };
                Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                // Log the error (replace with your logging framework)
                Console.WriteLine($"Error in ExportStudentsToPDF (Controller): {ex.Message}");
                return StatusCode(500, "An error occurred while exporting students to PDF.");
            }
        }

        // [Authorize(Roles = "admin")]
        [HttpGet("export/students/excel")]
        public async Task<IActionResult> ExportStudentsToExcel()
        {
            try
            {
                var fileBytes = await _IadminInterface.ExportStudentsToExcel();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Student_List.xlsx");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ExportStudentsToExcel: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while exporting students to Excel." });
            }
        }



        // [Authorize(Roles = "admin")]
        [HttpGet("dashboard/theme/{adminId}")]
        public async Task<IActionResult> GetTheme(int adminId)
        {
            try
            {
                var theme = await _IadminInterface.GetTheme(adminId);

                if (theme == null)
                {
                    return Ok(new { success = true, theme = new ThemeModel { AdminId = adminId, ThemeName = "Light" } });
                }

                return Ok(new { success = true, theme });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTheme: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while fetching the theme." });
            }
        }

        // [Authorize(Roles = "admin")]
        [HttpPost("dashboard/theme")]
        public async Task<IActionResult> ToggleTheme([FromBody] ThemeModel themeModel)
        {
            try
            {
                var success = await _IadminInterface.ToggleTheme(themeModel);

                if (!success)
                    return BadRequest(new { success = false, message = "Failed to update theme." });

                return Ok(new { success = true, message = "Theme changed successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ToggleTheme: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while changing the theme." });
            }
        }



        // [Authorize(Roles = "admin")]
        [HttpGet("dashboard/progress/all")]
        public async Task<IActionResult> GetAllSubjectProgress()
        {
            try
            {
                var progressList = await _IadminInterface.GetSubjectProgress();
                return Ok(new { success = true, data = progressList });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

      

        [HttpPost("TimeTable/create")]
        public async Task<IActionResult> CreateTimetable([FromBody] TimetableModel request)
        {
            if (request == null)
            {
                return BadRequest(new { sucess = false, message = "Invalid request data." });
            }

            var result = await _IadminInterface.CreateTimetable(
                request.ClassId,
                request.SubjectId,
                request.TeacherId,
                request.TimeSlot,
                request.DayOfWeek
            );

            if (!result.succes)
            {
                return BadRequest(new { success = false, message = result.message });
            }

            return Ok(new { success = true, message = result.message });
        }

        [HttpGet("TimeTable/GetClassTimeTable")]
        public async Task<IActionResult> GetTimetableByClass(int classId)
        {
            var (succes, message, ListTimeTable) = await _IadminInterface.GetTimetableByClass(classId);
            if (!succes)
            {
                return BadRequest(new { succes, message });
            }

            return Ok(new { succes, message, ListTimeTable });
        }

        [HttpPut("timetable/update/{timetableId}")]
        public async Task<IActionResult> UpdateTimetable(int timetableId, [FromBody] TimetableModel request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "Invalid request data." });
            }

            var (success, message) = await _IadminInterface.UpdateTimetable(
                timetableId,
                request.ClassId,
                request.SubjectId,
                request.TeacherId,
                request.TimeSlot,
                request.DayOfWeek
            );

            if (success)
            {
                return Ok(new { success = true, message });
            }
            else
            {
                return BadRequest(new { success = false, message });
            }
        }

        [HttpDelete("timetable/delete/{timetableId}")]
        public async Task<IActionResult> DeleteTimetable(int timetableId)
        {
            var (success, message) = await _IadminInterface.DeleteTimetable(timetableId);

            if (success)
            {
                return Ok(new { success = true, message });
            }
            else
            {
                return BadRequest(new { success = false, message });
            }
        }

        [HttpGet("timetable/all")]
        public async Task<IActionResult> GetAllTimetable()
        {
            var (success, message, timetables) = await _IadminInterface.GetAllTimetable();

            if (success)
            {
                return Ok(new { success, message, timetables });
            }

            return BadRequest(new { success, message });
        }

    }
}