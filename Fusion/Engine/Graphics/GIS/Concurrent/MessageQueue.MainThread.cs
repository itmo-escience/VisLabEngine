using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fusion.Engine.Graphics.GIS.Concurrent
{
	public partial class MessageQueue
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////

		// We should consider different use cases:
		// 1. Read file from disk only														// Executes in disk io thread
		// 2. Process only																	// Executes in process thread
		// 3. Read and process after and vice versa											// 
		// 4. Write data to disk
		// 5. Download file (from internet) to disk											// Same as 
		// 6. Download data from internet and send it to process and write it to disk		// 
		// 7. Read chunk of big file and process it, repeat to the end of the file

		private CancellationTokenSource CancellationRequest;

		private List<MessageQueue>	ProcessQueues;
		private List<MessageQueue>	ProcessQueuesHighPriority;
		private MessageQueue		DiskRWQueue;
		private MessageQueue		DiskRWQueueHighPriority;

		const int	ProcessQueueCount = 4;
		int			CurrentProcessQueueIndex = 0;

		const int ProcessQueueCountHighPriority = 2;
		int CurrentProcessQueueIndexHighPriority = 0;

		public int GetDiskRWQueueHighPriorityCount {  get { return DiskRWQueueHighPriority.queue.Count; } }

		/// <summary>
		/// Starts the main thread queue in a separate thread.  This method returns immediately.
		/// The thread created by this method will continue running until <see cref="Terminate"/>
		/// is called.
		/// </summary>
		public void StartMainThread()
		{
			lock (queue)
			{
				if (state != State.Stopped)
					throw new InvalidOperationException("The MessageQueue is already running.");
				state = State.Running;
			}


			ProcessQueues	= new List<MessageQueue>();
			DiskRWQueue		= new MessageQueue();
			DiskRWQueueHighPriority		= new MessageQueue();
			ProcessQueuesHighPriority	= new List<MessageQueue>();

			DiskRWQueue.StartInAnotherThread();
			DiskRWQueueHighPriority.StartPriorityInAnotherThread();

			for (int i = 0; i < ProcessQueueCount; i++) {
				var q = new MessageQueue();
				q.StartInAnotherThread();
				ProcessQueues.Add(q);
			}

			for (int i = 0; i < ProcessQueueCountHighPriority; i++) {
				var q = new MessageQueue();
				q.StartPriorityInAnotherThread();
				ProcessQueuesHighPriority.Add(q);
			}


			CancellationRequest = new CancellationTokenSource();

			//Thread thread = new Thread(() => MainThread(CancellationRequest.Token));
			//thread.IsBackground = true;
			//thread.Start();

			Task.Factory.StartNew(() => MainThread(CancellationRequest.Token));
		}



		public void MainThread(CancellationToken token)
		{
			Log.Debug("MainThread reporting for duty!!!");

			List<WorkRequest> current = new List<WorkRequest>();

			while (true)
			{
				// Check is cancellation requested
				if (token.IsCancellationRequested)
				{
					Log.Debug("MainThread finish his mission!");
					return;
				}

				// Check new items in the queue
				lock (queue)
				{
					if (queue.Count > 0) {
						current.AddRange(queue);
						queue.Clear();
					}
					else {
						Monitor.Wait(queue);

						current.AddRange(queue);
						queue.Clear();
					}
				}

				// Process items in queue
				//ProcessCurrentQueue(current);
				//Console.WriteLine(DateTime.Now + ": Current queue size is: " + current.Count + "	Thread id: " + Thread.CurrentThread.ManagedThreadId);
				foreach (var request in current) {
					if (request.Flags.HasFlag(WorkRequest.InfoFlags.FillProcessQueue) && request.Priority < 100) {
						request.ProcessQueue = ProcessQueues[CurrentProcessQueueIndex++ % ProcessQueueCount];
					}
					else {
						if (request.Flags.HasFlag(WorkRequest.InfoFlags.FillProcessQueue))
							request.ProcessQueue = ProcessQueuesHighPriority[CurrentProcessQueueIndexHighPriority++ % ProcessQueueCountHighPriority];
					}

					if (request.Flags.HasFlag(WorkRequest.InfoFlags.FillRWQueue) && request.Priority < 100) {
						request.DiskWRQueue = DiskRWQueue;
					} else {
						if(request.Flags.HasFlag(WorkRequest.InfoFlags.FillRWQueue))
							request.DiskWRQueue = DiskRWQueueHighPriority;
					}

					request.Callback(request);
				}

				current.Clear();
			}

		}
	}
}
