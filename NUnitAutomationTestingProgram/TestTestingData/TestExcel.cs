﻿using AutomationTestingProgram;
using AutomationTestingProgram.AutomationFramework;
using AutomationTestingProgram.AutomationFramework.Loggers_and_Reporters;
using AutomationTestingProgram.Builders;
using AutomationTestSetFramework;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TestingDriver;
using static AutomationTestingProgram.InformationObject;

namespace NUnitAutomationTestingProgram.TestTestingData
{
    class TestExcel
    {
        private string saveFileLocation;
        private string readFileLocation;
        private string logName;
        private string reportName;
        private string testType = "Excel";

        [SetUp]
        public void Setup()
        {
            string executingLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            saveFileLocation = $"{executingLocation}/SampleTests/Files";
            readFileLocation = $"{executingLocation}/SampleTests/Excel";
            logName = "/Log.txt";
            reportName = "/Report.txt";

            // Removes all previous ran test results
            // If directory does not exist, don't even try   
            if (Directory.Exists(saveFileLocation))
            {
                if (File.Exists(saveFileLocation + logName))
                    File.Delete(saveFileLocation + logName);
                if (File.Exists(saveFileLocation + reportName))
                    File.Delete(saveFileLocation + reportName);
            }
        }

        [Test]
        public void TestNoUrl()
        {
            TestSet testSet;
            Reporter reporter;

            testSet = buildTestSet("/TestNoURL.xlsx");

            AutomationTestSetDriver.RunTestSet(testSet);
            InformationObject.Reporter.Report();

            reporter = InformationObject.Reporter;

            Assert.IsTrue(reporter.TestSetStatuses[0].RunSuccessful, "Expeted to pass");
        }

        [Test]
        public void TestNoTest()
        {
            TestSet testSet;
            Reporter reporter;

            testSet = buildTestSet("/Test No Test.xlsx");

            AutomationTestSetDriver.RunTestSet(testSet);
            InformationObject.Reporter.Report();

            reporter = InformationObject.Reporter;

            Assert.IsTrue(reporter.TestSetStatuses[0].RunSuccessful, "Expeted to pass");
        }

        [Test]
        public void TestClickXPath()
        {
            TestSet testSet;
            Reporter reporter;

            testSet = buildTestSet("/Test Click Xpath.xlsx");

            AutomationTestSetDriver.RunTestSet(testSet);
            InformationObject.Reporter.Report();

            reporter = InformationObject.Reporter;

            Assert.IsTrue(reporter.TestSetStatuses[0].RunSuccessful, "Expeted to pass");
        }

        [Test]
        public void TestMissingAction()
        {
            TestSet testSet;
            Reporter reporter;

            testSet = buildTestSet("/Test Missing Test Action.xlsx");

            try
            {
                AutomationTestSetDriver.RunTestSet(testSet);
                Assert.Fail("An Exception should of been thrown");
            }
            catch (Exception)
            {
                InformationObject.Reporter.Report();

                reporter = InformationObject.Reporter;

                Assert.IsFalse(reporter.TestSetStatuses[0].RunSuccessful, "Expeted to pass");
            }
        }

        [Test]
        public void TestMultipleTestSteps()
        {
            TestSet testSet;
            Reporter reporter;

            testSet = buildTestSet("/Test Multiple Test Step.xlsx");

            AutomationTestSetDriver.RunTestSet(testSet);
            InformationObject.Reporter.Report();

            reporter = InformationObject.Reporter;

            Assert.IsTrue(reporter.TestSetStatuses[0].RunSuccessful, "Expeted to pass");
        }

        [Test]
        public void TestMultipleUsers()
        {
            TestSet testSet;
            Reporter reporter;

            testSet = buildTestSet("/Test Multiple Users.xlsx");

            AutomationTestSetDriver.RunTestSet(testSet);
            InformationObject.Reporter.Report();

            reporter = InformationObject.Reporter;

            Assert.IsTrue(reporter.TestSetStatuses[0].RunSuccessful, "Expeted to pass");
        }

