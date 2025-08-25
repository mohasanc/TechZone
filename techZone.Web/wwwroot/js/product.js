
var datatable;
$(document).ready(function () {
    loaddata();
});

function loaddata() {
    datatable = $("#myTable").DataTable({
        "ajax": {
            "url": "/Admin/Product/GetData"
        },
        "columns": [
            { "data": "name" },
            { "data": "description" },
            { "data": "price" },
            { "data": "category" },
            {
                data: "productId",
                render: function (data) {
                    return `
                                    <a href="/Admin/Product/Edit/${data}" class="btn btn-success btn-sm me-2">Edit</a>
                                    <button class="btn btn-danger btn-sm delete-btn" data-id="${data}">Delete</button>
                                `;
                }
            }
        ]
    });
}

$(document).on("click", ".delete-btn", function () {
    const id = $(this).data("id");

    Swal.fire({
        title: "Are you sure?",
        text: "You won’t be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#d33",
        cancelButtonColor: "#3085d6",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: `/Admin/Product/Delete/${id}`,
                type: "POST",
                headers: {
                    "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
                },
                success: function () {
                    Swal.fire(
                        "Deleted!",
                        "The product has been deleted.",
                        "success"
                    );
                    datatable.ajax.reload();
                },
                error: function () {
                    Swal.fire(
                        "Error!",
                        "Something went wrong.",
                        "error"
                    );
                }
            });
        }
    });
});