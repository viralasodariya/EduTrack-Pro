let token = localStorage.getItem("authToken");

$(document).ready(function () {
    // Fetch student data using AJAX when the page loads
    loadStudentProfile();

    // Custom tab click handler for the new tab structure
    $(".tab-header .tab").on('click', function () {
        const tabId = $(this).data("tab");
        console.log("Custom tab clicked:", tabId);

        // Update active tab
        $(".tab-header .tab").removeClass("active");
        $(this).addClass("active");

        // Update active content
        $(".tab-panel").removeClass("active");
        $("#" + tabId).addClass("active");

        // If 'Parents' tab is clicked, fetch guardians data
        if (tabId === "parents") {
            loadGuardians();
        }
    });

    // Keep Bootstrap support just in case it's still needed elsewhere
    var triggerTabList = [].slice.call(document.querySelectorAll('#profileTabs button'))
    if (triggerTabList.length > 0) {
        triggerTabList.forEach(function (triggerEl) {
            try {
                var tabTrigger = new bootstrap.Tab(triggerEl);

                triggerEl.addEventListener('click', function (event) {
                    event.preventDefault();
                    tabTrigger.show();

                    var tab = $(this).data("tab");
                    if (tab === "parents") {
                        loadGuardians();
                    }
                });
            } catch (e) {
                console.error("Bootstrap tab error:", e);
            }
        });
    }
});

function loadStudentProfile() {
    console.log("Loading student profile...");
    $.ajax({
        url: "http://localhost:5111/api/StudentApi/GetStudent",
        type: "GET",
        dataType: "json",
        headers: {
            "Authorization": "Bearer " + token
        },
        success: function (response) {
            console.log("Student data received:", response);
            if (response.success) {
                var student = response.students;

                // Update student profile data
                $("#studentName, #fullName").text(student.fullName);
                $("#studentClass").text(student.className + " (ID: " + student.classId + ")");
                $("#gender").text(student.gender);
                $("#dob").text(formatDate(student.dateOfBirth));
                $("#email, #emails").text(student.email);
                $("#enrollmentDate, #enrollmentDate2").text(formatDate(student.enrollmentDate));
                $("#createdAt").text(formatDate(student.createdAt));

                // Status badge
                if (student.status) {
                    $("#status").text("Active").removeClass("bg-danger").addClass("bg-success");
                } else {
                    $("#status").text("Inactive").removeClass("bg-success").addClass("bg-danger");
                }

                // Profile Picture Handling (default if null)
                var profilePictureUrl = student.profilePicture
                    ? student.profilePicture
                    : `https://ui-avatars.com/api/?name=${encodeURIComponent(student.fullName)}&size=150&background=random`;
                $("#profilePicture").attr("src", profilePictureUrl);

                // Add fade-in animation
                $(".profile-card, .card").addClass("animate__animated animate__fadeIn");
            } else {
                showError("Failed to load student data.");
                console.error("Server responded with error:", response);
            }
        },
        error: function (xhr, status, error) {
            console.error("AJAX Error:", status, error);
            console.log("Response:", xhr.responseText);
            showError("Error connecting to server. Please try again later.");
        }
    });
}

function loadGuardians() {
    console.log("Loading guardians data...");
    // Show loading spinner
    $("#guardiansList").html(`
        <div class="text-center py-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>
    `);

    $.ajax({
        url: "http://localhost:5111/api/StudentApi/GetGuardians",
        type: "GET",
        dataType: "json",
        headers: {
            "Authorization": "Bearer " + token
        },
        success: function (response) {
            console.log("Guardians data received:", response);
            if (response.success && response.guardians && response.guardians.length > 0) {
                var guardiansHtml = "";
                response.guardians.forEach(function (guardian, index) {
                    var relationshipBadgeClass = getRelationshipBadgeClass(guardian.relationship);
                    var relationshipIcon = getRelationshipIcon(guardian.relationship);

                    guardiansHtml += `
                        <div class="guardian-card position-relative mb-4 shadow-sm" style="animation-delay: ${index * 0.1}s">
                            <span class="badge ${relationshipBadgeClass} relation-badge">
                                ${relationshipIcon} ${guardian.relationship}
                            </span>
                            <div class="row">
                                <div class="col-md-6">
                                    <div class="info-group">
                                        <label><i class="fas fa-user me-2"></i>Full Name</label>
                                        <p>${guardian.fullName}</p>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="info-group">
                                        <label><i class="fas fa-phone me-2"></i>Phone</label>
                                        <p>${guardian.phoneNumber}</p>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="info-group">
                                        <label><i class="fas fa-envelope me-2"></i>Email</label>
                                        <p>${guardian.email || 'N/A'}</p>
                                    </div>
                                </div>
                                <div class="col-12">
                                    <div class="info-group">
                                        <label><i class="fas fa-map-marker-alt me-2"></i>Address</label>
                                        <p>${guardian.address}</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    `;
                });
                $("#guardiansList").html(guardiansHtml);

                // Add animation to the guardian cards
                $(".guardian-card").addClass("animate__animated animate__fadeInUp");
            } else {
                $("#guardiansList").html(`
                    <div class="alert alert-info">
                        <i class="fas fa-info-circle me-2"></i>No guardian information available.
                    </div>
                `);
            }
        },
        error: function (xhr, status, error) {
            console.error("AJAX Error:", status, error);
            console.log("Response:", xhr.responseText);
            $("#guardiansList").html(`
                <div class="alert alert-danger">
                    <i class="fas fa-exclamation-triangle me-2"></i>Error fetching guardian data.
                </div>
            `);
        }
    });
}

// Helper function to format date
function formatDate(dateString) {
    if (!dateString) return 'N/A';
    try {
        return new Date(dateString).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    } catch (e) {
        console.error("Date formatting error:", e);
        return dateString.split('T')[0] || 'N/A';
    }
}

// Show error message
function showError(message) {
    $(".container").prepend(`
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
            <strong>Error!</strong> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `);
}

// Get appropriate badge class based on relationship
function getRelationshipBadgeClass(relationship) {
    if (!relationship) return 'bg-secondary';

    relationship = relationship.toLowerCase();
    if (relationship.includes('father')) return 'bg-primary';
    if (relationship.includes('mother')) return 'bg-info';
    if (relationship.includes('guardian')) return 'bg-success';
    return 'bg-secondary';
}

// Get appropriate icon based on relationship
function getRelationshipIcon(relationship) {
    if (!relationship) return '<i class="fas fa-user"></i>';

    relationship = relationship.toLowerCase();
    if (relationship.includes('father')) return '<i class="fas fa-male"></i>';
    if (relationship.includes('mother')) return '<i class="fas fa-female"></i>';
    if (relationship.includes('guardian')) return '<i class="fas fa-shield-alt"></i>';
    return '<i class="fas fa-user"></i>';
}