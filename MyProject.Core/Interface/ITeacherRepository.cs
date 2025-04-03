using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MyProject.Core.Models;

namespace MyProject.Core.Interface
{
    public interface ITeacherRepository
    {


        Task<int> GetTeacherIdByUserId(int userId);
        Task<bool> AddTeacherProfile(int userId, TeacherProfileRequest model);
        Task<bool> UpdateTeacherProfile(int userId, TeacherProfileRequest model);
        Task<object?> GetTeacherProfile(int userId);
        Task<List<object>> GetAssignedSubjects(int teacherId);
        Task<object?> GetAssignedSubjectById(int assignmentId);
        Task<(bool success, string message)> UpdateSubjectProgress(UpdateSubjectProgressRequest request);
        Task<List<object>> GetAssignedStudentsByTeacherId(int teacherId);
        Task<(bool success, string message, List<object> data)> GetUpcomingClasses(int teacherId);
        Task<(bool success, string message)> UploadMaterial(int teacherId, int subjectId, IFormFile file);
        Task<(bool success, string message, List<object> materials)> GetMaterialsByTeacher(int teacherId);
        Task<(bool success, string message)> DeleteMaterial(int materialId);
        Task<(bool success, string message, string className)> GetTeacherClass(int teacherId);



        Task<List<object>> GetTeacherSubjectProgress(int teacherId);


    }
}