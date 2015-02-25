using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Configuration;
using System.IO;

namespace WCFClientConfigHelper
{
    public static class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        private static Configuration moConfig = null;

        /// <summary>
        /// Get Binding
        /// </summary>
        /// <param name="bindingName"></param>
        /// <param name="configName"></param>
        /// <returns></returns>
        public static Binding GetBinding(this string bindingName, string configName)
        {
            Binding loBinding = null;
            IBindingConfigurationElement loBindingConfigElement = null;
            var loBindingsSection = moGetBindingsSection(configName);

            if (loBindingsSection == null)
                return loBinding;

            foreach (var loElement in loBindingsSection.BindingCollections.Where(r => r.ConfiguredBindings.Count > 0))
            {
                loBindingConfigElement = loElement.ConfiguredBindings.FirstOrDefault(r => r.Name == bindingName);
                if (loBindingConfigElement != null)
                {
                    loBinding = Activator.CreateInstance(loElement.BindingType) as Binding;
                    loBinding.Name = bindingName;
                    loBindingConfigElement.ApplyConfiguration(loBinding);
                    break;
                }
            }

            return loBinding;
        }

        /// <summary>
        /// Get EndPoint Address
        /// </summary>
        /// <param name="endpointName"></param>
        /// <param name="configName"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static EndpointAddress GetEndpointAddress(this string endpointName, string configName, string url = "")
        {
            EndpointAddress loEndpointAddress = null;
            EndpointIdentity loEndpointIdentity = null;
            var loClientSection = moGetClientSection(configName);

            if (loClientSection == null)
                return loEndpointAddress;

            ChannelEndpointElement loChannelEndpoint = loClientSection.Endpoints
                                                                      .OfType<ChannelEndpointElement>()
                                                                      .FirstOrDefault(r => r.Name == endpointName);

            if (loChannelEndpoint != null && loChannelEndpoint.Identity != null)
            {
                var loIdentity = loChannelEndpoint.Identity;

                if (!string.IsNullOrEmpty(loIdentity.Dns.Value))
                    loEndpointIdentity = EndpointIdentity.CreateDnsIdentity(loIdentity.Dns.Value);
                else if (!string.IsNullOrEmpty(loIdentity.ServicePrincipalName.Value))
                    loEndpointIdentity = EndpointIdentity.CreateSpnIdentity(loIdentity.ServicePrincipalName.Value);
                else if (!string.IsNullOrEmpty(loIdentity.UserPrincipalName.Value))
                    loEndpointIdentity = EndpointIdentity.CreateUpnIdentity(loIdentity.UserPrincipalName.Value);
                else if (!string.IsNullOrEmpty(loIdentity.Rsa.Value))
                    loEndpointIdentity = EndpointIdentity.CreateRsaIdentity(loIdentity.Rsa.Value);

                if (string.IsNullOrEmpty(url))
                    loEndpointAddress = new EndpointAddress(loChannelEndpoint.Address, loEndpointIdentity);
                else
                    loEndpointAddress = new EndpointAddress(new Uri(url), loEndpointIdentity);
            }

            return loEndpointAddress;
        }

        /// <summary>
        /// Set Config File
        /// </summary>
        /// <param name="configName"></param>
        private static void moSetConfigFile(string configName)
        {
            ExeConfigurationFileMap moConfigMap = null;

            if (moConfig == null)
            {
                var loFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, configName, SearchOption.AllDirectories);

                if (loFiles.Length == 0)
                    return ;

                moConfigMap = new ExeConfigurationFileMap()
                {
                    ExeConfigFilename = loFiles[0]
                };

                moConfig = ConfigurationManager.OpenMappedExeConfiguration(moConfigMap, ConfigurationUserLevel.None);
            }

        }

        /// <summary>
        /// Get Bindings Section
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        private static BindingsSection moGetBindingsSection(string configName)
        {
            BindingsSection loBindingSection = null;

            moSetConfigFile(configName);

            if (moConfig != null)
                loBindingSection = moConfig.GetSection("system.serviceModel/bindings") as BindingsSection;

            return loBindingSection;
        }

        /// <summary>
        /// Get Client Section
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        private static ClientSection moGetClientSection(string configName)
        {
            ClientSection loClientSection = null;

            moSetConfigFile(configName);

            if (moConfig != null)
                loClientSection = moConfig.GetSection("system.serviceModel/client") as ClientSection;

            return loClientSection;
        }
    }
}
