using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HollowMan.Core.Test
{
    using HollowMan.Core.DB;
    using HollowMan.Core.Logging;
    using HollowMan.Core.Shared;
    using Moq;
    using Xunit;

    public class TestPrometheus
    {

        [Fact]
        public void TestPrometheusInvocation()
        {
            var testLogger = new Mock<IEventLogger>();
            var logger = new PrometheusLogger(testLogger.Object, "test", "http://192.168.1.128");
            var obs = new WeatherObservation();
            logger.Log(obs);
        }
    }
}
