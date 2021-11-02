using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;

namespace Synergy.Common.Security.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static string[] DefaultRoles { get; } = new[]
        {
            "Admin Manager",
            "Admin",
            "Loan Officer Manager",
            "Data",
            "Acquisition Manager",
            "Acquisition",
            "Data Manager",
            "Loan Officer",
            "Audit",
        };

        private static string[] DefaultPermissions { get; } = new[]
        {
            "Landing.Dashboard.Read",
            "Admin.Attachments.Delete",
            "Admin.Event.Read",
            "Admin.EventAssignment.Read",
            "Admin.EventDataCut.Read",
            "Admin.EventDataCut.Write",
            "Admin.Attachments.Write",
            "Admin.ReviewPage.Read",
            "Admin.ReviewComments.Read",
            "Admin.ReviewComments.Write",
            "Admin.Attachments.Read",
            "Admin.ReviewPage.Write",
            "Admin.EventResultsUpload.Write",
            "Admin.EventBidList.Delete",
            "Admin.EventBidList.Write",
            "Admin.EventBidList.Read",
            "Admin.Event.Write",
            "Admin.Event.Lock",
            "Admin.EventDataDump.Read",
            "Admin.EventAssignment.Write",
            "Admin.PropertyProfile.Write",
            "Admin.PropertyProfile.Read",
            "CRM.SensitiveData.Write",
            "CRM.Contacts.Read",
            "CRM.Contacts.Write",
            "CRM.Contacts.Delete",
            "CRM.Campaigns.Read",
            "CRM.Campaigns.Write",
            "CRM.CampaignsDataDump.Read",
            "CRM.CampaignComments.Read",
            "CRM.CampaignComments.Write",
            "CRM.RecordComments.Read",
            "CRM.RecordComments.Write",
            "CRM.Dashboard.Read",
            "CRM.SensitiveData.Read",
            "CRM.Records.Read",
            "CRM.Opportunities.Read",
            "CRM.Properties.Read",
            "CRM.Opportunities.Write",
            "Admin.User.Read",
            "Admin.User.Write",
            "Admin.Role.Read",
            "Admin.Role.Write",
            "Admin.CRMMailMerge.Read",
            "Admin.CRMMailMerge.Write",
            "Admin.CRMMailMerge.Delete",
            "Admin.UnderwritingMailMerge.Read",
            "Admin.UnderwritingMailMerge.Write",
            "Admin.UnderwritingMailMerge.Delete",
            "Underwriting.MailMerge.Read",
            "CRM.MailMerge.Read",
        };

        public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration, bool isDevelopment = false)
        {
            var useDevLogin = configuration["security:DevLogin"] == "true";
            if (isDevelopment && useDevLogin)
            {
                services
                    .AddAuthentication("DevelopAuthentication")
                    .AddScheme<DevelopAuthOptions, DevelopAuthenticationHandler>("DevelopAuthentication", options =>
                    {
                        options.Claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, "a08a87e2-b66a-4b64-8db8-98ccd5463893"),
                            new Claim(ClaimTypes.Email, "dandreyev@corevalue.net"),
                            new Claim(ClaimTypes.Name, "Dmytro Andreyev"),
                            new Claim(ClaimTypes.Role, JsonConvert.SerializeObject(DefaultRoles)),
                            new Claim("Permissions", JsonConvert.SerializeObject(DefaultPermissions)),
                        };
                    });
            }
            else
            {
                services.AddBearerTokenAuthentication(options => options.SecretKey = configuration["Security:SecretKey"]);
            }

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            return services;
        }
    }
}
