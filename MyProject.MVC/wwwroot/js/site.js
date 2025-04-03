// Function to get material notifications count and update UI
function updateNotificationCount() {
    $.ajax({
        url: 'http://localhost:5111/api/StudentApi/GetMaterialNotifications',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            console.log('Notification data:', data);
            // Update the notification count element
            $('#notificationcount').text(data.notifications.length || 0);
        },
        error: function (xhr, status, error) {
            console.error('Failed to fetch notifications:', error);
        }
    });
}

// Call on page load
$(document).ready(function () {
    updateNotificationCount();

    // Optional: Set up periodic refresh (every 60 seconds)
    setInterval(updateNotificationCount, 60000);
});