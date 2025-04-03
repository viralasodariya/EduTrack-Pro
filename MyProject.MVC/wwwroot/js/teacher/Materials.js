let authToken = localStorage.getItem("authToken");
let notificationWidget;
// Define allowed file extensions
const allowedFileExtensions = [".pdf", ".docx", ".pptx", ".xlsx"];

$(document).ready(function () {
    // Add notification div if it doesn't exist
    if ($("#notification").length === 0) {
        $("body").append('<div id="notification" class="notification-container"></div>');
    }

    // Add loader elements if they don't exist
    if ($("#uploadLoader").length === 0) {
        $("#uploadBtn").before('<div id="uploadLoader" class="spinner-border text-primary me-2 d-none" role="status"><span class="visually-hidden">Loading...</span></div>');
    }

    // Check if Kendo UI is available
    if (typeof kendo === 'undefined') {
        console.error("Kendo UI is not loaded. Notifications will use fallback method.");
        setupFallbackNotification();
    } else {
        // Initialize Kendo notification with improved configuration
        try {
            notificationWidget = $("#notification").kendoNotification({
                position: {
                    pinned: true,
                    top: 30,
                    right: 30
                },
                autoHideAfter: 5000,
                stacking: "down",
                templates: [
                    {
                        type: "success",
                        template: '<div class="notification-item notification-success"><span class="notification-icon"><i class="fas fa-check-circle"></i></span><div class="notification-text">#= message #</div></div>'
                    },
                    {
                        type: "error",
                        template: '<div class="notification-item notification-error"><span class="notification-icon"><i class="fas fa-exclamation-circle"></i></span><div class="notification-text">#= message #</div></div>'
                    },
                    {
                        type: "warning",
                        template: '<div class="notification-item notification-warning"><span class="notification-icon"><i class="fas fa-exclamation-triangle"></i></span><div class="notification-text">#= message #</div></div>'
                    },
                    {
                        type: "info",
                        template: '<div class="notification-item notification-info"><span class="notification-icon"><i class="fas fa-info-circle"></i></span><div class="notification-text">#= message #</div></div>'
                    },
                    {
                        type: "upload-success",
                        template: '<div class="notification-item notification-upload-success"><span class="notification-icon"><i class="fas fa-check-circle"></i></span><div class="notification-text">#= message #</div></div>'
                    }
                ]
            }).data("kendoNotification");

            console.log("Notification widget initialized:", notificationWidget);
        } catch (error) {
            console.error("Failed to initialize notification widget:", error);
            setupFallbackNotification();
        }
    }

    function setupFallbackNotification() {
        // Create custom notification functionality as fallback
        notificationWidget = {
            show: function (options) {
                const message = options.message || "Notification";
                const type = options.type || "info";

                // Create a custom notification element
                const notificationDiv = $(`<div class="custom-notification custom-notification-${type}">
                    <div class="custom-notification-content">
                        <i class="notification-icon fas ${getIconClass(type)}"></i>
                        <span>${message}</span>
                    </div>
                    <button class="close-btn">&times;</button>
                </div>`);

                // Add to the notification container
                $("#notification").append(notificationDiv);

                // Add close button functionality
                notificationDiv.find(".close-btn").on("click", function () {
                    notificationDiv.fadeOut(300, function () { $(this).remove(); });
                });

                // Auto-hide after 5 seconds
                setTimeout(function () {
                    notificationDiv.fadeOut(300, function () { $(this).remove(); });
                }, 5000);

                // Show with animation
                notificationDiv.hide().fadeIn(300);

                console.log(`Fallback notification - ${type}: ${message}`);
            }
        };

        // Add required CSS for the fallback notifications
        if ($("#customNotificationCSS").length === 0) {
            $("head").append(`
                <style id="customNotificationCSS">
                    .notification-container { position: fixed; top: 30px; right: 30px; z-index: 10000; }
                    .custom-notification { margin-bottom: 10px; padding: 15px; border-radius: 4px; width: 300px; display: flex; justify-content: space-between; box-shadow: 0 4px 8px rgba(0,0,0,0.2); }
                    .custom-notification-content { display: flex; align-items: center; }
                    .notification-icon { margin-right: 10px; }
                    .custom-notification-success { background-color: #dff0d8; color: #3c763d; border: 1px solid #d6e9c6; }
                    .custom-notification-error { background-color: #f2dede; color: #a94442; border: 1px solid #ebccd1; }
                    .custom-notification-warning { background-color: #fcf8e3; color: #8a6d3b; border: 1px solid #faebcc; }
                    .custom-notification-info { background-color: #d9edf7; color: #31708f; border: 1px solid #bce8f1; }
                    .custom-notification-upload-success { background-color: #c8e6c9; color: #2e7d32; border: 1px solid #a5d6a7; }
                    .close-btn { background: none; border: none; font-size: 20px; cursor: pointer; opacity: 0.5; }
                    .close-btn:hover { opacity: 1; }
                </style>
            `);
        }
    }

    function getIconClass(type) {
        switch (type) {
            case 'success': return 'fa-check-circle';
            case 'error': return 'fa-exclamation-circle';
            case 'warning': return 'fa-exclamation-triangle';
            case 'upload-success': return 'fa-check-circle';
            default: return 'fa-info-circle';
        }
    }

    // Utility function for showing notifications with error handling
    window.showNotification = function (message, type) {
        try {
            if (notificationWidget && typeof notificationWidget.show === 'function') {
                notificationWidget.show({
                    message: message,
                    type: type || "info"
                });
                console.log(`Notification displayed: ${message} (${type})`);
            } else {
                console.warn("Notification widget not available, using fallback");
                alert(`${type.toUpperCase()}: ${message}`);
            }
        } catch (error) {
            console.error("Error showing notification:", error);
            alert(`${type.toUpperCase()}: ${message}`);
        }
    };

    loadSubjects();
    loadMaterials();
    updateCurrentTime();

    // Update current time every second
    setInterval(updateCurrentTime, 1000);

    // File input change handler
    $("#fileUpload").on("change", function () {
        const file = this.files[0];
        if (file) {
            const fileName = file.name;
            const fileSize = (file.size / 1024).toFixed(2) + " KB";
            const fileExtension = "." + fileName.split('.').pop().toLowerCase();

            if (!allowedFileExtensions.includes(fileExtension)) {
                showNotification("Only PDF, Word, PowerPoint, and Excel files are allowed.", "error");
                // Clear the file input
                $(this).val('');
                $(".file-info").text("No file selected");
                $(".upload-time").addClass("d-none");
                return;
            }

            $(".file-info").text(`${fileName} (${fileSize})`);
            $(".upload-time").removeClass("d-none");
        } else {
            $(".file-info").text("No file selected");
            $(".upload-time").addClass("d-none");
        }
    });

    function updateCurrentTime() {
        const now = new Date();
        const formattedTime = now.toLocaleString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit'
        });
        $(".current-time").text(formattedTime);
    }

    function loadSubjects() {
        $.ajax({
            url: "http://localhost:5111/api/TeacherApi/Teacher/assignedSubjects",
            type: "GET",
            headers: {
                "Authorization": `Bearer ${authToken}`
            },
            success: function (response) {
                if (response.success) {
                    $("#subjectDropdown").empty().append('<option value="">Select Subject</option>');
                    $.each(response.data, function (i, subject) {
                        $("#subjectDropdown").append(`<option value="${subject.subjectId}">${subject.subjectName}</option>`);
                    });
                }
            }
        });
    }

    function loadMaterials() {
        $("#fileManager").kendoGrid({
            dataSource: {
                transport: {
                    read: {
                        url: "http://localhost:5111/api/TeacherApi/Teacher/get-materials",
                        dataType: "json",
                        headers: {
                            "Authorization": `Bearer ${authToken}`
                        }
                    }
                },
                schema: {
                    data: function (response) {
                        // Handle both formats: direct array or object with data property
                        if (Array.isArray(response)) {
                            return response;
                        } else if (response && response.data) {
                            return response.data;
                        } else {
                            console.warn("Unexpected API response format:", response);
                            return [];
                        }
                    },
                    parse: function (response) {
                        try {
                            // Handle both formats
                            const data = Array.isArray(response) ? response :
                                (response && response.data ? response.data : []);

                            return data.map(item => {
                                // Format the date
                                if (item && item.uploadDate) {
                                    try {
                                        const date = new Date(item.uploadDate);
                                        if (!isNaN(date.getTime())) {
                                            item.formattedDate = date.toLocaleString('en-US', {
                                                year: 'numeric',
                                                month: 'short',
                                                day: 'numeric',
                                                hour: '2-digit',
                                                minute: '2-digit'
                                            });
                                        } else {
                                            item.formattedDate = "Invalid date";
                                        }
                                    } catch (e) {
                                        console.error("Error parsing date:", e);
                                        item.formattedDate = "Unknown";
                                    }
                                } else {
                                    item.formattedDate = "Not available";
                                }
                                return item;
                            });
                        } catch (error) {
                            console.error("Error parsing response:", error);
                            return [];
                        }
                    }
                },
                error: function (e) {
                    console.error("DataSource error:", e);
                }
            },
            height: 550,
            sortable: true,
            filterable: true,
            pageable: {
                refresh: true,
                pageSize: 10,
                pageSizes: [5, 10, 20, "all"]
            },
            columns: [
                { field: "subjectName", title: "Subject" },
                { field: "fileName", title: "File Name" },
                { field: "fileType", title: "File Type" },
                { field: "formattedDate", title: "Uploaded On" },
                {
                    field: "filePath",
                    title: "Download",
                    template: '<a href="#=filePath#" target="_blank" class="btn btn-info btn-sm"><i class="fas fa-download me-1"></i>Download</a>'
                },
                {
                    title: "Actions",
                    template: '<button class="btn btn-danger btn-sm deleteFile" data-id="#=materialId#"><i class="fas fa-trash-alt me-1"></i>Delete</button>'
                }
            ],
            dataBound: function () {
                // Add hover effect to grid rows
                $(".k-grid tr").hover(
                    function () { $(this).addClass("row-hover"); },
                    function () { $(this).removeClass("row-hover"); }
                );
            }
        });
    }

    // Function to clear the upload form
    function clearUploadForm() {
        $("#subjectDropdown").val("");
        $("#fileUpload").val("");
        $(".file-info").text("No file selected");
        $(".upload-time").addClass("d-none");
    }

    $("#uploadBtn").click(function () {
        let subjectId = $("#subjectDropdown").val();
        let file = $("#fileUpload")[0].files[0];

        if (!subjectId || !file) {
            showNotification("Please select a subject and choose a file.", "error");
            return;
        }

        // Validate file extension
        const fileName = file.name;
        const fileExtension = "." + fileName.split('.').pop().toLowerCase();
        if (!allowedFileExtensions.includes(fileExtension)) {
            showNotification("Only PDF, Word, PowerPoint, and Excel files are allowed.", "error");
            return;
        }

        // Show loader
        $("#uploadLoader").removeClass("d-none");
        $("#uploadBtn").prop("disabled", true);

        let formData = new FormData();
        formData.append("subjectId", subjectId);
        formData.append("file", file);

        $.ajax({
            url: "http://localhost:5111/api/TeacherApi/teacher/upload-material",
            type: "POST",
            headers: {
                "Authorization": `Bearer ${authToken}`
            },
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                showNotification("File uploaded successfully!", "upload-success");
                clearUploadForm();
                loadMaterials();
            },
            error: function (xhr, status, error) {
                showNotification("Error uploading file: " + error, "error");
            },
            complete: function () {
                // Hide loader
                $("#uploadLoader").addClass("d-none");
                $("#uploadBtn").prop("disabled", false);
            }
        });
    });

    $("#fileManager").on("click", ".deleteFile", function () {
        let materialId = $(this).data("id");

        const $btn = $(this);
        const originalHtml = $btn.html();

        // Show loader inside the button
        $btn.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Deleting...').prop('disabled', true);

        $.ajax({
            url: `http://localhost:5111/api/TeacherApi/teacher/delete-material/${materialId}`,
            type: "DELETE",
            headers: {
                "Authorization": `Bearer ${authToken}`
            },
            success: function () {
                showNotification("File deleted successfully!", "success");
                loadMaterials();
            },
            error: function (xhr, status, error) {
                showNotification("Error deleting file: " + error, "error");
                // Restore button
                $btn.html(originalHtml).prop('disabled', false);
            }
        });
    });
});


