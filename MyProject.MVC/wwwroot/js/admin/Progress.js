let token = localStorage.getItem("authToken");
let progressData = [];
let currentChartType = 'donut';

$(document).ready(function () {
    loadProgressData();

    // Event handlers
    $("#refreshData").on("click", loadProgressData);
    $("#applyFilters").on("click", applyFilters);
    $("#showDonut").on("click", function () {
        currentChartType = 'donut';
        $(this).addClass('active');
        $("#showBar").removeClass('active');
        renderChart(filterData());
    });
    $("#showBar").on("click", function () {
        currentChartType = 'bar';
        $(this).addClass('active');
        $("#showDonut").removeClass('active');
        renderChart(filterData());
    });

    // Handle responsive behavior
    $(window).on('resize', function () {
        if (progressData.length > 0) {
            renderChart(filterData());
        }
    });
});

// Load Progress Data
function loadProgressData() {
    $("#progressChart").html('<div class="loading-spinner"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>');

    $.ajax({
        url: "http://localhost:5111/api/AdminApi/dashboard/progress/all",
        type: "GET",
        dataType: "json",
        headers: { "Authorization": "Bearer " + token },
        success: function (response) {
            if (response.success && response.data.length > 0) {
                progressData = response.data;
                populateFilters(progressData);
                renderChart(progressData);
                renderGrid(progressData);
                updateSummary(progressData);
            } else {
                console.warn("No progress data available");
                $("#progressChart").html("<div class='alert alert-warning'><i class='fas fa-exclamation-triangle me-2'></i>No progress data available</div>");
                $("#progressGrid").html("<div class='alert alert-warning'><i class='fas fa-exclamation-triangle me-2'></i>No progress data available</div>");
            }
        },
        error: function (xhr) {
            console.error("Error loading progress data:", xhr.responseText);
            $("#progressChart").html("<div class='alert alert-danger'><i class='fas fa-times-circle me-2'></i>Error loading data. Please try again.</div>");
        }
    });
}

// Populate filter dropdowns
function populateFilters(data) {
    let teachers = new Set();
    let subjects = new Set();

    data.forEach(item => {
        teachers.add(item.teacherName);
        subjects.add(item.subjectName);
    });

    // Clear existing options except the first one
    $("#teacherFilter option:not(:first)").remove();
    $("#subjectFilter option:not(:first)").remove();

    // Add new options
    teachers.forEach(teacher => {
        $("#teacherFilter").append(`<option value="${teacher}">${teacher}</option>`);
    });

    subjects.forEach(subject => {
        $("#subjectFilter").append(`<option value="${subject}">${subject}</option>`);
    });
}

// Apply filters
function applyFilters() {
    let filteredData = filterData();
    renderChart(filteredData);
    renderGrid(filteredData);
    updateSummary(filteredData);
}

// Filter data based on selections
function filterData() {
    let teacherFilter = $("#teacherFilter").val();
    let subjectFilter = $("#subjectFilter").val();

    return progressData.filter(item => {
        return (teacherFilter === "all" || item.teacherName === teacherFilter) &&
            (subjectFilter === "all" || item.subjectName === subjectFilter);
    });
}

// Update summary statistics
function updateSummary(data) {
    if (data.length === 0) {
        $("#avgCompletion, #highestProgress, #lowestProgress").text("N/A");
        return;
    }

    let sum = data.reduce((acc, item) => acc + item.completionPercentage, 0);
    let avg = Math.round(sum / data.length);
    let highest = Math.max(...data.map(item => item.completionPercentage));
    let lowest = Math.min(...data.map(item => item.completionPercentage));

    $("#avgCompletion").text(avg + "%");
    $("#highestProgress").text(highest + "%");
    $("#lowestProgress").text(lowest + "%");
}

// Render chart based on type
function renderChart(data) {
    if (data.length === 0) {
        $("#progressChart").html("<div class='alert alert-warning'><i class='fas fa-exclamation-triangle me-2'></i>No data available for the selected filters</div>");
        return;
    }

    if (currentChartType === 'donut') {
        renderDonutChart(data);
    } else {
        renderBarChart(data);
    }
}

