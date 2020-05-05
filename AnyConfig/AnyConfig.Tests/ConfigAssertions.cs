using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;

#if DEBUG
[assembly: AnyConfig.Tests.ConfigAssertions(AnyConfig.Tests.Target.TestContext)]
#endif

namespace AnyConfig.Tests
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Assembly, AllowMultiple = true)]
    public class ConfigAssertions : Attribute, ITestAction
    {
        public ActionTargets Targets => ActionTargets.Test;

        public ConfigAssertions(Target target)
        {
            switch (target)
            {
                case Target.StdOut:
                    ConfigAssert.Target = Console.WriteLine;
                    break;
                case Target.StdError:
                    ConfigAssert.Target = Console.Error.WriteLine;
                    break;
                case Target.TestContext:
                default:
                    ConfigAssert.Target = TestContext.WriteLine;
                    break;
            }
        }

        public void AfterTest(ITest test)
        {
            ConfigAssert.FlushToConsole();
        }

        public void BeforeTest(ITest test)
        {
        }
    }

    public enum Target
    {
        StdOut,
        StdError,
        TestContext
    }
}
