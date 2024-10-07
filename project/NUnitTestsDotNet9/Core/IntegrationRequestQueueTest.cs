using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core
{
	[TestFixture]
	public class IntegrationRequestQueueTest
	{
		private IntegrationRequestQueue queue;

		[SetUp]
		public void CreateIntegrationRequestQueue()
		{
			queue = new IntegrationRequestQueue();
		}

		[Test]
		public void SingleForceBuildRequestShouldBeRetrievableFromQueue()
		{
			queue.RequestBuild(BuildCondition.ForceBuild);
			ClassicAssert.IsTrue(queue.HasPendingRequests());
			ClassicAssert.AreEqual(BuildCondition.ForceBuild, queue.WaitForRequest());
		}

		[Test]
		public void SingleModificationBuildRequestShouldBeRetrievableFromQueue()
		{
			queue.RequestBuild(BuildCondition.IfModificationExists);
			ClassicAssert.IsTrue(queue.HasPendingRequests());
			ClassicAssert.AreEqual(BuildCondition.IfModificationExists, queue.WaitForRequest());
		}

		[Test]
		public void QueueShouldInitialBeEmpty()
		{
			ClassicAssert.IsFalse(queue.HasPendingRequests());
		}

		[Test]
		public void QueueShouldBeEmptyAfterRequestIsRetrieved()
		{
			queue.RequestBuild(BuildCondition.IfModificationExists);
			queue.WaitForRequest();
			ClassicAssert.IsFalse(queue.HasPendingRequests());
		}

		[Test]
		public void QueueShouldOnlyContainASingleRequest()
		{
			queue.RequestBuild(BuildCondition.IfModificationExists);
			queue.RequestBuild(BuildCondition.IfModificationExists);
			queue.WaitForRequest();
			ClassicAssert.IsFalse(queue.HasPendingRequests());
		}

		[Test]
		public void IfForceBuildIsRequestedAfterModificationBuildThenForceBuildShouldBeRetrieved()
		{
			queue.RequestBuild(BuildCondition.IfModificationExists);
			queue.RequestBuild(BuildCondition.ForceBuild);
			ClassicAssert.AreEqual(BuildCondition.ForceBuild, queue.WaitForRequest());
			ClassicAssert.IsFalse(queue.HasPendingRequests());
		}

		[Test]
		public void IfModificationBuildIsRequestedAfterForceBuildThenForceBuildShouldBeRetrieved()
		{
			queue.RequestBuild(BuildCondition.ForceBuild);
			queue.RequestBuild(BuildCondition.IfModificationExists);
			ClassicAssert.AreEqual(BuildCondition.ForceBuild, queue.WaitForRequest());
            ClassicAssert.IsFalse(queue.HasPendingRequests());
		}

		// TODO: This is causing ProjectIntegratorTest.Abort() to fail with a "LatchMock has not
		// been signalled problem"
		[Test]
        [Ignore("This entire class should be removed according to Owen")]
		public void WaitForRequestShouldBlockUntilNewBuildIsRequested()
		{
			int processedRequests = 0;
			int processedForcedBuildRequests = 0;
			int processedModExistsRequests = 0;

			Thread spawningThread = new Thread(new ThreadStart(SpawnNewThreads));
			spawningThread.Start();

			completedThreads = 0;
			while (completedThreads < totalThreads)
			{
				processedRequests++;
				BuildCondition condition = queue.WaitForRequest();
				if (condition == BuildCondition.ForceBuild) processedForcedBuildRequests++;
				else if (condition == BuildCondition.IfModificationExists) processedModExistsRequests++;
				else ClassicAssert.Fail("Unexpected build request");
			}
			
			ClassicAssert.IsTrue(spawningThread.Join(30000), "Build request threads did not complete within 30 seconds.");
			
			ClassicAssert.AreEqual(totalThreads * 2, processedRequests, "Not all threads which started, completed.");
			ClassicAssert.AreEqual(totalThreads, processedForcedBuildRequests, "Not all force build requests were received.");
			ClassicAssert.AreEqual(totalThreads, processedModExistsRequests, "Not all modification exists requests were received.");
		}

		private int completedThreads = 0;
		private int totalThreads = 400;

		private void SpawnNewThreads()
		{
			Thread[] threads = new Thread[totalThreads];
			for (int i = 0; i < totalThreads; i++)
			{
				Thread thread = new Thread(new ThreadStart(RequestNewBuild));
				thread.Priority = ThreadPriority.BelowNormal;
				thread.Start();
				threads[i] = thread;
			}
			foreach (Thread thread in threads)
			{
				thread.Join();
			}
		}

		private void RequestNewBuild()
		{
			Thread.Sleep(new Random().Next(4));
			queue.RequestBuild(BuildCondition.ForceBuild);
			Thread.Sleep(new Random().Next(4));
			queue.RequestBuild(BuildCondition.IfModificationExists);
			Interlocked.Increment(ref completedThreads);
		}

		public class IntegrationRequestQueue
		{
			private static readonly BuildRequest NoBuildRequested = new BuildRequest(BuildCondition.NoBuild);
			private BuildRequest request = NoBuildRequested;
			private ManualResetEvent latch = new ManualResetEvent(false);

			public void RequestBuild(BuildCondition condition)
			{
				lock (this)
				{
					if (request.IsHigherPriority(condition))
					{
						request = new BuildRequest(condition);
						latch.Set();
					}
				}
			}

			public bool HasPendingRequests()
			{
				lock(this)
				{
					return request != NoBuildRequested;
				} 
			}

			public BuildCondition WaitForRequest()
			{
				if (! HasPendingRequests())
				{
					latch.WaitOne();
				}

				lock (this)
				{
					BuildCondition result = request.Condition;
					request = NoBuildRequested;
					latch.Reset();

					Debug.Assert(result != BuildCondition.NoBuild);
					return result;					
				}
			}
		}

		public class BuildRequest
		{
			private readonly BuildCondition condition;

			public BuildRequest(BuildCondition condition)
			{
				this.condition = condition;
			}

			public bool IsHigherPriority(BuildCondition condition)
			{
				return this.condition < condition;
			}

			public BuildCondition Condition
			{
				get { return condition; }
			}
		}
	}
}
