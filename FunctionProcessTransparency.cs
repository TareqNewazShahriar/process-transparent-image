using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Web;

namespace Company.Function;

public class FunctionProcessTransparency
{
    private readonly ILogger<FunctionProcessTransparency> _logger;

    public FunctionProcessTransparency(ILogger<FunctionProcessTransparency> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Assigning Route is important. Otherwise it will return 404.
    /// </summary>
    /// <param name="req"></param>
    /// <param name=""></param>
    /// <returns></returns>
    [Function("ProcessTransparency")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "ProcessTransparency")] HttpRequestData req)
    {
        string? imageUrlKey = req.Query.AllKeys.FirstOrDefault(x => x?.ToLower() == Constants.ImageUrlKeyLower);
        string? fillTransparencyKey = req.Query.AllKeys.FirstOrDefault(x => x?.ToLower() == Constants.FillTransparencyKeyLower);
        string? smoothEdgingKey = req.Query.AllKeys.FirstOrDefault(x => x?.ToLower() == Constants.SmoothPixelKeyLower);
        
        object? fillTransparencyValue = null;
        bool smoothEdging = true;

        if (imageUrlKey is null || fillTransparencyKey is null
            || string.IsNullOrWhiteSpace(req.Query[imageUrlKey])
            || string.IsNullOrWhiteSpace(req.Query[fillTransparencyKey])
            || !Enum.TryParse(typeof(Constants.FillTransparency), req.Query[fillTransparencyKey], true, out fillTransparencyValue)
            || (smoothEdgingKey is not null && !bool.TryParse(req.Query[smoothEdgingKey], out smoothEdging)))
        {
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteStringAsync("Required parameters not provided.");
            return response;
        }

        try
        {
            string imageUrl = req.Query[imageUrlKey];
            string fillTransparency = req.Query[fillTransparencyKey];

            imageUrl = HttpUtility.UrlDecode(imageUrl);
            var imageBytes = await GetImageFromUrl(imageUrl);
            
            byte[] newImageBytes = null;
            if ((Constants.FillTransparency)fillTransparencyValue != Constants.FillTransparency.None)
            {
                var imageProcessing = new ImageProcessing(imageBytes);
                imageProcessing.SetColorToTransparentPixels((Constants.FillTransparency)fillTransparencyValue, smoothEdging);
                newImageBytes = imageProcessing.ReturnImageBytes();
            }
            else
            {
                newImageBytes = imageBytes;
            }

            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "image/png");
            await response.Body.WriteAsync(newImageBytes);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync(ex.ToString());
            return response;
        }
    }

    private async Task<byte[]> GetImageFromUrl(string uriString)
    {
        Uri uri = new Uri(uriString);
        using var httpClient = new HttpClient();

        // Get the file extension
        var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
        var fileExtension = Path.GetExtension(uriWithoutQuery);

        // Download the image and convert to byte array
        var imageBytes = await httpClient.GetByteArrayAsync(uri);

        return imageBytes;
    }
}
