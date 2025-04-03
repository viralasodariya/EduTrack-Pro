$(document).ready(function () {
    // Initialize Kendo Window (Modal)
    $("#myKModal").kendoWindow({
        width: "600px",
        title: "Edit Student",
        visible: false,
        modal: true
    });

    // Open Modal on Button Click
    $("#btnadd").click(function () {
        var kWindow = $("#myKModal").data("kendoWindow");

        if (kWindow) {
            kWindow.center(); // Ensure it's centered
            kWindow.open();
            initializeForm(); // Initialize form when opening modal
        }
    });

    $("#grid").kendoGrid({
        dataSource: GetStudentDataSource(),
        toolbar: ["search", {
            name: "customPdf",
            text: "Export to PDF",
            iconClass: "k-icon k-i-pdf",
        }, {
                name: "customExcel",
                text: "Export to Excel",
                iconClass: "k-icon k-i-excel",
            }],
        search: {
            fields: ["fullName", "email", "gender", "className"]
        },
        height: 650,
        scrollable: true,
        columns: [
            {
                field: "sequence",
                title: "No",
                width: 50,
                template: function (dataItem) {
                    return "<div style='text-align:center;'>" + ($("#grid").data("kendoGrid").dataSource.indexOf(dataItem) + 1) + "</div>";
                }
            },
            {
                field: "ProfilePicture",
                title: "Profile",
                width: 100,
                template: function (dataItem) {
                    // Google-style profile image with first letter
                    const firstLetter = dataItem.fullName ? dataItem.fullName.charAt(0).toUpperCase() : "?";
                    const colors = ["#1A73E8", "#EA4335", "#34A853", "#FBBC05", "#8AB4F8", "#F6ADA7", "#81C995", "#FDE293"];
                    const colorIndex = (dataItem.fullName ? dataItem.fullName.charCodeAt(0) : 0) % colors.length;
                    const bgColor = colors[colorIndex];

                    return `<div class="profile-pic-container">
                        <div class="google-profile-pic" style="background-color: ${bgColor}">
                            <span>${firstLetter}</span>
                        </div>
                    </div>`;
                },
                filterable: false
            },
            {
                field: "fullName",
                title: "Student Name",
                width: 150,
                template: "<div class='student-name' style='text-align:center;'>#=fullName#</div>",
                filterable: {
                    cell: {
                        operator: "contains",
                        suggestionOperator: "contains"
                    }
                }
            },
            {
                field: "email",
                title: "Student Email",
                width: 200,
                template: "<div class='student-email' style='text-align:center;'>#=email#</div>",
                filterable: {
                    cell: {
                        operator: "contains"
                    }
                }
            },
            {
                field: "gender",
                title: "Gender",
                width: 100,
                template: function (dataItem) {
                    const iconClass = dataItem.gender === "Male" ? "male-icon" : "female-icon";
                    return `<div class="gender-cell ${dataItem.gender.toLowerCase()}" style="text-align:center; justify-content:center;">
                        <span class="${iconClass}"></span> ${dataItem.gender}
                    </div>`;
                },
                filterable: {
                    multi: true,
                    dataSource: [
                        { text: "Male", value: "Male" },
                        { text: "Female", value: "Female" }
                    ]
                }
            },
            {
                field: "dateOfBirth",
                title: "Date of Birth",
                width: 150,
                template: function (dataItem) {
                    return `<div class="date-cell" style="text-align:center;">${formatDate(dataItem.dateOfBirth)}</div>`;
                },
                filterable: {
                    ui: "datepicker"
                }
            },
            {
                field: "className",
                title: "Class",
                width: 100,
                template: function (dataItem) {
                    return `<div class="class-badge" style="text-align:center; margin:0 auto;">${dataItem.className}</div>`;
                },
                filterable: {
                    multi: true,
                    search: true
                }
            },
            {
                field: "status",
                title: "Status",
                width: 120,
                template: function (dataItem) {
                    return `<div class="status-container">
                        <span class="status-badge ${dataItem.status ? 'active' : 'inactive'} status-dropdown" 
                        data-student-id="${dataItem.studentId}" data-value="${dataItem.status}">
                        ${dataItem.status ? "Active" : "Inactive"}</span>
                    </div>`;
                },
                filterable: {
                    ui: function (element) {
                        element.kendoDropDownList({
                            dataSource: [
                                { text: "All", value: "" },
                                { text: "Active", value: "true" },
                                { text: "Inactive", value: "false" }
                            ],
                            dataTextField: "text",
                            dataValueField: "value",
                            optionLabel: "--Select Status--"
                        });
                    }
                }
            },
            {
                title: "Actions",
                width: 150,
                template: function (dataItem) {
                    return `<div class="action-buttons">
                        <button class="k-button btn-edit" onclick="editStudent(${dataItem.studentId})">
                            <i class="k-icon k-i-edit"></i> Edit
                        </button>
                        <button class="k-button btn-delete" onclick="deleteStudent(${dataItem.studentId})">
                            <i class="k-icon k-i-delete"></i> Delete
                        </button>
                    </div>`;
                },
                filterable: false
            }
        ],
        pageable: {
            refresh: true,
            pageSizes: [5, 10, 20, 50],
            buttonCount: 5
        },
        sortable: true,
        filterable: {
            mode: "row",
            extra: false,
            operators: {
                string: {
                    contains: "Contains",
                    eq: "Equal to",
                    neq: "Not equal to",
                    startswith: "Starts with"
                },
                number: {
                    eq: "Equal to",
                    neq: "Not equal to",
                    gte: "Greater than or equal to",
                    lte: "Less than or equal to"
                },
                date: {
                    eq: "Equal to",
                    neq: "Not equal to",
                    gte: "After or equal to",
                    lte: "Before or equal to"
                }
            }
        },

        dataBinding: function () {
            // Show loader for 2 seconds
            $(".k-grid-content").append('<div class="k-loading-mask"><div class="loader-container"><div class="loader"></div><span>Loading students...</span></div></div>');

            // Ensure loader stays visible for at least 2 seconds
            setTimeout(function () {
                $(".k-grid-content .k-loading-mask").data("ready-to-remove", true);
                if ($(".k-grid-content .k-loading-mask").data("data-bound")) {
                    $(".k-grid-content .k-loading-mask").fadeOut(300, function () {
                        $(this).remove();
                    });
                }
            }, 2000);
        },
        dataBound: function () {
            // Remove loader after 2 seconds or keep it if less than 2 seconds have passed
            $(".k-grid-content .k-loading-mask").data("data-bound", true);
            if ($(".k-grid-content .k-loading-mask").data("ready-to-remove")) {
                $(".k-grid-content .k-loading-mask").fadeOut(300, function () {
                    $(this).remove();
                });
            }

            // Initialize dropdowns for status column
            $(".status-dropdown").on("click", function () {
                var studentId = $(this).data("student-id");
                var currentValue = $(this).data("value").toString();
                var newValue = currentValue === "true" ? "false" : "true";
                updateStudentStatus(studentId, newValue);

            });

            // Add custom CSS after grid renders
            $("<style>" +
                // Main Grid Container
                ".k-grid { border-radius: 10px; box-shadow: 0 6px 18px rgba(0,0,0,0.1); border: none; overflow: hidden; background-color: #fff; }" +
                ".k-grid td { text-align: center; }" + // Center all cells

                // Grid Header
                ".k-grid-header { background-color: #f8fafc; border-bottom: 1px solid #eaeaea; }" +
                ".k-grid-header th.k-header { font-weight: 600; color: #334155; text-transform: uppercase; letter-spacing: 0.5px; font-size: 12px; padding: 16px 12px; background: linear-gradient(180deg, #f8fafc, #f1f5f9); text-align: center; }" +
                ".k-grid-header th.k-header:hover { background: #e2e8f0; }" +
                ".k-grid-header th.k-header .k-link { color: #334155; }" +

                // Grid Content
                ".k-grid-content { border-color: #f1f5f9; }" +
                ".k-grid tr { transition: all 0.2s ease; }" +
                ".k-grid tr:hover { background-color: #f8fafc; transform: translateY(-1px); box-shadow: 0 2px 5px rgba(0,0,0,0.05); }" +
                ".k-grid td { padding: 12px; border-color: #f1f5f9; font-size: 14px; color: #334155; vertical-align: middle; }" +
                ".k-alt { background-color: #fafafa; }" +

                // Google-style Profile Picture
                ".profile-pic-container { display: flex; justify-content: center; }" +
                ".google-profile-pic { width: 50px; height: 50px; border-radius: 50%; display: flex; align-items: center; justify-content: center; box-shadow: 0 3px 8px rgba(0,0,0,0.2); transition: all 0.3s; }" +
                ".google-profile-pic span { font-size: 22px; font-weight: bold; color: white; text-shadow: 0 1px 2px rgba(0,0,0,0.2); }" +
                ".google-profile-pic:hover { transform: scale(1.15); box-shadow: 0 5px 12px rgba(0,0,0,0.25); cursor: pointer; }" +

                // Student Name 
                ".student-name { font-weight: 600; color: #1e293b; }" +

                // Student Email
                ".student-email { color: #475569; font-size: 13px; display: block; overflow: hidden; text-overflow: ellipsis; }" +

                // Gender Cell
                ".gender-cell { display: flex; align-items: center; gap: 5px; font-weight: 500; }" +
                ".male { color: #1a73e8; }" +
                ".female { color: #e91e63; }" +
                ".male-icon::before { content: '♂'; font-size: 16px; color: #1a73e8; }" +
                ".female-icon::before { content: '♀'; font-size: 16px; color: #e91e63; }" +

                // Date Cell
                ".date-cell { font-family: monospace; background: #f8fafc; padding: 4px 8px; border-radius: 4px; color: #475569; font-size: 13px; display: inline-block; }" +

                // Class Badge
                ".class-badge { background-color: #e0f2fe; color: #0369a1; padding: 4px 10px; border-radius: 20px; font-weight: 500; display: inline-block; text-align: center; font-size: 12px; box-shadow: 0 1px 2px rgba(0,0,0,0.05); }" +

                // Status badges
                ".status-container { position: relative; display: flex; justify-content: center; }" +
                ".status-badge { padding: 6px 12px; border-radius: 30px; font-weight: 600; display: inline-block; text-align: center; width: 85px; cursor: pointer; transition: all 0.3s; }" +
                ".active { background-color: #dcfce7; color: #166534; border: 1px solid #bbf7d0; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }" +
                ".inactive { background-color: #fee2e2; color: #991b1b; border: 1px solid #fecaca; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }" +
                ".status-dropdown:hover { transform: translateY(-2px); box-shadow: 0 3px 6px rgba(0,0,0,0.15); }" +

                // Action Buttons - Updated colors
                ".action-buttons { display: flex; gap: 6px; justify-content: center; }" +
                ".btn-edit { min-width: 38px; height: 34px; transition: all 0.3s; box-shadow: 0 2px 5px rgba(0,0,0,0.1); border-radius: 6px; background-color: #0ea5e9; border: none; padding: 0 12px; color: white; }" +
                ".btn-edit:hover { background-color: #0284c7; transform: translateY(-2px); box-shadow: 0 4px 8px rgba(14, 165, 233, 0.4); }" +
                ".btn-delete { min-width: 38px; height: 34px; background-color: #ef4444; color: white; border: none; transition: all 0.3s; box-shadow: 0 2px 5px rgba(0,0,0,0.1); border-radius: 6px; padding: 0 12px; }" +
                ".btn-delete:hover { background-color: #dc2626; transform: translateY(-2px); box-shadow: 0 4px 8px rgba(239, 68, 68, 0.4); }" +

                // Filter Row Styling 
                ".k-filter-row { background-color: #f8fafc; }" +
                ".k-filter-row th { padding: 8px 12px; }" +
                ".k-filtercell .k-filtercell-wrapper { box-shadow: 0 1px 3px rgba(0,0,0,0.1); border-radius: 6px; overflow: hidden; background-color: white; }" +
                ".k-filtercell .k-dropdown-operator { background-color: #f8fafc; }" +

                // Filter Menu
                ".k-filter-menu { border-radius: 8px; box-shadow: 0 10px 25px rgba(0,0,0,0.15); padding: 12px; border: none; }" +
                ".k-filter-menu .k-button { transition: all 0.2s; border-radius: 6px; }" +
                ".k-filter-menu .k-primary { background-color: #3b82f6; }" +
                ".k-filter-menu .k-primary:hover { transform: translateY(-1px); background-color: #2563eb; }" +

                // Toolbar Styling
                ".k-grid-toolbar { background: linear-gradient(135deg, #f8fafc, #f1f5f9); padding: 16px; border-bottom: 1px solid #ddd; display: flex; align-items: center; gap: 12px; }" +
                ".k-grid-toolbar .k-grid-search { width: 280px; border-radius: 30px; box-shadow: 0 2px 6px rgba(0,0,0,0.1); padding: 8px 16px; border: 1px solid #e2e8f0; }" +
                ".k-grid-toolbar .k-grid-customPdf { background-color: #dc2626; color: white; border-radius: 6px; padding: 8px 16px; transition: all 0.3s; border: none; }" +
                ".k-grid-toolbar .k-grid-customPdf:hover { background-color: #b91c1c; box-shadow: 0 4px 8px rgba(220, 38, 38, 0.3); transform: translateY(-2px); }" +
                ".k-grid-toolbar .k-grid-customExcel { background-color: #0d9488; color: white; border-radius: 6px; padding: 8px 16px; transition: all 0.3s; border: none; }" +
                ".k-grid-toolbar .k-grid-customExcel:hover { background-color: #0f766e; box-shadow: 0 4px 8px rgba(13, 148, 136, 0.3); transform: translateY(-2px); }" +

                // Pagination Styling
                ".k-pager-wrap { background-color: #f8fafc; border-top: 1px solid #f1f5f9; padding: 12px; border-bottom-left-radius: 10px; border-bottom-right-radius: 10px; }" +
                ".k-pager-wrap .k-pager-numbers .k-link { min-width: 32px; height: 32px; border-radius: 6px; transition: all 0.2s; display: flex; align-items: center; justify-content: center; margin: 0 2px; }" +
                ".k-pager-wrap .k-pager-numbers .k-link:hover { background-color: #e0f2fe; transform: translateY(-1px); }" +
                ".k-pager-wrap .k-pager-numbers .k-state-selected { background-color: #3b82f6; color: white; font-weight: bold; }" +

                // Improved Loader - 2-second display
                ".loader-container { display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100%; }" +
                ".loader { width: 50px; height: 50px; border: 5px solid rgba(59, 130, 246, 0.2); border-radius: 50%; border-left-color: #3b82f6; animation: spin 1.2s linear infinite; margin-bottom: 15px; }" +
                "@keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }" +
                ".loader-container span { font-size: 16px; color: #3b82f6; font-weight: 500; }" +
                ".k-loading-mask { background-color: rgba(255,255,255,0.85); }" +

                // Miscellaneous
                ".k-dropdown-wrap { border-radius: 6px; transition: all 0.3s; }" +
                ".k-dropdown-wrap:hover { box-shadow: 0 2px 5px rgba(0,0,0,0.1); }" +
                ".k-animation-container .k-list-container { border-radius: 8px; box-shadow: 0 10px 25px rgba(0,0,0,0.15); border: none; overflow: hidden; }" +
                ".k-list .k-item { padding: 8px 12px; transition: all 0.2s ease; }" +
                ".k-list .k-item:hover { background-color: #f1f5f9; }" +
                ".k-list .k-item.k-state-selected { background-color: #3b82f6; color: white; }" +

                "</style>").appendTo("head");

            // Add tooltip to some elements
            $(".status-badge").kendoTooltip({
                position: "top",
                content: function (e) {
                    var status = $(e.target).data("value");
                    return status ? "Click to deactivate student" : "Click to activate student";
                }
            });

            // Add animations to rows for better UX
            $(".k-grid tbody tr").each(function (index) {
                $(this).css({
                    "animation": "fadeIn 0.3s ease forwards",
                    "animation-delay": (index * 0.05) + "s",
                    "opacity": "0"
                });
            });

            $("<style>@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }</style>").appendTo("head");
        }
    });

    // Attach click events using .click() after the grid is initialized
    $(".k-grid-customPdf").click(function (e) {
        e.preventDefault();
        exportToPdf();
    });


    $(".k-grid-customExcel").click(function (e) {
        e.preventDefault();
        exportToExcel();
    });



    $("#editStudent").kendoWindow({
        width: "600px",
        title: "Edit Student",
        visible: false,
        modal: true
    });


});

