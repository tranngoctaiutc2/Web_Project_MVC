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
        public void userFailed()
        {

        }

        [Test]
        public void passwordFailed()
        {

        }
    }
}