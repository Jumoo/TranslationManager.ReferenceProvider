using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace ReferenceConnector;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
internal class ReferenceJobOptions
{
    /// <summary>
    ///  Id of job on our external system
    /// </summary>
    public string? ProjectId { get; set; }

    /// <summary>
    ///  user provided description of the job
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///  user provided deadline for the job.
    /// </summary>
    public DateTime Deadline { get; set; }
}