// get class data source
function getClassDataSource() {
    return new kendo.data.DataSource({
        transport: {
            read: {
                url: "http://localhost:5111/api/AdminApi/get-class",
                dataType: "json"
            },

        }, schema: {
            data: function (response) {
                return response.classes;
            }
        }
    });
}

// form rendering
function initializeForm() {

    $("#studentForm").empty();

    // Add custom styles for the form
    $("<style>" +
        // Form Container
        ".k-form { background: #fff; padding: 24px; border-radius: 12px; box-shadow: 0 4px 12px rgba(0,0,0,0.08); max-width: 600px; margin: 0 auto; }" +

        // Form Fields
        ".k-form .k-form-field { margin-bottom: 20px; }" +
        ".k-form .k-form-field > .k-label { color: #374151; font-weight: 500; font-size: 14px; margin-bottom: 8px; }" +

        // Input Fields
        ".k-form .k-input { height: 40px; border-radius: 6px; border-color: #e5e7eb; transition: all 0.3s; }" +
        ".k-form .k-input:hover { border-color: #3b82f6; }" +
        ".k-form .k-input:focus { border-color: #3b82f6; box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1); }" +

        // Dropdown Styling
        ".k-form .k-dropdown { width: 100%; }" +
        ".k-form .k-dropdown-wrap { border-radius: 6px; height: 40px; }" +
        ".k-form .k-dropdown-wrap:hover { border-color: #3b82f6; }" +

        // Date Picker
        ".k-form .k-datepicker { width: 100%; }" +
        ".k-form .k-datepicker .k-picker-wrap { border-radius: 6px; height: 40px; }" +

        // Submit Button
        ".k-form #submitBtn { width: 100%; padding: 12px 24px; background: linear-gradient(135deg, #3b82f6, #2563eb); border: none; border-radius: 6px; font-weight: 500; text-transform: uppercase; letter-spacing: 0.5px; transition: all 0.3s; margin-top: 20px; }" +
        ".k-form #submitBtn:hover { transform: translateY(-2px); box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3); }" +

        // Validation Messages
        ".k-form .k-form-error { color: #dc2626; font-size: 12px; margin-top: 4px; }" +

        // Required Field Indicator
        ".k-form .k-required { color: #dc2626; }" +

        // Form Animations
        ".k-form .k-form-field { animation: fadeInUp 0.4s ease-out; }" +
        "@keyframes fadeInUp { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }" +

        "</style>").appendTo("head");

    $("#studentForm").kendoForm({
        formData: {
            FullName: "",
            DateOfBirth: "",
            Gender: "",
            ClassId: "",
            EnrollmentDate: "",
            ProfilePicture: "",
            Status: ""
        },
        items: [
            { field: "FullName", label: "Full Name", validation: { required: true } },
            { field: "DateOfBirth", label: "Date of Birth", editor: "DatePicker", validation: { required: true } },
            {
                field: "Gender",
                label: "Gender",
                editor: "DropDownList",
                editorOptions: {
                    dataSource: [
                        { text: "-- Select Gender --", value: "" },
                        { text: "Male", value: "Male" },
                        { text: "Female", value: "Female" }
                    ],
                    dataTextField: "text",
                    dataValueField: "value"
                },
                validation: { required: true }
            },
            {
                field: "ClassId",
                label: "Class",
                editor: "DropDownList",
                editorOptions: {
                    dataSource: getClassDataSource(),
                    dataTextField: "className",
                    dataValueField: "classId",
                    optionLabel: "-- Select Class --"
                },
                validation: { required: true, }
            },
            { field: "EnrollmentDate", label: "Enrollment Date", editor: "DatePicker", validation: { required: true } },
            {
                field: "Status",
                label: "Status",
                editor: "DropDownList",
                editorOptions: {
                    dataSource: [
                        { text: "-- Select Status --", value: "" },
                        { text: "Active", value: "true" },
                        { text: "Inactive", value: "false" }
                    ],
                    dataTextField: "text",
                    dataValueField: "value",
                },
                validation: { required: true }
            }
        ],
        buttonsTemplate: '<button type="submit" id="submitBtn" class="k-button k-primary">Submit</button>'
    });

    $("#submitBtn").click(function (event) {
        console.log("Submit Button Clicked");
        event.preventDefault();
        submitStudentForm();
    });
}

