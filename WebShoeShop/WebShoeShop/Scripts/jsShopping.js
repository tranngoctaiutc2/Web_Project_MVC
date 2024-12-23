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
                }
            }
        });
    });
    $('body').on('click', '.btnUpdate', function (e) {
        e.preventDefault();
        var row = $(this).closest('tr');
        var id = row.find('.quantity-input').data("id");
        var quantity = row.find('.quantity-input').val();

        // Validate inputs
        if (isNaN(quantity) || quantity <= 0) {
            Swal.fire({
                icon: 'error',
                title: 'Giá trị không hợp lệ!',
                text: 'Vui lòng nhập số lượng hợp lệ.'
            });
            quantity = 1;
            row.find('.quantity-input').val(quantity);
            return;
        }
        Update(id, quantity);
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

    // Coupon
    $('#couponModal').on('show.bs.modal', function () {
        $.ajax({
            url: '/ShoppingCart/GetAvailableCoupons',
            type: 'GET',
            success: function (response) {
                var couponList = $('#couponList');
                couponList.empty(); // Clear existing list items

                if (response.length > 0) {
                    response.forEach(function (coupon) {
                        // Convert ExpirationDate string to Date object
                        var expirationDate = new Date(coupon.ExpirationDate);
                        var today = new Date();
                        var isExpired = expirationDate < today ? 'text-danger' : 'text-success';

                        // Format the expiration date
                        var formattedExpirationDate = expirationDate.toLocaleDateString('vi-VN');

                        // Determine the discount type
                        var discountText = '';
                        if (coupon.DiscountPercentage) {
                            discountText = `Giảm ${coupon.DiscountPercentage}% giá trị đơn hàng`;
                        } else if (coupon.DiscountAmount) {
                            discountText = `Giảm ${coupon.DiscountAmount.toLocaleString()}₫`;
                        }

                        // Create the card for each coupon
                        var cardHtml = `
                            <div class="col-md-4 mb-3">
                                <div class="card">
                                    <div class="card-body">
                                        <h5 class="card-title">${coupon.Code}</h5>
                                        <p class="card-text">${coupon.Description || 'Không có mô tả'}</p>
                                        <p class="card-text"><strong>Hạn sử dụng:</strong> <span class="${isExpired}">${formattedExpirationDate}</span></p>
                                        <p class="card-text">${discountText}</p>
                                        <button class="btn btn-primary select-coupon" data-coupon-id="${coupon.Id}">Chọn</button>
                                    </div>
                                </div>
                            </div>
                        `;
                        couponList.append(cardHtml);
                    });
                } else {
                    couponList.append('<p class="text-center">Không có mã giảm giá khả dụng.</p>');
                }
            },
            error: function () {
                alert("Có lỗi khi tải mã giảm giá.");
            }
        });
    });

    // Handle coupon selection
    $(document).on('click', '.select-coupon', function () {
        var couponId = $(this).data('coupon-id');

        // Apply selected coupon
        $.ajax({
            url: '/ShoppingCart/ApplyCoupon',
            type: 'POST',
            data: { couponId: couponId },
            success: function (response) {
                if (response.success) {
                    showAlert(response.message, "success");
                    setTimeout(() => location.reload(), 1000); // Reload to update the cart
                } else {
                    showAlert(response.message, "danger");
                    setTimeout(() => location.reload(), 1000);
                }
                $('#couponModal').modal('hide'); // Close the modal
            },
            error: function () {
                showAlert("Có lỗi xảy ra, vui lòng thử lại!", "danger");
            }
        });
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
    var quantity = $(element).closest('tr').find('.quantity-input').val();

    if ( isNaN(quantity) || quantity <= 0 ) {
        Swal.fire({
            icon: 'error',
            title: 'Lỗi!',
            text: 'Vui lòng nhập số lượng hợp lệ.'
        });
        return;
    }

    // Gửi AJAX cập nhật
    Update(id, quantity);
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
    function Update(id, quantity) {
        $.ajax({
            url: '/shoppingcart/Update',
            type: 'POST',
            data: { id: id, quantity: quantity },
            success: function (rs) {
                if (rs.Success) {
                    LoadCart();
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi!',
                        text: rs.Message
                    });
                }
            },
            error: function () {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi!',
                    text: 'Đã xảy ra lỗi trong quá trình cập nhật.'
                });
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

