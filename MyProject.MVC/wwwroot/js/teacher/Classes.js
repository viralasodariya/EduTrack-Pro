let token = localStorage.getItem("authToken");

$(document).ready(function () {
    $.ajax({
        url: "http://localhost:5111/api/TeacherApi/teacher/upcoming-classes",
        type: "GET",
        dataType: "json",
        headers: {
            "Authorization": `Bearer ${token}`
        },
        success: function (response) {
            if (response.success) {
                var events = response.data.map(function (item) {
                    return {
                        id: item.timetableId,
                        title: `${item.className} (${item.subjectName})`, // Class & Subject
                        start: getNextWeekdayDate(item.dayOfWeek, item.timeSlot),
                        end: getNextWeekdayDate(item.dayOfWeek, item.timeSlot, 1) // +1 hour duration
                    };
                });

                $("#scheduler").kendoScheduler({
                    date: new Date(),
                    views: ["week"],
                    startTime: new Date(2023, 1, 1, 9, 0, 0),
                    endTime: new Date(2023, 1, 1, 17, 0, 0),
                    editable: false,
                    eventTemplate: `
                        <div class="custom-event">
                            <div class="event-content">#: title #</div>
                        </div>
                    `, // Custom Template
                    dataSource: {
                        data: events,
                        schema: {
                            model: {
                                id: "id",
                                fields: {
                                    id: { type: "number" },
                                    title: { type: "string" },
                                    start: { type: "date" },
                                    end: { type: "date" }
                                }
                            }
                        }
                    }
                });
            }
        },
        error: function (xhr, status, error) {
            console.error("Error fetching upcoming classes:", error);
        }
    });

    // Function to get the correct date for the next occurrence of a given weekday
    function getNextWeekdayDate(dayOfWeek, time, duration = 0) {
        var daysMap = { "Monday": 1, "Tuesday": 2, "Wednesday": 3, "Thursday": 4, "Friday": 5, "Saturday": 6, "Sunday": 0 };
        var today = new Date();
        var resultDate = new Date(today);
        var currentDay = today.getDay();
        var targetDay = daysMap[dayOfWeek];

        var daysToAdd = targetDay >= currentDay ? targetDay - currentDay : 7 - (currentDay - targetDay);
        resultDate.setDate(today.getDate() + daysToAdd);

        var timeParts = time.split(":");
        resultDate.setHours(parseInt(timeParts[0]) + duration, parseInt(timeParts[1]), parseInt(timeParts[2]));

        return resultDate;
    }
});



