let token = localStorage.getItem("authToken");

$(document).ready(function () {

    loadTeacherStudentHierarchy();
    loadStudentCountChart();
    loadNotifications();
});


// Load Teacher-Student Hierarchy (TreeView)
function loadTeacherStudentHierarchy() {
    $.ajax({
        url: "http://localhost:5111/api/AdminApi/dashboard/hierarchy",
        type: "GET",
        dataType: "json",
        headers: { "Authorization": "Bearer " + token },
        success: function (data) {

            $("#PrincipleName").html(`<h4>${data.hierarchy.principal.principalName}</h4>`);

            var treeData = data && data.hierarchy.teachers ? data.hierarchy.teachers.map(teacher => ({
                text: teacher.teacherName,
                expanded: false,
                spriteCssClass: "k-icon k-i-plus",
                items: teacher.students.map(student => ({
                    text: student.fullName,
                    spriteCssClass: "k-icon k-i-user"
                }))
            })) : [];

            $("#teacherStudentTreeview").kendoTreeView({
                dataSource: treeData,
                expand: function (e) {
                    var item = $(e.node).find('> .k-in > .k-icon');
                    item.removeClass('k-i-plus').addClass('k-i-minus');
                },
                collapse: function (e) {
                    var item = $(e.node).find('> .k-in > .k-icon');
                    item.removeClass('k-i-minus').addClass('k-i-plus');
                }
            });
        },
        error: function (xhr) {
            console.error("Error loading hierarchy:", xhr.responseText);
        }
    });
}

// Load Student Count Chart(Bar Chart)
function loadStudentCountChart() {
    $.ajax({
        url: "http://localhost:5111/api/AdminApi/dashboard/student-count",
        type: "GET",
        dataType: "json",
        headers: { "Authorization": "Bearer " + token },
        success: function (response) {
            $("#studentCountChart").kendoChart({
                title: { text: "Student Count per Class" },
                legend: { position: "top" },
                seriesDefaults: { type: "column" },
                series: [{
                    data: response.studentCounts.map(item => item.studentCount),
                    name: "Students"
                }],
                categoryAxis: {
                    categories: response.studentCounts.map(item => item.className),
                    labels: { rotation: -45 }
                },
                valueAxis: { title: { text: "Students" } }
            });
        },
        error: function (xhr) {
            console.error("Error loading student count:", xhr.responseText);
        }
    });
}

//  Load Latest Notifications (ListView)
function loadNotifications() {
    $.ajax({
        url: "http://localhost:5111/api/AdminApi/dashboard/GetUnreadNotifications",
        type: "GET",
        dataType: "json",
        headers: { "Authorization": "Bearer " + token },
        success: function (response) {
            var latestNotifications = response.notifications.slice(0, 3); // Show only 3 latest

            $("#notificationsList").kendoListView({
                dataSource: latestNotifications,
                template: "<div class='notification-item'>" +
                    "<strong>#= message #</strong><br>" +
                    "<small>#= kendo.toString(new Date(createdAt), 'MMM dd, yyyy HH:mm') #</small>" +
                    "</div>"
            });
        },
        error: function (xhr) {
            console.error("Error loading notifications:", xhr.responseText);
        }
    });
}


