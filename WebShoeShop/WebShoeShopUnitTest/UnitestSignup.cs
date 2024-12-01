using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

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

            System.Threading.Thread.Sleep(10000);
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
