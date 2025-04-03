let token = localStorage.getItem('authToken');
let allStudents = [];

$(document).ready(function () {
    // Add search container to the page
    $("#studentContainer").prepend(`
        <div class="row">
            <div class="col-12">
                <div class="page-header">
                    <h2>My Students</h2>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-12">
                <div class="search-container">
                    <div class="input-group">
                        <input type="text" id="searchStudent" class="form-control" placeholder="Search students by name or class...">
                        <button class="btn btn-primary" type="button">
                            <i class="fas fa-search"></i> Search
                        </button>
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-12 student-count ms-3" id="studentCount"></div>
        </div>
    `);

    // Fetch students data
    $.ajax({
        url: "http://localhost:5111/api/TeacherApi/Teacher/assignedStudents",
        type: "GET",
        dataType: "json",
        headers: {
            "Authorization": `Bearer ${token}`
        },
        success: function (response) {
            if (response.success && Array.isArray(response.data) && response.data.length > 0) {
                allStudents = response.data;
                displayStudents(allStudents);
                $("#studentCount").html(`<i class="fas fa-users"></i> Total Students: ${allStudents.length}`);
            } else {
                $("#studentList").html(`
                    <div class="col-12 text-center">
                        <div class="alert alert-warning" role="alert">
                            No students allocated.
                        </div>
                    </div>
                `);
            }
        },
        error: function (xhr, status, error) {
            console.error("Error fetching students:", error);
            $("#studentList").html(`
                <div class="col-12 text-center">
                    <div class="alert alert-danger" role="alert">
                        Error loading students. Please try again later.
                    </div>
                </div>
            `);
        }
    });

    // Add search functionality
    $("#searchStudent").on("keyup", function () {
        let searchTerm = $(this).val().toLowerCase();
        if (searchTerm.length > 0) {
            let filteredStudents = allStudents.filter(student =>
                student.fullName.toLowerCase().includes(searchTerm) ||
                student.className.toLowerCase().includes(searchTerm)
            );
            displayStudents(filteredStudents);
            $("#studentCount").html(`<i class="fas fa-search"></i> Found ${filteredStudents.length} student(s)`);
        } else {
            displayStudents(allStudents);
            $("#studentCount").html(`<i class="fas fa-users"></i> Total Students: ${allStudents.length}`);
        }
    });
});

function displayStudents(students) {
    let studentList = $("#studentList");
    studentList.empty();

    if (students.length === 0) {
        studentList.html(`
            <div class="col-12 text-center">
                <div class="alert alert-info" role="alert">
                    <i class="fas fa-search"></i> No students found matching your search criteria.
                </div>
            </div>
        `);
        return;
    }

    students.forEach(student => {
        let statusBadge = student.status
            ? `<span class="badge bg-success">Active</span>`
            : `<span class="badge bg-danger">Inactive</span>`;

        let studentCard = `
            <div class="col-md-6 col-lg-4 mb-4">
                <div class="card student-card shadow">
                    <div class="card-header d-flex align-items-center">
                        <div class="avatar-placeholder">${student.fullName.charAt(0)}</div>
                        <div>
                            <h5 class="mb-0">${student.fullName}</h5>
                            ${statusBadge}
                        </div>
                    </div>
                    <div class="card-body">
                        <div class="student-info">
                            <span class="info-label">Class:</span>
                            <span class="info-value">${student.className}</span>
                        </div>
                        <div class="student-info">
                            <span class="info-label">DOB:</span>
                            <span class="info-value">${formatDate(student.dateOfBirth)}</span>
                        </div>
                        <div class="student-info">
                            <span class="info-label">Enrollment:</span>
                            <span class="info-value">${formatDate(student.enrollmentDate)}</span>
                        </div>
                        <div class="student-info">
                            <span class="info-label">Gender:</span>
                            <span class="info-value">${student.gender}</span>
                        </div>
                        <div class="mt-3">
                            <button class="btn btn-sm btn-outline-primary" data-id="${student.id}">View Details</button>
                        </div>
                    </div>
                </div>
            </div>
        `;
        studentList.append(studentCard);
    });
}

function formatDate(dateString) {
    let date = new Date(dateString);
    return date.toISOString().split("T")[0];
}
