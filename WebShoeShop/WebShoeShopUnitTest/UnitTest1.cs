using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace WebShoeShopUnitTest
{

    [TestFixture]

    public class LoginTest
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
            username.SendKeys("invalidUser"); // Invalid username

            IWebElement password = _driver.FindElement(By.Name("Password"));
            password.SendKeys("0985181215thanH@"); // Invalid password

            IWebElement loginSubmitBtn = _driver.FindElement(By.ClassName("form__button"));
            loginSubmitBtn.Click();

            IWebElement errorMessage = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".error-message")));
            Assert.IsNotNull(errorMessage);
            Assert.That(errorMessage.Text, Is.EqualTo("Đăng nhập không thành công. Vui lòng kiểm tra lại thông tin đăng nhập.")); // Adjusted error message
        }

        [Test]
        public void loginFailedWithIncorrectPassword()
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
            username.SendKeys("admin"); // Invalid username

            IWebElement password = _driver.FindElement(By.Name("Password"));
            password.SendKeys("invalidPassword"); // Invalid password

            IWebElement loginSubmitBtn = _driver.FindElement(By.ClassName("form__button"));
            loginSubmitBtn.Click();

            IWebElement errorMessage = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".error-message")));
            Assert.IsNotNull(errorMessage);
            Assert.That(errorMessage.Text, Is.EqualTo("Đăng nhập không thành công. Vui lòng kiểm tra lại thông tin đăng nhập.")); // Adjusted error message
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
            username.SendKeys("admin");

            IWebElement password = _driver.FindElement(By.Name("Password"));
            password.SendKeys("0985181215thanH@");

            IWebElement loginSubmitBtn = _driver.FindElement(By.ClassName("form__button"));
            loginSubmitBtn.Click();

            // Kiểm tra URL chuyển hướng
            wait.Until(ExpectedConditions.UrlToBe("https://localhost:44390/"));
            Assert.That(_driver.Url, Is.EqualTo("https://localhost:44390/"));
        }

        [Test]
        public void LoginWithSQLInjection()
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            IWebElement profileBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa.fa-user")));
            profileBtn.Click();

            IWebElement loginBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a[href='/account/login']")));
            loginBtn.Click();

            Assert.That(_driver.Url, Is.EqualTo("https://localhost:44390/account/login"));

            IWebElement username = _driver.FindElement(By.Name("UserName"));
            username.SendKeys("' OR '1'='1");

            IWebElement password = _driver.FindElement(By.Name("Password"));
            password.SendKeys("' OR '1'='1");

            IWebElement loginSubmitBtn = _driver.FindElement(By.ClassName("form__button"));
            loginSubmitBtn.Click();

            IWebElement errorMessage = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".error-message")));
            Assert.IsNotNull(errorMessage);
            Assert.That(errorMessage.Text, Is.EqualTo("Đăng nhập không thành công. Vui lòng kiểm tra lại thông tin đăng nhập."));
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