$(document).ready(function () {
    let classes = [];
    let teachers = [];
    let timetables = [];
    const apiBaseUrl = 'http://localhost:5111/api/AdminApi';

    // Initialize Kendo Notification
    const notificationElement = $("<div></div>").appendTo(document.body);
    const notification = notificationElement.kendoNotification({
        position: {
            pinned: true,
            top: 30,
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
                template: "<div class='notification-error'><span class='k-icon k-i-warning'></span> #= message #</div>"
            }
        ]
    }).data("kendoNotification");

    // Helper functions for notifications
    function showSuccessNotification(message) {
        notification.show({
            message: message
        }, "success");
    }

    function showErrorNotification(message) {
        notification.show({
            message: message
        }, "error");
    }

    // Show loading spinner
    function showLoading() {
        $(".k-loading-mask").show();
    }

    function hideLoading() {
        $(".k-loading-mask").hide();
    }

    // Fetch Classes with error handling
    $.ajax({
        url: `${apiBaseUrl}/get-class`,
        method: 'GET',
        beforeSend: showLoading,
        success: function (data) {
            classes = data.classes;
            $("#classDropdown").kendoDropDownList({
                dataTextField: "className",
                dataValueField: "classId",
                dataSource: classes,
                optionLabel: "Select Class"
            });
        },
        error: function (xhr) {
            showErrorNotification("Error loading classes: " + xhr.responseText);
        },
        complete: hideLoading
    });

    // Fetch Teachers with error handling
    $.ajax({
        url: `${apiBaseUrl}/GetAllTeachersWithSubjects`,
        method: 'GET',
        beforeSend: showLoading,
        success: function (data) {
            teachers = data.teacherSubjectData;
        },
        error: function (xhr) {
            showErrorNotification("Error loading teachers: " + xhr.responseText);
        },
        complete: hideLoading
    });

    // Populate Teacher Dropdown when Class is selected
    $("#classDropdown").change(function () {
        let classId = $(this).val();
        let filteredTeachers = teachers; // All teachers (assuming they are not linked to specific classes)

        $("#teacherDropdown").kendoDropDownList({
            dataTextField: "fullName",
            dataValueField: "teacherId",
            dataSource: filteredTeachers,
            optionLabel: "Select Teacher"
        });
    });

    // Populate Subject Dropdown based on Selected Teacher
    $("#teacherDropdown").change(function () {
        let teacherId = $(this).val();
        let selectedTeacher = teachers.find(t => t.teacherId == teacherId);
        let subjects = selectedTeacher ? selectedTeacher.assignedSubjects : [];

        $("#subjectDropdown").kendoDropDownList({
            dataTextField: "subjectName",
            dataValueField: "subjectId",
            dataSource: subjects,
            optionLabel: "Select Subject"
        });
    });

    // Time Slots (9 AM - 6 PM)
    $("#timeSlotDropdown").kendoDropDownList({
        dataSource: [
            "09:00:00", "10:00:00", "11:00:00", "12:00:00",
            "13:00:00", "14:00:00", "15:00:00", "16:00:00",
            "17:00:00", "18:00:00"
        ],
        optionLabel: "Select Time Slot"
    });

    // Days (Monday - Saturday)
    $("#dayDropdown").kendoDropDownList({
        dataSource: ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"],
        optionLabel: "Select Day"
    });

    // Fetch Timetable
    function loadTimetableGrid() {
        showLoading();
        $.ajax({
            url: `${apiBaseUrl}/timetable/all`,
            method: 'GET',
            success: function (data) {
                timetables = data.timetables;
                $("#timetableGrid").kendoGrid({
                    dataSource: {
                        data: timetables,
                        schema: {
                            model: {
                                fields: {
                                    timetableId: { type: "number" },
                                    className: { type: "string" },
                                    teacherName: { type: "string" },
                                    subjectName: { type: "string" },
                                    timeSlot: { type: "string" },
                                    dayOfWeek: { type: "string" }
                                }
                            }
                        },
                        pageSize: 10
                    },
                    pageable: true,
                    sortable: true,
                    filterable: true,
                    columns: [
                        { field: "timetableId", title: "ID", width: "80px" },
                        { field: "className", title: "Class" },
                        { field: "teacherName", title: "Teacher" },
                        { field: "subjectName", title: "Subject" },
                        { field: "timeSlot", title: "Time Slot" },
                        { field: "dayOfWeek", title: "Day" },
                        {
                            command: [
                                { name: "edit", text: "Edit", click: editTimetable },
                                { name: "delete", text: "Delete", click: deleteTimetable }
                            ],
                            title: "Actions",
                            width: "200px"
                        }
                    ],
                    editable: false
                });
            },
            error: function (xhr) {
                showErrorNotification("Error loading timetable: " + xhr.responseText);
            },
            complete: hideLoading
        });
    }

    // Validate input before adding/updating
    function validateInput() {
        const required = ['classDropdown', 'teacherDropdown', 'subjectDropdown', 'timeSlotDropdown', 'dayDropdown'];
        let isValid = true;
        required.forEach(field => {
            if (!$(`#${field}`).val()) {
                alert(`Please select a ${field.replace('Dropdown', '')}`);
                isValid = false;
            }
        });
        return isValid;
    }

    // Add Timetable with validation
    $("#addTimetable").click(function () {
        if (!validateInput()) return;

        let newEntry = {
            classId: $("#classDropdown").val(),
            teacherId: $("#teacherDropdown").val(),
            subjectId: $("#subjectDropdown").val(),
            timeSlot: $("#timeSlotDropdown").val(),
            dayOfWeek: $("#dayDropdown").val()
        };

        showLoading();
        $.ajax({
            url: `${apiBaseUrl}/TimeTable/create`,
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(newEntry),
            success: function () {
                showSuccessNotification("Timetable added successfully");
                loadTimetableGrid();
                resetForm();
            },
            error: function (xhr) {
                showErrorNotification("Error adding timetable: " + xhr.responseText);
            },
            complete: hideLoading
        });
    });

    // Simplified edit function
    function editTimetable(e) {
        e.preventDefault();
        let dataItem = this.dataItem($(e.currentTarget).closest("tr"));
        if (!dataItem) return;

        populateForm(dataItem);

        $("#addTimetable")
            .text("Update Timetable")
            .off("click")
            .click(function () {
                if (!validateInput()) return;

                updateTimetable(dataItem.timetableId);
            });
    }

    // Add the missing populateForm function
    function populateForm(dataItem) {
        // Set the class dropdown value
        let classDropdown = $("#classDropdown").data("kendoDropDownList");
        if (classDropdown) {
            classDropdown.value(dataItem.classId);
            classDropdown.trigger("change");

            // Wait for teacher dropdown to populate
            setTimeout(function () {
                let teacherDropdown = $("#teacherDropdown").data("kendoDropDownList");
                if (teacherDropdown) {
                    teacherDropdown.value(dataItem.teacherId);
                    teacherDropdown.trigger("change");

                    // Wait for subject dropdown to populate
                    setTimeout(function () {
                        let subjectDropdown = $("#subjectDropdown").data("kendoDropDownList");
                        if (subjectDropdown) {
                            subjectDropdown.value(dataItem.subjectId);
                        }

                        // Set time slot and day values
                        let timeSlotDropdown = $("#timeSlotDropdown").data("kendoDropDownList");
                        if (timeSlotDropdown) {
                            timeSlotDropdown.value(dataItem.timeSlot);
                        }

                        let dayDropdown = $("#dayDropdown").data("kendoDropDownList");
                        if (dayDropdown) {
                            dayDropdown.value(dataItem.dayOfWeek);
                        }
                    }, 200);
                }
            }, 200);
        }
    }

    function updateTimetable(id) {
        showLoading();
        $.ajax({
            url: `${apiBaseUrl}/timetable/update/${id}`,
            type: "PUT",
            contentType: "application/json",
            data: JSON.stringify({
                classId: $("#classDropdown").val(),
                teacherId: $("#teacherDropdown").val(),
                subjectId: $("#subjectDropdown").val(),
                timeSlot: $("#timeSlotDropdown").val(),
                dayOfWeek: $("#dayDropdown").val()
            }),
            success: function () {
                showSuccessNotification("Timetable updated successfully");
                loadTimetableGrid();
                resetForm();
            },
            error: function (xhr) {
                showErrorNotification("Error updating timetable: " + xhr.responseText);
            },
            complete: hideLoading
        });
    }

    function resetForm() {
        $("#addTimetable").text("Add Timetable");
        $("select").each(function () {
            const dropDown = $(this).data("kendoDropDownList");
            if (dropDown) {
                dropDown.select(0);
            }
        });
    }

    // Simplified delete function
    function deleteTimetable(e) {
        e.preventDefault();
        let dataItem = this.dataItem($(e.currentTarget).closest("tr"));

        if (!dataItem || !dataItem.timetableId) {
            showErrorNotification("Unable to delete: Timetable ID not found");
            return;
        }


        showLoading();
        $.ajax({
            url: `${apiBaseUrl}/timetable/delete/${dataItem.timetableId}`,
            type: "DELETE",
            success: function () {
                showSuccessNotification("Timetable deleted successfully");
                loadTimetableGrid();
            },
            error: function (xhr) {
                showErrorNotification("Error deleting timetable: " + xhr.responseText);
            },
            complete: hideLoading
        });

    }

    // Initial load
    loadTimetableGrid();
});