// Open Modal on Button Click
$("#btnadd").click(function () {
    $("#myKModal").data("kendoWindow").center().open();
    initializeForm(); // Initialize form when opening modal
});

// student add in database
function submitStudentForm() {
    // Get date values directly from form fields
    var dateOfBirthField = $("#studentForm [name='DateOfBirth']").val();
    var enrollmentDateField = $("#studentForm [name='EnrollmentDate']").val();

    // Convert dates to ISO format if they exist
    var dateOfBirth = dateOfBirthField ? new Date(dateOfBirthField).toISOString().split("T")[0] : null;
    var enrollmentDate = enrollmentDateField ? new Date(enrollmentDateField).toISOString().split("T")[0] : null;

    var formData = {
        FullName: $("#studentForm [name='FullName']").val(),
        DateOfBirth: dateOfBirth,
        Gender: $("#studentForm [name='Gender']").val(),
        ClassId: $("#studentForm [name='ClassId']").val(),
        EnrollmentDate: enrollmentDate,
        Status: $("#studentForm [name='Status']").val() === "true",
        ProfilePicture: null
    };

    console.log("Submitting Data:", formData);

    $.ajax({
        url: "http://localhost:5111/api/AdminApi/CreateStudent",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(formData),
        success: function () {
            showNotification("Student Added Successfully", "success");
            $("#myKModal").data("kendoWindow").close();
            $("#grid").data("kendoGrid").dataSource.read();
        },
        error: function (xhr) {
            console.log("Error:", xhr.responseText);
            showNotification("Failed to add student: " + (xhr.responseText || "Unknown error"), "error");
        }
    });
}

