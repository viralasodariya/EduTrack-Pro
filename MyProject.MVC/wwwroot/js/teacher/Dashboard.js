let token = localStorage.getItem("authToken")
$(document).ready(function () {
    loadTeacherProfile()
    StudentCount()
    SubjectCount()
    AssignmentCount()
    className()
    UpComingClassDataFetch()
    loadTeacherProgressChart()

})

function loadTeacherProfile() {
    $.ajax({
        url: "http://localhost:5111/api/TeacherApi/Teacher/getProfile",
        type: "GET",
        contentType: "application/json",
        headers: { "Authorization": "Bearer " + token },
        success: function (response) {
            if (response.success && response.profile) {
                renderTeacherProfile(response.profile);
            } else {
                renderTeacherProfile(null);
            }
        },
        error: function (error) {
            renderTeacherProfile(null)
        }

    })
}

function renderTeacherProfile(profile) {
    if (!profile) {
        $("#teacherProfileCard").html(`
            <div class="card text-center" style="padding: 20px;">
                <p>No Profile Found</p>
                <a href="/Teacher/AddProfile" class="btn btn-primary">Add Profile</a>
            </div>
        `);
    } else {
        $("#teacherName").text(profile.fullName);
        $("#teacherPhone").html(`<strong>Phone:</strong> ${profile.phoneNumber}`);
        $("#teacherDOB").html(`<strong>Date of Birth:</strong> ${new Date(profile.dateOfBirth).toLocaleDateString()}`);
        $("#teacherQualification").html(`<strong>Qualification:</strong> ${profile.qualification}`);
        $("#teacherExperience").html(`<strong>Experience:</strong> ${profile.experienceYears} years`);
        $("#teacherImage").attr("src", `https://ui-avatars.com/api/?name=${profile.fullName}&background=random&size=50`);
    }
}

function StudentCount() {
    $.ajax({
        url: "http://localhost:5111/api/TeacherApi/Teacher/assignedStudents",
        type: "GET",
        contentType: "application/json",
        headers: { "Authorization": "Bearer " + token },
        success: function (response) {
            if (response.data) {
                $("#studentCount").text(response.data.length);
            } else {
                $("#studentCount").text("0");
            }
        },
        error: function (error) {
            $("#studentCount").text("0");
        }
    })
}

function SubjectCount() {
    $.ajax({
        url: "http://localhost:5111/api/TeacherApi/Teacher/assignedSubjects",
        type: "GET",
        contentType: "application/json",
        headers: { "Authorization": "Bearer " + token },
        success: function (response) {
            if (response.data) {
                $("#subjectCount").text(response.data.length);
            } else {
                $("#subjectCount").text("0");
            }
        },
        error: function (error) {
            $("#subjectCount").text("0");
        }
    })
}

function AssignmentCount() {
    $.ajax({
        url: "http://localhost:5111/api/TeacherApi/teacher/get-materials",
        type: "GET",
        contentType: "application/json",
        headers: { "Authorization": "Bearer " + token },
        success: function (response) {
            if (response.data) {
                $("#assignmentCount").text(response.data.length);
            } else {
                $("#assignmentCount").text("0");
            }
        },
        error: function (error) {
            $("#assignmentCount").text("0");
        }
    })
}

function className() {
    $.ajax({
        url: "http://localhost:5111/api/TeacherApi/teacher/class",
        type: "GET",
        contentType: "application/json",
        headers: { "Authorization": "Bearer " + token },
        success: function (response) {

            if (response.className) {
                $("#className").text(response.className);
            } else {
                $("#className").text("No Class Assigned");

            }
        },
        error: function (error) {
            $("#className").text("No Class Assigned");
        }

    })
}


