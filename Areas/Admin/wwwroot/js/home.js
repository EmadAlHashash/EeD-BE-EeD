// ========== New Users per Month ==========
const usersCtx = document.getElementById('usersChart');
const usersData = {
    labels: @Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model.NewUsersPerMonth.Select(x => x.Month))),
    datasets: [{
        label: 'New Users',
        data: @Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model.NewUsersPerMonth.Select(x => x.Count))),
        borderColor: '#f9b406',
        backgroundColor: 'rgba(249,180,6,0.2)',
        tension: 0.3,
        fill: true,
    }]
};
new Chart(usersCtx, {
    type: 'line',
    data: usersData,
    options: {
        responsive: true,
        scales: {
            y: { beginAtZero: true }
        }
    }
});

// ========== Active Services per Category ==========
const servicesCtx = document.getElementById('servicesChart');
const servicesData = {
    labels: @Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model.ActiveServicesPerCategory.Select(x => x.Category))),
    datasets: [{
        label: 'Active Services',
        data: @Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model.ActiveServicesPerCategory.Select(x => x.Count))),
        backgroundColor: [
            '#f9b406',
            '#ffca28',
            '#ffa000',
            '#ffb300',
            '#ff8f00'
        ],
        borderWidth: 1
    }]
};
new Chart(servicesCtx, {
    type: 'bar',
    data: servicesData,
    options: {
        responsive: true,
        scales: {
            y: { beginAtZero: true }
        }
    }
});