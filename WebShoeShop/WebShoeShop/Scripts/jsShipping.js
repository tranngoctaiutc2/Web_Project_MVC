const apiKeyGHN = 'f4b8b19d-21a7-11ef-9687-e65979a55bd5';
const apiGoongKey = 'qlaCIAdI18xD0ufMV2e8zD7ng9HqSfT6iAos4Z8a';
const originLat = 10.84577744227382;
const originLon = 106.79418264976631;
var destiLat = 0;
var destiLon = 0;
let shippingCost = 0;
let originalProductPrice;
let phigiaohang = 0;

// Fetch and populate provinces
fetch('https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/province', {
    method: 'GET',
    headers: {
        'Content-Type': 'application/json',
        'Token': apiKeyGHN
    }
})
    .then(response => response.json())
    .then(data => {
        const citySelect = document.getElementById('citySelect');
        data.data.forEach(province => {
            const option = document.createElement('option');
            option.value = province.ProvinceID;
            option.textContent = province.ProvinceName;
            citySelect.appendChild(option);
        });
    })
    .catch(error => console.error('Error fetching provinces:', error));

function initializeOriginalProductPrice() {
    const totalElement = document.getElementById('total');
    originalProductPrice = parseFloat(totalElement.textContent.replace(/\D/g, '')); // Get the initial price
}

function resetLocationFields() {
    const addressDetailInput = document.getElementById('AddressDetail');
    addressDetailInput.value = '';

    const addressField = document.getElementById('Address');
    addressField.value = '';

    const totalElement = document.getElementById('total');
    totalElement.textContent = originalProductPrice.toLocaleString();

    const shippingTypeElement = document.querySelector('select[name="TypeShip"]');
    shippingTypeElement.value = '1';

    shippingCost = 0;
    updateDisplayCost();
}

function getCoordinates(cityName) {
    return fetch(`https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(cityName)}&format=json&limit=1`)
        .then(response => response.json())
        .then(data => {
            if (data.length > 0) {
                const { lat, lon } = data[0];
                document.getElementById('coordinates').innerText = `Tọa độ: Latitude = ${lat}, Longitude = ${lon}`;
                destiLat = lat;
                destiLon = lon;
            } else {
                document.getElementById('coordinates').innerText = 'Không tìm thấy kết quả.';
                throw new Error('Không tìm thấy kết quả.');
            }
        })
        .catch(error => console.error('Lỗi khi lấy tọa độ:', error));
}

function generateFullAddress() {
    const city = document.getElementById('citySelect').selectedOptions[0].textContent;
    const district = document.getElementById('districtSelect').selectedOptions[0].textContent;
    const ward = document.getElementById('wardSelect').selectedOptions[0].textContent;
    const deaddress = document.getElementById('AddressDetail').value;
    const fullAddress = `${deaddress}, ${ward}, ${district}, ${city}`;
    document.getElementById('Address').value = fullAddress;
}


// Fetch and populate districts based on selected province
function fetchDistricts() {
    const provinceId = document.getElementById('citySelect').value;
    fetch('https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/district', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Token': apiKeyGHN
        },
        body: JSON.stringify({ "province_id": parseInt(provinceId) })
    })
        .then(response => response.json())
        .then(data => {
            resetLocationFields();

            const districtSelect = document.getElementById('districtSelect');
            districtSelect.innerHTML = '<option value="">-- Chọn quận/huyện --</option>'; 
            const wardSelect = document.getElementById('wardSelect');
            wardSelect.innerHTML = '<option value="">-- Chọn phường/xã --</option>';
            data.data.forEach(district => {
                const option = document.createElement('option');
                option.value = district.DistrictID;
                option.textContent = district.DistrictName;
                districtSelect.appendChild(option);
            });

            updateDisplayCost();
        })
        .catch(error => console.error('Error fetching districts:', error));

}

function fetchWards() {
    const districtId = document.getElementById('districtSelect').value;
    fetch('https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/ward?district_id', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Token': apiKeyGHN
        },
        body: JSON.stringify({ "district_id": parseInt(districtId) })
    })
        .then(response => response.json())
        .then(data => {
            const wardSelect = document.getElementById('wardSelect');
            wardSelect.innerHTML = '<option value="">-- Chọn phường/xã --</option>'; 
            data.data.forEach(ward => {
                const option = document.createElement('option');
                option.value = ward.WardCode;
                option.textContent = ward.WardName;
                wardSelect.appendChild(option);
            });

            updateDisplayCost();
        })
        .catch(error => console.error('Error fetching wards:', error));
}

