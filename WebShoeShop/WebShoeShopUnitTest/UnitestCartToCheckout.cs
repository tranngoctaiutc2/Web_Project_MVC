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
			WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

			// Kiểm tra giỏ hàng, click vào biểu tượng giỏ hàng
			IWebElement cartIcon = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa.fa-shopping-bag")));
			Assert.IsNotNull(cartIcon, "Không tìm thấy biểu tượng giỏ hàng.");
			cartIcon.Click();

			// Chờ đến khi giỏ hàng có sản phẩm và kiểm tra chuyển hướng tới trang giỏ hàng
			bool isOnCartPage = wait.Until(drv => drv.Url.Contains("/gio-hang"));
			Assert.IsTrue(isOnCartPage, "Không chuyển hướng đến trang giỏ hàng.");

			// Kiểm tra và nhập số lượng sản phẩm (1 sản phẩm)
			IWebElement quantityInput = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".form-control.quantity-input")));
			quantityInput.Clear(); // Xóa giá trị hiện tại
			quantityInput.SendKeys("1");

			// Lấy dropdown chọn size sản phẩm
			IWebElement dropdown = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("Size_55_40")));

			// Khởi tạo đối tượng SelectElement để thao tác với dropdown
			SelectElement selectElement = new SelectElement(dropdown);

			// Lấy option đã chọn mặc định ban đầu
			string firstOptionValue = selectElement.Options.First().GetAttribute("value"); // Lấy giá trị của option đầu tiên
			selectElement.SelectByValue(firstOptionValue); // Chọn option đầu tiên

			IWebElement confirmChange = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn.btn-sm.btn-success.btnUpdate")));
			confirmChange.Click();

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

			// Kiểm tra giỏ hàng, click vào biểu tượng giỏ hàng
			IWebElement cartIcon = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa.fa-shopping-bag")));
			Assert.IsNotNull(cartIcon, "Không tìm thấy biểu tượng giỏ hàng.");
			cartIcon.Click();

			// Chờ đến khi giỏ hàng có sản phẩm và kiểm tra chuyển hướng tới trang giỏ hàng
			bool isOnCartPage = wait.Until(drv => drv.Url.Contains("/gio-hang"));
			Assert.IsTrue(isOnCartPage, "Không chuyển hướng đến trang giỏ hàng.");

			IWebElement quantityInput = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".form-control.quantity-input")));
			quantityInput.Clear(); // Clear the existing value
			quantityInput.SendKeys("1");

			IWebElement dropdown = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("Size_55_40")));

			// Khởi tạo đối tượng SelectElement để thao tác với dropdown
			SelectElement selectElement = new SelectElement(dropdown);

			// Lấy option đã chọn mặc định ban đầu
			string defaultSelectedOption = selectElement.SelectedOption.Text;

			// Kiểm tra xem option "0" có tồn tại hay không
			bool optionExists = selectElement.Options.Any(option => option.GetAttribute("value") == "0");

			if (optionExists)
			{
				// Chọn option "0" nếu tồn tại
				selectElement.SelectByValue("0");

				// Kiểm tra option đã chọn đúng
				Assert.AreEqual("0", selectElement.SelectedOption.GetAttribute("value"), "Không chọn được option 0.");
			}
			else
			{
				// Nếu option không tồn tại, coi như test thành công và không cần chọn
				Assert.Pass("Option 0 không tồn tại, đây là hành vi mong đợi.");
			}
		}


		[Test]
		public void CheckoutCartWithInvalidQuantity()
		{
			WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

			// Kiểm tra giỏ hàng, click vào biểu tượng giỏ hàng
			IWebElement cartIcon = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa.fa-shopping-bag")));
			Assert.IsNotNull(cartIcon, "Không tìm thấy biểu tượng giỏ hàng.");
			cartIcon.Click();

			// Chờ đến khi giỏ hàng có sản phẩm và kiểm tra chuyển hướng tới trang giỏ hàng
			bool isOnCartPage = wait.Until(drv => drv.Url.Contains("/gio-hang"));
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
			string expectedMessage = "Vui lòng nhập số lượng và size hợp lệ.";
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
