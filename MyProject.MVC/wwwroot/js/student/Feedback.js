let token = localStorage.getItem("authToken");

$(document).ready(function () {
    // Load teachers for selection
    function loadTeachers() {
        $.ajax({
            url: "http://localhost:5111/api/StudentApi/GetTeachersForFeedback",
            type: "GET",
            headers: {
                "Authorization": "Bearer " + token
            },
            success: function (response) {
                if (response.success) {
                    let teacherDropdown = $("#teacherSelect");
                    teacherDropdown.empty().append('<option value="">-- Select Teacher --</option>');
                    $.each(response.teachers, function (index, teacher) {
                        teacherDropdown.append(`<option value="${teacher.teacherId}">${teacher.teacherName} (${teacher.subjectName})</option>`);
                    });
                } else {
                    showNotification("Failed to fetch teachers.", "error");
                }
            },
            error: function (xhr) {
                console.error("API Error:", xhr.responseText);
                showNotification("Error fetching teachers.", "error");
            }
        });
    }

    // Initialize teacher dropdown on page load
    loadTeachers();

    // Handle star rating click
    $(".star").on("click", function () {
        let selectedRating = $(this).data("value");
        $("#experienceRating").val(selectedRating);

        $(".star").removeClass("active");
        $(this).addClass("active");
        $(this).prevAll().addClass("active"); // Highlight previous stars too
    });

    // Submit Feedback
    $("#submitFeedback").on("click", function () {
        let teacherId = $("#teacherSelect").val();
        let rating = $("#experienceRating").val();
        let feedbackText = $("#feedbackText").val();

        if (!teacherId) {
            showNotification("Please select a teacher.", "error");
            return;
        }
        if (rating == 0) {
            showNotification("Please select a rating.", " error");
            return;
        }
        if (feedbackText.trim() === "") {
            showNotification("Please enter feedback.", "error");
            return;
        }

        let feedbackData = {
            teacherId: parseInt(teacherId),
            experienceRating: parseInt(rating),
            feedback: feedbackText.trim()
        };

        $.ajax({
            url: "http://localhost:5111/api/StudentApi/SubmitTeacherFeedback",
            type: "POST",
            contentType: "application/json",
            headers: {
                "Authorization": "Bearer " + token
            },
            data: JSON.stringify(feedbackData),
            success: function (response) {
                if (response.success) {
                    showNotification("Feedback submitted successfully!", " success");
                    $("#teacherSelect").val("");
                    $("#experienceRating").val(0);
                    $("#feedbackText").val("");
                    $(".star").removeClass("active");
                } else {
                    showNotification("Failed to submit feedback: " + (response.message || "Unknown error"), "error");
                }
            },
            error: function (xhr) {
                console.error("API Error:", xhr.responseText);
                showNotification(xhr.responseText, "error");
            }
        });
    });
});

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