function UpComingClassDataFetch() {
    $.ajax({
        url: "http://localhost:5111/api/TeacherApi/teacher/upcoming-classes",
        type: "GET",
        contentType: "application/json",
        headers: { "Authorization": "Bearer " + token },
        success: function (response) {
            console.log(response.data)
            if (response.data) {
                let NextThree = getNextThreeClasses(response.data);
                renderUpcomingClasses(NextThree);
            }
        }
    })
}

function getNextThreeClasses(timetableData) {
    let daysOfWeek = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
    let now = new Date();
    let todayIndex = now.getDay(); // 0 = Sunday, 6 = Saturday
    let currentHour = now.getHours(); // Get current hour (24-hour format)

    let sortedClasses = [];

    // Loop through the next 7 days (including today)
    for (let i = 0; i < 7; i++) {
        let checkDay = daysOfWeek[(todayIndex + i) % 7];

        // Get valid classes for the current day
        let dayClasses = timetableData
            .filter(cls => cls.dayOfWeek.toLowerCase() === checkDay.toLowerCase())
            .map(cls => {
                let hour = parseInt(cls.timeSlot, 10); // Extract hour

                // Convert 12-hour format to 24-hour format
                if (hour >= 1 && hour <= 5) {
                    hour += 12; // Convert PM times (1-5) to 24-hour format
                }

                return { ...cls, hour }; // Add converted 24-hour time
            })
            .filter(cls => {
                // Ignore past times if today
                if (i === 0 && cls.hour <= currentHour) return false;

                // Only allow 9 AM - 5 PM
                return cls.hour >= 9 && cls.hour <= 17;
            })
            .sort((a, b) => a.hour - b.hour); // Sort by time (AM before PM)

        sortedClasses.push(...dayClasses);
        if (sortedClasses.length >= 3) break; // Stop when 3 classes are found
    }

    return sortedClasses.slice(0, 3); // Return only 3 classes
}

function renderUpcomingClasses(classes) {
    let tbody = document.getElementById("classTableBody"); // Get the table body
    tbody.innerHTML = ""; // Clear previous content

    classes.forEach(cls => {
        let row = document.createElement("tr"); // Create a new row

        row.innerHTML = `
                    <td>${cls.className}</td>
                    <td>${cls.subjectName}</td>
                    <td>${formatTimeWithAmPm(cls.timeSlot)}</td>
                    <td>${cls.dayOfWeek}</td>
                `;

        tbody.appendChild(row); // Append the row to the table
    });

}

function formatTimeWithAmPm(timeSlot) {
    let hour = parseInt(timeSlot); // Convert string to integer

    if (hour >= 9 && hour <= 11) {
        return hour + " AM";
    } else if (hour === 12) {
        return hour + " PM";
    } else if (hour >= 1 && hour <= 5) {
        return hour + " PM";
    } else {
        return "Invalid Time"; // Outside 9 AM - 5 PM range
    }
}


function loadTeacherProgressChart() {
    $.ajax({
        url: "http://localhost:5111/api/TeacherApi/teacher/progress",
        type: "GET",
        dataType: "json",
        headers: { "Authorization": "Bearer " + token },
        success: function (response) {
            if (response.success) {
                renderKendoChart(response.data);
            } else {
                console.error("Error: API response unsuccessful.");
            }
        },
        error: function (xhr, status, error) {
            console.error("API request failed:", error);
        }
    });
}

function renderKendoChart(data) {
    $("#teacherProgressChart").kendoChart({
        title: {
            text: "Teacher's Progress by Subject"
        },
        legend: {
            position: "bottom"
        },
        seriesDefaults: {
            type: "bar"
        },
        series: [{
            name: "Completion %",
            data: data.map(item => item.completionPercentage),
            color: "#007bff"
        }],
        categoryAxis: {
            categories: data.map(item => item.subjectName),
            title: {
                text: "Subjects"
            }
        },
        valueAxis: {
            title: {
                text: "Completion (%)"
            },
            min: 0,
            max: 100
        },
        tooltip: {
            visible: true,
            format: "{0}%"
        }
    });
}