// get student data source
function GetStudentDataSource() {
    return new kendo.data.DataSource({
        transport: {
            read: {
                url: "http://localhost:5111/api/AdminApi/GetAllStudent",
                dataType: "json"
            }
        },
        schema: {
            data: function (response) {
                return response.students;
            }
        }
    });
}

// format date
function formatDate(date) {
    // yyyy-mm-dd
    var d = new Date(date);
    var month = '' + (d.getMonth() + 1);
    var day = '' + d.getDate();
    var year = d.getFullYear();
    return [year, month, day].join('-');

}

// update student status
function updateStudentStatus(studentId, newValue) {
    $.ajax({
        url: "http://localhost:5111/api/AdminApi/dashboard/approve-student",
        type: "PUT",
        contentType: "application/json",
        data: JSON.stringify({
            studentId: studentId,
            newStatus: newValue === "true"
        }),
        success: function (response) {
            showNotification("Student status updated successfully", "success");
            $("#grid").data("kendoGrid").dataSource.read();
        },
        error: function (xhr, status, error) {
            console.log("Error: " + error);
            console.log("Response:", xhr.responseText);
            showNotification("Failed to update student status: " + (xhr.responseText || "Unknown error"), "error");
        }
    });
}

// edit student fetch bystudentid
function editStudent(studentId) {
    $.ajax({
        url: `http://localhost:5111/api/AdminApi/Students/${studentId}`,
        type: "GET",
        success: function (response) {
            initializeEditForm(response.student);
        },
        error: function (xhr, status, error) {
            console.error("Error fetching student:", error);
        }
    })
}

