using System;
using Xunit;

namespace Adliance.Project.Server.Web.Test;

public static class AssertMore
{
    public static void IsNow(DateTime now)
    {
        Assert.InRange(now, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow);
    }
}