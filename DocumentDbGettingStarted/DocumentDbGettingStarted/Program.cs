﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Net;
using Newtonsoft.Json;
using System.Threading;

namespace DocumentDbGettingStarted
{
    class Program
    {
        private const string EndpointURL = "https://sakshi2710.documents.azure.com:443/";
        private const string PrimaryKEY = "1e5E9kOLYcUYIDOvbX6PHfCTObGVhQqPqxg4lmx6QgSo56iidvr0N6YHfzDo8QQtwPPPcRJWe7piacii7ujLZg==";
        private DocumentClient client;

        static void Main(string[] args)
        {
            try
            {
                Program pgm = new Program();
                pgm.GetStartedDemo().Wait();
            }
            catch (DocumentClientException e)
            {
                Exception baseexpection = e.GetBaseException();
                Console.WriteLine("{0} error occured: {1} , Message : {2}",e.StatusCode, e.Message, baseexpection.Message);
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine("End of demo, press any key to exit.");
                Console.ReadKey();
            }
        }

        private async Task GetStartedDemo()
        {
            this.client = new DocumentClient(new Uri(EndpointURL), PrimaryKEY);
            await this.CreateDatabaseIfNotExists("TrackingData");
            await this.CreateDocumentCollectionIfNotExists("TrackingData", "Milestones");
            // ADD THIS PART TO YOUR CODE
            
            await populateDataSingleMessage();

            #region Family
            //    Family andersenFamily = new Family
            //    {
            //        Id = "Andersen.1",
            //        LastName = "Andersen",
            //        Parents = new Parent[]
            //            {
            //        new Parent { FirstName = "Thomas" },
            //        new Parent { FirstName = "Mary Kay" }
            //            },
            //        Children = new Child[]
            //            {
            //        new Child
            //        {
            //                FirstName = "Henriette Thaulow",
            //                Gender = "female",
            //                Grade = 5,
            //                Pets = new Pet[]
            //                {
            //                        new Pet { GivenName = "Fluffy" }
            //                }
            //        }
            //            },
            //        Address = new Address { State = "WA", County = "King", City = "Seattle" },
            //        IsRegistered = true
            //    };

            //    await this.CreateFamilyDocumentIfNotExists("FamilyDB", "FamilyCollection", andersenFamily);

            //    Family wakefieldFamily = new Family
            //    {
            //        Id = "Wakefield.7",
            //        LastName = "Wakefield",
            //        Parents = new Parent[]
            //{
            //        new Parent { FamilyName = "Wakefield", FirstName = "Robin" },
            //        new Parent { FamilyName = "Miller", FirstName = "Ben" }
            //},
            //        Children = new Child[]
            //{
            //        new Child
            //        {
            //                FamilyName = "Merriam",
            //                FirstName = "Jesse",
            //                Gender = "female",
            //                Grade = 8,
            //                Pets = new Pet[]
            //                {
            //                        new Pet { GivenName = "Goofy" },
            //                        new Pet { GivenName = "Shadow" }
            //                }
            //        },
            //        new Child
            //        {
            //                FamilyName = "Miller",
            //                FirstName = "Lisa",
            //                Gender = "female",
            //                Grade = 1
            //        }
            //},
            //        Address = new Address { State = "NY", County = "Manhattan", City = "NY" },
            //        IsRegistered = false
            //    };

            //    await this.CreateFamilyDocumentIfNotExists("FamilyDB", "FamilyCollection", wakefieldFamily);

            //    this.ExecuteSimpleQuery("FamilyDB", "FamilyCollection");

            //    //Update
            //    andersenFamily.Children[0].Grade = 6;

            //    await this.ReplaceFamilyDocument("FamilyDB", "FamilyCollection", "Andersen.1", andersenFamily);

            //    this.ExecuteSimpleQuery("FamilyDB", "FamilyCollection");

            //    //Delete
            //    await this.DeleteFamilyDocument("FamilyDB", "FamilyCollection", "Andersen.1");

            //    //Delete database
            //    await this.client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri("FamilyDB"));
            #endregion

        }

        private void WriteToConsoleAndPromptToContinue(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
        }