// initialize edit form
function initializeEditForm(student) {
    var editWindow = $("#editStudent").data("kendoWindow");

    // Add custom styles for edit form
    $("<style>" +
        // Form Container
        "#editForm { background: linear-gradient(to bottom, #ffffff, #f8fafc); padding: 30px; border-radius: 15px; box-shadow: 0 8px 24px rgba(0,0,0,0.12); }" +

        // Form Fields
        "#editForm .k-form-field { margin-bottom: 24px; animation: slideIn 0.3s ease-out; }" +
        "@keyframes slideIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }" +

        // Labels
        "#editForm .k-label { color: #1e293b; font-weight: 600; font-size: 14px; margin-bottom: 8px; letter-spacing: 0.3px; }" +

        // Input Fields
        "#editForm .k-input { height: 45px; border-radius: 8px; border: 2px solid #e2e8f0; transition: all 0.3s ease; background: #fff; }" +
        "#editForm .k-input:hover { border-color: #94a3b8; }" +
        "#editForm .k-input:focus { border-color: #3b82f6; box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.15); }" +

        // Dropdowns
        "#editForm .k-dropdown { width: 100%; }" +
        "#editForm .k-dropdown-wrap { border-radius: 8px; height: 45px; border: 2px solid #e2e8f0; }" +
        "#editForm .k-dropdown-wrap:hover { border-color: #94a3b8; }" +
        "#editForm .k-dropdown-wrap.k-state-focused { border-color: #3b82f6; box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.15); }" +

        // Date Picker
        "#editForm .k-datepicker { width: 100%; }" +
        "#editForm .k-datepicker .k-picker-wrap { border-radius: 8px; height: 45px; border: 2px solid #e2e8f0; }" +
        "#editForm .k-datepicker .k-picker-wrap:hover { border-color: #94a3b8; }" +

        // Submit Button
        "#editForm #submitBtn { width: 100%; padding: 14px 28px; background: linear-gradient(135deg, #3b82f6, #2563eb); color: white; border: none; border-radius: 8px; font-weight: 600; text-transform: uppercase; letter-spacing: 1px; transition: all 0.3s ease; margin-top: 20px; }" +
        "#editForm #submitBtn:hover { transform: translateY(-2px); box-shadow: 0 6px 15px rgba(37, 99, 235, 0.3); background: linear-gradient(135deg, #2563eb, #1d4ed8); }" +
        "#editForm #submitBtn:active { transform: translateY(0); }" +

        // Validation Messages
        "#editForm .k-form-error { color: #ef4444; font-size: 13px; margin-top: 6px; animation: shake 0.5s ease-in-out; }" +
        "@keyframes shake { 0%, 100% { transform: translateX(0); } 25% { transform: translateX(-5px); } 75% { transform: translateX(5px); } }" +

        // Required Field Indicator
        "#editForm .k-required { color: #ef4444; font-weight: bold; }" +

        "</style>").appendTo("head");

    // Form initialization logic
    var form = $("#editForm").data("kendoForm");
    if (!form) {
        $("#editForm").kendoForm({
            formData: {
                FullName: student.fullName || "",
                DateOfBirth: student.dateOfBirth ? formatDate(student.dateOfBirth) : "",
                Gender: student.gender || "",
                Status: student.status || "",
                ProfilePicture: null
            },
            items: [
                { field: "FullName", label: "Full Name", validation: { required: true } },
                { field: "DateOfBirth", label: "Date of Birth", editor: "DatePicker", validation: { required: true } },
                {
                    field: "Gender",
                    label: "Gender",
                    editor: "DropDownList",
                    editorOptions: {
                        dataSource: [
                            { text: "-- Select--", value: "" },
                            { text: "Male", value: "Male" },
                            { text: "Female", value: "Female" }
                        ],
                        dataTextField: "text",
                        dataValueField: "value"
                    }
                },
                {
                    field: "Status",
                    label: "Status",
                    editor: "DropDownList",
                    editorOptions: {
                        dataSource: [
                            { text: "-- Select--", value: "" },
                            { text: "Active", value: "true" },
                            { text: "Inactive", value: "false" }
                        ],
                        dataTextField: "text",
                        dataValueField: "value"
                    }
                }
            ],
            buttonsTemplate: '<button type="submit" id="submitBtn" class="k-button k-primary">Update Student</button>'
        });
    } else {
        form.setOptions({
            formData: {
                FullName: student.fullName || "",
                DateOfBirth: student.dateOfBirth ? formatDate(student.dateOfBirth) : "",
                Gender: student.gender || "",
                Status: student.status || "",
                ProfilePicture: null
            }
        });
    }

    $("#submitBtn").off("click").on("click", function (event) {
        event.preventDefault();
        submitEditForm(student.studentId);
    });

    editWindow.center().open();

    editWindow.unbind("close").bind("close", function () {
        console.log("Modal Closed - But Data Stays");
    });
}

