let token = localStorage.getItem("authToken");
if (token) {
    token = "Bearer " + token;
}

$(document).ready(function () {
    fetchMaterials();
    applyCustomStyles();
});

function applyCustomStyles() {
    // Add custom CSS for the grid and related elements
    const customStyles = `
        <style>
            #materialsGrid {
                border-radius: 8px;
                overflow: hidden;
                box-shadow: 0 4px 12px rgba(0,0,0,0.1);
            }
            #materialsGrid .k-grid-header {
                background-color: #f8f9fa;
                font-weight: bold;
            }
            #materialsGrid .k-grid-content tr:hover {
                background-color: #f1f9fe !important;
                transition: background-color 0.2s ease;
            }
            .loading, .error-message, .no-data {
                padding: 30px;
                text-align: center;
                background-color: #fff;
                border-radius: 8px;
                box-shadow: 0 2px 8px rgba(0,0,0,0.1);
                margin: 20px 0;
                font-size: 16px;
            }
            .loading { color: #3498db; }
            .error-message { color: #e74c3c; }
            .no-data { color: #7f8c8d; }
            .download-btn {
                display: inline-block;
                padding: 6px 12px;
                background-color: #3498db;
                color: white !important;
                border-radius: 4px;
                text-decoration: none;
                transition: all 0.3s ease;
            }
            .download-btn:hover {
                background-color: #2980b9;
                text-decoration: none;
                transform: translateY(-2px);
                box-shadow: 0 2px 5px rgba(0,0,0,0.2);
            }
            .download-btn i {
                margin-right: 6px;
            }
            .k-pager-wrap {
                background-color: #f8f9fa !important;
                border-top: 1px solid #e9ecef;
            }
        </style>
    `;
    $('head').append(customStyles);
}

function fetchMaterials() {
    showLoading();
    $.ajax({
        url: "http://localhost:5111/api/StudentApi/GetAllMaterials",
        type: "GET",
        headers: {
            "Authorization": token
        },
        success: function (response) {
            // hideLoading();
            if (response && response.success && response.materials && response.materials.length > 0) {
                renderGrid(response.materials);
            } else {
                $("#materialsGrid").html("<div class='no-data'><i class='fas fa-info-circle me-2'></i>No materials available</div>");
                console.log("Response data:", response);
            }
        },
        error: function (xhr) {
            hideLoading();
            console.error("Error details:", xhr);
            $("#materialsGrid").html("<div class='error-message'><i class='fas fa-exclamation-triangle me-2'></i>Error loading materials. Please try again later.</div>");
        }
    });
}

function showLoading() {
    $("#materialsGrid").html("<div class='loading'><i class='fas fa-spinner fa-spin me-2'></i>Loading materials...</div>");
}

function hideLoading() {
    // Will be replaced by the grid or error message
}

function renderGrid(materials) {
    $("#materialsGrid").kendoGrid({
        dataSource: {
            data: materials,
            schema: {
                model: {
                    fields: {
                        fileName: { type: "string" },
                        fileType: { type: "string" },
                        uploadDate: { type: "date" },
                        teacherName: { type: "string" },
                        subjectName: { type: "string" },
                        filePath: { type: "string" }
                    }
                }
            },
            pageSize: 10
        },
        height: 550,
        pageable: {
            refresh: true,
            pageSizes: [5, 10, 20, 50],
            buttonCount: 5
        },
        sortable: true,
        filterable: true,
        responsive: true,
        mobile: true,
        columns: [
            {
                field: "fileName",
                title: "File Name",
                width: "25%",
                template: "<div class='file-name'><i class='fas fa-file me-2'></i>#=fileName#</div>"
            },
            {
                field: "fileType",
                title: "Type",
                width: "10%",
                template: function (dataItem) {
                    let iconClass = "fa-file";
                    if (dataItem.fileType === "PDF") iconClass = "fa-file-pdf";
                    else if (dataItem.fileType === "DOC" || dataItem.fileType === "DOCX") iconClass = "fa-file-word";
                    else if (dataItem.fileType === "XLS" || dataItem.fileType === "XLSX") iconClass = "fa-file-excel";
                    else if (dataItem.fileType === "PPT" || dataItem.fileType === "PPTX") iconClass = "fa-file-powerpoint";
                    else if (dataItem.fileType === "ZIP" || dataItem.fileType === "RAR") iconClass = "fa-file-archive";
                    else if (dataItem.fileType === "JPG" || dataItem.fileType === "PNG" || dataItem.fileType === "GIF") iconClass = "fa-file-image";

                    return `<div class="file-type"><i class="fas ${iconClass} me-1"></i> ${dataItem.fileType}</div>`;
                }
            },
            {
                field: "uploadDate",
                title: "Uploaded Date",
                format: "{0:yyyy-MM-dd}",
                width: "15%",
                attributes: {
                    "class": "text-center"
                }
            },
            { field: "teacherName", title: "Teacher", width: "15%" },
            { field: "subjectName", title: "Subject", width: "15%" },
            {
                field: "filePath",
                title: "Download",
                width: "20%",
                attributes: {
                    "class": "text-center"
                },
                template: '<a href="#=filePath#" class="download-btn" download><i class="fas fa-download"></i>Download</a>'
            }
        ],
        dataBound: function () {
            // Apply alternating row colors for better readability
            $("#materialsGrid tbody tr:odd").css("background-color", "#f9f9f9");
        }
    });

    // Make grid responsive on window resize
    $(window).resize(function () {
        var grid = $("#materialsGrid").data("kendoGrid");
        if (grid) {
            grid.resize();
        }
    });
}
