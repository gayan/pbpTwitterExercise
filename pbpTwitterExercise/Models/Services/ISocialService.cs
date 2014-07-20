using System;
using System.Collections.Generic;

namespace pbpTwitterExercise.Models.Services
{
    public interface ISocialService
    {
        AggregateFeed GetAggregateFeed(IList<string> feedIds, DateTime fromDateTime);
    }
}