// submit edit form
function submitEditForm(studentId) {
    var dateOfBirth = $("#editForm [name='DateOfBirth']").val();
    var formData = {
        FullName: $("#editForm [name='FullName']").val(),
        DateOfBirth: dateOfBirth ? new Date(dateOfBirth).toISOString() : null,
        Gender: $("#editForm [name='Gender']").val(),
        status: $("#editForm [name='Status']").val() === "true",
        ProfilePicture: null
    }
    console.log("Submitting Edit Data:", formData)

    $.ajax({
        url: `http://localhost:5111/api/AdminApi/students/${studentId}`,
        type: "PUT",
        contentType: "application/json",
        data: JSON.stringify(formData),
        success: function (response) {
            console.log("Student Updated:", response);
            showNotification("Student updated successfully", "success");
            $("#editStudent").data("kendoWindow").close();
            $("#grid").data("kendoGrid").dataSource.read();
        },
        error: function (xhr, status, error) {
            console.log("Error: " + error);
            console.log("Response:", xhr.responseText);
            showNotification("Failed to update student: " + (xhr.responseText || "Unknown error"), "error");
        }
    })

}

// delete student
function deleteStudent(studentId) {

    $.ajax({
        url: `http://localhost:5111/api/AdminApi/students/${studentId}`,
        type: "DELETE",
        success: function (response) {
            showNotification("Student deleted successfully", "success");
            $("#grid").data("kendoGrid").dataSource.read();
        },
        error: function (xhr, status, error) {
            console.error("Error:", error);
            console.error("Response:", xhr.responseText);
            showNotification("Failed to delete student: " + (xhr.responseText || "Unknown error"), "error");
        }
    });

}


