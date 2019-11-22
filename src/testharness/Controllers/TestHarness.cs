using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Diagnostics;
using TestHarness.Models;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;

using System.Net.Http;
using System.Threading;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace TestHarness.Controllers
{
    [Route("api/[controller]")]
    public class TestHarnessController : Controller
    {
        // GET api/TestHarness
        [HttpGet]
        public IEnumerable<string> Get()
        {
            Console.WriteLine("TestHarness was called.");
            return new string[] { "{'service-status': 'TestHarness is up!'}" };
        }

        [HttpGet("reset")]
        public IEnumerable<string> Reset()
        {
            Console.WriteLine("TestHarness reset was called.");
            testRuns.reset();
            return new string[] { "{'rest-status': 'Completed!'}" };
        }

        [HttpGet("test/{loopTimes}/{parallelInvocations}")]
        public string TestMSSQLRunScalar(int loopTimes, int parallelInvocations) {
            return ECSInvoker.StartTest(loopTimes, parallelInvocations, "Direct");
        }

    [HttpGet("test/pool/{loopTimes}/{parallelInvocations}")]
        public string PooledMSSQLRunScalar(int loopTimes, int parallelInvocations) {
            return ECSInvoker.StartTest(loopTimes, parallelInvocations, "Pool");
        }

    }




public class ECSInvoker
{
static testRuns CurRuns = new testRuns();
static  int millisec = 100;
//static string basePath = @"./output/";
static string logResults = "";
//public static List<decimal> runValues = new List<decimal>();

    static public string StartTest(int loopTimes, int parallelInvocations, string runType)
    {
        TimeSpan loopDelay = TimeSpan.FromMilliseconds(millisec);
        var now = DateTime.Now;
        List<decimal> runValues = new List<decimal>();
        List<decimal> totalCallTime = new List<decimal>();

        //var path = Path.Combine(basePath, $"Results.csv");
        string headers = $"datetime, Run, Calls(L), LambdaTime, LambdaTimeMilli, ElapsedTime, ElapsedTime Milli, IPAddress";
        logResults = headers + "\n";
        
        Console.WriteLine($"Starting trial run: {loopTimes} loops, {parallelInvocations} parallel invocations per loop, {loopDelay} ms delay");
        testRuns.runType.Add(runType);

        for (var i = 0; i < loopTimes; i++)
        {
            Console.WriteLine($"Starting run {i + 1} of {loopTimes} runs.");
            for (var j = 0; j < parallelInvocations; j++)
            {
                try {
                    var tct = new Stopwatch();
                    tct.Start();
                    runValues.Add(runTest(j, runType).Result); 
                    tct.Stop(); 
                    totalCallTime.Add(Convert.ToDecimal(tct.Elapsed.Ticks)/10000);                    
                }
                catch (Exception ex){
                    Console.WriteLine($"Error running test: {ex.Message}");

                }
            }
            Console.WriteLine();
            Thread.Sleep(loopDelay.Milliseconds);
        }

        decimal[] d  = runValues.ToArray();
        string runValuesString = "[";
        for (int i = 0; i < d.Length; i++) {
            runValuesString += d[i];
            if(i + 1 < d.Length) {
                runValuesString += ",";
            } else {
                runValuesString += "]";
            }
        }
        
        Console.WriteLine("Run Length:" +  runValues.ToArray().Length);
        Console.WriteLine("Run values:" +  runValuesString);

        testRuns.runs.Add(runValues);
        testRuns.totalCallTime.Add(totalCallTime);

        Console.WriteLine("CurRun Length:" +  testRuns.runs.ToArray().Length);
        Console.WriteLine("Run Type Count:" +  testRuns.runType.Count);
        Console.WriteLine($"Test completed");
        return logResults;
    }


    static async Task<decimal> runTest(int CurCount, string runType)
    {
       
        //string baseUrl = Environment.GetEnvironmentVariable("baseUrl");
        string baseUrl = "https://6l5g0zi2xi.execute-api.us-east-1.amazonaws.com/Prod";
        if (runType == "Pool") {
            baseUrl += "?pool=webapi";
        }
        decimal returnValue = 0;

        using (HttpClient client = new HttpClient()) {
        
            //Setting up the response...         
            
            using (HttpResponseMessage res = await client.GetAsync(baseUrl)) {
                using (HttpContent content = res.Content) {
                    string data = await content.ReadAsStringAsync();
                    if (data != null) {
                        DataConnectionViewModel DC = JsonConvert.DeserializeObject<DataConnectionViewModel>(data);
                        string singleRun = $"{DateTime.Now.Month}/{DateTime.Now.Day} {DateTime.Now.Hour}.{DateTime.Now.Minute}.{DateTime.Now.Second}.{DateTime.Now.Millisecond}, {DC.LamCalls}, {DC.LamElapsedTime}, {DC.LamElapsedTimeMilli}, {DC.ElapsedTime}, {DC.ElapsedTimeMilli}, {DC.IPAddress}";
                        Console.WriteLine($" {CurCount + 1}...{singleRun}");
                        logResults += singleRun + "\n";
                        returnValue= Convert.ToDecimal(DC.LamElapsedTimeMilli);
                        //runValues.Add(returnValue);
                    }
                    else {
                        Console.WriteLine("No data returned");
                    }
                }
            }
        }
        return returnValue;
    }

}


}
