let token = localStorage.getItem('authToken');

$(document).ready(function () {
    loadStudentData();
    fetchMaterialNotifications();
    fetchUpcomingClasses();
})

function loadStudentData() {
    $.ajax({
        url: 'http://localhost:5111/api/StudentApi/GetStudent',
        type: 'GET',
        dataType: 'json',
        headers: {
            'Authorization': 'Bearer ' + token
        },
        success: function (data) {
            if (data.success) {
                var student = data.students;

                $("#studentName").text(student.fullName);
                // Update Profile Details

                $('#studentEmail').text(student.email);
                $('#fullName').text(student.fullName);
                $('#dateOfBirth').text(new Date(student.dateOfBirth).toLocaleDateString());
                $('#gender').text(student.gender);
                $('#className').text(student.className);
                $('#enrollmentDate').text(new Date(student.enrollmentDate).toLocaleDateString());
                // Update Profile Picture if available
                if (student.profilePicture) {
                    $('#profileImage').attr('src', student.profilePicture);
                } else {
                    $('#profileImage').attr('src', 'https://ui-avatars.com/api/?name=' + student.fullName + '&size=128');
                }
            } else {
                console.error('Failed to retrieve student data:', data.message);
            }
        },
        error: function (xhr, status, error) {
            console.error('Error fetching data:', error);
        }
    });
}


function fetchMaterialNotifications() {
    $.ajax({
        url: "http://localhost:5111/api/StudentApi/GetMaterialNotifications?subjectId=0",
        type: "GET",
        dataType: "json",
        headers: {
            'Authorization': 'Bearer ' + token
        },
        success: function (response) {
            if (response.success && response.notifications.length > 0) {
                let htmlContent = `<div class="notification-list">`;

               

                // Show only first 3 notifications
                let limitedNotifications = response.notifications.slice(0, 3);

                limitedNotifications.forEach(notification => {
                    const date = new Date(notification.createdAt || new Date()).toLocaleDateString();

                    htmlContent += `
                        <div class="notification-card">
                            <div class="notification-header d-flex justify-content-between align-items-center">
                                <h6 class="mb-0">${notification.subjectName}</h6>
                                <span class="badge bg-primary rounded-pill">New</span>
                            </div>
                            <div class="notification-body">
                                <p>${notification.message}</p>
                                <small class="text-muted"><i class="far fa-clock me-1"></i>${date}</small>
                            </div>
                            <div class="progress mt-2" style="height: 5px;">
                                <div class="progress-bar bg-info" role="progressbar" style="width: 100%" 
                                    aria-valuenow="100" aria-valuemin="0" aria-valuemax="100"></div>
                            </div>
                        </div>
                    `;
                });

                htmlContent += `</div>`;
                $("#notifications-container").html(htmlContent);

                // Add CSS for better styling
                $("<style>")
                    .text(`
                        .notification-list { max-height: 400px; overflow-y: auto; }
                        .notification-card { background-color: #f8f9fa; border-radius: 8px; padding: 12px; margin-bottom: 12px; box-shadow: 0 2px 4px rgba(0,0,0,.05); }
                        .notification-card:hover { box-shadow: 0 4px 8px rgba(0,0,0,.1); transition: all 0.3s; }
                        .notification-header { margin-bottom: 8px; }
                        .notification-body p { margin-bottom: 5px; }
                    `)
                    .appendTo("head");
            } else {
                $("#notifications-container").html(`
                    <div class="text-center p-4">
                        <i class="fas fa-bell-slash text-muted mb-3" style="font-size: 2rem;"></i>
                        <p class="text-muted">No new notifications</p>
                    </div>
                `);
            }
        },
        error: function () {
            $("#notifications-container").html(`
                <div class="alert alert-danger" role="alert">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    Failed to load notifications. Please try again later.
                </div>
            `);
        }
    });
}


function fetchUpcomingClasses() {
    $.ajax({
        url: "http://localhost:5111/api/StudentApi/GetclassTimeTable",
        type: "GET",
        dataType: "json",
        headers: {
            "Authorization": "Bearer " + token
        },
        success: function (response) {
            if (response.succes && response.listTimeTable.length > 0) {
                let htmlContent = "";

                // Show only the first 4 records
                let upcomingClasses = response.listTimeTable.slice(0, 4);

                upcomingClasses.forEach(classItem => {
                    let timeColor = getTimeColor(classItem.timeSlot);

                    htmlContent += `
                        <tr>
                            <td>${classItem.subjectName}</td>
                            <td>${classItem.teacherName}</td>
                            <td><span class="badge ${timeColor}">${classItem.timeSlot}</span></td>
                            <td>${classItem.dayOfWeek}</td>
                        </tr>
                    `;
                });

                $("#class-schedule-container").html(htmlContent);
            } else {
                $("#class-schedule-container").html(`<tr><td colspan="4" class="text-center text-muted">No upcoming classes.</td></tr>`);
            }
        },
        error: function () {
            $("#class-schedule-container").html(`<tr><td colspan="4" class="text-center text-danger">Failed to load schedule.</td></tr>`);
        }
    });
}

// Assign different colors for different days
function getTimeColor(time) {
    let hour = parseInt(time.split(":")[0]);

    if (hour >= 9 && hour < 11) return "bg-primary";  // 9 AM - 11 AM (Blue)
    if (hour >= 11 && hour < 13) return "bg-success"; // 11 AM - 1 PM (Green)
    if (hour >= 13 && hour < 15) return "bg-warning"; // 1 PM - 3 PM (Yellow)
    if (hour >= 15 && hour <= 17) return "bg-danger"; // 3 PM - 5 PM (Red)

    return "bg-secondary"; // Default (if time is outside 9 AM - 5 PM)
}
