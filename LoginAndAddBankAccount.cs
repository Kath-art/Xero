using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Support.UI;


namespace Xero
{
    public class LoginAndAddBankAccountTest
    {
        private static readonly IWebDriver driver = new ChromeDriver();
        readonly XmlDocument LoginDetails = new XmlDocument();
        readonly WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));


        [Test]
        public void LoginAndAddBankAccount()
        {
            driver.Navigate().GoToUrl("https://login.xero.com");
            driver.Manage().Window.Maximize();

            string xmlFile = File.ReadAllText("TestData.xml");
            LoginDetails.LoadXml(xmlFile);

            XmlNodeList usernameNode = LoginDetails.GetElementsByTagName("username");
            string username = usernameNode[0].InnerText;

            XmlNodeList passwordNode = LoginDetails.GetElementsByTagName("password");
            string password = passwordNode[0].InnerText;

            driver.FindElement(By.CssSelector("input[data-automationid='Username--input']")).SendKeys(username);
            driver.FindElement(By.CssSelector("input[data-automationid='PassWord--input']")).SendKeys(password);
            driver.FindElement(By.CssSelector("button[data-automationid='LoginSubmit--button']")).Click();
            driver.FindElement(By.CssSelector("button[data-automationid='auth-authsetupqa']")).Click();
            driver.FindElement(By.CssSelector("button[data-automationid='auth-authwithsecurityquestionsbutton']")).Click();

            string firstSecurityQuestion = driver.FindElement(By.CssSelector("label[data-automationid='auth-firstanswer--label']")).Text;
            string secondSecurityQuestion = driver.FindElement(By.CssSelector("label[data-automationid='auth-secondanswer--label']")).Text;

            new List<string> { firstSecurityQuestion, secondSecurityQuestion };

            var questionsAndAnswers = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("What is your dream car?", "ferrari"),
                new KeyValuePair<string, string>("As a child, what did you want to be when you grew up?", "astronaut"),
                new KeyValuePair<string, string>("What was the name of your first girlfriend / boyfriend?", "matt")
            };


            foreach (var question in questionsAndAnswers.Where(q => q.Key == firstSecurityQuestion))
            {
                driver.FindElement(By.CssSelector("input[data-automationid='auth-firstanswer--input']"))
                    .SendKeys(question.Value + Keys.Tab);
            }

            foreach (var question in questionsAndAnswers.Where(q => q.Key == secondSecurityQuestion))
            {
                driver.FindElement(By.CssSelector("input[data-automationid='auth-secondanswer--input']"))
                    .SendKeys(question.Value + Keys.Tab);
            }

            driver.FindElement(By.CssSelector("button[data-automationid='auth-submitanswersbutton']")).Click();

            wait.Until(ExpectedConditions.ElementToBeClickable(By
                .XPath("//button[@data-name='navigation-menu/accounting']")))
                .Click();

            wait.Until(ExpectedConditions.ElementToBeClickable(By.
                XPath("//a[@data-name='navigation-menu/accounting/bank-accounts']")))
                .Click();

            wait.Until(ExpectedConditions.ElementToBeClickable(By.
                Id("ext-gen16")))
                .Click();

            wait.Until(ExpectedConditions.ElementToBeClickable(By
                .CssSelector("a[data-automationid='popularBank-0']")))
                .Click();

            string testAccountName = "Test Account";

            wait.Until(ExpectedConditions.ElementIsVisible(By
                .CssSelector("input[componentid='accountname-1025']")))
                .SendKeys(testAccountName);

            driver.FindElement(By.Id("accounttype-1027-trigger-picker")).Click();

            wait.Until(ExpectedConditions.ElementToBeClickable(By
                .XPath("//ul/li[@data-recordindex='0']")))
                .Click();

            wait.Until(ExpectedConditions.ElementToBeClickable(By
                .Id("accountnumber-1056-inputEl")))
                .SendKeys("5525252552525");

            wait.Until(ExpectedConditions.ElementToBeClickable(By
                .CssSelector("a[data-automationid='continueButton']")))
                .Click();

            wait.Until(ExpectedConditions.ElementToBeClickable(By
                .CssSelector("a[data-automationid='connectbank-buttonIHaveAForm']")))
                .Click();

            wait.Until(ExpectedConditions.ElementToBeClickable(By
                .CssSelector("a[data-automationid='uploadForm-uploadLaterButton']")))
                .Click();

            wait.Until(ExpectedConditions.ElementToBeClickable(By
                .CssSelector("a[data-automationid='uploadFormLater-goToDashboardButton']")))
                .Click();

            // check that account exists on the dashboard
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//a[@data-automationid='bankWidget']")));
            var bankWidgetAccountName = driver.FindElement(By.XPath("//a[@data-automationid='bankWidget']/h3"));
            Assert.That(bankWidgetAccountName.Text, Is.EqualTo(testAccountName));


            wait.Until(ExpectedConditions.ElementToBeClickable(By
                .XPath("//button[@data-name='navigation-menu/accounting']")))
                .Click();

            wait.Until(ExpectedConditions.ElementToBeClickable(By
                .XPath("//a[@data-name='chart-of-accounts']")))
                .Click();

            wait.Until(ExpectedConditions.ElementIsVisible(By.
                Id("WillDelete")))
                .Click();

            driver.FindElement(By.Id("ext-gen20")).Click();

            wait.Until(ExpectedConditions.ElementIsVisible(By
                .Id("popupOK")))
                .Click();

            // check that account is deleted
            var bankAccountDeleted = driver.FindElement(By.Id("ext-gen15"));
            Assert.That(bankAccountDeleted.Text, Is.EqualTo("1 account has been deleted"));

            driver.Quit();
        }
    }
}
