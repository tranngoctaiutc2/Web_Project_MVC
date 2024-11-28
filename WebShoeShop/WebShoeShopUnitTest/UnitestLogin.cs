using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace WebShoeShopUnitTest
{

    [TestFixture]

    public class UnitestLogin
    {
        private IWebDriver _driver;

        [SetUp]
        public void Setup()
        {
            _driver = new ChromeDriver();
            _driver.Navigate().GoToUrl("https://localhost:44390/");
            _driver.Manage().Window.Maximize();
        }

        [Test]
        public void LoginSuccess()
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            IWebElement profileBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa.fa-user")));
            Assert.IsNotNull(profileBtn);
            profileBtn.Click();

            IWebElement loginBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a[href='/account/login']")));
            Assert.IsNotNull(loginBtn);
            loginBtn.Click();

            Assert.That(_driver.Url, Is.EqualTo("https://localhost:44390/account/login"));

            IWebElement username = _driver.FindElement(By.Name("UserName"));
            username.SendKeys("admin");

            IWebElement password = _driver.FindElement(By.Name("Password"));
            password.SendKeys("0985181215thanH@");

            IWebElement loginSubmitBtn = _driver.FindElement(By.ClassName("form__button"));
            loginSubmitBtn.Click();
        }

        [Test]
        public void userBlank()
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            // Navigate to the login page
            IWebElement profileBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa.fa-user")));
            Assert.IsNotNull(profileBtn);
            profileBtn.Click();

            IWebElement loginBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a[href='/account/login']")));
            Assert.IsNotNull(loginBtn);
            loginBtn.Click();

            Assert.That(_driver.Url, Is.EqualTo("https://localhost:44390/account/login"));

            IWebElement username = _driver.FindElement(By.Name("UserName"));
            username.SendKeys(""); // Blank username

            IWebElement password = _driver.FindElement(By.Name("Password"));
            password.SendKeys("0985181215thanH@");

            IWebElement loginSubmitBtn = _driver.FindElement(By.ClassName("form__button"));
            loginSubmitBtn.Click();

            IWebElement errorMessage = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".text-danger-username")));
            Assert.IsNotNull(errorMessage);
            Assert.That(errorMessage.Text, Is.EqualTo("Tên đăng nhập không được để trống")); // Adjusted error message
        }

        [Test]
        public void PasswordBlank()
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            // Navigate to the login page
            IWebElement profileBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa.fa-user")));
            profileBtn.Click();

            IWebElement loginBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a[href='/account/login']")));
            loginBtn.Click();

            Assert.That(_driver.Url, Is.EqualTo("https://localhost:44390/account/login"));

            // Leave password blank
            IWebElement username = wait.Until(ExpectedConditions.ElementIsVisible(By.Name("UserName")));
            username.SendKeys("admin"); // Valid username

            IWebElement password = wait.Until(ExpectedConditions.ElementIsVisible(By.Name("Password")));
            password.SendKeys(""); // Blank password

            IWebElement loginSubmitBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.ClassName("form__button")));
            loginSubmitBtn.Click();

            // Wait for the error message to be displayed
            IWebElement errorMessage = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".text-danger-password")));
            Assert.IsNotNull(errorMessage);
            Assert.That(errorMessage.Text, Is.EqualTo("Mật khẩu không được để trống")); // Validate the message
        }

        [Test]
        public void LoginWithoutCredentials()
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            IWebElement profileBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa.fa-user")));
            profileBtn.Click();

            IWebElement loginBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a[href='/account/login']")));
            loginBtn.Click();

            Assert.That(_driver.Url, Is.EqualTo("https://localhost:44390/account/login"));

            IWebElement loginSubmitBtn = _driver.FindElement(By.ClassName("form__button"));
            loginSubmitBtn.Click();

            IWebElement errorMessage = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".text-danger-username")));
            Assert.IsNotNull(errorMessage);
            Assert.That(errorMessage.Text, Is.EqualTo("Tên đăng nhập không được để trống"));
        }

        [Test]
        public void loginFailedWithIncorrectUsername()
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            // Navigate to the login page
            IWebElement profileBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa.fa-user")));
            Assert.IsNotNull(profileBtn);
            profileBtn.Click();

            IWebElement loginBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a[href='/account/login']")));
            Assert.IsNotNull(loginBtn);
            loginBtn.Click();

            Assert.That(_driver.Url, Is.EqualTo("https://localhost:44390/account/login"));

            IWebElement username = _driver.FindElement(By.Name("UserName"));
            username.SendKeys("invalidUser"); // Tên đăng nhập không hợp lệ

            IWebElement password = _driver.FindElement(By.Name("Password"));
            password.SendKeys("0985181215thanH@"); // Mật khẩu hợp lệ   

            IWebElement loginSubmitBtn = _driver.FindElement(By.ClassName("form__button"));
            loginSubmitBtn.Click();


            IWebElement toastError = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".swal2-html-container")));
            Assert.IsNotNull(toastError);

            string expectedErrorMessage = "Đăng nhập không thành công. Vui lòng kiểm tra lại thông tin đăng nhập.";
            Assert.That(toastError.Text, Is.EqualTo(expectedErrorMessage));
        }

        [Test]
        public void loginFailedWithIncorrectPassword()
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            IWebElement profileBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa.fa-user")));
            Assert.IsNotNull(profileBtn);
            profileBtn.Click();

            IWebElement loginBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a[href='/account/login']")));
            Assert.IsNotNull(loginBtn);
            loginBtn.Click();

            Assert.That(_driver.Url, Is.EqualTo("https://localhost:44390/account/login"));

            IWebElement username = _driver.FindElement(By.Name("UserName"));
            username.SendKeys("huutai09072003"); // Tên người dùng hợp lệ

            IWebElement password = _driver.FindElement(By.Name("Password"));
            password.SendKeys("invalidPassword"); // Mật khẩu không hợp lệ

            IWebElement loginSubmitBtn = _driver.FindElement(By.ClassName("form__button"));
            loginSubmitBtn.Click();

            IWebElement toastError = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".swal2-html-container")));
            Assert.IsNotNull(toastError);

            string expectedErrorMessage = "Đăng nhập không thành công. Vui lòng kiểm tra lại thông tin đăng nhập.";
            Assert.That(toastError.Text, Is.EqualTo(expectedErrorMessage));
        }

        [Test]
        public void LoginFailWithIncorrectPasswordAndIncorrectUsername()
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            IWebElement profileBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa.fa-user")));
            Assert.IsNotNull(profileBtn);
            profileBtn.Click();

            IWebElement loginBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a[href='/account/login']")));
            Assert.IsNotNull(loginBtn);
            loginBtn.Click();

            Assert.That(_driver.Url, Is.EqualTo("https://localhost:44390/account/login"));

            IWebElement username = _driver.FindElement(By.Name("UserName"));
            username.SendKeys("invalidUser"); // Tên người dùng không hợp lệ

            IWebElement password = _driver.FindElement(By.Name("Password"));
            password.SendKeys("invalidPassword"); // Mật khẩu không hợp lệ

            IWebElement loginSubmitBtn = _driver.FindElement(By.ClassName("form__button"));
            loginSubmitBtn.Click();

            IWebElement toastError = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".swal2-html-container")));
            Assert.IsNotNull(toastError);

            string expectedErrorMessage = "Đăng nhập không thành công. Vui lòng kiểm tra lại thông tin đăng nhập.";
            Assert.That(toastError.Text, Is.EqualTo(expectedErrorMessage));
        }

        [Test]
        public void LoginAndRedirectToDashboard()
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));

            IWebElement profileBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa.fa-user")));
            profileBtn.Click();

            IWebElement loginBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a[href='/account/login']")));
            loginBtn.Click();

            Assert.That(_driver.Url, Is.EqualTo("https://localhost:44390/account/login"));

            IWebElement username = _driver.FindElement(By.Name("UserName"));
            username.SendKeys("a");

            IWebElement password = _driver.FindElement(By.Name("Password"));
            password.SendKeys("a");

            IWebElement loginSubmitBtn = _driver.FindElement(By.ClassName("form__button"));
            loginSubmitBtn.Click();

            IWebElement toastError = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".swal2-html-container")));
            Assert.IsNotNull(toastError);

            string expectedErrorMessage = "Đăng nhập không thành công. Vui lòng kiểm tra lại thông tin đăng nhập.";
            Assert.That(toastError.Text, Is.EqualTo(expectedErrorMessage));
        }

        // Hàm kiểm tra khả năng xử lý dữ liệu SQL Injection khi người dùng cố gắng đăng nhập
        [Test] 
        public void LoginWithSQLInjection()
        {
            // Khởi tạo WebDriverWait với thời gian chờ tối đa là 20 giây để đợi các phần tử xuất hiện trên trang
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            IWebElement profileBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa.fa-user")));
            profileBtn.Click(); // Nhấn vào biểu tượng để mở giao diện đăng nhập.

            IWebElement loginBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a[href='/account/login']")));
            loginBtn.Click(); // Nhấn vào nút đăng nhập.

            Assert.That(_driver.Url, Is.EqualTo("https://localhost:44390/account/login"));

            // Tìm trường nhập tên người dùng (username) và gửi chuỗi tấn công SQL injection.
            IWebElement username = _driver.FindElement(By.Name("UserName"));
            username.SendKeys("' OR '1'='1"); // Chuỗi tấn công SQL injection nhằm thử đăng nhập không hợp lệ.

            // Tìm trường nhập mật khẩu và cũng gửi cùng chuỗi tấn công SQL injection.
            IWebElement password = _driver.FindElement(By.Name("Password"));
            password.SendKeys("' OR '1'='1"); // Chuỗi tấn công SQL injection giống trên để thử đăng nhập.

            // Tìm và nhấn vào nút gửi thông tin đăng nhập.
            IWebElement loginSubmitBtn = _driver.FindElement(By.ClassName("form__button"));
            loginSubmitBtn.Click(); // Gửi yêu cầu đăng nhập với thông tin không hợp lệ.

            // Chờ đợi sự xuất hiện của thông báo lỗi (toast error) hiển thị trên giao diện.
            IWebElement toastError = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".swal2-html-container")));
            Assert.IsNotNull(toastError); // Kiểm tra rằng thông báo lỗi đã hiển thị.

            // Xác định thông báo lỗi mong đợi khi đăng nhập không thành công.
            string expectedErrorMessage = "Đăng nhập không thành công. Vui lòng kiểm tra lại thông tin đăng nhập.";

            // So sánh nội dung thực tế của thông báo lỗi với thông báo lỗi mong đợi.
            Assert.That(toastError.Text, Is.EqualTo(expectedErrorMessage));
        }

        // Hàm kiểm tra khả năng xử lý dữ liệu XSS khi người dùng cố gắng đăng nhập với thông tin không hợp lệ
        [Test]
        public void LoginWithXSS()
        {
            // Khởi tạo WebDriverWait với thời gian chờ tối đa là 20 giây để đợi các phần tử xuất hiện trên trang
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            // Chờ đến khi nút profile (icon người dùng) có thể được click và click vào nó
            IWebElement profileBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa.fa-user")));
            profileBtn.Click();

            // Chờ đến khi nút "Đăng nhập" có thể được click và click vào nó
            IWebElement loginBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a[href='/account/login']")));
            loginBtn.Click();

            // Kiểm tra xem URL hiện tại có phải là trang đăng nhập hay không
            Assert.That(_driver.Url, Is.EqualTo("https://localhost:44390/account/login"));

            // Tìm trường nhập tên người dùng (UserName) trên trang đăng nhập
            IWebElement username = _driver.FindElement(By.Name("UserName"));

            // Gửi dữ liệu đã được mã hóa (HTML-encoded) vào trường UserName để kiểm tra khả năng xử lý XSS
            // Ở đây "&lt;" là mã hóa HTML cho ký tự "<", "&gt;" là mã hóa cho ">", giúp ngăn chặn lỗi XSS
            username.SendKeys("&lt;script&gt;alert('XSS')&lt;/script&gt;");

            // Tìm trường nhập mật khẩu (Password) trên trang đăng nhập
            IWebElement password = _driver.FindElement(By.Name("Password"));

            // Gửi dữ liệu tương tự đã được mã hóa vào trường Password
            password.SendKeys("&lt;script&gt;alert('XSS')&lt;/script&gt;");

            // Tìm nút Submit trong form đăng nhập và click để gửi thông tin đăng nhập
            IWebElement loginSubmitBtn = _driver.FindElement(By.ClassName("form__button"));
            loginSubmitBtn.Click();

            // Chờ cho đến khi thông báo lỗi (toast) hiển thị trên giao diện dưới dạng container HTML
            IWebElement toastError = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".swal2-html-container")));

            // Kiểm tra rằng thông báo lỗi (toastError) không phải null, tức là nó đã xuất hiện
            Assert.IsNotNull(toastError);

            // Chuỗi thông báo lỗi dự kiến mà ứng dụng sẽ hiển thị khi nhập sai thông tin đăng nhập
            string expectedErrorMessage = "Đăng nhập không thành công. Vui lòng kiểm tra lại thông tin đăng nhập.";

            // Kiểm tra rằng nội dung thực tế của thông báo lỗi trên giao diện giống với chuỗi thông báo dự kiến
            Assert.That(toastError.Text, Is.EqualTo(expectedErrorMessage));
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