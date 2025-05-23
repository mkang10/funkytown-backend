using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Identity.Client;

public class CloudinaryConfig
{
    public Cloudinary Cloudinary { get; }

    public CloudinaryConfig()
    {
        Account account = new Account(
    "dvbbfcxdz",
    "488563858349271",
    "mJi0vi5akmxSkOFBQBUn5wW1gIw");

        Cloudinary cloudinary = new Cloudinary(account);
        Cloudinary = new Cloudinary(account);
        Cloudinary.Api.Secure = true;        
    }
}
public static class ServiceExtensions
{
    public static void AddCloudinary(this IServiceCollection services)
    {
        var cloudinaryConfig = new CloudinaryConfig();
        services.AddSingleton(cloudinaryConfig.Cloudinary);
    }
}

