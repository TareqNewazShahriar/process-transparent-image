using System.Web;

namespace RemoveImageTransparency;

/// <summary>
/// Lowercase propperties used for QueryString props; to make it prominent for not using.
/// </summary>
public class ParameterModel
{
    public FillTransparency FillTransparencyValue { get; set; }
    public string fillTransparency
    {
        set
        {
            FillTransparencyValue = Enum.Parse<FillTransparency>(value, ignoreCase: true); // Throws exception for bad value
        }
    }

    public bool SmoothEdgeValue { get; set; }
    public string smoothEdge
    {
        set
        {
            if (value is not null)
                SmoothEdgeValue = bool.Parse(value); // Throws exception for bad value
            else
                SmoothEdgeValue = false;
        }
    }

    public string? ImageUrlValue { get; set; }
    public string? imageUrl
    {
        set
        {
            ImageUrlValue = value is not null ? HttpUtility.UrlDecode(value) : null;
        }
    }

    public string? BlobName { get; set; }
}
