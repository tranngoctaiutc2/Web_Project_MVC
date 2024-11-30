using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace WebShoeShopUnitTest
{
    [TestFixture]
    public class UnitestProductAddtocart
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
        }

        [Test]
        public void AddtoCartSuccess()
        {
            // Khởi tạo WebDriverWait với thời gian chờ tối đa là 20 giây
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            // Chờ đến khi sản phẩm đầu tiên với class "product-item dep" có thể được click và click vào nó
            IWebElement firstProduct = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".product-item.dep")));
            Assert.IsNotNull(firstProduct, "Không tìm thấy sản phẩm đầu tiên.");
            firstProduct.Click();

            // Chờ đến khi chuyển hướng thành công tới trang chi tiết sản phẩm
            bool isOnProductDetailPage = wait.Until(drv => drv.Url.Contains("/chi-tiet/"));
            Assert.IsTrue(isOnProductDetailPage, "Không chuyển hướng đến trang chi tiết sản phẩm.");

            // Chờ đến khi nút size đầu tiên có thể click và click vào size
            IWebElement firstSizeButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".size-item")));
            Assert.IsNotNull(firstSizeButton, "Không tìm thấy size đầu tiên.");
            firstSizeButton.Click();

            // Chờ đến khi nút "Thêm vào giỏ hàng" có thể được click
            IWebElement addToCartButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btnAddToCart1")));
            Assert.IsNotNull(addToCartButton, "Không tìm thấy nút Thêm vào giỏ hàng.");

            // Click vào nút "Thêm vào giỏ hàng" sau khi chọn size
            addToCartButton.Click();

            // Chờ cho đến khi thông báo toast xuất hiện với class "swal2-html-container"
            IWebElement toastMessage = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".swal2-html-container")));
            Assert.IsNotNull(toastMessage, "Không tìm thấy thông báo toast.");

            // Kiểm tra nội dung của thông báo toast
            string expectedMessage = "Thêm sản phẩm vào giỏ hàng thành công!";
            Assert.That(toastMessage.Text, Is.EqualTo(expectedMessage), "Thông báo không đúng.");
        }


        [Test]
        public void AddToCartWithoutSelectingSize()
        {
            // Khởi tạo WebDriverWait với thời gian chờ tối đa là 20 giây
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            // Chờ đến khi sản phẩm đầu tiên với class "product-item dep" có thể được click và click vào nó
            IWebElement firstProduct = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".product-item.dep")));
            Assert.IsNotNull(firstProduct, "Không tìm thấy sản phẩm đầu tiên.");
            firstProduct.Click();

            // Chờ đến khi chuyển hướng thành công tới trang chi tiết sản phẩm
            bool isOnProductDetailPage = wait.Until(drv => drv.Url.Contains("/chi-tiet/"));
            Assert.IsTrue(isOnProductDetailPage, "Không chuyển hướng đến trang chi tiết sản phẩm.");

            // Chờ đến khi nút "Thêm vào giỏ hàng" có thể được click (dùng class btnAddToCart1 trong <button>)
            IWebElement addToCartButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btnAddToCart1")));
            Assert.IsNotNull(addToCartButton, "Không tìm thấy nút Thêm vào giỏ hàng.");

            // Click vào nút "Thêm vào giỏ hàng" mà không chọn size
            addToCartButton.Click();

            // Chờ cho đến khi thông báo toast xuất hiện với class "swal2-html-container"
            IWebElement toastMessage = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".swal2-html-container")));
            Assert.IsNotNull(toastMessage, "Không tìm thấy thông báo toast.");

            // Kiểm tra nội dung của thông báo toast
            string expectedMessage = "Vui lòng chọn size trước khi thêm vào giỏ hàng.";
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
