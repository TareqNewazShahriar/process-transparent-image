using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace RemoveImageTransparency;

public class FunctionRemoveTransparency
{
    private readonly ILogger<FunctionRemoveTransparency> _logger;

    public FunctionRemoveTransparency(ILogger<FunctionRemoveTransparency> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Assigning Route is important. Otherwise it will return 404.
    /// </summary>
    /// <param name="req"></param>
    /// <param name=""></param>
    /// <returns></returns>
    [Function("RemoveImageTransparency")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "RemoveImageTransparency")] HttpRequestData req)
    {
        ParameterModel parameterModel = null;
        try
        {
            string json = JsonSerializer.Serialize(req.Query.Cast<string>().ToDictionary(k => k, v => req.Query[v]));
            parameterModel = JsonSerializer.Deserialize<ParameterModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if(parameterModel.BlobName is null && parameterModel.ImageUrlValue is null)
                throw new ArgumentNullException($"{nameof(parameterModel.BlobName)} or {nameof(parameterModel.imageUrl)} is not provided.");
        }
        catch (Exception ex)
        {
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteStringAsync($"Invalid parameter or required parameter is not provided! Error: {ex.Message}");
            return response;
        }
        
        try
        {
            byte[] newImageBytes = null;
            (byte[] imageBytes, string contentType) = parameterModel.BlobName is object ?
                await GetBlobImage(parameterModel.BlobName) :
                await GetImageFromUrl(parameterModel.ImageUrlValue);

            if (parameterModel.FillTransparencyValue != FillTransparency.None)
            {
                var imageProcessing = new TransparencyRemovalProcess(imageBytes);
                imageProcessing.SetColorToTransparentPixels(parameterModel.FillTransparencyValue, parameterModel.SmoothEdgeValue);
                newImageBytes = imageProcessing.ReturnImageBytes();
            }
            else
            {
                newImageBytes = imageBytes;
            }

            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", contentType);
            await response.Body.WriteAsync(newImageBytes);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync(ex.Message);
            return response;
        }
    }

    private async Task<(byte[], string)> GetBlobImage(string blobNameWithPath)
    {
        string connectionString = Environment.GetEnvironmentVariable("AzureStorageAccountConnectionString");
        string container = Environment.GetEnvironmentVariable("AzureStorageImageContainerName");
        
        // Get a reference to a container named "sample-container" and then create it
        BlobContainerClient blobContainerClient = new BlobContainerClient(connectionString, container);
        var blobClient = blobContainerClient.GetBlobClient(blobNameWithPath);
        var blobResponse = await blobClient.DownloadContentAsync();

        var bytes = (blobResponse.GetRawResponse().Status == (int)HttpStatusCode.OK && blobResponse.Value.Details.ContentLength > 0) ?
            blobResponse.Value.Content.ToArray() :
            throw new FileNotFoundException();

        return (bytes, blobResponse.Value.Details.ContentType);
    }

    private async Task<(byte[], string)> GetImageFromUrl(string uriString)
    {
        Uri uri = new Uri(uriString);
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Azure Function v4; .Net8-C#;");

        // Download the image and convert to byte array
        var response = await httpClient.GetAsync(uri);
        
        return response.IsSuccessStatusCode ?
            (await response.Content.ReadAsByteArrayAsync(), response.Content.Headers.ContentType.MediaType) :
            throw new FileNotFoundException();
    }
}