        private async Task CreateDatabaseIfNotExists(string databaseName)
        {
            // Check to verify a database with the id=FamilyDB does not exist
            try
            {
                await this.client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseName));
                this.WriteToConsoleAndPromptToContinue("Found {0}", databaseName);
            }
            catch (DocumentClientException de)
            {
                // If the database does not exist, create a new database
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDatabaseAsync(new Database { Id = databaseName });
                    this.WriteToConsoleAndPromptToContinue("Created {0}", databaseName);
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateDocumentCollectionIfNotExists(string databaseName, string collectionName)
        {
            try
            {
                await this.client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName));
                this.WriteToConsoleAndPromptToContinue("Found {0}", collectionName);
            }
            catch (DocumentClientException de)
            {
                // If the document collection does not exist, create a new collection
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    DocumentCollection collectionInfo = new DocumentCollection();
                    collectionInfo.Id = collectionName;

                    // Configure collections for maximum query flexibility including string range queries.
                    collectionInfo.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });

                    // Here we create a collection with 400 RU/s.
                    await this.client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(databaseName),
                        collectionInfo,
                        new RequestOptions { OfferThroughput = 400 });

                    this.WriteToConsoleAndPromptToContinue("Created {0}", collectionName);
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateFamilyDocumentIfNotExists(string databaseName, string collectionName, TrackingData family)
        {
            try
            {
                await this.client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, family.CurrentStageId));
                this.WriteToConsoleAndPromptToContinue("Found {0}", family.CurrentStageId);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), family);
                    this.WriteToConsoleAndPromptToContinue("Created Family {0}", family.CurrentStageId);
                }
                else
                {
                    throw;
                }
            }
        }

        private void ExecuteSimpleQuery(string databaseName, string collectionName)
        {
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            // Here we find the Andersen family via its LastName
            IQueryable<Family> familyQuery = this.client.CreateDocumentQuery<Family>(
                    UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
                    .Where(f => f.LastName == "Andersen");

            // The query is executed synchronously here, but can also be executed asynchronously via the IDocumentQuery<T> interface
            Console.WriteLine("Running LINQ query...");
            foreach (Family family in familyQuery)
            {
                Console.WriteLine("\tRead {0}", family);
            }

            // Now execute the same query via direct SQL
            IQueryable<Family> familyQueryInSql = this.client.CreateDocumentQuery<Family>(
                    UriFactory.CreateDocumentCollectionUri(databaseName, collectionName),
                    "SELECT * FROM Family WHERE Family.lastName = 'Andersen'",
                    queryOptions);

            Console.WriteLine("Running direct SQL query...");
            foreach (Family family in familyQueryInSql)
            {
                Console.WriteLine("\tRead {0}", family);
            }

            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
        }

        private async Task ReplaceFamilyDocument(string databaseName, string collectionName, string familyName, Family updatedFamily)
        {
            try
            {
                await this.client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, familyName), updatedFamily);
                this.WriteToConsoleAndPromptToContinue("Replaced Family {0}", familyName);
            }
            catch (DocumentClientException de)
            {
                throw;
            }
        }

        private async Task DeleteFamilyDocument(string databaseName, string collectionName, string documentName)
        {
            try
            {
                await this.client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, documentName));
                Console.WriteLine("Deleted Family {0}", documentName);
            }
            catch (DocumentClientException de)
            {
                throw;
            }
        }

        public class TrackingData
        {
            /// <summary>
            /// Tracking system properties
            /// </summary>
            private Dictionary<string, object> systemProperties;

            /// <summary>
            /// Tracking business properties
            /// </summary>
            private Dictionary<string, object> businessProperties;

            public TrackingData()
            {
                this.ParentStageIds = new List<string>();
                this.CorrelationIds = new List<string>();
                this.systemProperties = new Dictionary<string, object>();
                this.businessProperties = new Dictionary<string, object>();
            }

            /// <summary>
            /// Tracking message Id
            /// </summary>
            public string MessageId { get; set; }

            /// <summary>
            /// business property indentifier for business correlation
            /// </summary>
            public KeyValuePair<string, string> BusinessPropertyIdentifier { get; set; }

            /// <summary>
            /// Correlation Id(if batch craete then it will be collection of correlation ids)
            /// </summary>
            public List<string> CorrelationIds { get; set; }

            /// <summary>
            /// Parent Stage Ids
            /// </summary>
            public List<string> ParentStageIds { get; set; }

            /// <summary>
            /// Current Stage Id
            /// </summary>
            public string CurrentStageId { get; set; }

            /// <summary>
            /// Milestone name
            /// </summary>
            public string MileStoneName { get; set; }

            /// <summary>
            /// Event name
            /// </summary>
            public string EventName { get; set; }

            /// <summary>
            /// Event sequence number
            /// </summary>
            public int EventSequenceNumber { get; set; }

            /// <summary>
            /// Event time
            /// </summary>
            public DateTime EventTime { get; set; }
            public double EventTimeEpoch { get; set; }

            /// <summary>
            /// Transaction type
            /// </summary>
            public string TransactionType { get; set; }

            /// <summary>
            /// Partner name
            /// </summary>
            public string PartnerName { get; set; }

            /// <summary>
            /// Description
            /// </summary>
            public string Description { get; set; }

            public Dictionary<string, object> BusinessProperties
            {
                get
                {
                    return this.businessProperties;
                }
            }

            public Dictionary<string, object> SystemProperties
            {
                get
                {
                    return this.systemProperties;
                }
            }

            public void SetBusinessProperties(Dictionary<string, object> properties)
            {
                this.businessProperties = properties;
            }

            public void SetSystemProperties(Dictionary<string, object> properties)
            {
                this.systemProperties = properties;
            }

            /// <summary>
            /// serialize object
            /// </summary>
            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }

        public static double convertDateTimeToEpoch(DateTime time)
        {
            DateTime epoch = new DateTime(1970, 1, 1);

            return time.Subtract(epoch).TotalMilliseconds;
        }

        public async Task populateDataSingleMessage()
        {
            for (int i = 1; i <= 1; i++)
            {
                string correlationId = Guid.NewGuid().ToString();
                string CSID1 = Guid.NewGuid().ToString();
                string CSID2 = Guid.NewGuid().ToString();
                string CSID3 = Guid.NewGuid().ToString();
                string CSID4 = Guid.NewGuid().ToString();
                string CSID5 = Guid.NewGuid().ToString();
                string CSID6 = Guid.NewGuid().ToString();

                TrackingData mtpbTrackingData = new TrackingData()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    ParentStageIds = null,
                    CurrentStageId = CSID1,
                    CorrelationIds = new List<string>() { correlationId },
                    MileStoneName = "MTPB",
                    EventName = "Receive",
                    EventSequenceNumber = 1,
                    EventTime = DateTime.UtcNow,
                    EventTimeEpoch = convertDateTimeToEpoch(DateTime.UtcNow),
                };
                await this.CreateFamilyDocumentIfNotExists("TrackingData", "Milestones", mtpbTrackingData);

                //this.ExecuteSimpleQuery("TrackingData", "Milestones");

                Thread.Sleep(1000);

                TrackingData babatTrackingDataReceive = new TrackingData()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    ParentStageIds = new List<string>() { CSID1 },
                    CurrentStageId = CSID2,
                    CorrelationIds = new List<string>() { correlationId },
                    MileStoneName = "BA/BAT",
                    EventName = "Receive",
                    EventSequenceNumber = 2,
                    EventTime = DateTime.UtcNow,
                    EventTimeEpoch = convertDateTimeToEpoch(DateTime.UtcNow),
                };
                await this.CreateFamilyDocumentIfNotExists("TrackingData", "Milestones", babatTrackingDataReceive);

                //this.ExecuteSimpleQuery("TrackingData", "Milestones");
                Thread.Sleep(1000);
                TrackingData babatTrackingDataSend = new TrackingData()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    ParentStageIds = new List<string>() { CSID2 },
                    CurrentStageId = CSID3,
                    CorrelationIds = new List<string>() { correlationId },
                    MileStoneName = "BA/BAT",
                    EventName = "Send",
                    EventSequenceNumber = 3,
                    EventTime = DateTime.UtcNow,
                    EventTimeEpoch = convertDateTimeToEpoch(DateTime.UtcNow),
                };
                await this.CreateFamilyDocumentIfNotExists("TrackingData", "Milestones", babatTrackingDataSend);

                //this.ExecuteSimpleQuery("TrackingData", "Milestones");
                Thread.Sleep(1000);
                TrackingData lobTrackingDataReceive = new TrackingData()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    BusinessPropertyIdentifier = new KeyValuePair<string, string>("PONumber", "12345"),
                    ParentStageIds = new List<string>() { CSID3 },
                    CurrentStageId = CSID4,
                    CorrelationIds = new List<string>() { correlationId },
                    MileStoneName = "LOB",
                    EventName = "Receive",
                    EventSequenceNumber = 4,
                    EventTime = DateTime.UtcNow,
                    EventTimeEpoch = convertDateTimeToEpoch(DateTime.UtcNow),
                    TransactionType = "Purchase Order",
                    PartnerName = "Fabrikam"
                };
                await this.CreateFamilyDocumentIfNotExists("TrackingData", "Milestones", lobTrackingDataReceive);

                //this.ExecuteSimpleQuery("TrackingData", "Milestones");
                Thread.Sleep(1000);

                TrackingData lobTrackingDataSend = new TrackingData()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    BusinessPropertyIdentifier = new KeyValuePair<string, string>("PONumber", "12345"),
                    ParentStageIds = new List<string>() { CSID4 },
                    CurrentStageId = CSID5,
                    CorrelationIds = new List<string>() { correlationId },
                    MileStoneName = "LOB",
                    EventName = "Send",
                    EventSequenceNumber = 5,
                    EventTime = DateTime.UtcNow,
                    EventTimeEpoch = convertDateTimeToEpoch(DateTime.UtcNow),
                    TransactionType = "Purchase Order",
                    PartnerName = "Fabrikam"
                };
                await this.CreateFamilyDocumentIfNotExists("TrackingData", "Milestones", lobTrackingDataSend);

                //this.ExecuteSimpleQuery("TrackingData", "Milestones");


                Console.WriteLine("i={0} \n correlationid={1} ", i, correlationId);
            }
        }

        public async Task populateDataBatchAtEDIMessage()
        {
            for (int i = 1; i <= 1; i++)
            {
                string correlationId = Guid.NewGuid().ToString();
                string CSID1 = Guid.NewGuid().ToString();
                
                string CSID2 = Guid.NewGuid().ToString();
                string CSID3 = Guid.NewGuid().ToString();
                string CSID4 = Guid.NewGuid().ToString();
                string CSID5 = Guid.NewGuid().ToString();
                string CSID6 = Guid.NewGuid().ToString();
                string CSID7 = Guid.NewGuid().ToString();
                string CSID8 = Guid.NewGuid().ToString();

                TrackingData mtpbTrackingData = new TrackingData()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    ParentStageIds = null,
                    CurrentStageId = CSID1,
                    CorrelationIds = new List<string>() { correlationId },
                    MileStoneName = "MTPB",
                    EventName = "Receive",
                    EventSequenceNumber = 1,
                    EventTime = DateTime.UtcNow,
                    EventTimeEpoch = convertDateTimeToEpoch(DateTime.UtcNow),
                };
                await this.CreateFamilyDocumentIfNotExists("TrackingData", "Milestones", mtpbTrackingData);

                //this.ExecuteSimpleQuery("TrackingData", "Milestones");

                TrackingData mtpbTrackingDataSend = new TrackingData()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    ParentStageIds = new List<string>() { CSID1 },
                    CurrentStageId = CSID2,
                    CorrelationIds = new List<string>() { correlationId },
                    MileStoneName = "MTPB",
                    EventName = "Send",
                    EventSequenceNumber = 1,
                    EventTime = DateTime.UtcNow,
                    EventTimeEpoch = convertDateTimeToEpoch(DateTime.UtcNow),
                };
                await this.CreateFamilyDocumentIfNotExists("TrackingData", "Milestones", mtpbTrackingDataSend);

                //Thread.Sleep(1000);

                TrackingData babatTrackingDataReceive = new TrackingData()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    ParentStageIds = new List<string>() { CSID2 },
                    CurrentStageId = CSID3,
                    CorrelationIds = new List<string>() { correlationId },
                    MileStoneName = "BA/BAT",
                    EventName = "Receive",
                    EventSequenceNumber = 2,
                    EventTime = DateTime.UtcNow,
                    EventTimeEpoch = convertDateTimeToEpoch(DateTime.UtcNow),
                };
                await this.CreateFamilyDocumentIfNotExists("TrackingData", "Milestones", babatTrackingDataReceive);

                //this.ExecuteSimpleQuery("TrackingData", "Milestones");
                Thread.Sleep(1000);
                TrackingData babatTrackingDataSend = new TrackingData()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    ParentStageIds = new List<string>() { CSID3 },
                    CurrentStageId = CSID4,
                    CorrelationIds = new List<string>() { correlationId },
                    MileStoneName = "BA/BAT",
                    EventName = "Send",
                    EventSequenceNumber = 3,
                    EventTime = DateTime.UtcNow,
                    EventTimeEpoch = convertDateTimeToEpoch(DateTime.UtcNow),
                };
                await this.CreateFamilyDocumentIfNotExists("TrackingData", "Milestones", babatTrackingDataSend);

                //this.ExecuteSimpleQuery("TrackingData", "Milestones");
                //Thread.Sleep(1000);
                TrackingData lobTrackingDataReceive = new TrackingData()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    BusinessPropertyIdentifier = new KeyValuePair<string, string>("PONumber", "12345"),
                    ParentStageIds = new List<string>() { CSID4 },
                    CurrentStageId = CSID5,
                    CorrelationIds = new List<string>() { correlationId },
                    MileStoneName = "LOB",
                    EventName = "Receive",
                    EventSequenceNumber = 4,
                    EventTime = DateTime.UtcNow,
                    EventTimeEpoch = convertDateTimeToEpoch(DateTime.UtcNow),
                    TransactionType = "Purchase Order",
                    PartnerName = "Fabrikam"
                };
                await this.CreateFamilyDocumentIfNotExists("TrackingData", "Milestones", lobTrackingDataReceive);


                TrackingData lobTrackingDataSend1 = new TrackingData()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    BusinessPropertyIdentifier = new KeyValuePair<string, string>("PONumber", "12345"),
                    ParentStageIds = new List<string>() { CSID5 },
                    CurrentStageId = CSID6,
                    CorrelationIds = new List<string>() { correlationId },
                    MileStoneName = "LOB",
                    EventName = "Send",
                    EventSequenceNumber = 4,
                    EventTime = DateTime.UtcNow,
                    EventTimeEpoch = convertDateTimeToEpoch(DateTime.UtcNow),
                    TransactionType = "Purchase Order",
                    PartnerName = "DHL"
                };
                await this.CreateFamilyDocumentIfNotExists("TrackingData", "Milestones", lobTrackingDataSend1);


                TrackingData lobTrackingDataSend2 = new TrackingData()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    BusinessPropertyIdentifier = new KeyValuePair<string, string>("PONumber", "12345"),
                    ParentStageIds = new List<string>() { CSID5 },
                    CurrentStageId = CSID7,
                    CorrelationIds = new List<string>() { correlationId },
                    MileStoneName = "LOB",
                    EventName = "Send",
                    EventSequenceNumber = 4,
                    EventTime = DateTime.UtcNow,
                    EventTimeEpoch = convertDateTimeToEpoch(DateTime.UtcNow),
                    TransactionType = "Purchase Order",
                    PartnerName = "Fabrikam"
                };
                await this.CreateFamilyDocumentIfNotExists("TrackingData", "Milestones", lobTrackingDataSend2);
                //this.ExecuteSimpleQuery("TrackingData", "Milestones");
                //Thread.Sleep(1000);

                TrackingData lobTrackingDataSend = new TrackingData()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    BusinessPropertyIdentifier = new KeyValuePair<string, string>("PONumber", "12345"),
                    ParentStageIds = new List<string>() { CSID6, CSID7 },
                    CurrentStageId = CSID8,
                    CorrelationIds = new List<string>() { correlationId },
                    MileStoneName = "LOB",
                    EventName = "Send",
                    EventSequenceNumber = 5,
                    EventTime = DateTime.UtcNow,
                    EventTimeEpoch = convertDateTimeToEpoch(DateTime.UtcNow),
                    TransactionType = "Purchase Order",
                    PartnerName = "Fabrikam"
                };
                await this.CreateFamilyDocumentIfNotExists("TrackingData", "Milestones", lobTrackingDataSend);

                //this.ExecuteSimpleQuery("TrackingData", "Milestones");


                Console.WriteLine("i={0} \n correlationid={1} ", i, correlationId);
            }
        }


        #region Family
        public class Family
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }
            public string LastName { get; set; }
            public Parent[] Parents { get; set; }
            public Child[] Children { get; set; }
            public Address Address { get; set; }
            public bool IsRegistered { get; set; }

            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }

        public class Parent
        {
            public string FamilyName { get; set; }
            public string FirstName { get; set; }
        }

        public class Child
        {
            public string FamilyName { get; set; }
            public string FirstName { get; set; }
            public string Gender { get; set; }
            public int Grade { get; set; }
            public Pet[] Pets { get; set; }
        }

        public class Pet
        {
            public string GivenName { get; set; }
        }

        public class Address
        {
            public string State { get; set; }
            public string County { get; set; }
            public string City { get; set; }
        }
        #endregion
    }
}
