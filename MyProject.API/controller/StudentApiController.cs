using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Core.Interface;
using MyProject.Core.Models;

namespace MyProject.API.controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentApiController : ControllerBase
    {
        private readonly IStudentServices _studentServices;

        private readonly IadminInterface _IadminInterface;

        public StudentApiController(IStudentServices studentServices, IadminInterface iadminInterface)
        {
            _studentServices = studentServices;
            _IadminInterface = iadminInterface;
        }

        private int GetUserIdFromToken()
        {

            var userIdClaim = HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }


        // [Authorize(Roles = "student")]
        [HttpGet("GetStudent")]
        public async Task<IActionResult> GetStudent()
        {
            int userId = GetUserIdFromToken();
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Unauthorized" });

            int studentId = await _studentServices.GetStudentIdByUserId(userId);

            if (studentId <= 0)
                return NotFound(new { success = false, message = "student profile not found." });

            var (success, message, students) = await _studentServices.GetStudentByIdAsync(studentId);

            if (!success)
                return NotFound(new { success = false, message });

            return Ok(new { success = true, message, students });

        }

        [Authorize(Roles = "student")]
        [HttpGet("GetclassTimeTable")]
        public async Task<IActionResult> GetTimeTable()
        {
            int userId = GetUserIdFromToken();
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Unauthorized" });

            int classId = await _studentServices.GetStudentClassIdByUserId(userId);

            if (classId <= 0)
                return NotFound(new { success = false, message = "Student class not found." });

            var (succes, message, ListTimeTable) = await _IadminInterface.GetTimetableByClass(classId);
            if (!succes)
            {
                return BadRequest(new { succes, message });
            }

            return Ok(new { succes, message, ListTimeTable });
        }

        [HttpGet("GetClassTeacher")]
        public async Task<IActionResult> GetClassTeacher()
        {
            int userId = GetUserIdFromToken();
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Unauthorized" });

            int classId = await _studentServices.GetStudentClassIdByUserId(userId);

            if (classId <= 0)
                return NotFound(new { success = false, message = "Student class not found." });

            var teacher = await _studentServices.GetClassTeacherByClassId(classId);

            if (teacher == null)
                return NotFound(new { success = false, message = "Class teacher not found." });

            return Ok(new { success = true, message = "Class teacher retrieved successfully.", teacher });
        }

        [HttpPut("EditStudentProfile")]
        public async Task<IActionResult> EditStudentProfile([FromForm] EditStudentDto model)
        {
            int userId = GetUserIdFromToken();
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Unauthorized" });

            int studentId = await _studentServices.GetStudentIdByUserId(userId);
            model.StudentId = studentId;

            // **Find the MVC project's root directory**
            string mvcProjectPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "MyProject.MVC");
            string uploadsFolder = Path.Combine(mvcProjectPath, "wwwroot", "studentProfile");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var file = model.Image;
            string fileExtension = Path.GetExtension(file.FileName);
            string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string relativeFilePath = $"/studentProfile/{uniqueFileName}";
            model.ProfilePicture = relativeFilePath;

            var (success, message) = await _studentServices.EditStudentProfileAsync(model);

            if (!success)
                return BadRequest(new { success = false, message });

            return Ok(new { success = true, message });

        }

        [HttpPost("AddGuardian")]
        public async Task<IActionResult> AddGuardian([FromBody] AddGuardianDto guardianDto)
        {
            int userId = GetUserIdFromToken();
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Unauthorized" });

            int studentId = await _studentServices.GetStudentIdByUserId(userId);
            if (studentId <= 0)
                return NotFound(new { success = false, message = "Student profile not found." });

            var (success, message) = await _studentServices.AddGuardianAsync(studentId, guardianDto);
            if (!success)
                return BadRequest(new { success = false, message });

            return Ok(new { success = true, message });
        }

        [HttpGet("GetGuardians")]
        public async Task<IActionResult> GetGuardians()
        {
            int userId = GetUserIdFromToken();
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Unauthorized" });

            int studentId = await _studentServices.GetStudentIdByUserId(userId);
            if (studentId <= 0)
                return NotFound(new { success = false, message = "Student profile not found." });

            var (success, message, guardians) = await _studentServices.GetGuardiansByStudentIdAsync(studentId);

            if (!success)
                return NotFound(new { success = false, message });

            return Ok(new { success = true, message, guardians });
        }

        [HttpPut("UpdateGuardian/{guardianId}")]
        public async Task<IActionResult> UpdateGuardian(int guardianId, [FromBody] AddGuardianDto guardianDto)
        {
            int userId = GetUserIdFromToken();
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Unauthorized" });

            int studentId = await _studentServices.GetStudentIdByUserId(userId);
            if (studentId <= 0)
                return NotFound(new { success = false, message = "Student profile not found." });

            var (success, message) = await _studentServices.UpdateGuardianAsync(guardianId, guardianDto);

            if (!success)
                return BadRequest(new { success = false, message });

            return Ok(new { success = true, message });
        }

        [HttpGet("GetTeachersForFeedback")]
        public async Task<IActionResult> GetTeachersForFeedback()
        {
            int userId = GetUserIdFromToken();
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Unauthorized" });

            int classId = await _studentServices.GetStudentClassIdByUserId(userId);
            if (classId <= 0)
                return NotFound(new { success = false, message = "Student class not found." });

            var teachers = await _studentServices.GetTeachersForFeedbackAsync(classId);

            if (teachers == null || !teachers.Any())
                return NotFound(new { success = false, message = "No teachers found for feedback." });

            return Ok(new { success = true, message = "Teachers fetched successfully.", teachers });
        }

        [HttpPost("SubmitTeacherFeedback")]
        public async Task<IActionResult> SubmitTeacherFeedback([FromBody] SubmitFeedbackDto feedbackDto)
        {
            int userId = GetUserIdFromToken();
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Unauthorized" });

            int studentId = await _studentServices.GetStudentIdByUserId(userId);
            if (studentId <= 0)
                return NotFound(new { success = false, message = "Student profile not found." });

            var (success, message) = await _studentServices.SubmitTeacherFeedbackAsync(studentId, feedbackDto);

            if (!success)
                return BadRequest(new { success = false, message });

            return Ok(new { success = true, message });
        }

        [HttpGet("GetMaterialNotifications")]
        public async Task<IActionResult> GetMaterialNotifications(int subjectId = 0)
        {
            // int userId = GetUserIdFromToken();
            // if (userId == 0)
            //     return Unauthorized(new { success = false, message = "Unauthorized" });

            var notifications = await _studentServices.GetMaterialNotificationsAsync(subjectId);

            if (notifications == null || !notifications.Any())
                return NotFound(new { success = false, message = "No notifications found." });

            return Ok(new { success = true, message = "Notifications fetched successfully.", notifications });

        }

        [Authorize(Roles = "student")]
        [HttpGet("GetAllMaterials")]
        public async Task<IActionResult> GetAllMaterials()
        {
            int userId = GetUserIdFromToken();
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Unauthorized" });

            var materials = await _studentServices.GetAllMaterial();

            if (materials == null || !materials.Any())
                return NotFound(new { success = false, message = "No materials found." });

            return Ok(new { success = true, message = "Materials fetched successfully.", materials });
        }


    }
}