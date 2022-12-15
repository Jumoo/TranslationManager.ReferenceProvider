using Jumoo.TranslationManager.Core.Configuration;
using Jumoo.TranslationManager.Core.Models;
using Jumoo.TranslationManager.Core.Providers;
using Jumoo.TranslationManager.Core.Serializers;
using Jumoo.TranslationManager.Serializers;
using Jumoo.TranslationManager.Utilities;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;

namespace ReferenceConnector;

internal class ReferenceConnector : ITranslationProvider
{
    private readonly TranslationConfigService _configService;
    private readonly ILogger<ReferenceConnector> _logger;
    private readonly IHostingEnvironment _hostEnvironment;

    public ReferenceConnector(
        TranslationConfigService configService,
        ILogger<ReferenceConnector> logger,
        IHostingEnvironment hostEnvironment)
    {
        _configService = configService;
        _logger = logger;
        _hostEnvironment = hostEnvironment;
    }

    public string Name => ReferenceConstants.Name;
    public string Alias => ReferenceConstants.Alias;
    public Guid Key => ReferenceConstants.Key;

    public TranslationProviderViews Views => new TranslationProviderViews
    {
        Config = TranslateUriUtility.ToAbsolute(ReferenceConstants.AppPlugins + "config.html"),
        Pending = TranslateUriUtility.ToAbsolute(ReferenceConstants.AppPlugins + "pending.html"),
        Submitted = TranslateUriUtility.ToAbsolute(ReferenceConstants.AppPlugins + "submitted.html"),
        Approved = TranslateUriUtility.ToAbsolute(ReferenceConstants.AppPlugins + "submitted.html")
    };

    /// <summary>
    ///  will report the connector as active if there is anything in the "Key" config value
    /// </summary>
    /// <remarks>
    ///  if a connector is not active the editors don't see it in the connector lists.
    /// </remarks>
    public bool Active()
        => !string.IsNullOrWhiteSpace(_configService.GetProviderSetting(ReferenceConstants.Alias, "key", string.Empty));

    /// <summary>
    ///  submit a job to the connector.
    /// </summary>
    /// <param name="job"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<Attempt<TranslationJob>> Submit(TranslationJob job)
    {
        // load in any setting the user might have added to the job submission
        var jobProperties = JsonConvert.DeserializeObject<ReferenceJobOptions>(job.ProviderProperties);

        // create some XLIFF

        // verious options to control how the serializer behaves. 
        var translationOptions = new TranslationSerializerOptions
        {
            RemoveBlanks = true,
            SplitHtml = true,
        };

        // create an xlif 2.0 document (there is an xliff 1.2 version if required)
        var loader = new Xliff20Loader();
        var serializer = new Xliff2Serializer();

        var attempt = serializer.Serialize(job,
            job.SourceCulture.Name,
            job.TargetCulture.Name,
            translationOptions);

        // something went wrong creating xliff ?
        if (!attempt.Success)
            throw new InvalidOperationException("Failed to serialize content", attempt.Exception);

        // at this point you can create a project and send the XLIFF to your service.

        // here we are saving to disk. 
        using (var stream = new MemoryStream()) {
            loader.WriteToStream(stream, attempt.Result);
            stream.Seek(0, SeekOrigin.Begin);
            SaveXliff(job.Id, stream);
        }

        // create a fake projectId
        if (jobProperties != null)
            jobProperties.ProjectId = Guid.NewGuid().ToString();
        // write it back to the job. 
        job.ProviderProperties = JsonConvert.SerializeObject(jobProperties);

        // if we return success - translation manager will save the job for us.
        return Attempt.Succeed(job);
    }