// Render Enhanced Donut Chart
function renderDonutChart(data) {
    let chartData = data.map(item => ({
        category: `${item.teacherName} - ${item.subjectName}`,
        value: item.completionPercentage
    }));

    $("#progressChart").kendoChart({
        theme: "default",
        title: {
            text: "Assignment Progress (%)",
            font: "20px Arial, sans-serif",
            align: "center",
            color: "#1E88E5"
        },
        legend: {
            position: "bottom",
            labels: {
                font: "14px Arial, sans-serif",
                color: "#444"
            }
        },
        series: [{
            type: "donut",
            holeSize: 40,
            overlay: {
                gradient: "roundedBevel"
            },
            startAngle: 150,
            labels: {
                visible: true,
                background: "transparent",
                color: "#000",
                template: "#= category # (#= value #%)"
            },
            data: chartData
        }],
        chartArea: {
            background: "white"
        },
        seriesColors: ["#4CAF50", "#2196F3", "#FF9800", "#F44336", "#9C27B0", "#00BCD4", "#FFEB3B", "#795548"],
        tooltip: {
            visible: true,
            background: "#333",
            color: "#fff",
            font: "14px Arial, sans-serif",
            template: "<strong>#= category #</strong>: #= value #%"
        },
        transitions: true
    });
}

// Render Bar Chart
function renderBarChart(data) {
    let categories = data.map(item => `${item.teacherName} - ${item.subjectName}`);
    let seriesData = data.map(item => item.completionPercentage);

    $("#progressChart").kendoChart({
        theme: "default",
        title: {
            text: "Assignment Progress (%)",
            font: "20px Arial, sans-serif",
            align: "center",
            color: "#1E88E5"
        },
        legend: {
            visible: false
        },
        seriesDefaults: {
            type: "column",
            labels: {
                visible: true,
                background: "transparent",
                template: "#= value #%",
                font: "12px Arial, sans-serif"
            }
        },
        series: [{
            name: "Completion Rate",
            data: seriesData,
            color: "#2196F3"
        }],
        valueAxis: {
            max: 100,
            line: {
                visible: false
            },
            title: {
                text: "Completion Percentage",
                font: "14px Arial, sans-serif"
            }
        },
        categoryAxis: {
            categories: categories,
            majorGridLines: {
                visible: false
            },
            labels: {
                rotation: "auto",
                font: "12px Arial, sans-serif"
            }
        },
        chartArea: {
            background: "white"
        },
        tooltip: {
            visible: true,
            template: "<strong>#= category #</strong>: #= value #%"
        },
        transitions: true
    });
}

// Render Data Grid
function renderGrid(data) {
    $("#progressGrid").kendoGrid({
        dataSource: {
            data: data,
            schema: {
                model: {
                    fields: {
                        teacherName: { type: "string" },
                        subjectName: { type: "string" },
                        completionPercentage: { type: "number" }
                    }
                }
            }
        },
        height: 400,
        sortable: true,
        filterable: true,
        columnMenu: true,
        columns: [
            { field: "teacherName", title: "Teacher", width: "30%" },
            { field: "subjectName", title: "Subject", width: "30%" },
            {
                field: "completionPercentage",
                title: "Progress",
                width: "40%",
                template: function (dataItem) {
                    let progressClass = "";
                    if (dataItem.completionPercentage < 30) progressClass = "danger";
                    else if (dataItem.completionPercentage < 70) progressClass = "warning";
                    else progressClass = "success";

                    return `<div class="progress">
                              <div class="progress-bar bg-${progressClass}" role="progressbar" 
                                   style="width: ${dataItem.completionPercentage}%" 
                                   aria-valuenow="${dataItem.completionPercentage}" aria-valuemin="0" aria-valuemax="100">
                                ${dataItem.completionPercentage}%
                              </div>
                            </div>`;
                }
            }
        ]
    });
}
