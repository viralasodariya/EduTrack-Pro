let token = localStorage.getItem("authToken");


$(document).ready(function () {
    loadUnreadNotifications();
    loadReadNotifications();
});

// Load Unread Notifications (ListView)
function loadUnreadNotifications() {
    $.ajax({
        url: "http://localhost:5111/api/AdminApi/dashboard/GetUnreadNotifications",
        type: "GET",
        dataType: "json",
        headers: { "Authorization": "Bearer " + token },
        success: function (response) {
            const dataSource = new kendo.data.DataSource({
                data: response.notifications || []
            });

            if (!response.notifications || response.notifications.length === 0) {
                $("#unreadNotificationsList").html('<div class="no-notifications">No unread notifications</div>');
                return;
            }

            $("#unreadNotificationsList").kendoListView({
                dataSource: dataSource,
                template: `<div class='notification-item unread-notification' style='cursor: pointer;' data-id='#= notificationId #'>
                    <strong>#= message #</strong>
                    <div class="notification-time">
                        <i class="far fa-clock"></i>
                        <small>#= kendo.toString(new Date(createdAt), 'MMM dd, yyyy HH:mm') #</small>
                    </div>
                </div>`,
                dataBound: function () {
                    $(".notification-item").off("click").on("click", function () {
                        let notificationId = $(this).data("id");
                        markNotificationAsRead(notificationId);
                    });
                }
            });
        },
        error: function (xhr) {
            console.error("Error loading unread notifications:", xhr.responseText);
            $("#unreadNotificationsList").html('<div class="no-notifications">Failed to load notifications</div>');
        }
    });
}

// Load Read Notifications (ListView)
function loadReadNotifications() {
    $.ajax({
        url: "http://localhost:5111/api/AdminApi/dashboard/GetreadNotifications",
        type: "GET",
        dataType: "json",
        headers: { "Authorization": "Bearer " + token },
        success: function (response) {

            $("#readNotificationsList").kendoListView({
                dataSource: response.notifications,
                template: `<div class='notification-item read-notification'>
                    <strong>#= message #</strong>
                    <div class="notification-time">
                        <i class="far fa-clock"></i>
                        <small>#= kendo.toString(new Date(createdAt), 'MMM dd, yyyy HH:mm') #</small>
                    </div>
                </div>`,
            });
        },
        error: function (xhr) {
            console.error("Error loading read notifications:", xhr.responseText);
            $("#readNotificationsList").html('<div class="no-notifications">Failed to load notifications</div>');
        }
    });
}

// Mark Notification as Read
function markNotificationAsRead(notificationId) {
    $.ajax({
        url: `http://localhost:5111/api/AdminApi/dashboard/notifications/mark-read/${notificationId}`,
        type: "PUT",
        headers: { "Authorization": "Bearer " + token },
        success: function () {
            $("#unreadNotificationsList").data("kendoListView").dataSource.data([]);
            loadUnreadNotifications(); // Reload unread notifications
            $("#readNotificationsList").data("kendoListView").dataSource.data([]);
            loadReadNotifications();   // Reload read notifications
        },
        error: function (xhr) {
            console.error("Error marking notification as read:", xhr.responseText);
        }
    });
}

