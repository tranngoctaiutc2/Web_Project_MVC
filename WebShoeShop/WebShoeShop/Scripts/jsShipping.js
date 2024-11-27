const apiKeyGHN = 'f4b8b19d-21a7-11ef-9687-e65979a55bd5';
const apiGoongKey = 'qlaCIAdI18xD0ufMV2e8zD7ng9HqSfT6iAos4Z8a';
const originLat = 10.84577744227382;
const originLon = 106.79418264976631;
var destiLat = 0;
var destiLon = 0;
let shippingCost = 0;
let originalProductPrice;


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

    shippingCost = 0;
    const costShipElement = document.getElementById('cost');
    costShipElement.textContent = '0';
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

                let shippingCost = 0;  // Khởi tạo chi phí vận chuyển
                if (distance < 20) {
                    shippingCost = 0;  // Nếu khoảng cách < 20 km, chi phí là 0
                } else if (distance < 50) {
                    shippingCost = 16500;  // Nếu khoảng cách < 50 km, chi phí là 16,500 VND
                } else if (distance < 100) {
                    shippingCost = 30000;  // Nếu khoảng cách < 100 km, chi phí là 30,000 VND
                } else {
                    shippingCost = 45000;  // Nếu khoảng cách > 100 km, chi phí là 45,000 VND
                }

                document.getElementById('cost').textContent = shippingCost;  // Cập nhật chi phí lên trang web
            } else {
                console.log('No distance data found.');  // Nếu không có dữ liệu khoảng cách
            }
        })
        .catch(error => console.error('Error calculating shipping cost:', error));  // Bắt và in lỗi nếu có
}


document.querySelector('select[name="TypeShip"]').addEventListener('change', function () {
    var totalElement = document.getElementById('total');
    var costShipElement = document.getElementById('cost');
    var tongtien = parseFloat(totalElement.textContent.replace(/\D/g, '')); // Lấy giá trị số từ chuỗi trong <td>
    var phigiaohang = 0;
    // Check the selected shipping type
    if (this.value === '2') { // Shipping cost for option 2
        phigiaohang = 50000; // Set shipping cost for option 2
    }

    // Calculate the new total based on the selected shipping type
    var newTotal = originalProductPrice + phigiaohang + shippingCost; // Always base on the original product price

    // Update the total and shipping cost display
    totalElement.textContent = newTotal.toLocaleString(); // Update total value in <td>
    costShipElement.textContent = (phigiaohang + shippingCost).toLocaleString(); // Update shipping cost display
});

document.addEventListener('DOMContentLoaded', () => {
    initializeOriginalProductPrice(); 

    document.getElementById('citySelect').addEventListener('change', function () {
        resetLocationFields(); // Reset fields first
        fetchDistricts(); // Then fetch the districts for the selected province
    });
});