        [Test]
        public void TestNavigate()
        {
            TestSet testSet;
            Reporter reporter;

            testSet = buildTestSet("/Test Navigate.xlsx");

            AutomationTestSetDriver.RunTestSet(testSet);
            InformationObject.Reporter.Report();

            reporter = InformationObject.Reporter;

            Assert.IsTrue(reporter.TestSetStatuses[0].RunSuccessful, "Expeted to pass");
        }

        [Test]
        public void TestOneUser()
        {
            TestSet testSet;
            Reporter reporter;

            testSet = buildTestSet("/Test One User.xlsx");

            try
            {
                AutomationTestSetDriver.RunTestSet(testSet);
                Assert.Fail("An exception should be thrown");
            }
            catch (Exception)
            {
                InformationObject.Reporter.Report();

                reporter = InformationObject.Reporter;

                Assert.IsFalse(reporter.TestSetStatuses[0].RunSuccessful, "Expeted to pass");
            }
        }

        private TestSet buildTestSet(string testFileName, string url = "testUrl")
        {
            SetEnvironmentVariable(EnvVar.Browser, "chrome");
            SetEnvironmentVariable(EnvVar.Environment, "");
            SetEnvironmentVariable(EnvVar.TimeOutThreshold, "5");
            SetEnvironmentVariable(EnvVar.WarningThreshold, "5");
            SetEnvironmentVariable(EnvVar.URL, url);
            SetEnvironmentVariable(EnvVar.DataFile, $"{readFileLocation}{testFileName}");
            SetEnvironmentVariable(EnvVar.CsvSaveFileLocation, saveFileLocation);
            SetEnvironmentVariable(EnvVar.LogSaveFileLocation, saveFileLocation);
            SetEnvironmentVariable(EnvVar.ReportSaveFileLocation, saveFileLocation);
            SetEnvironmentVariable(EnvVar.ScreenshotSaveLocation, saveFileLocation);
            SetEnvironmentVariable(EnvVar.TestAutomationDriver, "selenium");
            SetEnvironmentVariable(EnvVar.TestSetDataType, testType);
            SetEnvironmentVariable(EnvVar.TestSetDataArgs, $"{readFileLocation}{testFileName}");
            SetEnvironmentVariable(EnvVar.TestCaseDataType, GetEnvironmentVariable(EnvVar.TestSetDataType));
            SetEnvironmentVariable(EnvVar.TestStepDataType, GetEnvironmentVariable(EnvVar.TestCaseDataType));
            SetEnvironmentVariable(EnvVar.TestCaseDataArgs, GetEnvironmentVariable(EnvVar.TestSetDataArgs));
            SetEnvironmentVariable(EnvVar.TestStepDataArgs, GetEnvironmentVariable(EnvVar.TestCaseDataArgs));
            SetEnvironmentVariable(EnvVar.RespectRepeatFor, "true");
            SetEnvironmentVariable(EnvVar.RespectRunAODAFlag, "true");

            InformationObject.SetUp();
            TestSetBuilder builder = new TestSetBuilder();
            BuildAutomationDriver();

            return builder.Build();
        }

        /// <summary>
        /// The original one uses config files which nunit cant read.
        /// </summary>
        private void BuildAutomationDriver()
        {
            ITestingDriver automationDriver = new SeleniumDriver(
                GetEnvironmentVariable(EnvVar.Browser),
                int.Parse(GetEnvironmentVariable(EnvVar.TimeOutThreshold)),
                GetEnvironmentVariable(EnvVar.Environment),
                GetEnvironmentVariable(EnvVar.URL),
                GetEnvironmentVariable(EnvVar.ScreenshotSaveLocation),
                int.Parse("5"),
                GetEnvironmentVariable(EnvVar.LoadingSpinner),
                GetEnvironmentVariable(EnvVar.ErrorContainer),
                string.Empty);

            TestAutomationDriver = automationDriver;
        }
    }
}
