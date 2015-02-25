# WCFClientConfigHelper
WCF Client Config Helper

This is a string extension that will return the WCF bindings and endpoint configuration from configuration file.

Usage: 

using WCFClientConfigHelper.Extensions;

Getting the Binding: 

var loBinding = "binding name".GetBinding(configName);

Getting the EndPointAddress: 

var loEndPointAddress = "endpoint name".GetEndpointAddress(configname, url(optional)); // if url is not defined it will use the one from the configuration.
