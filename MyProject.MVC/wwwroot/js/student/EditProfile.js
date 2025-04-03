let token = localStorage.getItem("authToken");

$(document).ready(function () {
    loadStudentData();

    loadGuardians();

    // Save button click events (always enabled)
    $("#saveFather").click(function () {
        saveGuardian("father", "Father");
    });

    $("#saveMother").click(function () {
        saveGuardian("mother", "Mother");
    });

    $("#saveGuardian").click(function () {
        saveGuardian("guardian", "Guardian");
    });
});

// ✅ Load Student Data
function loadStudentData() {
    $.ajax({
        url: "http://localhost:5111/api/StudentApi/GetStudent",
        type: "GET",
        dataType: "json",
        headers: { "Authorization": "Bearer " + token },
        success: function (response) {
            if (response.success) {
                var student = response.students;
                console.log(student);
                $("#profileName").text(student.fullName);
                $("#classnameText").text(student.className);
                $("#studentId").val(student.studentId);
                $("#fullName").val(student.fullName);
                $("#dob").val(student.dateOfBirth.split("T")[0]);
                $("#gender").val(student.gender);
                $("#className").val(student.classId);
                $("#enrollmentDate").val(student.enrollmentDate.split("T")[0]);
                $("#email").val(student.email);

                // Profile Picture Handling
                if (student.profilePicture) {
                    $("#profileImage").attr("src", student.profilePicture);
                } else {
                    $("#profileImage").attr("src", "https://ui-avatars.com/api/?name=" + student.fullName);
                }
            }
        },
        error: function (xhr) {
            console.error(xhr);
            showNotification("Error fetching student data.", "error");
        }
    });
}

// ✅ Update Student Profile
function updateStudent() {
    var formData = new FormData();
    formData.append("studentId", $("#studentId").val());
    formData.append("fullName", $("#fullName").val());
    formData.append("dateOfBirth", $("#dob").val());
    formData.append("gender", $("#gender").val());

    var fileInput = document.getElementById("Image");
    if (fileInput.files.length > 0) {
        formData.append("Image", fileInput.files[0]);
    }

    $.ajax({
        url: "http://localhost:5111/api/StudentApi/EditStudentProfile",
        type: "PUT",
        processData: false,
        contentType: false,
        data: formData,
        headers: { "Authorization": "Bearer " + token },
        success: function (response) {
            if (response.success) {
                showNotification("Profile updated successfully!", "success");
                loadStudentData();
            } else {
                showNotification("Error: " + response.message, "error");
            }
        },
        error: function (xhr) {
            console.error(xhr);
            alert("Error updating profile: " + (xhr.responseJSON?.message || "Unknown error"));
        }
    });
}




function loadGuardians() {
    $.ajax({
        url: "http://localhost:5111/api/StudentApi/GetGuardians",
        type: "GET",
        dataType: "json",
        headers: { "Authorization": "Bearer " + token },
        success: function (response) {
            if (response.success) {
                response.guardians.forEach(guardian => {
                    let type = guardian.relationship.toLowerCase();
                    $("#" + type + "Id").val(guardian.guardianId);
                    $("#" + type + "Name").val(guardian.fullName);
                    $("#" + type + "Phone").val(guardian.phoneNumber);
                    $("#" + type + "Email").val(guardian.email);
                    $("#" + type + "Address").val(guardian.address);
                });
            }
        },
        error: function () {
            showNotification("Error fetching guardian data.", "error");
        }
    });
}


function saveGuardian(type, relationship) {
    let guardianId = $("#" + type + "Id").val();
    let data = {
        fullName: $("#" + type + "Name").val(),
        phoneNumber: $("#" + type + "Phone").val(),
        email: $("#" + type + "Email").val(),
        address: $("#" + type + "Address").val(),
        relationship: relationship
    };

    let isEmpty = !data.fullName && !data.phoneNumber && !data.email && !data.address;
    if (isEmpty) {
        alert("Please fill in the details.");
        return;
    }

    let url = guardianId ? `http://localhost:5111/api/StudentApi/UpdateGuardian/${guardianId}` : "http://localhost:5111/api/StudentApi/AddGuardian";
    let method = guardianId ? "PUT" : "POST";

    $.ajax({
        url: url,
        type: method,
        contentType: "application/json",
        data: JSON.stringify(data),
        headers: { "Authorization": "Bearer " + token },
        success: function (response) {
            if (response.success) {
                showNotification(guardianId ? "Guardian updated successfully." : "Guardian added successfully.", "success");
            } else {
                showNotification("Error: " + response.message, "error");
            }
        },
        error: function () {
            showNotification("Error updating guardian data.", "error");
        }
    });
}

function showNotification(message, type) {
    var notification = $('#notification').data('kendoNotification');

    if (!notification) {
        $('#notification').kendoNotification({
            position: {
                pinned: true,
                top: 100,
                right: 30
            },
            stacking: "down",
            autoHideAfter: 3000,
            templates: [
                {
                    type: "info",
                    template: "<div class='k-notification-info'>#= message #</div>"
                },
                {
                    type: "success",
                    template: "<div class='k-notification-success'>#= message #</div>"
                },
                {
                    type: "error",
                    template: "<div class='k-notification-error'>#= message #</div>"
                },
                {
                    type: "warning",
                    template: "<div class='k-notification-warning'>#= message #</div>"
                }
            ]
        });
        notification = $('#notification').data('kendoNotification');
    }

    notification.show({ message: message }, type);
}