/* JS Document */

/******************************

[Table of Contents]

1. Vars and Inits
2. Set Header
3. Init Menu
4. Init Favorite
5. Init Fix Product Border
6. Init Isotope Filtering
7. Init Price Slider
8. Init Checkboxes



******************************/

jQuery(document).ready(function ($) {
	"use strict";

	/* 

	1. Vars and Inits

	*/

	var header = $('.header');
	var topNav = $('.top_nav')
	var mainSlider = $('.main_slider');
	var hamburger = $('.hamburger_container');
	var menu = $('.hamburger_menu');
	var menuActive = false;
	var hamburgerClose = $('.hamburger_close');
	var fsOverlay = $('.fs_menu_overlay');

	setHeader();

	$(window).on('resize', function () {
		initFixProductBorder();
		setHeader();
	});

	$(document).on('scroll', function () {
		setHeader();
	});

	initMenu();
	initFixProductBorder();
	initIsotopeFiltering();
	initPriceSlider();
	initCheckboxes();

	/* 

	2. Set Header

	*/

	function setHeader() {
		if (window.innerWidth < 992) {
			if ($(window).scrollTop() > 100) {
				header.css({ 'top': "0" });
			}
			else {
				header.css({ 'top': "0" });
			}
		}
		else {
			if ($(window).scrollTop() > 100) {
				header.css({ 'top': "-50px" });
			}
			else {
				header.css({ 'top': "0" });
			}
		}
		if (window.innerWidth > 991 && menuActive) {
			closeMenu();
		}
	}

	/* 

	3. Init Menu

	*/

	function initMenu() {
		if (hamburger.length) {
			hamburger.on('click', function () {
				if (!menuActive) {
					openMenu();
				}
			});
		}

		if (fsOverlay.length) {
			fsOverlay.on('click', function () {
				if (menuActive) {
					closeMenu();
				}
			});
		}

		if (hamburgerClose.length) {
			hamburgerClose.on('click', function () {
				if (menuActive) {
					closeMenu();
				}
			});
		}

		if ($('.menu_item').length) {
			var items = document.getElementsByClassName('menu_item');
			var i;

			for (i = 0; i < items.length; i++) {
				if (items[i].classList.contains("has-children")) {
					items[i].onclick = function () {
						this.classList.toggle("active");
						var panel = this.children[1];
						if (panel.style.maxHeight) {
							panel.style.maxHeight = null;
						}
						else {
							panel.style.maxHeight = panel.scrollHeight + "px";
						}
					}
				}
			}
		}
	}

	function openMenu() {
		menu.addClass('active');
		// menu.css('right', "0");
		fsOverlay.css('pointer-events', "auto");
		menuActive = true;
	}

	function closeMenu() {
		menu.removeClass('active');
		fsOverlay.css('pointer-events', "none");
		menuActive = false;
	}

	/* 

	4. Init Favorite

	*/

	/*

	5. Init Fix Product Border

	*/

	function initFixProductBorder() {
		if ($('.product_filter').length) {
			var products = $('.product_filter:visible');
			var wdth = window.innerWidth;

			// reset border
			products.each(function () {
				$(this).css('border-right', 'solid 1px #e9e9e9');
			});

			// if window width is 991px or less

			if (wdth < 480) {
				for (var i = 0; i < products.length; i++) {
					var product = $(products[i]);
					product.css('border-right', 'none');
				}
			}

			else if (wdth < 576) {
				if (products.length < 5) {
					var product = $(products[products.length - 1]);
					product.css('border-right', 'none');
				}
				for (var i = 1; i < products.length; i += 2) {
					var product = $(products[i]);
					product.css('border-right', 'none');
				}
			}

			else if (wdth < 768) {
				if (products.length < 5) {
					var product = $(products[products.length - 1]);
					product.css('border-right', 'none');
				}
				for (var i = 2; i < products.length; i += 3) {
					var product = $(products[i]);
					product.css('border-right', 'none');
				}
			}

			else if (wdth < 992) {
				if (products.length < 5) {
					var product = $(products[products.length - 1]);
					product.css('border-right', 'none');
				}
				for (var i = 2; i < products.length; i += 3) {
					var product = $(products[i]);
					product.css('border-right', 'none');
				}
			}

			//if window width is larger than 991px
			else {
				if (products.length < 5) {
					var product = $(products[products.length - 1]);
					product.css('border-right', 'none');
				}
				for (var i = 3; i < products.length; i += 4) {
					var product = $(products[i]);
					product.css('border-right', 'none');
				}
			}
		}
	}

	/* 

	6. Init Isotope Filtering

	*/

	function initIsotopeFiltering() {
		var sortTypes = $('.type_sorting_btn');
		var sortNums = $('.num_sorting_btn');
		var sortTypesSelected = $('.sorting_type .item_sorting_btn is-checked span');
		var filterButton = $('.filter_button');

		if ($('.product-grid').length) {
			$('.product-grid').isotope({
				itemSelector: '.product-item',
				getSortData: {
					price: function (itemElement) {
						var priceEle = $(itemElement).find('.in_product_price').text();
						return parseFloat(priceEle);
					},
					name: '.product_name'
				},
				animationOptions: {
					duration: 750,
					easing: 'linear',
					queue: false
				}
			});

			// Short based on the value from the sorting_type dropdown
			sortTypes.each(function () {
				$(this).on('click', function () {
					$('.type_sorting_text').text($(this).text());
					var option = $(this).attr('data-isotope-option');
					option = JSON.parse(option);
					$('.product-grid').isotope(option);
				});
			});

			// Show only a selected number of items
			sortNums.each(function () {
				$(this).on('click', function () {
					var numSortingText = $(this).text();
					var numFilter = ':nth-child(-n+' + numSortingText + ')';
					$('.num_sorting_text').text($(this).text());
					$('.product-grid').isotope({ filter: numFilter });
				});
			});

			// Filter based on the price range slider
			filterButton.on('click', function () {
				$('.product-grid').isotope({
					filter: function () {
						var priceRange = $('#amount').val();
						var priceMin = parseFloat(priceRange.split('-')[0].replace('đ', ''));
						var priceMax = parseFloat(priceRange.split('-')[1].replace('đ', ''));
						var itemPrice = $(this).find('.in_product_price').clone().children().remove().end().text().replace('$', '');

						return (itemPrice > priceMin) && (itemPrice < priceMax);
					},
					animationOptions: {
						duration: 750,
						easing: 'linear',
						queue: false
					}
				});
			});
		}
	}

	/* 

	7. Init Price Slider

	*/

	function initPriceSlider() {
		// Khởi tạo thanh trượt
		$("#slider-range").slider({
			range: true,
			min: 0,
			max: 5000000,
			values: [0, 1000000],
			slide: function (event, ui) {
				// Cập nhật giá trị thanh trượt trên input và slider với định dạng phần nghìn
				$("#amount").val("đ" + ui.values[0] + " - đ" + ui.values[1]);
				$('#minPrice').val(ui.values[0]);
				$('#maxPrice').val(ui.values[1]);
			}
		});

		$("#amount").val("đ" + $("#slider-range").slider("values", 0) + " - đ" + $("#slider-range").slider("values", 1));

		// Không cho phép nhập số âm và ký tự đặc biệt
		function validateInput(event) {
			// Chỉ cho phép nhập số (0-9)
			const charCode = event.which || event.keyCode;
			if (charCode < 48 || charCode > 57) {
				event.preventDefault();
			}
		}

		// Sự kiện khi người dùng nhập giá trị vào input từ bàn phím cho minPrice
		$("#minPrice").on('keypress', validateInput).on('input', function () {
			var minVal = parseInt($(this).val());
			var maxVal = parseInt($("#maxPrice").val());
			// Kiểm tra giá trị min không lớn hơn max
			if (minVal >= 0 && minVal <= maxVal) {
				// Cập nhật giá trị cho thanh trượt
				$("#slider-range").slider("values", 0, minVal);
				$("#amount").val("đ" + minVal + " - đ" + maxVal);
			} else {
				$(this).val($("#slider-range").slider("values", 0)); // Reset nếu nhập không hợp lệ
			}
		});

		// Sự kiện khi người dùng nhập giá trị vào input từ bàn phím cho maxPrice
		$("#maxPrice").on('keypress', validateInput).on('input', function () {
			var minVal = parseInt($("#minPrice").val());
			var maxVal = parseInt($(this).val());
			// Kiểm tra giá trị max không nhỏ hơn min
			if (maxVal >= minVal && maxVal <= 5000000) {
				// Cập nhật giá trị cho thanh trượt
				$("#slider-range").slider("values", 1, maxVal);
				$("#amount").val("đ" + minVal + " - đ" + maxVal);
			} else {
				$(this).val($("#slider-range").slider("values", 1)); // Reset nếu nhập không hợp lệ
			}
		});

		// Tự động chọn giá trị khi nhấp vào input
		$(document).ready(function () {
			$('#minPrice, #maxPrice').on('focus', function () {
				$(this).select();
			});
		});
	}
	//
	/* 

	8. Init Checkboxes

	*/

	function initCheckboxes() {
		if ($('.checkboxes li').length) {
			var boxes = $('.checkboxes li');

			boxes.each(function () {
				var box = $(this);
				box.on('click', function () {
					if (box.hasClass('active')) {
						box.find('i').removeClass('fa-square');
						box.find('i').addClass('fa-square-o');
						box.toggleClass('active');
					}
					else {
						box.find('i').removeClass('fa-square-o');
						box.find('i').addClass('fa-square');
						box.toggleClass('active');
					}
					// box.toggleClass('active');
				});
			});

			if ($('.show_more').length) {
				var checkboxes = $('.checkboxes');

				$('.show_more').on('click', function () {
					checkboxes.toggleClass('active');
				});
			}
		};
	}
});