
namespace PoshPredictiveText.SyntaxTreeHelpers
{
    using System.Collections.Concurrent;
    using System.Runtime.ExceptionServices;

    /// <summary>
    ///		A synchronisation context that runs all calls scheduled on it (via <see cref="SynchronizationContext.Post"/>) on a single thread.
    /// </summary>
    /// <remarks>
    ///     Original code copied from PSKubectl (License: MIT)
    ///     https://github.com/felixfbecker/PSKubectl/blob/70356d1ece3bda420d4ca87d5b8f9b538d2668c5/src/ThreadAffinitiveSynchronizationContext.cs
    /// </remarks>
    public sealed class ThreadAffinitiveSynchronizationContext
        : SynchronizationContext, IDisposable
    {
        /// <summary>
        ///		A blocking collection (effectively a queue) of work items to execute, consisting of callback delegates and their callback state (if any).
        /// </summary>
        BlockingCollection<KeyValuePair<SendOrPostCallback, object>>? _workItemQueue = new();

        /// <summary>
        ///		Create a new thread-affinitive synchronisation context.
        /// </summary>
        ThreadAffinitiveSynchronizationContext()
        {
        }

        /// <summary>
        ///		Dispose of resources being used by the synchronisation context.
        /// </summary>
        void IDisposable.Dispose()
        {
            if (_workItemQueue is not null)
            {
                _workItemQueue.Dispose();
                _workItemQueue = null;
            }
        }

        /// <summary>
        ///		Check if the synchronisation context has been disposed.
        /// </summary>
        void CheckDisposed()
        {
            if (_workItemQueue is null)
                throw new ObjectDisposedException(GetType().Name);
        }

        /// <summary>
        ///		Run the message pump for the callback queue on the current thread.
        /// </summary>
        void RunMessagePump()
        {
            CheckDisposed();

            while (_workItemQueue?.TryTake(out KeyValuePair<SendOrPostCallback, object> workItem, Timeout.InfiniteTimeSpan) ?? false)
            {
                workItem.Key(workItem.Value);
                // Has the synchronisation context been disposed?
                if (_workItemQueue is null)
                    break;
            }
        }

        /// <summary>
        ///		Terminate the message pump once all callbacks have completed.
        /// </summary>
        void TerminateMessagePump()
        {
            CheckDisposed();
            _workItemQueue?.CompleteAdding();
        }

        /// <summary>
        ///		Dispatch an asynchronous message to the synchronization context.
        /// </summary>
        /// <param name="callback">
        ///		The <see cref="SendOrPostCallback"/> delegate to call in the synchronisation context.
        /// </param>
        /// <param name="callbackState">
        ///		Optional state data passed to the callback.
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///		The message pump has already been started, and then terminated by calling <see cref="TerminateMessagePump"/>.
        /// </exception>
        public override void Post(SendOrPostCallback callback, object? callbackState)
        {
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            CheckDisposed();

            try
            {
                _workItemQueue?.Add(
                    new KeyValuePair<SendOrPostCallback, object>(
                        key: callback,
                        value: callbackState ?? default!
                    )
                );
            }
            catch (InvalidOperationException eMessagePumpAlreadyTerminated)
            {
                throw new InvalidOperationException(
                    "Cannot enqueue the specified callback because the synchronisation context's message pump has already been terminated.",
                    eMessagePumpAlreadyTerminated
                    );
            }
        }

        /// <summary>
        ///		Run an asynchronous operation using the current thread as its synchronisation context.
        /// </summary>
        /// <param name="asyncOperation">
        ///		A <see cref="Func{TResult}"/> delegate representing the asynchronous operation to run.
        /// </param>
        public static void RunSynchronized(Func<Task> asyncOperation)
        {
            if (asyncOperation == null)
                throw new ArgumentNullException(nameof(asyncOperation));

            SynchronizationContext savedContext = Current!;
            try
            {
                using ThreadAffinitiveSynchronizationContext synchronizationContext = new();
                SetSynchronizationContext(synchronizationContext);

                Task rootOperationTask = asyncOperation();
                if (rootOperationTask == null)
                    throw new InvalidOperationException("The asynchronous operation delegate cannot return null.");

                rootOperationTask.ContinueWith(
                    operationTask =>
                        synchronizationContext.TerminateMessagePump(),
                    scheduler:
                        TaskScheduler.Default
                );

                synchronizationContext.RunMessagePump();

                try
                {
                    rootOperationTask
                        .GetAwaiter()
                        .GetResult();
                }
                catch (AggregateException eWaitForTask) // The TPL will almost always wrap an AggregateException around any exception thrown by the async operation.
                {
                    // Is this just a wrapped exception?
                    AggregateException flattenedAggregate = eWaitForTask.Flatten();
                    if (flattenedAggregate.InnerExceptions.Count != 1)
                        throw; // Nope, genuine aggregate.


                    // Yep, so rethrow (preserving original stack-trace).
                    ExceptionDispatchInfo
                        .Capture(
                            flattenedAggregate
                                .InnerExceptions[0]
                        )
                        .Throw();
                }
            }
            finally
            {
                SetSynchronizationContext(savedContext);
            }
        }


        /// <summary>
        ///		Run an asynchronous operation using the current thread as its synchronisation context.
        /// </summary>
        /// <typeparam name="TResult">
        ///		The operation result type.
        /// </typeparam>
        /// <param name="asyncOperation">
        ///		A <see cref="Func{TResult}"/> delegate representing the asynchronous operation to run.
        /// </param>
        /// <returns>
        ///		The operation result.
        /// </returns>
        public static TResult RunSynchronized<TResult>(Func<Task<TResult>> asyncOperation)
        {
            if (asyncOperation == null)
                throw new ArgumentNullException(nameof(asyncOperation));

            SynchronizationContext savedContext = Current!;
            try
            {
                using ThreadAffinitiveSynchronizationContext synchronizationContext = new();
                SetSynchronizationContext(synchronizationContext);


                Task<TResult> rootOperationTask = asyncOperation();
                if (rootOperationTask == null)
                    throw new InvalidOperationException("The asynchronous operation delegate cannot return null.");


                rootOperationTask.ContinueWith(
                    operationTask =>
                        synchronizationContext.TerminateMessagePump(),
                    scheduler:
                        TaskScheduler.Default
                );

                synchronizationContext.RunMessagePump();

                try
                {
                    return
                        rootOperationTask
                            .GetAwaiter()
                            .GetResult();
                }
                catch (AggregateException eWaitForTask) // The TPL will almost always wrap an AggregateException around any exception thrown by the async operation.
                {
                    // Is this just a wrapped exception?
                    AggregateException flattenedAggregate = eWaitForTask.Flatten();
                    if (flattenedAggregate.InnerExceptions.Count != 1)
                        throw; // Nope, genuine aggregate.

                    // Yep, so rethrow (preserving original stack-trace).
                    ExceptionDispatchInfo
                        .Capture(
                            flattenedAggregate
                                .InnerExceptions[0]
                        )
                        .Throw();

                    throw; // Never reached.
                }
            }
            finally
            {
                SetSynchronizationContext(savedContext);
            }
        }
    }
}