// Function to export to PDF using your backend API
function exportToPdf() {
    // Show loading indicator
    kendo.ui.progress($("#grid"), true);

    // Make an AJAX request to the backend API
    $.ajax({
        url: "http://localhost:5111/api/AdminApi/export/students/pdf", // Backend API endpoint
        method: "GET",
        xhrFields: {
            responseType: 'blob' // Ensure the response is treated as a binary file
        },
        success: function (blob, status, xhr) {
            console.log("PDF Export Response:", blob, status, xhr);

            // Get the filename from the Content-Disposition header
            var filename = "Students.pdf"; // Default filename
            var disposition = xhr.getResponseHeader('Content-Disposition');
            if (disposition && disposition.indexOf('attachment') !== -1) {
                var filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
                var matches = filenameRegex.exec(disposition);
                if (matches != null && matches[1]) {
                    filename = matches[1].replace(/['"]/g, ''); // Extract filename from header
                }
            }

            // Check if the response is a valid blob
            if (blob && blob.size > 0) {
                // Create a link element to trigger the download
                var link = document.createElement('a');
                var url = window.URL.createObjectURL(blob);
                link.href = url;
                link.download = filename; // Set the filename for the download
                document.body.appendChild(link);
                link.click(); // Trigger the download

                // Clean up
                setTimeout(function () {
                    document.body.removeChild(link);
                    window.URL.revokeObjectURL(url);
                }, 100);

                // Show success message
                showNotification("PDF exported successfully", "success");
            } else {
                console.error("Empty or invalid blob received");
                showNotification("Failed to export PDF: Invalid response", "error");
            }

            // Hide loading indicator
            kendo.ui.progress($("#grid"), false);
        },
        error: function (xhr, status, error) {
            console.error("PDF Export Error:", error);
            console.error("Status:", status);
            console.error("Response:", xhr.responseText);
            kendo.ui.progress($("#grid"), false);
            showNotification("Failed to export PDF: " + (error || "Unknown error"), "error");
        }
    });
}

// Function to export to Excel using your backend API
function exportToExcel() {
    // Show loading indicator
    kendo.ui.progress($("#grid"), true);

    $.ajax({
        url: "http://localhost:5111/api/AdminApi/export/students/excel",
        method: "GET",  // Changed from type to method for consistency
        xhrFields: {
            responseType: 'blob'
        },
        success: function (blob, status, xhr) {
            // Get filename from Content-Disposition header if available
            var filename = "Students.xlsx";
            var disposition = xhr.getResponseHeader('Content-Disposition');
            if (disposition && disposition.indexOf('attachment') !== -1) {
                var filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
                var matches = filenameRegex.exec(disposition);
                if (matches != null && matches[1]) {
                    filename = matches[1].replace(/['"]/g, '');
                }
            }

            // Check if the response is actually a blob and has content
            if (blob && blob.size > 0) {
                // Create a link element and trigger download
                var link = document.createElement('a');
                var url = window.URL.createObjectURL(blob);
                link.href = url;
                link.download = filename;
                document.body.appendChild(link);
                link.click();

                // Clean up
                setTimeout(function () {
                    document.body.removeChild(link);
                    window.URL.revokeObjectURL(url);
                }, 100);

                // Show success message
                showNotification("Excel exported successfully", "success");
            } else {
                console.error("Empty or invalid blob received");
                showNotification("Failed to export Excel: Invalid response", "error");
            }

            // Hide loading indicator
            kendo.ui.progress($("#grid"), false);
        },
        error: function (xhr, status, error) {
            console.error("Excel Export Error:", error);
            console.error("Status:", status);
            console.error("Response:", xhr.responseText);
            ke
            console.error("Response:", xhr.responseText);
            kendo.ui.progress($("#grid"), false);
            showNotification("Failed to export Excel: " + (error || "Unknown error"), "error");
        }
    });
}

// Helper function to show notifications
function showNotification(message, type) {
    // If KendoUI notification is available
    if ($.fn.kendoNotification) {
        var notification = $("<div/>").appendTo("body").kendoNotification({
            position: {
                pinned: false,
                top: 70,
                right: 30
            },
            autoHideAfter: 3000,
            stacking: "down",
            templates: [
                {
                    type: "success",
                    template: "<div class='notification-success'><span class='k-icon k-i-check'></span> #= message #</div>"
                },
                {
                    type: "error",
                    template: "<div class='notification-error'><span class='k-icon k-i-close'></span> #= message #</div>"
                }
            ]
        }).data("kendoNotification");

        notification.show({ message: message }, type);
    } else {
        // Fallback to alert
        alert(message);
    }
}

