let token = localStorage.getItem("authToken");
let subjectsGrid;
let isInitialized = false; // Track initialization state

$(document).ready(function () {
    // Clean up any existing resources on page reload
    cleanupResources();

    // Show loading indicator
    $("#loadingIndicator").removeClass("d-none");

    // Fetch data from API and initialize grid
    fetchSubjectsData();

    // Event listeners
    // Remove previous event handlers to prevent duplicates on reload
    $("#refreshData").off("click").on("click", function (e) {
        e.preventDefault();
        fetchSubjectsData();
    });

    $("#toggleCompletedSubjects").off("click").on("click", function (e) {
        e.preventDefault();
        toggleCompletedSubjects();
    });

    // Handle window resize for responsive grid
    $(window).off("resize.subjectsGrid").on("resize.subjectsGrid", function () {
        if (subjectsGrid) {
            adjustGridColumns();
        }
    });

    // Handle page beforeunload to clean up resources
    $(window).off("beforeunload.subjectsGrid").on("beforeunload.subjectsGrid", function () {
        cleanupResources();
    });

    function cleanupResources() {
        // Destroy existing Kendo widgets to prevent memory leaks
        if (subjectsGrid) {
            try {
                subjectsGrid.destroy();
            } catch (e) {
                console.log("Error destroying grid:", e);
            }
            subjectsGrid = null;
        }

        // Remove any lingering toasts
        $(".toast").parent().remove();

        // Flag as not initialized
        isInitialized = false;
    }

    // Ensure token is valid before making requests
    function validateToken() {
        token = localStorage.getItem("authToken");
        if (!token) {
            showErrorMessage("Authentication token missing. Please log in again.");
            return false;
        }
        return true;
    }

    function fetchSubjectsData() {
        if (!validateToken()) return;

        $("#loadingIndicator").removeClass("d-none");
        $("#subjectsGrid").empty();

        $.ajax({
            url: "http://localhost:5111/api/TeacherApi/Teacher/assignedSubjects",
            type: "GET",
            dataType: "json",
            headers: { "Authorization": "Bearer " + token },
            success: function (response) {
                $("#loadingIndicator").addClass("d-none");
                if (response.success && Array.isArray(response.data)) {
                    loadSubjectsGrid(response.data);
                    isInitialized = true;
                } else {
                    showErrorMessage("No subjects assigned.");
                }
            },
            error: function (xhr) {
                $("#loadingIndicator").addClass("d-none");
                if (xhr.status === 401) {
                    // Handle authorization error
                    localStorage.removeItem("authToken");
                    showErrorMessage("Your session has expired. Please log in again.");
                } else {
                    showErrorMessage("Failed to load subjects. " + (xhr.responseJSON?.message || ""));
                }
            }
        });
    }

    function loadSubjectsGrid(subjects) {
        // Validate data to prevent type errors
        subjects = subjects.map(subject => ({
            assignmentId: subject.assignmentId || 0,
            subjectName: subject.subjectName || "",
            startDate: subject.startDate ? new Date(subject.startDate) : null,
            endDate: subject.endDate ? new Date(subject.endDate) : null,
            completionPercentage: typeof subject.completionPercentage === 'number' ? subject.completionPercentage : 0
        }));

        let gridColumns = getResponsiveColumns();

        subjectsGrid = $("#subjectsGrid").kendoGrid({
            dataSource: {
                data: subjects,
                schema: {
                    model: {
                        id: "assignmentId",
                        fields: {
                            subjectName: { type: "string" },
                            startDate: { type: "date" },
                            endDate: { type: "date" },
                            completionPercentage: { type: "number" }
                        }
                    }
                },
                pageSize: 10
            },
            height: "500px",
            pageable: {
                refresh: true,
                pageSizes: [5, 10, 20, "All"],
                buttonCount: 5
            },
            mobile: "phone",
            responsive: true,
            editable: true,
            sortable: true,
            filterable: true,
            toolbar: ["search"],
            columns: gridColumns,
            dataBound: function () {
                initializeSliders();
                applyStatusColors();

                // Adjust grid for mobile devices
                adjustGridColumns();
            }
        }).data("kendoGrid");
    }

    function getResponsiveColumns() {
        let columns = [
            { field: "subjectName", title: "Subject Name", width: "20%" },
            {
                field: "startDate",
                title: "Start Date",
                format: "{0:yyyy-MM-dd}",
                width: "15%",
                editor: dateEditor
            },
            {
                field: "endDate",
                title: "End Date",
                format: "{0:yyyy-MM-dd}",
                width: "15%",
                editor: dateEditor
            },
            {
                field: "completionPercentage",
                title: "Completion",
                width: "30%",
                editor: sliderEditor,
                template: function (dataItem) {
                    return `
                        <div style='width:100%'>
                            <div class='d-flex justify-content-between'>
                                <span class='completion-text'>${dataItem.completionPercentage}%</span>
                                <span class='status-indicator'></span>
                            </div>
                            <div class='completion-bar'>
                                <div class='completion-progress' style='width: ${dataItem.completionPercentage}%'></div>
                            </div>
                        </div>
                    `;
                }
            },
            {
                command: [
                    {
                        name: "update",
                        text: "Update",
                        click: updateSubject,
                        className: "btn-action btn-primary btn-sm"
                    }
                ],
                title: "Actions",
                width: "20%"
            }
        ];

        // Adjust for small screens
        if (window.innerWidth < 768) {
            // For mobile, simplify columns
            return columns.filter(c => c.field !== "startDate" || c.title !== "Actions");
        }

        return columns;
    }

    function adjustGridColumns() {
        // Check if subjectsGrid is properly initialized
        if (!subjectsGrid || typeof subjectsGrid.hideColumn !== 'function' ||
            typeof subjectsGrid.showColumn !== 'function') {
            console.log("Grid not fully initialized yet");
            return;
        }

        let screenWidth = window.innerWidth;
        if (screenWidth < 576) {
            // Extra small devices
            subjectsGrid.hideColumn("startDate");
            subjectsGrid.hideColumn("endDate");
        } else if (screenWidth < 768) {
            // Small devices
            subjectsGrid.showColumn("endDate");
            subjectsGrid.hideColumn("startDate");
        } else {
            // Medium and larger devices
            subjectsGrid.showColumn("startDate");
            subjectsGrid.showColumn("endDate");
        }
    }

    function dateEditor(container, options) {
        $('<input name="' + options.field + '" class="k-input k-textbox"/>')
            .appendTo(container)
            .kendoDatePicker({
                format: "yyyy-MM-dd",
                value: options.model[options.field],
                placeholder: options.model[options.field] ? "" : "Select Date"
            });
    }

    function sliderEditor(container, options) {
        $('<input name="' + options.field + '"/>')
            .appendTo(container)
            .kendoSlider({
                min: 0,
                max: 100,
                smallStep: 5,
                largeStep: 20,
                tooltip: {
                    enabled: true,
                    template: "#= value #%"
                },
                showButtons: true,
                value: options.model[options.field],
                change: function (e) {
                    // Add visual feedback when slider changes
                    let sliderColor = isLegacyReference(options.model)
                        ? getLegacyCompletionColor(e.value)
                        : getCompletionColor(e.value);
                    $(e.sender.wrapper).find(".k-slider-selection").css("background-color", sliderColor);
                }
            });
    }

    // Helper function to check if we should use legacy styles
    function isLegacyReference(model) {
        // Check if the model has a property indicating legacy reference
        // Customize this based on your actual requirements
        return model && model.isLegacy === true;
    }

    // Legacy color scheme for backward compatibility
    function getLegacyCompletionColor(percentage) {
        if (percentage < 30) return "#cc0000"; // old red
        if (percentage < 60) return "#ff9900"; // old orange
        if (percentage < 90) return "#3399ff"; // old blue
        return "#009900"; // old green
    }

    function initializeSliders() {
        $(".slider").each(function () {
            let $this = $(this);
            let value = parseInt($this.val(), 10);
            let dataItem = $(this).closest("tr").data("kendoBindingTarget") ?
                $(this).closest("tr").data("kendoBindingTarget").source : null;

            $this.kendoSlider({
                min: 0,
                max: 100,
                smallStep: 5,
                largeStep: 20,
                tooltip: {
                    enabled: true,
                    template: "#= value #%"
                },
                showButtons: false,
                value: value,
                dragHandleTitle: "Drag to adjust completion",
                tickPlacement: "none"
            });

            // Set the color based on value
            let sliderColor = isLegacyReference(dataItem)
                ? getLegacyCompletionColor(value)
                : getCompletionColor(value);
            $this.closest(".k-slider").find(".k-slider-selection").css("background-color", sliderColor);
        });
    }

    // Helper function to get color based on completion percentage
    function getCompletionColor(percentage) {
        if (percentage < 25) return "#dc3545"; // danger
        if (percentage < 50) return "#fd7e14"; // warning
        if (percentage < 75) return "#0dcaf0"; // info
        if (percentage < 100) return "#0d6efd"; // primary
        return "#198754"; // success
    }

    function applyStatusColors() {
        $(".status-indicator").each(function () {
            let $row = $(this).closest("tr");
            // Check if $row exists and has data before proceeding
            if ($row.length > 0 && subjectsGrid) {
                let dataItem = subjectsGrid.dataItem($row);

                // Check if dataItem exists and has required properties
                if (dataItem) {
                    let today = new Date();
                    let endDate = dataItem.endDate ? new Date(dataItem.endDate) : null;
                    let completionColor = getCompletionColor(dataItem.completionPercentage);

                    // Apply row hover effect
                    $row.hover(
                        function () { $(this).addClass("subject-row-hover"); },
                        function () { $(this).removeClass("subject-row-hover"); }
                    );

                    // Set progress bar color
                    $row.find(".completion-progress").css("background-color", completionColor);

                    if (dataItem.completionPercentage >= 100) {
                        $(this).text("Completed").addClass("status-completed");
                        $row.addClass("subject-completed");
                    } else if (endDate && endDate < today) {
                        $(this).text("Overdue").addClass("status-overdue");
                        $row.addClass("subject-overdue");
                    } else {
                        $(this).text("In Progress").addClass("status-pending");
                        $row.addClass("subject-in-progress");
                    }
                }
            }
        });

        // Add CSS to head if not already present
        if (!$("#grid-custom-styles").length) {
            $("head").append(`
                <style id="grid-custom-styles">
                    .completion-bar {
                        background-color: #f0f0f0;
                        border-radius: 4px;
                        height: 12px;
                        overflow: hidden;
                        margin-top: 4px;
                        box-shadow: inset 0 1px 2px rgba(0,0,0,0.1);
                    }
                    .completion-progress {
                        height: 100%;
                        transition: width 0.6s ease;
                    }
                    .completion-text {
                        font-weight: 500;
                    }
                    .status-indicator {
                        font-size: 0.8rem;
                        padding: 2px 6px;
                        border-radius: 3px;
                    }
                    .status-completed {
                        background-color: #d4edda;
                        color: #155724;
                    }
                    .status-overdue {
                        background-color: #f8d7da;
                        color: #721c24;
                    }
                    .status-pending {
                        background-color: #fff3cd;
                        color: #856404;
                    }
                    .subject-row-hover {
                        background-color: rgba(0,123,255,0.05) !important;
                        transition: background-color 0.2s ease;
                    }
                    .subject-completed td {
                        background-color: rgba(40,167,69,0.05);
                    }
                    .subject-overdue td {
                        background-color: rgba(220,53,69,0.05);
                    }
                    .k-grid-content tr.k-alt {
                        background-color: rgba(0,0,0,0.02);
                    }
                    .k-grid tr:hover {
                        box-shadow: 0 0 10px rgba(0,0,0,0.1);
                        z-index: 1;
                        position: relative;
                    }
                    .btn-action {
                        transition: all 0.2s;
                    }
                    .btn-action:hover {
                        transform: scale(1.05);
                    }
                </style>
            `);
        }
    }

    function toggleCompletedSubjects() {
        let dataSource = subjectsGrid.dataSource;
        let filter = dataSource.filter() || { logic: "and", filters: [] };

        let completedFilter = filter.filters.find(f =>
            f.field === "completionPercentage" && f.operator === "lt");

        if (completedFilter) {
            // Remove filter
            filter.filters = filter.filters.filter(f =>
                !(f.field === "completionPercentage" && f.operator === "lt"));
            $("#toggleCompletedSubjects").text("Hide Completed");
        } else {
            // Add filter to hide completed subjects
            filter.filters.push({
                field: "completionPercentage",
                operator: "lt",
                value: 100
            });
            $("#toggleCompletedSubjects").text("Show Completed");
        }

        dataSource.filter(filter);
    }

    function updateSubject(e) {
        e.preventDefault();
        if (!validateToken()) return;

        let dataItem = this.dataItem($(e.currentTarget).closest("tr"));

        $.ajax({
            url: "http://localhost:5111/api/TeacherApi/Teacher/updateSubjectProgress",
            type: "PUT",
            contentType: "application/json",
            headers: { "Authorization": "Bearer " + token },
            data: JSON.stringify({
                assignmentId: dataItem.assignmentId,
                startDate: dataItem.startDate ? dataItem.startDate.toISOString() : null,
                endDate: dataItem.endDate ? dataItem.endDate.toISOString() : null,
                completionPercentage: dataItem.completionPercentage
            }),
            success: function () {
                showToast("Success", "Subject progress updated successfully.", "success");
                fetchSubjectsData(); // Refresh grid
            },
            error: function (xhr) {
                if (xhr.status === 401) {
                    localStorage.removeItem("authToken");
                    showToast("Error", "Your session has expired. Please log in again.", "error");
                } else {
                    showToast("Error", "Failed to update subject progress. " +
                        (xhr.responseJSON?.message || ""), "error");
                }
            }
        });
    }

    function showErrorMessage(message) {
        $("#subjectsGrid").html(`<div class="alert alert-warning">${message}</div>`);
    }

    function showToast(title, message, type) {
        // Remove existing toasts first
        $(".toast").parent().remove();

        $("body").append(`
            <div class="position-fixed bottom-0 end-0 p-3" style="z-index: 5">
                <div class="toast show" role="alert" aria-live="assertive" aria-atomic="true">
                    <div class="toast-header ${type === 'success' ? 'bg-success text-white' : 'bg-danger text-white'}">
                        <strong class="me-auto">${title}</strong>
                        <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
                    </div>
                    <div class="toast-body">
                        ${message}
                    </div>
                </div>
            </div>
        `);

        setTimeout(function () {
            $(".toast").toast("hide");
            setTimeout(function () {
                $(".toast").parent().remove();
            }, 500);
        }, 3000);
    }
});