    /// <summary>
    ///  check the status of a job created by this connector
    /// </summary>
    /// <remarks>
    ///  Check is called when a user 'checks' a job or when the background
    ///  service checks currently submitted jobs (if turned on). 
    ///  
    ///  If you have a webhook in your connector you can then call
    ///  the TranslationJob service .Check method and it will in turn
    ///  call this check method on any given job ids.
    /// </remarks>
    public async Task<Attempt<TranslationJob>> Check(TranslationJob job)
    {
        // load in the job properties
        var jobProperties = JsonConvert.DeserializeObject<ReferenceJobOptions>(job.ProviderProperties);
        if (jobProperties == null)
            return Attempt.Fail(job, new Exception("No Job Properties"));

        // 1. Connect to translation service.
        _logger.LogInformation("Checking Job : {jobId} {projectId}", job.Id, jobProperties.ProjectId);

        // 2. If the job not ready return a fail - job will remain at 'submitted'

        // 3. if Job is ready, 
        //   3.1 Download the file. 


        // !! this reference connector doesn't read the xliff from anywhere
        // !! so never actually processes a job from submitted 

        // assume we get a stream from the api. 
        var stream = new MemoryStream();
        var loader = new Xliff20Loader();
        var xliff = loader.ReadFromStream(stream);

        //   3.2 process it. 
        var serializer = new Xliff2Serializer();

        // example config for when the file comes back.
        var options = new TranslationSerializerOptions
        {
            LanguagesMustMatch = false, // doesn't matter to us if the xliff lanugages match
            PreserveWhiteSpace = true,
        };

        // serializer will load xliff back into job.
        var attempt = serializer.Deserialize(xliff, job, options);

        if (attempt.Success)
        {
            // if the xliff matches the job and has all the right 'target' bits
            // then the job can be returned to Translation Manager which will 
            // update everything and let the user approve the job back into Umbraco.
            return Attempt.Succeed(job);
        }

        return Attempt.Fail(job, attempt.Exception);
    }

    /// <summary>
    ///  called when the user cancels a job inside umbraco.
    /// </summary>
    /// <remarks>
    ///  if your serivce allows the user to cancel a job then this should
    ///  be called here. If you want to stop users cancelling a job, then 
    ///  returning a fail, or throwing an exception will stop the job 
    ///  cancellation.
    /// </remarks>
    public async Task<Attempt<TranslationJob>> Cancel(TranslationJob job)
    {
        return await Task.FromResult(Attempt.Succeed(job));
    }

    /// <summary>
    ///  remove a job
    /// </summary>
    /// <remarks>
    ///  called when a user 'removes' a translation job. 
    ///  
    ///  jobs can only be removed once they are complete all cancelled.
    ///  
    ///  a removal deletes a job from the Umbraco database,
    /// </remarks>
    public async Task<Attempt<TranslationJob>> Remove(TranslationJob job)
        => await Task.FromResult(Attempt.Succeed(job));


    /// <summary>
    ///  RESERVED FOR FUTURE USE : Can this connector translate this job 
    /// </summary>
    /// <remarks>
    ///  this method is not currently checked on job submission
    /// </remarks>
    public bool CanTranslate(TranslationJob job) => true;

    /// <summary>
    ///  RESERVED FOR FUTURE USE : List of supported languages.
    /// </summary>
    /// <remarks>
    ///  a list of languages supported by this connector 
    ///  (not currenty checked on job submission)
    /// </remarks>
    public IEnumerable<string> GetTargetLanguages(string sourceLanguage)
        => Enumerable.Empty<string>();


    /// <summary>
    ///  reload the connector, called when config values have been updated.
    /// </summary>
    /// <remarks>
    ///  if your connector stores any settings between requests etc, you
    ///  should reload them here.
    /// </remarks>
    public void Reload()
    {
        // in this example we read all config as required so do not need to reload
    }

    /// <summary>
    ///  For demo purposes - save.  
    /// </summary>
    /// <param name="jobId"></param>
    /// <param name="direction"></param>
    /// <param name="stream"></param>
    private void SaveXliff(int jobId, Stream stream)
    {
        // save in the temp folder (usually umbraco/data/temp)
        var tempFolderPath = Path.Combine(_hostEnvironment.LocalTempPath, "refrence");
        using (var fileStream = new MemoryStream())
        {
            stream.CopyTo(fileStream);


            fileStream.Seek(0, SeekOrigin.Begin);
            using (var sr = new StreamReader(fileStream))
            {
                var content = sr.ReadToEnd();
                Directory.CreateDirectory(tempFolderPath);

                var filePath = Path.Combine(tempFolderPath,
                     $"{jobId}-{DateTime.Now:yyyyMMdd_hhmmss}.xlf");

                File.WriteAllText(filePath, content);
            }
        }
    }
}
