let token = localStorage.getItem("authToken");

$(document).ready(function () {
    loadTeacherProfile()

    $("#teacherProfileForm").kendoForm({
        orientation: "vertical",
        formData: {}, // This will be updated dynamically
        items: [
            { field: "fullName", label: "Full Name" },
            { field: "phoneNumber", label: "Phone Number" },
            { field: "dateOfBirth", label: "Date of Birth", editor: "DatePicker" },
            { field: "qualification", label: "Qualification" },
            { field: "experienceYears", label: "Experience (Years)", editor: "NumericTextBox" }
        ],
        buttonsTemplate: `
            <button type="submit" class="k-button k-primary" id="updateProfile">Update</button>
            <a href="/Teacher/" class="k-button k-secondary">Cancel</a>
        `
    });


    $(document).on("click", "#updateProfile", function (e) {
        e.preventDefault();
        updateTeacherProfile();
    });
})

function loadTeacherProfile() {
    $.ajax({
        url: "http://localhost:5111/api/TeacherApi/Teacher/getProfile",
        type: "GET",
        contentType: "application/json",
        headers: { "Authorization": "Bearer " + token },
        success: function (response) {
            if (response.success && response.profile) {
                populateForm(response.profile);
            }
        },
        error: function (error) {
            console.log("Error fetching profile:", error);
        }
    });
}

// Populate Kendo Form with existing profile data
function populateForm(profile) {
    let form = $("#teacherProfileForm").data("kendoForm");
    form.setOptions({
        formData: {
            fullName: profile.fullName,
            phoneNumber: profile.phoneNumber,
            dateOfBirth: new Date(profile.dateOfBirth), // Convert to Date object
            qualification: profile.qualification,
            experienceYears: profile.experienceYears
        }
    });
}


// Function to update the teacher's profile
function updateTeacherProfile() {
    let updatedProfile = {
        fullName: $("#fullName").val(),
        phoneNumber: $("#phoneNumber").val(),
        dateOfBirth: new Date($("#dateOfBirth").val()).toISOString(),
        qualification: $("#qualification").val(),
        experienceYears: $("#experienceYears").val(),
    }

    $.ajax({
        url: "http://localhost:5111/api/TeacherApi/Teacher/updateProfile",
        type: "put",
        contentType: "application/json",
        data: JSON.stringify(updatedProfile),
        headers: { "Authorization": "Bearer " + token },
        success: function (response) {
            if (response.success) {
                alert("Profile updated successfully!");
                window.location.href = "/Teacher";
            } else {
                alert("Error: " + response.message);
            }
        },
        error: function (error) {
            console.log("Error updating profile:", error);
            alert("Failed to update profile. Please try again.");
        }
    });
}