function getLatLong(address) {
    const url = `https://rsapi.goong.io/Geocode?address=${encodeURIComponent(address)}&api_key=${apiGoongKey}`;

    return fetch(url)  // Trả về một Promise
        .then(response => response.json())
        .then(data => {
            if (data.results && data.results.length > 0) {
                const location = data.results[0].geometry.location;
                destiLat = location.lat;
                destiLon = location.lng;
            } else {
                console.log('No results found for the address.');
                destiLat = 0;
                destiLon = 0;
            }
        })
        .catch(error => console.error('Error getting latitude and longitude:', error));
}

// Function to calculate shipping cost using the Distance Matrix API
async function calculateShippingCost() {
    fullAddress = document.getElementById('Address').value;

    // Đợi lấy tọa độ thành công trước khi tiếp tục
    await getLatLong(fullAddress);

    const url = `https://rsapi.goong.io/DistanceMatrix?origins=${originLat},${originLon}&destinations=${destiLat},${destiLon}&api_key=${apiGoongKey}`;

    fetch(url)  // Gọi API để lấy dữ liệu
        .then(response => response.json())  // Chuyển đổi dữ liệu JSON từ phản hồi API
        .then(data => {
            // Kiểm tra dữ liệu có hợp lệ không
            if (data.rows && data.rows.length > 0) {
                const distance = data.rows[0].elements[0].distance.value / 1000; // Chuyển từ mét sang km

                if (distance < 20) {    
                    shippingCost = 0;  // Nếu khoảng cách < 20 km, chi phí là 0
                } else if (distance < 50) {
                    shippingCost = 16500;  // Nếu khoảng cách < 50 km, chi phí là 16,500 VND
                } else if (distance < 100) {
                    shippingCost = 30000;  // Nếu khoảng cách < 100 km, chi phí là 30,000 VND
                } else {
                    shippingCost = 45000;  // Nếu khoảng cách > 100 km, chi phí là 45,000 VND
                }

                updateDisplayCost();
            } else {
                console.log('No distance data found.');  // Nếu không có dữ liệu khoảng cách
            }
        })
        .catch(error => console.error('Error calculating shipping cost:', error));  // Bắt và in lỗi nếu có
}

document.querySelector('select[name="TypeShip"]').addEventListener('change', function () {
    // Lấy các phần tử DOM
    var totalElement = document.getElementById('total');
    var costShipElement = document.getElementById('cost');

    // Kiểm tra loại giao hàng được chọn
    if (this.value === '2') { // Nếu chọn giao hàng nhanh
        phigiaohang = 30000; // Phí giao hàng nhanh là 30,000 VND
    } else {
        phigiaohang = 0; // Không áp dụng phí giao hàng nhanh
    }

    // Tính tổng giá mới: giá sản phẩm + phí giao theo khoảng cách + phí giao hàng nhanh
    var newTotal = originalProductPrice + shippingCost + phigiaohang;
    // Cập nhật giao diện
    totalElement.textContent = newTotal.toLocaleString(); // Cập nhật tổng giá trị
    costShipElement.textContent = (shippingCost + phigiaohang).toLocaleString();
    const shipCostInput = document.querySelector('input[name="ShipCost"]');
    if (shipCostInput) {
        shipCostInput.value = shippingCost + phigiaohang; // Gán giá trị shippingCost vào input
    }// Cập nhật phí vận chuyển
});

function updateDisplayCost() {
    const totalElement = document.getElementById('total');
    const costShipElement = document.getElementById('cost');

    // Tính tổng giá: giá sản phẩm + phí giao theo khoảng cách + phí giao hàng nhanh
    const newTotal = originalProductPrice + (shippingCost || 0);

    // Cập nhật giao diện
    totalElement.textContent = newTotal.toLocaleString(); // Hiển thị tổng giá
    costShipElement.textContent = (shippingCost + phigiaohang).toLocaleString(); // Hiển thị phí vận chuyển
    const shipCostInput = document.querySelector('input[name="ShipCost"]');
    if (shipCostInput) {
        shipCostInput.value = shippingCost + phigiaohang; // Gán giá trị shippingCost vào input
    }
}

document.addEventListener('DOMContentLoaded', () => {
    initializeOriginalProductPrice(); 

    document.getElementById('citySelect').addEventListener('change', function () {
        resetLocationFields(); // Reset fields first
        fetchDistricts(); // Then fetch the districts for the selected province
    });
});