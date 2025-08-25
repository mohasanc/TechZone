var datatable;

$(document).ready(function () {
    loaddata();
});

function loaddata() {
    datatable = $("#myTable").DataTable({
        "ajax": {
            "url": "/Admin/Order/GetData"
        },
        "columns": [
            { "data": "id" },
            { "data": "name" },
            { "data": "phone" },
            { "data": "email" },
            { "data": "city" },
            { "data": "orderStatus" },
            {
                "data": "orderDate",
                "render": function (data) {
                    if (data) {
                        let date = new Date(data);
                        return date.toLocaleDateString('en-GB', {  // DD/MM/YYYY
                            day: '2-digit', month: '2-digit', year: 'numeric'
                        });
                    }
                    return "";
                }
            },
            {
                "data": "totalPrice",
                "render": function (data) {
                    return `$${data.toFixed(2)}`;
                }
            },
            {
                data: "id",
                render: function (data) {
                    return `
                                    <a href="/Admin/Order/Details?id=${data}" class="btn btn-warning btn-sm me-2">Details</a>
                                `;
                }
            },
        ]
    });
}