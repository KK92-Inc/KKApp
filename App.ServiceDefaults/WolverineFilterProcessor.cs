using System.Diagnostics;
using OpenTelemetry;

namespace Microsoft.Extensions.Hosting;

public class WolverineFilterProcessor : BaseProcessor<Activity>
{
    public override void OnEnd(Activity activity)
    {
        // Drop spans generated directly by Wolverine source instrumentation
        if (activity.Source.Name.Contains("Wolverine", StringComparison.OrdinalIgnoreCase))
        {
            activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
            return;
        }

        // Drop Postgres/EF Core database queries targeting Wolverine tables
        var sqlQuery = activity.GetTagItem("db.statement")?.ToString() 
                    ?? activity.GetTagItem("db.query.text")?.ToString() 
                    ?? activity.DisplayName;

        if (!string.IsNullOrEmpty(sqlQuery) && sqlQuery.Contains("wolverine", StringComparison.OrdinalIgnoreCase))
        {
            activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
        }
    }
}