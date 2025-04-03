let token = localStorage.getItem("authToken");

$(document).ready(function () {
    loadTeacherData();

    // Add loading animation to the card
    $(".teacher-card").css("opacity", "0").animate({
        opacity: 1
    }, 800);
});

function loadTeacherData() {
    // Show loading state
    showLoading(true);

    $.ajax({
        url: "http://localhost:5111/api/StudentApi/GetClassTeacher",
        type: "GET",
        dataType: "json",
        headers: {
            "Authorization": "Bearer " + token
        },
        success: function (response) {
            showLoading(false);
            if (response.success && response.teacher) {

                populateTeacherData(response.teacher);
            } else {
                showError("No teacher data available.");
            }
        },
        error: function (xhr) {
            showLoading(false);
            showError("Failed to load class teacher details. Please try again later.");
            console.error("API Error:", xhr.responseText);
        }
    });
}

function populateTeacherData(teacher) {
    $("#teacherId").val(teacher.teacherId);
    console.log("Teacher experience years:", teacher.experienceYears);    
    // Fill in teacher details with animation
    animateText("#teacherName", teacher.fullName !== undefined && teacher.fullName !== null ? teacher.fullName : "N/A");
    animateText("#teacherPhone", teacher.phoneNumber || "N/A");
    animateText("#teacherEmail", teacher.email || "N/A");
    animateText("#teacherDOB", teacher.dateOfBirth ? teacher.dateOfBirth.split("T")[0] : "N/A");
    animateText("#teacherQualification", teacher.qualification || "N/A");
    ("#teacherExperience", teacher.experienceYears || "N/A");

    // Set teacher image with nice animation
    let imageUrl = "https://ui-avatars.com/api/?name=" + encodeURIComponent(teacher.fullName) + "&size=150&background=random&color=fff";
    $("#previewImage").fadeOut(200, function () {
        $(this).attr("src", imageUrl).fadeIn(400);
    });
}

function animateText(selector, text) {
    $(selector).fadeOut(200, function () {
        $(this).text(text).fadeIn(400);
    });
}

function showLoading(isLoading) {
    if (isLoading) {
        $(".info-value").html('<small class="text-muted">Loading...</small>');
        $("#previewImage").attr("src", "https://via.placeholder.com/150");
    }
}

function showError(message) {
    $(".teacher-info").html(`
        <div class="alert alert-warning m-3">
            <i class="fas fa-exclamation-triangle mr-2"></i> ${message}
        </div>
    `);
}
