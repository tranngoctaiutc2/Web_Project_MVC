using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace WebShoeShopUnitTest
{
	[TestFixture]
	public class UnitestCartToCheckout
	{
		private IWebDriver _driver;

		[SetUp]
		public void Setup()
		{
			_driver = new ChromeDriver();
			_driver.Navigate().GoToUrl("https://localhost:44390/");
			_driver.Manage().Window.Maximize();

			// Khởi tạo WebDriverWait với thời gian chờ tối đa là 20 giây
			WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

			// Login
			IWebElement profileBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa.fa-user")));
			Assert.IsNotNull(profileBtn);
			profileBtn.Click();

			IWebElement loginBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a[href='/account/login']")));
			Assert.IsNotNull(loginBtn);
			loginBtn.Click();

			// Kiểm tra URL hiện tại là trang đăng nhập
			Assert.That(_driver.Url, Is.EqualTo("https://localhost:44390/account/login"));

			IWebElement username = _driver.FindElement(By.Name("UserName"));
			username.SendKeys("admin");

			IWebElement password = _driver.FindElement(By.Name("Password"));
			password.SendKeys("0985181215thanH@");

			IWebElement loginSubmitBtn = _driver.FindElement(By.ClassName("form__button"));
			loginSubmitBtn.Click();

			// Chờ cho đến khi URL chuyển hướng về trang chủ (hoặc trang mong đợi sau khi đăng nhập)
			bool isRedirected = wait.Until(drv => drv.Url == "https://localhost:44390/");
			Assert.IsTrue(isRedirected, "Không chuyển hướng đến trang chủ sau khi đăng nhập.");

			// Chờ đến khi nút có class "swal2-confirm swal2-styled" xuất hiện và nhấn vào nút đó
			IWebElement confirmButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".swal2-confirm.swal2-styled")));
			Assert.IsNotNull(confirmButton);
			confirmButton.Click();

			// Khởi tạo WebDriverWait với thời gian chờ tối đa là 20 giây
			// Chờ đến khi sản phẩm đầu tiên với class "product-item dep" có thể được click và click vào nó
			IWebElement firstProduct = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".product-item.dep")));
			Assert.IsNotNull(firstProduct, "Không tìm thấy sản phẩm đầu tiên.");
			firstProduct.Click();

			// Chờ đến khi nút size có thể được click và chọn size đầu tiên
			IWebElement firstSizeOption = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".size-item:first-child")));
			Assert.IsNotNull(firstSizeOption, "Không tìm thấy size.");
			firstSizeOption.Click();

			// Chờ đến khi nút "Thêm vào giỏ hàng" có thể click và click vào nút
			IWebElement addToCartButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btnAddToCart1")));
			Assert.IsNotNull(addToCartButton, "Không tìm thấy nút Thêm vào giỏ hàng.");
			addToCartButton.Click();

			// Đợi một chút để đảm bảo toast đã biến mất
			System.Threading.Thread.Sleep(4000); // Chờ 4 giây
		}

		[Test]
		public void CheckoutCartSuccessfully()
		{
			WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));

			// Kiểm tra giỏ hàng, click vào nút xem giỏ hàng
			IWebElement cartIcon = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn.btn-outline-dark.btn-sm")));
			Assert.IsNotNull(cartIcon, "Không tìm thấy nút xem giỏ hàng.");
			cartIcon.Click();

			// Chờ đến khi giỏ hàng có sản phẩm và kiểm tra chuyển hướng tới trang giỏ hàng
			bool isOnCartPage = wait.Until(drv => drv.Url.Contains("/shoppingcart"));
			Assert.IsTrue(isOnCartPage, "Không chuyển hướng đến trang giỏ hàng.");

			// Kiểm tra và nhập số lượng sản phẩm (1 sản phẩm)
			IWebElement quantityInput = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".form-control.quantity-input")));
			quantityInput.Clear(); // Xóa giá trị hiện tại
			quantityInput.SendKeys("1");

			// Kiểm tra size sản phẩm
			IWebElement sizeInput = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn.btn-link.btnUpdateSize")));
            Assert.IsNotNull(sizeInput, "Không tìm thấy nút thay đổi size.");
            sizeInput.Click();

            // Lấy danh sách kích thước và loại trừ kích thước đầu tiên
            IWebElement sizeListContainer = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".product_size.d-flex.flex-column.flex-sm-row.align-items-sm-center")));
            IList<IWebElement> sizeOptions = sizeListContainer.FindElements(By.TagName("li")); // Giả định mỗi option là một thẻ <li>

            // Kiểm tra danh sách không rỗng
            Assert.IsTrue(sizeOptions.Count > 0, "Không tìm thấy các tùy chọn kích thước.");

            // Lấy kích thước đầu tiên để loại trừ
            IWebElement firstSizeOption = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".size-item:first-child")));
            Assert.IsNotNull(firstSizeOption, "Không tìm thấy size.");
            string firstSizeText = firstSizeOption.Text;

            // Loại bỏ kích thước đầu tiên khỏi danh sách
            IList<IWebElement> filteredSizeOptions = sizeOptions.Where(option => option.Text != firstSizeText).ToList();

            // Đảm bảo danh sách còn lại không rỗng
            Assert.IsTrue(filteredSizeOptions.Count > 0, "Không có tùy chọn kích thước nào ngoài kích thước đầu tiên.");

            // Chọn ngẫu nhiên một kích thước từ danh sách đã lọc
            Random random = new Random();
            int randomIndex = random.Next(filteredSizeOptions.Count);
            IWebElement randomSizeOption = filteredSizeOptions[randomIndex];

            // Click vào tùy chọn kích thước ngẫu nhiên
            randomSizeOption.Click();

            IWebElement confirmChange = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn.btn-dark.btnSaveSize")));
			confirmChange.Click();

            IWebElement confirmFinalChange = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn.btn-sm.btn-success.btnUpdate")));
            confirmFinalChange.Click();

            // Tiến hành thanh toán
            IWebElement checkoutButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn.btn-success.btnpayment")));
			checkoutButton.Click();

			// Kiểm tra xem có chuyển hướng đến trang thanh toán thành công hay không
			bool isOnCheckoutPage = wait.Until(drv => drv.Url.Contains("/thanh-toan"));
			Assert.IsTrue(isOnCheckoutPage, "Không chuyển hướng đến trang thanh toán.");
		}


		[Test]
		public void CheckoutCartWithInvalidSize()
		{
			WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            // Kiểm tra giỏ hàng, click vào nút xem giỏ hàng
            IWebElement cartIcon = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn.btn-outline-dark.btn-sm")));
            Assert.IsNotNull(cartIcon, "Không tìm thấy nút xem giỏ hàng.");
            cartIcon.Click();

            // Chờ đến khi giỏ hàng có sản phẩm và kiểm tra chuyển hướng tới trang giỏ hàng
            bool isOnCartPage = wait.Until(drv => drv.Url.Contains("/shoppingcart"));
			Assert.IsTrue(isOnCartPage, "Không chuyển hướng đến trang giỏ hàng.");

			IWebElement quantityInput = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".form-control.quantity-input")));
			quantityInput.Clear(); // Clear the existing value
			quantityInput.SendKeys("1");

            // Kiểm tra size sản phẩm
            IWebElement sizeInput = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn.btn-link.btnUpdateSize")));
            Assert.IsNotNull(sizeInput, "Không tìm thấy nút thay đổi size.");
            sizeInput.Click();

            // Tìm danh sách trong lớp "list-inline d-flex flex-warp"
            IWebElement sizeListContainer = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".product_size.d-flex.flex-column.flex-sm-row.align-items-sm-center")));
            IList<IWebElement> sizeOptions = sizeListContainer.FindElements(By.TagName("li")); // Giả định mỗi option là một thẻ <li>

            // Kiểm tra danh sách không rỗng
            Assert.IsTrue(sizeOptions.Count > 0, "Không tìm thấy các tùy chọn kích thước.");

            // Kiểm tra danh sách size có giá trị 0 hay không
            bool hasZeroValue = sizeOptions.Any(option =>
            {
                string value = option.GetAttribute("data-value"); // Giả sử giá trị nằm trong thuộc tính "data-value"
                return value == "0";
            });

            // Đảm bảo không có giá trị 0
            Assert.IsFalse(hasZeroValue, "Danh sách kích thước có chứa giá trị 0, không đúng mong đợi.");

        }


        [Test]
		public void CheckoutCartWithInvalidQuantity()
		{
			WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            // Kiểm tra giỏ hàng, click vào biểu tượng giỏ hàng
            // Kiểm tra giỏ hàng, click vào nút xem giỏ hàng
            IWebElement cartIcon = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn.btn-outline-dark.btn-sm")));
            Assert.IsNotNull(cartIcon, "Không tìm thấy nút xem giỏ hàng.");
            cartIcon.Click();

            // Chờ đến khi giỏ hàng có sản phẩm và kiểm tra chuyển hướng tới trang giỏ hàng
            bool isOnCartPage = wait.Until(drv => drv.Url.Contains("/shoppingcart"));
            Assert.IsTrue(isOnCartPage, "Không chuyển hướng đến trang giỏ hàng.");

            IWebElement quantityInput = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".form-control.quantity-input")));
			quantityInput.Clear(); // Clear the existing value
			quantityInput.SendKeys("0");

			IWebElement confirmBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn.btn-sm.btn-success.btnUpdate")));
			confirmBtn.Click();

			IWebElement paymentButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn.btn-success.btnpayment")));

			// Chờ cho đến khi thông báo toast xuất hiện với class "swal2-html-container"
			IWebElement toastMessage = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".swal2-html-container")));
			Assert.IsNotNull(toastMessage, "Không tìm thấy thông báo toast.");

			// Kiểm tra nội dung của thông báo toast
			string expectedMessage = "Vui lòng nhập số lượng hợp lệ.";
			Assert.That(toastMessage.Text, Is.EqualTo(expectedMessage), "Thông báo không đúng.");
		}


		[TearDown]
		public void TearDown()
		{
			if (_driver != null)
			{
				_driver.Quit();
			}
		}
	}
}
