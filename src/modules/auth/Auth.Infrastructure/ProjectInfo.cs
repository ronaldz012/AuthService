using System;

namespace Auth.Infrastructure;

public class ProjectInfo
{
    public const string SectionName = "ProjectInfo";
    public AppBranding AppBranding { get; set; } = default!;
    public EmailTemplateDefaults EmailTemplateDefaults { get; set; } = default!;
}

public class AppBranding
{
    public const string SectionName = "AppBranding";
    public string AppName { get; set; } = string.Empty;
    public string AppLogoUrl { get; set; } = string.Empty;
    public string AppPrimaryColor { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
public class EmailTemplateDefaults
{
    public const string SectionName = "EmailTemplateDefaults";
    public string DefaultSenderName { get; set; } = string.Empty;
    public string DefaultFooterText { get; set; } = string.Empty;
    public string DefaultContactLink { get; set; } = string.Empty;
}


//   "ProjectInfo": {
//     "AppBranding": {
//       "AppName": "Nombre de la Aplicación Actual",
//       "AppLogoUrl": "https://cdn.nombreapp.com/logo.png",
//       "AppPrimaryColor": "#FF5733", 
//       "SupportEmail": "soporte@nombreapp.com",
//       "Address": "Dirección de la Empresa, Ciudad, País"
//     },
//     "EmailTemplateDefaults": {
//       "DefaultSenderName": "Support Team",
//       "DefaultFooterText": "Your Default Footer Text Here",
//       "DefaultContactLink": "https://yourdomain.com/support"
//     }
//   },