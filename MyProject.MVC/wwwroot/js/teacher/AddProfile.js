let token = localStorage.getItem("authToken");
$(document).ready(function () {
    $("#teacherProfileForm").kendoForm({
        orientation: "vertical",
        formData: {
            experienceYears: 0
        },
        layout: "grid",
        grid: {
            cols: 2,
            gutter: 20
        },
        items: [
            {
                field: "fullName",
                label: "Full Name",
                validation: { required: true, message: "Full name is required" }
            },
            {
                field: "phoneNumber",
                label: "Phone Number",
                editor: function (container, options) {
                    $('<input type="tel" class="k-textbox form-control" name="phoneNumber" id="phoneNumber" required pattern="^\\d{10}$" placeholder="Enter 10-digit phone number" />')
                        .appendTo(container);
                },
                validation: { required: true, pattern: "^\\d{10}$", message: "Please enter a valid 10-digit phone number" }
            },
            {
                field: "dateOfBirth",
                label: "Date Of Birth",
                editor: "DatePicker",
                validation: { required: true, message: "Date of birth is required" }
            },
            {
                field: "qualification",
                label: "Qualification",
                validation: { required: true, message: "Qualification is required" }
            },
            {
                field: "experienceYears",
                label: "Experience (Years)",
                editor: "NumericTextBox",
                validation: { required: true, min: 0, message: "Experience must be 0 or greater" }
            }
        ],
        buttonsTemplate: `
            <div class="form-buttons">
                <button type="submit" class="k-button k-primary btn-submit" id="addProfile">
                    <i class="fas fa-save"></i> Submit
                </button>
                <a href="/Teacher/" class="k-button k-secondary btn-cancel">
                    <i class="fas fa-times"></i> Cancel
                </a>
            </div>
        `
    });

    // Form validation before submission
    function validateForm() {
        const fullName = $("#fullName").val();
        const phoneNumber = $("#phoneNumber").val();
        const dateOfBirth = $("#dateOfBirth").val();
        const qualification = $("#qualification").val();
        const experienceYears = $("#experienceYears").val();

        let isValid = true;

        // Full Name validation
        if (!fullName) {
            $("#fullName").addClass("k-invalid");
            showNotification("Error", "Full name is required.", "error");
            isValid = false;
        } else {
            $("#fullName").removeClass("k-invalid");
        }

        // Phone Number validation
        if (!phoneNumber || !phoneNumber.match(/^\d{10}$/)) {
            $("#phoneNumber").addClass("k-invalid");
            showNotification("Error", "Please enter a valid 10-digit phone number.", "error");
            isValid = false;
        } else {
            $("#phoneNumber").removeClass("k-invalid");
        }

        // Date of Birth validation
        if (!dateOfBirth) {
            $("#dateOfBirth").addClass("k-invalid");
            showNotification("Error", "Date of birth is required.", "error");
            isValid = false;
        } else {
            $("#dateOfBirth").removeClass("k-invalid");
        }

        // Qualification validation
        if (!qualification) {
            $("#qualification").addClass("k-invalid");
            showNotification("Error", "Qualification is required.", "error");
            isValid = false;
        } else {
            $("#qualification").removeClass("k-invalid");
        }

        // Experience validation
        if (experienceYears < 0) {
            $("#experienceYears").addClass("k-invalid");
            showNotification("Error", "Experience years cannot be negative.", "error");
            isValid = false;
        } else {
            $("#experienceYears").removeClass("k-invalid");
        }

        return isValid;
    }

    // Submit form with loading indicator
    $("#addProfile").click(function (e) {
        e.preventDefault();

        if (validateForm()) {
            $("#loadingOverlay").removeClass("d-none");
            submitTeacherProfile();
        }
    });
});

function submitTeacherProfile() {
    let dateValue = new Date($("#dateOfBirth").val());
    let profileData = {
        fullName: $("#fullName").val(),
        phoneNumber: $("#phoneNumber").val(),
        dateOfBirth: dateValue.toISOString(),
        qualification: $("#qualification").val(),
        experienceYears: $("#experienceYears").val()
    };

    $.ajax({
        url: "http://localhost:5111/api/TeacherApi/Teacher/addProfile",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(profileData),
        headers: { "Authorization": "Bearer " + token },
        success: function (response) {
            $("#loadingOverlay").addClass("d-none");
            if (response.success) {
                showNotification("Success", "Profile added successfully!", "success");
                setTimeout(() => {
                    window.location.href = "/Teacher";
                }, 1500);
            } else {
                showNotification("Error", "Failed to add profile.", "error");
            }
        },
        error: function (error) {
            $("#loadingOverlay").addClass("d-none");
            console.log(error);
            const errorMessage = error.responseJSON?.message || "Something went wrong!";
            showNotification("Error", errorMessage, "error");
        }
    });
}

function showNotification(title, message, type) {
    $("<div>").kendoNotification({
        position: {
            pinned: true,
            top: 30,
            right: 30
        },
        autoHideAfter: 3000,
        stacking: "down"
    }).data("kendoNotification").show({
        title: title,
        message: message
    }, type);
}