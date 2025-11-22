namespace ImageService.Models.Responses;

public class UploadUrlResponse
{
    public string UploadUrl { get; set; } = string.Empty;
    public string ImageKey { get; set; } = string.Empty;
    public string FinalImageUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}