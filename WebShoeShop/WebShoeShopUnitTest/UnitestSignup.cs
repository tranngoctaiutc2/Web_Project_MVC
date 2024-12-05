using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using WebShoeShop.Helpers;

namespace WebShoeShopUnitTest
{
    [TestFixture]

    public class UnitestSignup
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
        public void SignUpSuccessfully()
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            IWebElement profileBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa.fa-user")));
            Assert.IsNotNull(profileBtn);
            profileBtn.Click();

            IWebElement loginBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a[href='/account/Register']")));
            Assert.IsNotNull(loginBtn);
            loginBtn.Click();

            Assert.That(_driver.Url, Is.EqualTo("https://localhost:44390/account/Register"));

            IWebElement fullname = _driver.FindElement(By.Name("FullName"));
            fullname.SendKeys("Administrator");

            IWebElement username = _driver.FindElement(By.Name("UserName"));
            username.SendKeys("admin1");

            IWebElement email = _driver.FindElement(By.Name("Email"));
            email.SendKeys("admin1@example.com");

            IWebElement password = _driver.FindElement(By.Name("Password"));
            password.SendKeys("Password1@");

            IWebElement confirmpassword = _driver.FindElement(By.Name("ConfirmPassword"));
            confirmpassword.SendKeys("Password1@");

            IWebElement loginSubmitBtn = _driver.FindElement(By.ClassName("form__button"));
            loginSubmitBtn.Click();

            bool isRedirected = wait.Until(drv => drv.Url == "https://localhost:44390/Account/EmailConfirmation");
            Assert.IsTrue(isRedirected, "Không chuyển hướng đến trang xác nhận sau khi đăng ký.");

