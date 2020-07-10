using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AnyConfig.PerformanceTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting measurement");
            var startTime = Stopwatch.StartNew();

            var options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 16; // 16 cores * 2

            //Test_AnyConfig_Get(options);
            Test_AnyConfig_GetDefault(options);
            //Test_AnyConfig_ConfigurationManager(options);
            //Test_Microsoft_ConfigurationManager(options);

            startTime.Stop();
            Console.WriteLine($"Completed in {startTime.Elapsed}. Done!");
        }

        static void Test_AnyConfig_Get(ParallelOptions options)
        {
            Parallel.For(0, 1000 * 1000, options, (i) =>
            {
                var boolValue = AnyConfig.Config.Get<bool>("BoolValue");
                if (boolValue != true)
                    throw new ArgumentException(nameof(boolValue));
                var intValue = AnyConfig.Config.Get<int>("IntValue");
                if (intValue != 12345)
                    throw new ArgumentException(nameof(intValue));
                var stringValue = AnyConfig.Config.Get<string>("StringValue");
                if (stringValue != "Test.String.Value")
                    throw new ArgumentException(nameof(stringValue));
                if (i % 50000 == 0)
                    System.Diagnostics.Debug.WriteLine(i);
            });
        }

        static void Test_AnyConfig_GetDefault(ParallelOptions options)
        {
            Parallel.For(0, 1000 * 1000, options, (i) =>
            {
                var intValue = AnyConfig.Config.Get<int>("NonExistantValue", 6666);
                if (intValue != 6666)
                    throw new ArgumentException(nameof(intValue));
                if (i % 50000 == 0)
                    System.Diagnostics.Debug.WriteLine(i);
            });
        }

        static void Test_AnyConfig_ConfigurationManager(ParallelOptions options)
        {
            Parallel.For(0, 1000 * 1000, options, (i) =>
            {
                var boolValue = AnyConfig.ConfigurationManager.AppSettings["BoolValue"].As<bool>();
                if (boolValue != true)
                    throw new ArgumentException(nameof(boolValue));
                var intValue = AnyConfig.ConfigurationManager.AppSettings["IntValue"].As<int>();
                if (intValue != 12345)
                    throw new ArgumentException(nameof(intValue));
                var stringValue = AnyConfig.ConfigurationManager.AppSettings["StringValue"].As<string>();
                if (stringValue != "Test.String.Value")
                    throw new ArgumentException(nameof(stringValue));
                if (i % 50000 == 0)
                    System.Diagnostics.Debug.WriteLine(i);
            });
        }

        static void Test_Microsoft_ConfigurationManager(ParallelOptions options)
        {
            Parallel.For(0, 1000 * 1000, options, (i) =>
            {
                var boolValue = System.Configuration.ConfigurationManager.AppSettings["BoolValue"];
                if (boolValue != "true")
                    throw new ArgumentException(nameof(boolValue));
                var intValue = System.Configuration.ConfigurationManager.AppSettings["IntValue"];
                if (intValue != "12345")
                    throw new ArgumentException(nameof(intValue));
                var stringValue = System.Configuration.ConfigurationManager.AppSettings["StringValue"];
                if (stringValue != "Test.String.Value")
                    throw new ArgumentException(nameof(stringValue));
                if (i % 50000 == 0)
                    System.Diagnostics.Debug.WriteLine(i);
            });
        }
    }
}
