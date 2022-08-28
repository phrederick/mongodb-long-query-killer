using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;

namespace MongoDbLongQueryKiller
{
	public class LongQueryKiller
	{
		private readonly IConfiguration _config;
		public static MongoClient _mongoClient;
		public static IMongoDatabase _mongoDb;
		public static int _maxSeconds;

		public LongQueryKiller(IConfiguration config)
		{
			_config = config;
			_mongoClient = new MongoClient(_config.GetConnectionString("MongoDbDetails:ConnectionString"));
			_maxSeconds = Convert.ToInt32(_config.GetValue<string>("MaxSeconds"));
		}

		[FunctionName("LongQueryKiller")]
		public void Run([TimerTrigger("0 */2 * * * *")] TimerInfo myTimer, ILogger log)
		{
			KillOps(_mongoClient, _maxSeconds);
		}
		private void KillOps(MongoClient mongoClient, int maxSeconds)
		{
			var db = mongoClient.GetDatabase("admin");

			// Pipeline stages.
			var currentOp = new BsonDocument("$currentOp", new BsonDocument("allUsers", true)); // If 'allUsers' is set to false, it will only report on operations / idle connections / idle cursors / idle sessions belonging to the user who ran the command.
			var match = new BsonDocument("$match", new BsonDocument("secs_running", new BsonDocument("$gte", maxSeconds))); // Matches any query exceeding the value configured in the settings.
			var project = new BsonDocument("$project", new BsonDocument("opid", 1)); // Just grab the opid as that's all we need to kill the operation.

			// Adjust the line below to target specific long-running queries. Generally this will be something under the 'command' object.
			var yourCustomMatch = new BsonDocument("$match", new BsonDocument("command.<something>", "<somethingUniqueAboutTheNaughtyQuery>"));


			// Combine all the pipeline stages together.
			var pipeline = new[] {
				currentOp,
				match,
				yourCustomMatch,
				project
			};

			// Find the long-running operations to kill.
			var opsToKill = db.Aggregate<BsonDocument>(pipeline).ToList();

			foreach (var op in opsToKill)
			{
				int opId = (int)op.GetValue(0);
				Console.WriteLine($"Killing operation with opid {opId}");
				Console.WriteLine(db.RunCommand<BsonDocument>(new BsonDocument()
					.Add("killOp", 1)
					.Add("op", opId)));
			}
		}
	}
}
