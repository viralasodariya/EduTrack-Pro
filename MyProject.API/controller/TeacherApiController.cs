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
    public class TeacherApiController : ControllerBase
    {
        private readonly ITeacherRepository _teacherRepository;

        public TeacherApiController(ITeacherRepository teacherRepository)
        {
            _teacherRepository = teacherRepository;

        }

        private int GetUserIdFromToken()
        {

            var userIdClaim = HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        [Authorize(Roles = "teacher")]
        [HttpGet("Teacher/getProfile")]
        public async Task<IActionResult> GetProfile()
        {

            int userId = GetUserIdFromToken();
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Unauthorized" });

            var profile = await _teacherRepository.GetTeacherProfile(userId);
            if (profile == null)
                return NotFound(new { success = false, message = "Profile not found" });

            return Ok(new { success = true, profile });
        }


        [Authorize(Roles = "teacher")]
        [HttpPost("Teacher/addProfile")]
        public async Task<IActionResult> AddProfile([FromBody] TeacherProfileRequest model)
        {
            int userId = GetUserIdFromToken();
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Unauthorized" });

            var profileExists = await _teacherRepository.GetTeacherProfile(userId);
            if (profileExists != null)
                return BadRequest(new { success = false, message = "Profile already exists" });

            var success = await _teacherRepository.AddTeacherProfile(userId, model);
            if (success)
                return Ok(new { success = true, message = "Profile added successfully" });

            return BadRequest(new { success = false, message = "Failed to add profile" });
        }

        [Authorize(Roles = "teacher")]
        [HttpPut("Teacher/updateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] TeacherProfileRequest model)
        {
            int userId = GetUserIdFromToken();
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Unauthorized" });

            var profileExists = await _teacherRepository.GetTeacherProfile(userId);
            if (profileExists == null)
                return NotFound(new { success = false, message = "Profile not found" });

            var success = await _teacherRepository.UpdateTeacherProfile(userId, model);
            if (success)
                return Ok(new { success = true, message = "Profile updated successfully" });

            return BadRequest(new { success = false, message = "Failed to update profile" });
        }


        [Authorize(Roles = "teacher")]
        [HttpGet("Teacher/assignedSubjects")]
        public async Task<IActionResult> GetAssignedSubjects()
        {
            try
            {
                int userId = GetUserIdFromToken();

                if (userId <= 0)
                {
                    return Unauthorized(new { success = false, message = "Invalid or missing token." });
                }

                // Get the teacher ID using the user ID
                int teacherId = await _teacherRepository.GetTeacherIdByUserId(userId);
                if (teacherId <= 0)
                {
                    return NotFound(new { success = false, message = "Teacher profile not found." });
                }

                var subjects = await _teacherRepository.GetAssignedSubjects(teacherId);

                if (subjects == null || !subjects.Any())
                {
                    return Ok(new { success = false, message = "No assigned subjects found." });
                }

                return Ok(new
                {
                    success = true,
                    message = "Subjects retrieved successfully.",
                    data = subjects
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [Authorize(Roles = "teacher")]
        [HttpPut("Teacher/updateSubjectProgress")]
        public async Task<IActionResult> UpdateSubjectProgress([FromBody] UpdateSubjectProgressRequest model)
        {
            try
            {
                // Ensure the assignment exists
                var existingAssignment = await _teacherRepository.GetAssignedSubjectById(model.AssignmentId);
                if (existingAssignment == null)
                {
                    return NotFound(new { success = false, message = "Assignment not found." });
                }

                // Perform the update
                var (success, message) = await _teacherRepository.UpdateSubjectProgress(model);

                if (success)
                {
                    return Ok(new { success, message = "Progress updated successfully." });
                }
                else
                {
                    return StatusCode(500, new { success = false, message });
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while updating progress." });
            }
        }

        [Authorize(Roles = "teacher")]
        [HttpGet("Teacher/assignedStudents")]
        public async Task<IActionResult> GetAssignedStudentsByTeacherId()
        {
            try
            {

                int userId = GetUserIdFromToken();

                if (userId <= 0)
                    return Unauthorized(new { success = false, message = "Invalid or missing token." });


                int teacherId = await _teacherRepository.GetTeacherIdByUserId(userId);
                if (teacherId <= 0)
                    return NotFound(new { success = false, message = "Teacher profile not found." });

                var students = await _teacherRepository.GetAssignedStudentsByTeacherId(teacherId);

                if (students == null || students.Count == 0)
                {
                    return NotFound(new { success = false, message = "No students assigned to this teacher." });
                }

                return Ok(new { success = true, data = students });
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while GetAssignedStudentsByTeacherId." });
            }
        }

        [Authorize(Roles = "teacher")]
        [HttpGet("teacher/upcoming-classes")]
        public async Task<IActionResult> GetUpcomingClasses()
        {
            int userId = GetUserIdFromToken();

            if (userId <= 0)
            {
                return Unauthorized(new { success = false, message = "Invalid or missing token." });
            }

            // Get the teacher ID using the user ID
            int teacherId = await _teacherRepository.GetTeacherIdByUserId(userId);
            if (teacherId <= 0)
            {
                return NotFound(new { success = false, message = "Teacher profile not found." });
            }




            var (success, message, classes) = await _teacherRepository.GetUpcomingClasses(teacherId);
            return success
                ? Ok(new { success, message, data = classes })
                : BadRequest(new { success, message, data = classes });
        }

        [Authorize(Roles = "teacher")]
        [HttpPost("teacher/upload-material")]
        public async Task<IActionResult> UploadMaterial([FromForm] int subjectId, IFormFile file)
        {
            int userId = GetUserIdFromToken();

            if (userId <= 0)
            {
                return Unauthorized(new { success = false, message = "Invalid or missing token." });
            }

            // Get the teacher ID using the user ID
            int teacherId = await _teacherRepository.GetTeacherIdByUserId(userId);
            if (teacherId <= 0)
            {
                return NotFound(new { success = false, message = "Teacher profile not found." });
            }

            var (success, message) = await _teacherRepository.UploadMaterial(teacherId, subjectId, file);

            if (success)
                return Ok(new { success, message });
            else
                return BadRequest(new { success, message });
        }

        [Authorize(Roles = "teacher")]
        [HttpGet("teacher/get-materials")]
        public async Task<IActionResult> GetMaterials()
        {
            int userId = GetUserIdFromToken();

            if (userId <= 0)
            {
                return Unauthorized(new { success = false, message = "Invalid or missing token." });
            }

            // Get the teacher ID using the user ID
            int teacherId = await _teacherRepository.GetTeacherIdByUserId(userId);
            if (teacherId <= 0)
            {
                return NotFound(new { success = false, message = "Teacher profile not found." });
            }
            var (success, message, data) = await _teacherRepository.GetMaterialsByTeacher(teacherId);

            if (success)
                return Ok(new { success, message, data });
            else
                return BadRequest(new { success, message });
        }

        [Authorize(Roles = "teacher")]
        [HttpDelete("teacher/delete-material/{materialId}")]
        public async Task<IActionResult> DeleteMaterial(int materialId)
        {
            int userId = GetUserIdFromToken();

            if (userId <= 0)
            {
                return Unauthorized(new { success = false, message = "Invalid or missing token." });
            }

            // Get the teacher ID using the user ID
            int teacherId = await _teacherRepository.GetTeacherIdByUserId(userId);
            if (teacherId <= 0)
            {
                return NotFound(new { success = false, message = "Teacher profile not found." });
            }

            var (success, message) = await _teacherRepository.DeleteMaterial(materialId);

            if (success)
                return Ok(new { success, message });
            else
                return BadRequest(new { success, message });
        }

        [Authorize(Roles = "teacher")]
        [HttpGet("teacher/progress/")]
        public async Task<IActionResult> GetTeacherSubjectProgress()
        {
            try
            {
                int userId = GetUserIdFromToken();

                if (userId <= 0)
                {
                    return Unauthorized(new { success = false, message = "Invalid or missing token." });
                }

                int teacherId = await _teacherRepository.GetTeacherIdByUserId(userId);
                if (teacherId <= 0)
                {
                    return NotFound(new { success = false, message = "Teacher profile not found." });
                }

                var progressList = await _teacherRepository.GetTeacherSubjectProgress(teacherId);
                return Ok(new { success = true, data = progressList });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [Authorize(Roles = "teacher")]
        [HttpGet("teacher/class")]
        public async Task<IActionResult> GetTeacherClass()
        {
            try
            {
                int userId = GetUserIdFromToken();

                if (userId <= 0)
                {
                    return Unauthorized(new { success = false, message = "Invalid or missing token." });
                }

                int teacherId = await _teacherRepository.GetTeacherIdByUserId(userId);
                if (teacherId <= 0)
                {
                    return NotFound(new { success = false, message = "Teacher profile not found." });
                }

                var (success, message, className) = await _teacherRepository.GetTeacherClass(teacherId);
                return success
                    ? Ok(new { success, message, className })
                    : BadRequest(new { success, message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }


    }
}