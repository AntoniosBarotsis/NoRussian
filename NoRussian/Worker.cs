using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Threading.Tasks.Dataflow;
using Pastel;

namespace NoRussian;

public class Worker
{
    private readonly ActionBlock<string> _action;
    private readonly ActionBlock<string> _retry;
    private int _count;
    private int _requestTimeoutSeconds;
    private int _retryDelaySeconds;
    private int _size;
    private int _threads;
    private int _throttleAmount;

    public Worker(int size)
    {
        SetEnvVariables();

        _size = size;

        // Main handler
        async Task ActionHandler(string link)
        {
            await MakeRequest(link);
        }

        // Retries failed requests after some period of time
        async Task RetryActionHandler(string link)
        {
            Thread.Sleep(TimeSpan.FromSeconds(_retryDelaySeconds));

            var retrying = "[RETRYING]".Pastel(Color.Gray);
            Console.WriteLine("{0} {1}", retrying, link);

            await MakeRequest(link);
        }

        // Init action blocks
        _action = new ActionBlock<string>(ActionHandler, new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = _threads
        });
        _retry = new ActionBlock<string>(RetryActionHandler, new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = _threads / 2
        });

        // Completion 
        var handleCompletion =
            new Action<Task>(task => Console.WriteLine("Handler finished with status {0}", task.Status));

        _action.Completion.ContinueWith(handleCompletion);
        _retry.Completion.ContinueWith(handleCompletion);
    }

    private void SetEnvVariables()
    {
        var retryDelay     = Environment.GetEnvironmentVariable("RetryDelay")     ?? "60";
        var requestTimeout = Environment.GetEnvironmentVariable("RequestTimeout") ?? "15";
        var threads        = Environment.GetEnvironmentVariable("Threads")        ?? "20";
        var throttleAmount = Environment.GetEnvironmentVariable("Throttle")       ?? "5";

        _retryDelaySeconds        = int.Parse(retryDelay);
        _requestTimeoutSeconds    = int.Parse(requestTimeout);
        _threads                  = int.Parse(threads);
        _throttleAmount           = int.Parse(throttleAmount);

        var info = @$"
Retry delay:     {_retryDelaySeconds}
Request timeout: {_requestTimeoutSeconds}
Threads:         {_threads}
Throttle:        {_throttleAmount}
";

        Console.WriteLine(info);
    }

    /// <summary>
    ///     Makes a GET request to the provided URL. If the request fails, it will be retried again later. If it succeeds
    ///     it will be fired again as soon as possible.
    /// </summary>
    /// <param name="link">The URL</param>
    private async Task MakeRequest(string link)
    {
        Thread.Sleep(TimeSpan.FromSeconds(_throttleAmount));
        
        var client = new HttpClient();

        client.Timeout = TimeSpan.FromSeconds(_requestTimeoutSeconds);

        try
        {
            _count++;
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            var res = await client.GetAsync(link);
            stopwatch.Stop();

            // Reset count if already looped over everything
            if (_count > _size) _count = 0;

            var status = $"[{res.StatusCode}]";

            // If status code is bad, do not retry
            if (res.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("{0} - {1}", status.Pastel(Color.Red), link);
                _size--;
                return;
            }

            Console.WriteLine("{0} {1}ms - {2} - {3}/{4}", status.Pastel(Color.Green), stopwatch.ElapsedMilliseconds,
                link, _count, _size);

            // Add the link again
            Post(link);
        }
        catch (Exception e)
        {
            var timeOut = "[TIMEOUT]".Pastel(Color.Orange);
            Console.WriteLine("{0} {1} {2}, will retry in {3}", timeOut, link, e.Message, _retryDelaySeconds);
            _retry.Post(link);
        }
    }

    public void Post(string input)
    {
        _action.Post(input);
    }

    public void Complete()
    {
        _action.Complete();
        _retry.Completion.Wait();
        
        _action.Completion.Wait();
        _retry.Completion.Wait();
        
    }

    public void Wait()
    {
        try
        {
            _action.Completion.Wait();
            _retry.Completion.Wait();
        }
        catch (AggregateException ae)
        {
            ae.Handle(e =>
            {
                Console.WriteLine("Encountered {0}: {1}", e.GetType().Name, e.Message);
                return true;
            });
        }
    }
}