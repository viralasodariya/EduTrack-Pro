let token = localStorage.getItem('authToken');

$(document).ready(function () {
    // Initialize notification widget
    $("#notification").kendoNotification({
        position: {
            pinned: false,
            top: 30,
            right: 30
        },
        autoHideAfter: 3000,
        stacking: "down"
    });

    // Get class list
    $("#classListGrid").kendoGrid({
        dataSource: GetClass(),
        height: 400,
        sortable: true,
        filterable: true,
        toolbar: ["search"],
        search: {
            fields: ["className", "classId"]
        },
        pageable: {
            refresh: true,
            pageSize: 5,
            pageSizes: [5, 10, 15, 20],
            buttonCount: 5
        },
        columns: [
            { field: "classId", title: "Class No", width: "30" },
            { field: "className", title: "Class Name", width: "30" }
        ]
    })

    // add new class
    $("#addNewClasskendForm").kendoForm({
        orientation: "vertical",
        formData: {
            className: "",
        },
        items: [
            {
                field: "className",
                label: "Class Name",
                validation: {
                    required: true
                },
            }
        ],
        buttonsTemplate: `<button type="submit" class="k-button k-primary mt-3" id="addNewClassBtn">Add Class</button>`,
    });

    $("#addNewClassBtn").click(function (e) {
        e.preventDefault();
        AddClassName();
    })

    loadUnassignedData();
    loadAssignedData();
})

//Get class list
function GetClass() {
    return new kendo.data.DataSource({
        transport: {
            read: {
                url: "http://localhost:5111/api/AdminApi/get-class",
                dataType: "json",
                headers: {
                    'Authorization': 'Bearer ' + token
                }
            },
        },
        schema: {
            data: function (response) {
                return response.classes;
            }
        }
    });
}

// Add new class
function AddClassName() {
    var className = $("#addNewClasskendForm [name='className']").val();
    var classNameToLowerCase = className.toLowerCase();


    $.ajax({
        url: "http://localhost:5111/api/AdminApi/createclass",
        type: "POST",
        contentType: "application/json",
        headers: {
            'Authorization': 'Bearer ' + token
        },
        data: JSON.stringify(classNameToLowerCase),
        success: function (response) {
            if (response.success) {

                showNotification("Class added successfully!", "success");
                $("#classListGrid").data("kendoGrid").dataSource.read();
            }
            else {
                showNotification(response.message, "error");
            }
        },
        error: function (xhr, error) {
            if (xhr.responseJSON && xhr.responseJSON.message) {
                alert(xhr.responseJSON.message);
            } else {
                alert("An error occurred while adding the class.");
            }
            console.log(xhr.responseText);
            console.log(error);
        }
    })
}


function loadUnassignedData() {
    $.ajax({
        url: "http://localhost:5111/api/AdminApi/UnassignedTeachersAndClasses",
        type: "GET",
        headers: {
            'Authorization': 'Bearer ' + token
        },
        success: function (response) {
            if (response.success) {
                $("#teachersList").kendoListView({
                    dataSource: response.teachers,
                    template: "<div class='k-listview-item'><i class='fas fa-user-tie me-2'></i>#: fullName #</div>"
                });

                $("#classesList").kendoListView({
                    dataSource: response.classes,
                    template: "<div class='k-listview-item'><i class='fas fa-chalkboard me-2'></i>#: className #</div>"
                });

                // Populate ComboBox
                $("#teacherDropdown").kendoComboBox({
                    dataSource: response.teachers,
                    dataTextField: "fullName",
                    dataValueField: "teacherId",
                    optionLabel: "Select a Teacher",
                    filter: "contains",
                    placeholder: "Select a teacher...",
                    clearButton: true
                });

                $("#classDropdown").kendoComboBox({
                    dataSource: response.classes,
                    dataTextField: "className",
                    dataValueField: "classId",
                    optionLabel: "Select a Class",
                    filter: "contains",
                    placeholder: "Select a class...",
                    clearButton: true
                });
            }
        }
    });
}

$("#assignBtn").kendoButton({
    icon: "check",
    click: function () {
        let teacherId = $("#teacherDropdown").val();
        let classId = $("#classDropdown").val();

        if (!teacherId || !classId) {
            showNotification("Please select a teacher and a class", "error");
            return;
        }

        $.ajax({
            url: "http://localhost:5111/api/AdminApi/AssignClassToTeacher",
            type: "POST",
            headers: {
                'Authorization': 'Bearer ' + token
            },
            contentType: "application/json",
            data: JSON.stringify({ teacherId: parseInt(teacherId), classId: parseInt(classId) }),
            success: function (response) {
                if (response.success) {
                    showNotification("Class assigned successfully!", "success");

                    loadUnassignedData(); // Refresh the list
                    loadAssignedData(); // Refresh assigned list

                      // Reset the dropdowns properly
                      let teacherDropdown = $("#teacherDropdown").data("kendoDropDownList");
                      let classDropdown = $("#classDropdown").data("kendoDropDownList");
  
                      if (teacherDropdown) {
                          teacherDropdown.value(null); // Clear selection
                          teacherDropdown.trigger("change"); // Ensure UI updates
                          teacherDropdown.refresh(); // Refresh dropdown
                      }
  
                      if (classDropdown) {
                          classDropdown.value(null);
                          classDropdown.trigger("change");
                          classDropdown.refresh();
                      }

                } else {
                    showNotification(response.message, "error");
                }
            }
        });
    }
});

function loadAssignedData() {
    $.ajax({
        url: "http://localhost:5111/api/AdminApi/GetAssignedClassesAndTeachers",
        type: "GET",
        success: function (response) {
            if (response.success) {
                $("#assignedGrid").kendoGrid({
                    dataSource: {
                        data: response.data,
                        schema: {
                            model: {
                                fields: {
                                    className: { type: "string" },
                                    teacherName: { type: "string" }
                                }
                            }
                        }
                    },
                    height: 400,
                    sortable: true,
                    filterable: true,
                    toolbar: ["search"],
                    search: {
                        fields: ["className", "teacherName"]
                    },
                    pageable: {
                        refresh: true,
                        pageSize: 10,
                        pageSizes: [5, 10, 15, 20]
                    },
                    columns: [
                        { field: "className", title: "Class Name", width: "40%" },
                        { field: "teacherName", title: "Teacher Name", width: "40%" }
                    ]
                });
            }
        }
    });
}

function showNotification(message, type) {
    var notification = $("#notification").data("kendoNotification");
    notification.show({
        message: message
    }, type);
}
