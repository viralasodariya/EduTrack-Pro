using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyProject.Core.Models;

namespace MyProject.Core.Interface
{
    public interface IStudentServices
    {

        Task<int> GetStudentIdByUserId(int userId);
        Task<(bool success, string message, object)> GetStudentByIdAsync(int studentId);
        Task<int> GetStudentClassIdByUserId(int userId);
        Task<object> GetClassTeacherByClassId(int classId);
        Task<(bool success, string message)> EditStudentProfileAsync(EditStudentDto model);
        Task<(bool success, string message)> AddGuardianAsync(int studentId, AddGuardianDto guardianDto);
        Task<(bool success, string message, object guardians)> GetGuardiansByStudentIdAsync(int studentId);
        Task<(bool success, string message)> UpdateGuardianAsync(int guardianId, AddGuardianDto guardianDto);
        Task<List<object>> GetTeachersForFeedbackAsync(int classId);

        Task<(bool success, string message)> SubmitTeacherFeedbackAsync(int studentId, SubmitFeedbackDto feedbackDto);
        Task<List<object>> GetMaterialNotificationsAsync(int subjectId = 0);

        //  getall material
        Task<List<object>> GetAllMaterial();
    }
}