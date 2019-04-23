using System.Threading.Tasks;
using LDAPAuth.App_Start;
using Microsoft.Owin;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.Security;

[assembly: OwinStartup("UmbracoCustomOwinStartup", typeof(UmbracoCustomOwinStartup))]

namespace LDAPAuth.App_Start
{
    public class UmbracoCustomOwinStartup : UmbracoDefaultOwinStartup
    {
        protected IContentSection ContentSection = Current.Configs.Settings().Content;

        /// <summary>
        /// Configure the Identity user manager for use with Umbraco Back office
        /// </summary>
        /// <param name="app"></param>
        protected override void ConfigureUmbracoUserManager(IAppBuilder app)
        {

            // Overload the following method to add a custom user-pass check
            app.ConfigureUserManagerForUmbracoBackOffice<BackOfficeUserManager, BackOfficeIdentityUser>(
                RuntimeState,
                GlobalSettings,
                (options, context) =>
                {
                    var membershipProvider = MembershipProviderExtensions.GetUsersMembershipProvider().AsUmbracoMembershipProvider();
                    var userManager = BackOfficeUserManager.Create(
                        options,
                        Services.UserService,
                        Services.MemberTypeService,
                        Services.EntityService,
                        Services.ExternalLoginService,
                        membershipProvider,
                        ContentSection, //content section config
                        GlobalSettings
                    );
                    userManager.BackOfficeUserPasswordChecker = new HybrideAuthenticator();
                    return userManager;
                });
        }
    }

    /// <summary>
    /// Allow to check password first on LDAP directory and then (in case of fail) switch to umbraco internal database authentication
    /// </summary>
    public class HybrideAuthenticator : IBackOfficeUserPasswordChecker
    {
        Task<BackOfficeUserPasswordCheckerResult> IBackOfficeUserPasswordChecker.CheckPasswordAsync(BackOfficeIdentityUser user, string password)
        {
            ActiveDirectoryBackOfficeUserPasswordChecker ldapAuthentication = new ActiveDirectoryBackOfficeUserPasswordChecker();

            var result = Task.Run(async () => await ldapAuthentication.CheckPasswordAsync(user, password));

            // check the result, if it's not successfull fallback to the default user/pass login
            try
            {
                switch (result.Result)
                {
                    case BackOfficeUserPasswordCheckerResult.ValidCredentials:
                        return result;
                    case BackOfficeUserPasswordCheckerResult.InvalidCredentials:
                        return Task.FromResult(BackOfficeUserPasswordCheckerResult.FallbackToDefaultChecker);
                    case BackOfficeUserPasswordCheckerResult.FallbackToDefaultChecker:
                        return Task.FromResult(BackOfficeUserPasswordCheckerResult.FallbackToDefaultChecker);
                    default:
                        return result;
                }
            }
            catch (System.Exception)
            {
                return Task.FromResult(BackOfficeUserPasswordCheckerResult.FallbackToDefaultChecker);
            }


        }
    }
}