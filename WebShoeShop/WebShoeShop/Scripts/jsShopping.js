$(document).ready(function () {
    ShowCount();
    $('body').on('click', '.btnAddToCart', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var quatity = 1;
        var tQuantity = $('#quantity_value').text();
        if (tQuantity != '') {
            quatity = parseInt(tQuantity);
        }
        
        //alert(id + " " + quatity);
        $.ajax({
            url: '/shoppingcart/addtocart',
            type: 'POST',
            data: { id: id, quantity: quatity },
            success: function (rs) {
                if (rs.Success) {
                    $('#checkout_items').html(rs.Count);
                    //alert(rs.msg);
                    toastr.success(rs.msg, null, { progressBar: true, positionClass: 'toast-top-right' });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi!',
                        text: 'Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng',
                        confirmButtonText: "Đăng nhập",
                        cancelButtonText: "Quay lại",
                        showCancelButton: true,
                        closeOnConfirm: false
                    }).then((result) => {
                        if (result.isConfirmed) {
                            window.location.href = '/account/login';  
                        } else if (result.dismiss === Swal.DismissReason.cancel) {
                            Swal.close(); 
                        }
                    });


                }
            }
        });
    });
    $('body').on('click', '.btnUpdate', function (e) {
        e.preventDefault();

        // Get information from the current row
        var row = $(this).closest('tr');
        var id = row.find('.quantity-input').data("id");
        var newSize = row.find('.size-input').val();
        var quantity = row.find('.quantity-input').val();

        // Validate inputs
        if (isNaN(newSize) || isNaN(quantity) || newSize <= 0 || quantity <= 0) {
            Swal.fire({
                icon: 'error',
                title: 'Giá trị không hợp lệ!',
                text: 'Vui lòng nhập số lượng và size hợp lệ.'
            });
            return;
        }

        // Send update data
        Update(id, quantity, newSize);
    });

    $('body').on('click', '.btnDeleteAll', function (e) {
        e.preventDefault();
        deleteAllProductConfirmation();
    });

    $('body').on('click', '.btnDelete', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var size = $(this).data('size');
        deleteProductConfirmation(id, size);
    });
    function deleteProductConfirmation(id, size) {
        Swal.fire({
            title: 'Xóa sản phẩm',
            text: 'Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy',
        }).then((result) => {
            if (result.isConfirmed) {
                deleteProduct(id, size);
            }
        });
    }
    function deleteAllProductConfirmation() {
        Swal.fire({
            title: 'Xóa sản phẩm',
            text: 'Bạn có chắc muốn xóa tất cả sản phẩm này khỏi giỏ hàng?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy',
        }).then((result) => {
            if (result.isConfirmed) {
                DeleteAll();
            }
        });
    }
    function deleteProduct(id, size) {
        $.ajax({
            url: '/shoppingcart/Delete',
            type: 'POST',
            data: { id: id, size: size },
            success: function (rs) {
                if (rs.Success) {
                    $('#checkout_items').html(rs.Count);
                    $('#trow_' + id).remove();
                    LoadCart();
                }
            }
        });
    }
    function updateProduct(element) {
    var id = $(element).data("id");
    var oldSize = $(element).data("size");
    var newSize = $(element).closest('tr').find('.size-input').val();
    var quantity = $(element).closest('tr').find('.quantity-input').val();

    // Kiểm tra giá trị nhập hợp lệ
    if (isNaN(newSize) || isNaN(quantity) || quantity <= 0 || newSize <= 0) {
        Swal.fire({
            icon: 'error',
            title: 'Lỗi!',
            text: 'Vui lòng nhập số lượng và size hợp lệ.'
        });
        return;
    }

    // Gửi AJAX cập nhật
    Update(id, quantity, newSize);
}
function ShowCount() {
    $.ajax({
        url: '/shoppingcart/ShowCount',
        type: 'GET',
        success: function (rs) {
            $('#checkout_items').html(rs.Count);
        }
    });
}
function DeleteAll() {
    $.ajax({
        url: '/shoppingcart/DeleteAll',
        type: 'POST',
        success: function (rs) {
            if (rs.Success) {
                LoadCart();
            }
        }
    });
}
    function Update(id, quantity, size) {
        $.ajax({
            url: '/shoppingcart/Update',
            type: 'POST',
            data: { id: id, quantity: quantity, size: size },
            success: function (rs) {
                if (rs.Success) {
                    LoadCart(); // Reload lại giỏ hàng
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi!',
                        text: 'Cập nhật không thành công.'
                    });
                }
            }
        });
    }



function LoadCart() {
    $.ajax({
        url: '/shoppingcart/Partial_Item_Cart',
        type: 'GET',
        success: function (rs) {
            $('#load_data').html(rs);
        }
    });
}
});

