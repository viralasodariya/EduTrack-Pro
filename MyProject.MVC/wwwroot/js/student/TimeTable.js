// Get authentication token
var token = localStorage.getItem("authToken");
$(document).ready(function () {
    fetchTimeTable();

    // Week selector change event
    $("#weekSelect").on("change", function () {
        fetchTimeTable($(this).val());
    });

    // Print button click event
    $("#printButton").on("click", function () {
        window.print();
    });

    // Download PDF button click event
    $("#downloadButton").on("click", function () {
        generatePDF();
    });
});

function fetchTimeTable(week = "current") {
    // Show loading spinner
    $("#loadingSpinner").show();
    $("#timeTable").hide();

    $.ajax({
        url: "http://localhost:5111/api/StudentApi/GetclassTimeTable",
        type: "GET",
        dataType: "json",
        headers: { "Authorization": "Bearer " + token },
        data: { week: week },
        success: function (response) {
            // Hide spinner
            $("#loadingSpinner").hide();
            $("#timeTable").show();

            if (response.succes) {
                renderTimeTable(response.listTimeTable);
            } else {
                showError("Failed to fetch timetable.");
            }
        },
        error: function (xhr, status, error) {
            // Hide spinner
            $("#loadingSpinner").hide();
            $("#timeTable").show();

            console.error("Error:", error);
            showError("Error fetching timetable. Please try again later.");
        }
    });
}

function renderTimeTable(data) {
    let daysOfWeek = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

    // Define time slots from 9 AM to 5 PM (1-hour intervals)
    let timeSlots = [];
    for (let hour = 9; hour <= 17; hour++) {
        let formattedTime = `${hour.toString().padStart(2, '0')}:00:00`; // Format as HH:00:00
        timeSlots.push(formattedTime);
    }

    let tableBody = $("#timeTable tbody");
    tableBody.empty(); // Clear previous data

    timeSlots.forEach(time => {
        let row = `<tr><td>${formatTime(time)}</td>`;

        daysOfWeek.forEach(day => {
            let subjectData = data.find(item => item.timeSlot === time && item.dayOfWeek === day);

            if (subjectData) {
                // Determine subject class for color coding
                let subjectClass = getSubjectClass(subjectData.subjectName);

                row += `<td>
                    <div class="subject-cell ${subjectClass}">
                        <div class="subject-name">${subjectData.subjectName}</div>
                        <div class="teacher-name">${subjectData.teacherName}</div>
                    </div>
                </td>`;
            } else {
                row += `<td>-</td>`;
            }
        });

        row += `</tr>`;
        tableBody.append(row);
    });
}

// Function to convert time (HH:MM:SS) into AM/PM format
function formatTime(time) {
    let [hour, minute, second] = time.split(":").map(Number);
    let period = hour >= 12 ? "PM" : "AM";
    hour = hour > 12 ? hour - 12 : hour;
    return `${hour}:00 ${period}`;
}

// Function to determine subject class for color coding
function getSubjectClass(subjectName) {
    subjectName = subjectName.toLowerCase();

    if (subjectName.includes("math")) return "subject-math";
    if (subjectName.includes("science") || subjectName.includes("chemistry") ||
        subjectName.includes("physics") || subjectName.includes("biology")) return "subject-science";
    if (subjectName.includes("english") || subjectName.includes("language") ||
        subjectName.includes("literature")) return "subject-english";
    if (subjectName.includes("history") || subjectName.includes("geography") ||
        subjectName.includes("social")) return "subject-history";
    if (subjectName.includes("computer") || subjectName.includes("programming") ||
        subjectName.includes("it")) return "subject-computer";

    // Rotate through classes for other subjects
    const hash = stringToHashCode(subjectName);
    const classes = ["subject-math", "subject-science", "subject-english", "subject-history", "subject-computer"];
    return classes[Math.abs(hash) % classes.length];
}

// Simple string hash function
function stringToHashCode(str) {
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
        const char = str.charCodeAt(i);
        hash = ((hash << 5) - hash) + char;
        hash = hash & hash; // Convert to 32bit integer
    }
    return hash;
}

// Function to show error messages
function showError(message) {
    let tableBody = $("#timeTable tbody");
    tableBody.html(`<tr><td colspan="7" class="text-center text-danger">${message}</td></tr>`);
}

// Function to generate PDF
function generatePDF() {
    const element = document.querySelector('.timetable-container');

    // Clone the element to modify it without affecting the displayed content
    const clonedElement = element.cloneNode(true);

    // Remove buttons from the cloned element
    const buttons = clonedElement.querySelectorAll('.view-options');
    buttons.forEach(button => button.remove());

    // PDF options
    const options = {
        margin: 10,
        filename: 'class-timetable.pdf',
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2 },
        jsPDF: { unit: 'mm', format: 'a4', orientation: 'landscape' }
    };

    // Generate PDF
    html2pdf().from(clonedElement).set(options).save();
}