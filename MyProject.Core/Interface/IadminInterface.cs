using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyProject.Core.Models;

namespace MyProject.Core.Interface
{
    public interface IadminInterface
    {
        // this is done
        // class
        Task<(bool success, string message)> CreateClass(string className);
        Task<(bool success, string message, List<object>)> GetAllClasses();
        Task<(bool success, string message, List<object>)> GetAssignedClassesAndTeachers();
        Task<(bool success, string message, List<object>, List<object>)> GetUnassignedTeachersAndClasses();
        Task<(bool success, string message)> AssignClassToTeacher(int classId, int teacherId);


        // this is done
        // subject 
        Task<(bool success, string message)> AddSubject(string subjectName);
        Task<(bool success, string message)> AssignSubjectToTeacher(int teacherId, int subjectId);
        Task<(bool success, string message, List<object> teacherSubjectData)> GetAllTeachersWithUnassignedSubjects();
        Task<(bool success, string message, List<object> teacherSubjectData)> GetAllTeachersWithAssignedSubjects();



        // this is done
        // student add admin
        Task<(bool success, string message, StudentModel)> AddStudent(StudentModel studentModel);
        Task<(bool success, string message, List<object>)> GetAllStudent();
        Task<(bool success, string message)> UpdateStudent(StudentModel studentModel);
        Task<(bool success, string message, object)> GetStudentById(int Id);
        Task<(bool success, string message)> DeleteStudent(int Id);
        Task<bool> ApproveStudent(int studentId, bool newStatus);
        Task<byte[]> ExportStudentsToPDF();
        Task<byte[]> ExportStudentsToExcel();

        // this is done
        // dashboard 
        Task<(bool succes, object principalName)> GetPrincipalName();
        Task<(bool success, string message, object data)> GetSchoolHierarchy();
        Task<(bool success, List<object> studentCounts)> GetStudentCountPerClass();
        Task<(bool success, List<object> notifications)> GetUnreadNotifications();
        Task<(bool success, List<object> notifications)> GetreadNotifications();
        Task<bool> MarkNotificationAsRead(int notificationId);
        // not add current 
        Task<ThemeModel> GetTheme(int adminId);
        Task<bool> ToggleTheme(ThemeModel theme);



        Task<List<object>> GetSubjectProgress();
        
        


        // time table
        Task<(bool succes, string message)> CreateTimetable(int classId, int subjectId, int teacherId, TimeSpan timeSlot, string dayOfWeek);
        Task<(bool succes, string message, List<object>)> GetTimetableByClass(int classId);
        Task<(bool success, string message)> UpdateTimetable(int timetableId, int classId, int subjectId, int teacherId, TimeSpan timeSlot, string dayOfWeek);
        Task<(bool success, string message)> DeleteTimetable(int timetableId);

        // Get all timetable 
        Task<(bool success, string message, List<object>)> GetAllTimetable();

    }
}