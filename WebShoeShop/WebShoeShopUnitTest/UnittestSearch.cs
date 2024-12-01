using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace WebShoeShopUnitTest
{
	[TestFixture]
	public class UnittestSearch
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
		public void Search_ValidKeyword()
		{
			WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

			IWebElement searchButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa-search")));
			Assert.IsNotNull(searchButton, "Nút tìm kiếm không tìm thấy");
			searchButton.Click();

			IWebElement searchInput = wait.Until(ExpectedConditions.ElementIsVisible(By.Name("Searchtext")));
			Assert.IsNotNull(searchInput, "Ô nhập từ khóa không hiển thị");
			searchInput.SendKeys("Giày"); // Ví dụ từ khóa hợp lệ
			searchInput.SendKeys(Keys.Enter);

			wait.Until(ExpectedConditions.UrlContains("/Search?Searchtext="));
			var results = _driver.FindElements(By.CssSelector(".product-item"));
			Assert.IsTrue(results.Count > 0, "Không tìm thấy sản phẩm nào phù hợp với từ khóa");
		}
		[Test]
		public void Search_InvalidKeyword()
		{
			WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

			IWebElement searchButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".fa-search")));
			searchButton.Click();

			IWebElement searchInput = wait.Until(ExpectedConditions.ElementIsVisible(By.Name("Searchtext")));
			searchInput.SendKeys("Từ khóa không tồn tại");
			searchInput.SendKeys(Keys.Enter);

			wait.Until(ExpectedConditions.UrlContains("/Search?Searchtext="));
			IWebElement noResultsMessage = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".no-products-found")));
			Assert.IsTrue(noResultsMessage.Text.Contains("Không tìm thấy sản phẩm"), "Thông báo không chính xác");
		}
		[Test]
		public void Search_EmptyKeyword()
		{
			WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

			IWebElement searchButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".input-group-append")));
			searchButton.Click();

			IWebElement searchInput = wait.Until(ExpectedConditions.ElementIsVisible(By.Name("Searchtext")));
			searchInput.SendKeys("");
			searchInput.SendKeys(Keys.Enter);

			IReadOnlyCollection<IWebElement> results = wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.CssSelector(".product-item")));
			Assert.IsTrue(results.Count > 0, "Không tìm thấy sản phẩm khi từ khóa rỗng.");
		}
		[TearDown]
		public void TearDown()
		{
			_driver.Quit();
		}
	}
}