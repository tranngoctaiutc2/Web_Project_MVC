$(document).ready(function () {
    ShowCount();
    $(document).on('click', '.btnAddToCart1', function (event) {
        event.preventDefault();

        const quantityValue = parseInt($('#quantity_value').text());
        const selectedSizeElement = $('.size-item.selected');

        if (selectedSizeElement.length === 0) {
            Swal.fire({
                icon: 'error',
                title: 'Lỗi',
                text: 'Vui lòng chọn size trước khi thêm vào giỏ hàng.'
            });
            return;
        }

        const size = selectedSizeElement.data('size');
        const quantityProduct = selectedSizeElement.data('quantity');

        if (quantityValue > quantityProduct) {
            Swal.fire({
                icon: 'error',
                title: 'Lỗi',
                text: 'Số lượng bạn muốn mua vượt quá số lượng hiện có trong kho.'
            });
        } else {
            const id = $(this).data('id');

            $.ajax({
                url: '/shoppingcart/addtocart',
                type: 'POST',
                data: { id: id, quantity: quantityValue, size: size },
                success: function (rs) {
                    if (rs.Success) {
                        $('#checkout_items').html(rs.Count);
                        $('#modalProductImage').attr('src', rs.ProductImg);

                        const productInfo = `
                            <div class="d-flex align-items-center">
                                <div class="mr-4">
                                    <img src="${rs.ProductImg}" 
                                         alt="${rs.ProductName}" 
                                         style="width: 120px; height: 120px; object-fit: cover; border-radius: 8px;">
                                </div>
                                <div>
                                    <h6 class="mb-2">${rs.ProductName}</h6>
                                    <p class="text-muted mb-1">Size: ${rs.Size}</p>
                                    <p class="text-muted mb-1">Số lượng: ${rs.Quantity}</p>
                                    <p class="font-weight-bold text-dark">${rs.TotalPrice} ₫</p>
                                </div>
                            </div>
                        `;

                        $('#modalProductInfo').html(productInfo);

                        // Hiển thị modal
                        $('#productAddedModal').modal('show');
                    }
                }
            });
        }
    });
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
   // Update Size
    let selectedSize = null;

    $(document).on('click', '.size-item', function () {
        const quantity = $(this).data('quantity');
        $('.size-item').removeClass('selected bg-primary text-white');
        $(this).addClass('selected bg-primary text-white');
        selectedSize = parseInt($(this).data('size'));
        $('.btnSaveSize').prop('disabled', false);
    });
    $(document).on('click', '.btnUpdateSize', function (e) {
        e.preventDefault();

        var productId = $(this).data('id');
        var oldSize = $(this).data('size');
        $('#updateSizeModal').data('product-id', productId);
        $('#updateSizeModal').data('product-size', oldSize);
        console.log('Product ID:', productId);
        $.ajax({
            url: '/Products/GetProductDetails',
            type: 'GET',
            data: { id: productId },
            success: function (data) {
                if (data.success === false) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: data.message
                    });
                    return; // Nếu server trả về lỗi, không tiếp tục
                }
                $('#productDetailsContainer').html(data);
                $('#updateSizeModal').modal('show');
            },
            error: function () {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi',
                    text: 'Không thể tải thông tin sản phẩm.'
                });
            }
        });
    });
    $(document).on('click', '.btnSaveSize', function () {
        var productId = $('#updateSizeModal').data('product-id');
        var oldSize = $('#updateSizeModal').data('product-size');
        var newSize = selectedSize;
        $.ajax({
            url: '/ShoppingCart/UpdateSize',
            type: 'POST',
            data: { id: productId, oldSize: oldSize, newSize: newSize },
            success: function (response) {
                if (response.success) {
                    $('#updateSizeModal').modal('hide');
                    LoadCart();
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: response.message
                    });
                }
            },
            error: function () {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi',
                    text: 'Đã xảy ra lỗi trong quá trình cập nhật size.'
                });
            }
        });
    });

    $('body').on('click', '.btnUpdate', function (e) {
        e.preventDefault();
        var row = $(this).closest('tr');
        var id = row.find('.quantity-input').data("id");
        var quantity = row.find('.quantity-input').val();
        var size = row.find('.btnUpdateSize').data("size");
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
        Update(id, quantity, size);
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
        var row = $(element).closest('tr');
    var id = $(element).data("id");
    var quantity = $(element).closest('tr').find('.quantity-input').val();
    var size = row.find('.btnUpdateSize').data("size");
    if ( isNaN(quantity) || quantity <= 0 ) {
        Swal.fire({
            icon: 'error',
            title: 'Lỗi!',
            text: 'Vui lòng nhập số lượng hợp lệ.'
        });
        return;
    }

    // Gửi AJAX cập nhật
    Update(id, quantity, size);
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
                    LoadCart();
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi!',
                        text: rs.Message
                    });
                    var $quantityInput = $('.quantity-input[data-id="' + id + '"][data-size="' + size + '"]');
                    $quantityInput.val($quantityInput.attr('data-original-value'));
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

