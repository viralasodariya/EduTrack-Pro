let token = localStorage.getItem('authToken');

$(document).ready(function () {
    loadAssignedSubjects();
    loadUnassignedSubjects();
    loadTeachersForDropdown();

    $("#notification").kendoNotification({
        position: {
            pinned: false,
            top: 30,
            right: 30
        },
        autoHideAfter: 3000,
        stacking: "down"
    });

    $("#addSubjectForm").kendoForm({
        formData: {
            subjectName: ""
        },
        items: [
            {
                field: "subjectName",
                label: "Subject Name:",
                validation: { required: true },
                editor: "TextBox",
                editorOptions: {
                    placeholder: "Enter subject name",
                    classes: "form-control"
                }
            }
        ],
        buttonsTemplate: `<button type="button" id="addSubjectBtn" class="k-button k-primary w-100 mt-3">Add Subject</button>`
    });


    $("#addSubjectBtn").click(function () {
        addSubject();
    });

    $("#assignSubjectBtn").kendoButton({
        click: function () {
            assignSubjectToTeacher();
        }
    });
});


function addSubject() {
    var subjectName = $("#addSubjectForm [name='subjectName']").val();

    if (!subjectName) {
        alert("Please enter a subject name.");
        return;
    }

    var subjectToLower = subjectName.toLowerCase();

    $.ajax({
        url: "http://localhost:5111/api/AdminApi/addSubject",
        type: "POST",
        contentType: "application/json",
        headers: {
            'Authorization': 'Bearer ' + token
        },
        data: JSON.stringify({ subjectName: subjectToLower }),
        success: function (response) {
            showNotification("Subject added successfully!", "success");
            $("#addSubjectForm [name='subjectName']").val(""); // Clear input field
        },
        error: function (error, xhr) {
            showNotification(xhr.responseJSON.message, "error");
        }
    });
}

// ✅ Load Teachers with Assigned Subjects (Kendo Grid)
function loadAssignedSubjects() {
    $("#gridAssignedSubjects").kendoGrid({
        dataSource: {
            transport: {
                read: {
                    url: "http://localhost:5111/api/AdminApi/GetAllTeachersWithSubjects",
                    dataType: "json",
                    headers: {
                        'Authorization': 'Bearer ' + token
                    },
                }
            },
            schema: {
                data: "teacherSubjectData"
            }
        },
        height: 350,
        pageable: {
            refresh: true,
            pageSizes: [5, 10, 20],
            buttonCount: 5
        },
        sortable: true,
        filterable: true,
        columns: [
            { field: "fullName", title: "Teacher Name", width: "40%" },
            {
                field: "assignedSubjects",
                title: "Assigned Subjects",
                width: "60%",
                template: "#= assignedSubjects.map(s => '<span class=\"badge bg-info text-dark me-1 mb-1\">' + s.subjectName + '</span>').join(' ') #"
            }
        ]
    });
}

// ✅ Load Teachers with Unassigned Subjects (Kendo Grid)
function loadUnassignedSubjects() {
    $("#gridUnassignedSubjects").kendoGrid({
        dataSource: {
            transport: {
                read: {
                    url: "http://localhost:5111/api/AdminApi/GetAllTeachersWithUnassignedSubjects",
                    dataType: "json",
                    headers: {
                        'Authorization': 'Bearer ' + token
                    },
                }
            },
            schema: {
                data: "teacherSubjectData"
            }
        },
        height: 350,
        pageable: {
            refresh: true,
            pageSizes: [5, 10, 20],
            buttonCount: 5
        },
        sortable: true,
        filterable: true,
        columns: [
            { field: "fullName", title: "Teacher Name", width: "40%" },
            {
                field: "unassignedSubjects",
                title: "Unassigned Subjects",
                width: "60%",
                template: "#= unassignedSubjects.map(s => '<span class=\"badge bg-secondary me-1 mb-1\">' + s.subjectName + '</span>').join(' ') #"
            }
        ]
    });
}

// ✅ Load Teachers in Kendo ComboBox
function loadTeachersForDropdown() {
    $("#teacherDropdown").kendoComboBox({
        placeholder: "Select Teacher",
        dataTextField: "fullName",
        dataValueField: "teacherId",
        filter: "contains",
        dataSource: {
            transport: {
                read: {
                    url: "http://localhost:5111/api/AdminApi/GetAllTeachersWithUnassignedSubjects",
                    dataType: "json",
                    headers: {
                        'Authorization': 'Bearer ' + token
                    },
                }
            },
            schema: {
                data: "teacherSubjectData"
            }
        },
        change: function () {
            loadSubjectsForDropdown();
        }
    });
}


// ✅ Load Unassigned Subjects in Kendo ComboBox
function loadSubjectsForDropdown() {
    var selectedTeacher = $("#teacherDropdown").data("kendoComboBox").value();

    if (selectedTeacher) {
        $.ajax({
            url: "http://localhost:5111/api/AdminApi/GetAllTeachersWithUnassignedSubjects",
            type: "GET",
            headers: {
                'Authorization': 'Bearer ' + token
            },
            success: function (response) {
                var teacher = response.teacherSubjectData.find(t => t.teacherId == selectedTeacher);

                if (teacher) {
                    $("#subjectDropdown").kendoComboBox({
                        placeholder: "Select Subject",
                        dataTextField: "subjectName",
                        dataValueField: "subjectId",
                        dataSource: teacher.unassignedSubjects
                    });
                }
            },
            error: function () {
                alert("Failed to load subjects.");
            }
        });
    }
}


// ✅ Assign Subject to Teacher
function assignSubjectToTeacher() {
    var teacherId = $("#teacherDropdown").data("kendoComboBox").value();
    var subjectId = $("#subjectDropdown").data("kendoComboBox").value();

    if (!teacherId || !subjectId) {
        alert("Please select both a teacher and a subject.");
        return;
    }

    $.ajax({
        url: "http://localhost:5111/api/AdminApi/assign-subject",
        type: "POST",
        contentType: "application/json",
        headers: {
            'Authorization': 'Bearer ' + token
        },
        data: JSON.stringify({ teacherId: teacherId, subjectId: subjectId }),
        success: function () {
            showNotification("Subject assigned successfully!", "success");
            $("#teacherDropdown").data("kendoComboBox").value("");
            $("#subjectDropdown").data("kendoComboBox").value("");
            loadAssignedSubjects();
            loadUnassignedSubjects();
        },
        error: function (error, xhr) {
            console.log(error);
            showNotification(xhr.responseJSON.message, "error");
        }
    });
}

function showNotification(message, type) {
    var notification = $("#notification").data("kendoNotification");
    notification.show({
        message: message
    }, type);
}
