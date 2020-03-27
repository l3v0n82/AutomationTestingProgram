﻿// <copyright file="DatabaseCaseData.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AutomationTestingProgram.TestingData.TestDrivers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Text;
    using AutomationTestingProgram.AutomationFramework;
    using AutomationTestingProgram.Exceptions;
    using AutomationTestSetFramework;
    using DatabaseConnector;

    /// <summary>
    /// A concrete implementation of the ITestCaseData for databases.
    /// </summary>
    public class DatabaseCaseData : ITestCaseData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseCaseData"/> class.
        /// </summary>
        /// <param name="args">Args passe in.</param>
        public DatabaseCaseData(string args)
        {
        }

        /// <inheritdoc/>
        public string TestArgs { get; set; }

        /// <inheritdoc/>
        public string Name { get; } = "Database";

        /// <summary>
        /// Gets or sets connection established to test database.
        /// </summary>
        private OracleDatabase TestDB { get; set; }

        /// <summary>
        /// Gets or sets connection established to environment database.
        /// </summary>
        private OracleDatabase EnvDB { get; set; }

        /// <summary>
        /// Gets or sets list of test steps to run.
        /// </summary>
        private List<ITestStep> TestSteps { get; set; }

        /// <summary>
        /// Gets the SKIP.
        /// </summary>
        private string SKIP { get; } = "#";

        /// <inheritdoc/>
        public bool ExistNextTestStep()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public ITestStep GetNextTestStep()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public ITestCase SetUpTestCase(string testCaseName, bool performAction = true)
        {
            return this.CreateTestCase(testCaseName);
        }

        /// <summary>
        /// Creates a new test step.
        /// </summary>
        /// <param name="testCaseName">The name of the test step.</param>
        /// <returns>The test case.</returns>
        private ITestCase CreateTestCase(string testCaseName)
        {
            try
            {
                this.TestSteps = new List<ITestStep>();
                List<List<object>> table = this.QueryTestCase(testCaseName, collection, release);

                foreach (List<object> row in table)
                {
                    ITestStep testStep = this.CreateTestStep(row);
                    this.TestSteps.Add(testStep);
                }

                // create and return test case
                ITestCase testCase = new TestCase()
                {
                    Name = testCaseName,
                };

                return testCase;
            }
            catch (TestActionNotFound tanf)
            {
                throw tanf;
            }
            catch (Exception e)
            {
                throw new TestCaseCreationFailed(e.ToString());
            }
        }

        /// <summary>
        /// The A Test Step.
        /// </summary>
        /// <param name="row">The row<see cref="T:List{object}"/>.</param>
        /// <returns>The <see cref="ITestStep"/>.</returns>
        private ITestStep CreateTestStep(List<object> row)
        {
            TestStep testStep;

            // // ignore TESTCASE
            string testStepDesc = row[1]?.ToString() ?? string.Empty;   // TESTCASEDESCRIPTION
            string action = row[3]?.ToString() ?? string.Empty;         // ACTIONONOBJECT (test action)
            string attribValue = row[4]?.ToString() ?? string.Empty;    // OBJECT
            string value = row[5]?.ToString() ?? string.Empty;          // VALUE (of the control/field)
            string attribute = row[6]?.ToString() ?? string.Empty;      // COMMENTS (selected attribute)

            string stLocAttempts = row[8]?.ToString() ?? "0"; // LOCAL_ATTEMPTS
            string stLocTimeout = row[9]?.ToString() ?? "0";  // LOCAL_TIMEOUT
            string control = row[10]?.ToString() ?? string.Empty;       // CONTROL

            string testStepType = row[12]?.ToString() ?? "0"; // TESTSTEPTYPE (formerly SEVERITY)
            string goToStep = row[13]?.ToString() ?? string.Empty;      // GOTOSTEP

            int localAttempts = int.Parse(string.IsNullOrEmpty(stLocAttempts) ? "0" : stLocAttempts);
            if (localAttempts == 0)
            {
                localAttempts = alm.AlmGlobalAttempts;
            }

            int localTimeout = int.Parse(string.IsNullOrEmpty(stLocTimeout) ? "0" : stLocTimeout);
            if (localTimeout == 0)
            {
                localTimeout = alm.AlmGlobalTimeOut;
            }

            int testStepTypeId = int.Parse(string.IsNullOrEmpty(testStepType) ? "0" : testStepType);
            if (testStepTypeId == 0)
            {
                testStepTypeId = 1;
            }

            testStep = ReflectiveGetter.GetEnumerableOfType<TestStep>()
                .Find(x => x.Name.Equals(action));

            testStep.TestStepStatus.Description = testStepDesc;
            testStep.Arguments = attribValue;
            testStep.Attempts = localAttempts;
            testStep.ShouldExecuteVariable = control == this.SKIP;

            return testStep.
        }

        /// <summary>
        /// Queries a test case from the test database given the testcase name, collection, and release.
        /// </summary>
        /// <param name="testcase">Name of the testcase.</param>
        /// <param name="collection">Collection that the testcase is part of.</param>
        /// <param name="release">Release of the collection.</param>
        /// <returns>A test case from the test database.</returns>
        private List<List<object>> QueryTestCase(string testcase, string collection, string release)
        {
            this.TestDB = this.ConnectToDatabase(this.TestDB);
            string query = "SELECT T.TESTCASE, T.TESTSTEPDESCRIPTION, T.STEPNUM, T.ACTIONONOBJECT, T.OBJECT, T.VALUE, T.COMMENTS, T.RELEASE, T.LOCAL_ATTEMPTS, T.LOCAL_TIMEOUT, T.CONTROL, T.COLLECTION, T.TEST_STEP_TYPE_ID, T.GOTOSTEP FROM QA_AUTOMATION.TESTCASE T WHERE T.TESTCASE = '" + testcase + "' AND T.COLLECTION = '" + collection + "' AND T.RELEASE = '" + release + "' ORDER BY T.STEPNUM";
            Logger.Info("Querying the following: [" + query + "]");
            var result = this.TestDB.ExecuteQuery(query);
            this.TestDB.Disconnect();
            Logger.Info("Closed connection to database.\n");
            if (result == null || result.Count == 0)
            {
                throw new DatabaseTestCaseNotFound("Database Test Case Not Found");
            }

            return result;
        }

        /// <summary>
        /// connects the given database and returns it.
        /// </summary>
        private OracleDatabase ConnectToDatabase(OracleDatabase database)
        {
            if (database == null || !database.IsConnected())
            {
                int count = 0;

                // trys 3 times
                while (count < 3)
                {
                    string host = ConfigurationManager.AppSettings["DBHost"].ToString();
                    string port = ConfigurationManager.AppSettings["DBPort"].ToString();
                    string serviceName = ConfigurationManager.AppSettings["DBServiceName"].ToString();
                    string userID = ConfigurationManager.AppSettings["DBUserId"].ToString();
                    string password = ConfigurationManager.AppSettings["DBPassword"].ToString();
                    database = new OracleDatabase(host, port, serviceName, userID, password);
                    database.Connect();
                    if (database.IsConnected())
                    {
                        Logger.Info("Connected to database: RVDEV1");
                        break;
                    }

                    count++;
                }
            }

            return database;
        }
    }
}
