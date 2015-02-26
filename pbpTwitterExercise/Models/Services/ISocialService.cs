using System;
using System.Collections.Generic;

namespace pbpTwitterExercise.Models.Services
{
    public interface ISocialService
    {
        AggregateFeed GetAggregateFeed(IList<string> feedIds, DateTime fromDateTime);
        Dictionary<string, int> GetFans(string feedId, DateTime fromDateTime);
    }
}
