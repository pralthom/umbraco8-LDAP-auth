# umbraco8-LDAP-auth
The purpose of this project is to demonstrate how to integrate LDAP authentication into an Umbraco 8 website

With this solution, the local authentication still works and plays a fallback role. If the system cannot authenticate through LDAP, it will fallback to the classic Umbraco authentication.
It is a very simple approach. Follow these steps:

1. First you need an Umbraco 8 project.
2. In ```/config/umbracoSettings.config``` set ```usernameIsEmail``` to ```false```
3. Create a local user which has the same login and email as a user of your active directory
4. Create a folder ```App_Start``` at the root of your project
5. Add the class ```UmbracoCustomOwinStartup.cs``` in the ```App_Start``` folder (You can find the class in the project example)
6. In your web.config, add the key ```<add key="ActiveDirectoryDomain" value="YOUR_AD_DOMAIN" />``` to the appSettings element. Change ```YOUR_AD_DOMAIN``` by your active directory DNS.
7. Eventually, in your web.config, change the key value of ```owin:appStartup``` by ```UmbracoCustomOwinStartup```

## Example

With the example project you can test this solution. 

The local administrator user has the following credentials:
User : ```LDAPAuth```
Email : ```LDAPAuth@demo.com```
Password : ```LDAPAuthPassword```