            IWebElement toastMessage = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".container1.b-container")));
            Assert.IsNotNull(toastMessage);

            // Kiểm tra nội dung của thông báo có phải là "Đăng ký thành công!" hay không
            string expectedSuccessMessage = "THÔNG TIN ĐÃ GỬI VỀ EMAIL.\r\nHÃY KIỂM TRA EMAIL.";
            Assert.That(toastMessage.Text, Is.EqualTo(expectedSuccessMessage), "Thông báo sau khi đăng ký không đúng.");
        }
        [Test]
        public void SignUpWithInvalidFullName()
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            IWebElement profileBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa.fa-user")));
            Assert.IsNotNull(profileBtn);
            profileBtn.Click();

            IWebElement loginBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a[href='/account/Register']")));
            Assert.IsNotNull(loginBtn);
            loginBtn.Click();

            Assert.That(_driver.Url, Is.EqualTo("https://localhost:44390/account/Register"));

            IWebElement fullname = _driver.FindElement(By.Name("FullName"));
            fullname.SendKeys("");

            IWebElement username = _driver.FindElement(By.Name("UserName"));
            username.SendKeys("admin1");

            IWebElement email = _driver.FindElement(By.Name("Email"));
            email.SendKeys("admin1@example.com");

            IWebElement password = _driver.FindElement(By.Name("Password"));
            password.SendKeys("Password1@");

            IWebElement confirmpassword = _driver.FindElement(By.Name("ConfirmPassword"));
            confirmpassword.SendKeys("Password1@");

            IWebElement loginSubmitBtn = _driver.FindElement(By.ClassName("form__button"));
            loginSubmitBtn.Click();

            IWebElement errorMessage = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".text-danger.field-validation-error")));
            Assert.IsNotNull(errorMessage);
            Assert.That(errorMessage.Text, Is.EqualTo("Họ tên không được để trống"));
        }
        [Test]
        public void SignUpWithInvalidUserName()
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            IWebElement profileBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa.fa-user")));
            Assert.IsNotNull(profileBtn);
            profileBtn.Click();

            IWebElement loginBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a[href='/account/Register']")));
            Assert.IsNotNull(loginBtn);
            loginBtn.Click();

            Assert.That(_driver.Url, Is.EqualTo("https://localhost:44390/account/Register"));

            IWebElement fullname = _driver.FindElement(By.Name("FullName"));
            fullname.SendKeys("Administrator");

            IWebElement username = _driver.FindElement(By.Name("UserName"));
            username.SendKeys("");

            IWebElement email = _driver.FindElement(By.Name("Email"));
            email.SendKeys("admin1@example.com");

            IWebElement password = _driver.FindElement(By.Name("Password"));
            password.SendKeys("Password1@");

            IWebElement confirmpassword = _driver.FindElement(By.Name("ConfirmPassword"));
            confirmpassword.SendKeys("Password1@");

            IWebElement loginSubmitBtn = _driver.FindElement(By.ClassName("form__button"));
            loginSubmitBtn.Click();

            IWebElement errorMessage = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".text-danger.field-validation-error")));
            Assert.IsNotNull(errorMessage);
            Assert.That(errorMessage.Text, Is.EqualTo("Tên đăng nhập không được để trống"));
        }
        [Test]
        public void SignUpWithInvalidEmail()
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            IWebElement profileBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa.fa-user")));
            Assert.IsNotNull(profileBtn);
            profileBtn.Click();

            IWebElement loginBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a[href='/account/Register']")));
            Assert.IsNotNull(loginBtn);
            loginBtn.Click();

            Assert.That(_driver.Url, Is.EqualTo("https://localhost:44390/account/Register"));

            IWebElement fullname = _driver.FindElement(By.Name("FullName"));
            fullname.SendKeys("Administrator");

            IWebElement username = _driver.FindElement(By.Name("UserName"));
            username.SendKeys("admin1");

            IWebElement email = _driver.FindElement(By.Name("Email"));
            email.SendKeys("admin1@example.com");

            IWebElement password = _driver.FindElement(By.Name("Password"));
            password.SendKeys("");

            IWebElement confirmpassword = _driver.FindElement(By.Name("ConfirmPassword"));
            confirmpassword.SendKeys("Password1@");

            IWebElement loginSubmitBtn = _driver.FindElement(By.ClassName("form__button"));
            loginSubmitBtn.Click();

            IWebElement errorMessage = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".text-danger.field-validation-error")));
            Assert.IsNotNull(errorMessage);
            Assert.That(errorMessage.Text, Is.EqualTo("Mật khẩu không trùng khớp"));
        }
        [Test]
        public void SignUpWithInvalidPassword()
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            IWebElement profileBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa.fa-user")));
            Assert.IsNotNull(profileBtn);
            profileBtn.Click();

            IWebElement loginBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a[href='/account/Register']")));
            Assert.IsNotNull(loginBtn);
            loginBtn.Click();

            Assert.That(_driver.Url, Is.EqualTo("https://localhost:44390/account/Register"));

            IWebElement fullname = _driver.FindElement(By.Name("FullName"));
            fullname.SendKeys("Administrator");

            IWebElement username = _driver.FindElement(By.Name("UserName"));
            username.SendKeys("admin1");

            IWebElement email = _driver.FindElement(By.Name("Email"));
            email.SendKeys("");

            IWebElement password = _driver.FindElement(By.Name("Password"));
            password.SendKeys("Password1@");

            IWebElement confirmpassword = _driver.FindElement(By.Name("ConfirmPassword"));
            confirmpassword.SendKeys("Password1@");

            IWebElement loginSubmitBtn = _driver.FindElement(By.ClassName("form__button"));
            loginSubmitBtn.Click();

            IWebElement errorMessage = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".text-danger.field-validation-error")));
            Assert.IsNotNull(errorMessage);
            Assert.That(errorMessage.Text, Is.EqualTo("Email không được để trống"));
        }
        [Test]
        public void SignUpWithInvalidConfirmPassword()
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            IWebElement profileBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa.fa-user")));
            Assert.IsNotNull(profileBtn);
            profileBtn.Click();

            IWebElement loginBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a[href='/account/Register']")));
            Assert.IsNotNull(loginBtn);
            loginBtn.Click();

            Assert.That(_driver.Url, Is.EqualTo("https://localhost:44390/account/Register"));

            IWebElement fullname = _driver.FindElement(By.Name("FullName"));
            fullname.SendKeys("Administrator");

            IWebElement username = _driver.FindElement(By.Name("UserName"));
            username.SendKeys("admin1");

            IWebElement email = _driver.FindElement(By.Name("Email"));
            email.SendKeys("admin1@example.com");

            IWebElement password = _driver.FindElement(By.Name("Password"));
            password.SendKeys("Password1@");

            IWebElement confirmpassword = _driver.FindElement(By.Name("ConfirmPassword"));
            confirmpassword.SendKeys("");

            IWebElement loginSubmitBtn = _driver.FindElement(By.ClassName("form__button"));
            loginSubmitBtn.Click();

            IWebElement errorMessage = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".text-danger.field-validation-error")));
            Assert.IsNotNull(errorMessage);
            Assert.That(errorMessage.Text, Is.EqualTo("Mật khẩu không trùng khớp"));
        }
        [TearDown]
        public void TearDown()
        {
            if (_driver != null)
            {
                _driver.Quit();
            }
            //TestSeeder.ClearTestUsers();
        }
    }
}
