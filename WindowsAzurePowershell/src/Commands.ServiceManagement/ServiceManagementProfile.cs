namespace Microsoft.WindowsAzure.Commands.ServiceManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using AutoMapper;
    using Management.Compute.Models;
    using Management.Models;
    using Management.Storage.Models;
    using Model;
    using Utilities.Common;
    using PVM = Model.PersistentVMModel;
    using NSM = Microsoft.WindowsAzure.Management.Compute.Models;
    using System.Xml.Linq;
    using System.Xml;
    using System.IO;

    public class ServiceManagementProfile : Profile
    {
        private static readonly Lazy<bool> initialize;

        static ServiceManagementProfile()
        {
            initialize = new Lazy<bool>(() =>
            {
                Mapper.Initialize(m => m.AddProfile<ServiceManagementProfile>());
                return true;
            });
        }

        public static bool Initialize()
        {
            return initialize.Value;
        }

        public override string ProfileName
        {
            get { return "ServiceManagementProfile"; }
        }

        protected override void Configure()
        {
            //SM to NewSM mapping
            Mapper.CreateMap<PVM.InputEndpoint, NSM.InputEndpoint>()
                .ForMember(c => c.VirtualIPAddress, o => o.MapFrom(r => r.Vip != null ? IPAddress.Parse(r.Vip) : null));
            Mapper.CreateMap<PVM.DataVirtualHardDisk, NSM.DataVirtualHardDisk>();
            Mapper.CreateMap<PVM.OSVirtualHardDisk, NSM.OSVirtualHardDisk>()
                .ForMember(c => c.OperatingSystem, o => o.MapFrom(r => r.OS));
            Mapper.CreateMap<PVM.NetworkConfigurationSet, NSM.ConfigurationSet>()
                .ForMember(c => c.InputEndpoints, o => o.MapFrom(r => r.InputEndpoints != null ? r.InputEndpoints.ToList() : null))
                .ForMember(c => c.SubnetNames, o => o.MapFrom(r => r.SubnetNames != null ? r.SubnetNames.ToList() : null));
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet, NSM.ConfigurationSet>();
            Mapper.CreateMap<PVM.LinuxProvisioningConfigurationSet, NSM.ConfigurationSet>();
            Mapper.CreateMap<PVM.ProvisioningConfigurationSet, NSM.ConfigurationSet>();
            Mapper.CreateMap<PVM.ConfigurationSet, NSM.ConfigurationSet>();
            Mapper.CreateMap<PVM.InstanceEndpoint, NSM.InstanceEndpoint>()
                .ForMember(c => c.VirtualIPAddress, o => o.MapFrom(r => r.Vip != null ? IPAddress.Parse(r.Vip) : null));
            Mapper.CreateMap<PVM.InstanceEndpointList, IList<NSM.InstanceEndpoint>>();
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.WinRmConfiguration, NSM.WindowsRemoteManagementSettings>();
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.WinRmListenerProperties, NSM.WindowsRemoteManagementListener>()
                .ForMember(c => c.ListenerType, o => o.MapFrom(r => r.Protocol));
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.WinRmListenerCollection, IList<NSM.WindowsRemoteManagementListener>>();

            //NewSM to SM mapping
            Mapper.CreateMap<NSM.InputEndpoint, PVM.InputEndpoint>()
                .ForMember(c => c.Vip, o => o.MapFrom(r => r.VirtualIPAddress != null ? r.VirtualIPAddress.ToString() : null));
            Mapper.CreateMap<NSM.DataVirtualHardDisk, PVM.DataVirtualHardDisk>();
            Mapper.CreateMap<NSM.OSVirtualHardDisk, PVM.OSVirtualHardDisk>()
                .ForMember(c => c.OS, o => o.MapFrom(r => r.OperatingSystem));
            Mapper.CreateMap<NSM.ConfigurationSet, PVM.ConfigurationSet>();
            Mapper.CreateMap<NSM.ConfigurationSet, PVM.NetworkConfigurationSet>();
            Mapper.CreateMap<NSM.ConfigurationSet, PVM.WindowsProvisioningConfigurationSet>();
            Mapper.CreateMap<NSM.ConfigurationSet, PVM.LinuxProvisioningConfigurationSet>();
            Mapper.CreateMap<NSM.InstanceEndpoint, PVM.InstanceEndpoint>()
                .ForMember(c => c.Vip, o => o.MapFrom(r => r.VirtualIPAddress != null ? r.VirtualIPAddress.ToString() : null));
            Mapper.CreateMap<IList<NSM.InstanceEndpoint>, PVM.InstanceEndpointList>();
            Mapper.CreateMap<NSM.WindowsRemoteManagementSettings, PVM.WindowsProvisioningConfigurationSet.WinRmConfiguration>();
            Mapper.CreateMap<NSM.WindowsRemoteManagementListener, PVM.WindowsProvisioningConfigurationSet.WinRmListenerProperties>()
                .ForMember(c => c.Protocol, o => o.MapFrom(r => r.ListenerType.ToString()));
            Mapper.CreateMap<IList<NSM.WindowsRemoteManagementListener>, PVM.WindowsProvisioningConfigurationSet.WinRmListenerCollection>();

            //Common mapping
            Mapper.CreateMap<OperationStatusResponse, ManagementOperationContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            Mapper.CreateMap<OperationResponse, ManagementOperationContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.RequestId))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.StatusCode.ToString()));

            //AffinityGroup mapping
            Mapper.CreateMap<AffinityGroupGetResponse, AffinityGroupContext>();
            Mapper.CreateMap<AffinityGroupListResponse.AffinityGroup, AffinityGroupContext>();
            Mapper.CreateMap<AffinityGroupGetResponse.HostedServiceReference, AffinityGroupContext.Service>()
                  .ForMember(c => c.Url, o => o.MapFrom(r => r.Uri));
            Mapper.CreateMap<AffinityGroupGetResponse.StorageServiceReference, AffinityGroupContext.Service>()
                  .ForMember(c => c.Url, o => o.MapFrom(r => r.Uri));
            Mapper.CreateMap<OperationStatusResponse, AffinityGroupContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            //Location mapping
            Mapper.CreateMap<LocationsListResponse.Location, LocationsContext>();
            Mapper.CreateMap<OperationStatusResponse, LocationsContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));
            
            //ServiceCertificate mapping
            Mapper.CreateMap<ServiceCertificateGetResponse, CertificateContext>()
                .ForMember(c => c.Data, o => o.MapFrom(r => r.Data != null ? Convert.ToBase64String(r.Data) : null));
            Mapper.CreateMap<ServiceCertificateListResponse.Certificate, CertificateContext>()
                .ForMember(c => c.Url, o => o.MapFrom(r => r.CertificateUri))
                .ForMember(c => c.Data, o => o.MapFrom(r => r.Data != null ? Convert.ToBase64String(r.Data) : null));
            Mapper.CreateMap<OperationStatusResponse, CertificateContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));
            Mapper.CreateMap<ComputeOperationStatusResponse, ManagementOperationContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            //OperatingSystems mapping
            Mapper.CreateMap<OperatingSystemListResponse.OperatingSystem, OSVersionsContext>();
            Mapper.CreateMap<OperationStatusResponse, OSVersionsContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            //Service mapping
            Mapper.CreateMap<HostedServiceGetResponse, HostedServiceDetailedContext>()
                  .ForMember(c => c.Url, o => o.MapFrom(r => r.Uri));
            Mapper.CreateMap<HostedServiceProperties, HostedServiceDetailedContext>();
            Mapper.CreateMap<HostedServiceListResponse.HostedService, HostedServiceDetailedContext>()
                  .ForMember(c => c.Url, o => o.MapFrom(r => r.Uri));
            Mapper.CreateMap<OperationStatusResponse, HostedServiceDetailedContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            //Disk mapping
            //TODO: https://github.com/WindowsAzure/azure-sdk-for-net-pr/issues/104
            //TODO: https://github.com/WindowsAzure/azure-sdk-for-net-pr/issues/105
            Mapper.CreateMap<VirtualMachineDiskListResponse.VirtualMachineDisk, DiskContext>()
                  .ForMember(c => c.MediaLink, o => o.MapFrom(r => r.MediaLinkUri))
                  .ForMember(c => c.DiskSizeInGB, o => o.MapFrom(r => r.LogicalSizeInGB))
                  .ForMember(c => c.OS, o => o.MapFrom(r => r.OperatingSystemType))
                  .ForMember(c => c.DiskName, o => o.MapFrom(r => r.Name))
                  .ForMember(c => c.AttachedTo, o => o.MapFrom(r => r.UsageDetails));
            Mapper.CreateMap<VirtualMachineDiskListResponse.VirtualMachineDiskUsageDetails, DiskContext.RoleReference>();
            Mapper.CreateMap<OperationStatusResponse, DiskContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            //Image mapping
            //TODO: https://github.com/WindowsAzure/azure-sdk-for-net-pr/issues/107
            Mapper.CreateMap<VirtualMachineImageListResponse.VirtualMachineImage, OSImageContext>()
                  .ForMember(c => c.MediaLink, o => o.MapFrom(r => r.MediaLinkUri))
                  .ForMember(c => c.ImageName, o => o.MapFrom(r => r.Name))                  
                  .ForMember(c => c.OS, o => o.MapFrom(r => r.OperatingSystemType))                  
                  .ForMember(c => c.PublishedDate, o => o.MapFrom(r => new DateTime?(r.PublishedDate)))
                  .ForMember(c => c.IconUri, o => o.MapFrom(r => r.SmallIconUri));
            Mapper.CreateMap<OperationStatusResponse, OSImageContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            //Storage mapping
            Mapper.CreateMap<StorageServiceGetResponse, StorageServicePropertiesOperationContext>()
                  .ForMember(c => c.StorageAccountName, o => o.MapFrom(r => r.ServiceName));
            Mapper.CreateMap<StorageServiceProperties, StorageServicePropertiesOperationContext>()
                  .ForMember(c => c.StorageAccountDescription, o => o.MapFrom(r => r.Description))
                  .ForMember(c => c.GeoPrimaryLocation, o => o.MapFrom(r => r.GeoPrimaryRegion))
                  .ForMember(c => c.GeoSecondaryLocation, o => o.MapFrom(r => r.GeoSecondaryRegion))
                  .ForMember(c => c.StorageAccountStatus, o => o.MapFrom(r => r.Status))
                  .ForMember(c => c.StatusOfPrimary, o => o.MapFrom(r => r.StatusOfGeoPrimaryRegion))
                  .ForMember(c => c.StatusOfSecondary, o => o.MapFrom(r => r.StatusOfGeoSecondaryRegion));
            Mapper.CreateMap<StorageServiceListResponse.StorageService, StorageServicePropertiesOperationContext>()
                  .ForMember(c => c.StorageAccountName, o => o.MapFrom(r => r.ServiceName));
            Mapper.CreateMap<OperationStatusResponse, StorageServicePropertiesOperationContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            Mapper.CreateMap<StorageAccountGetKeysResponse, StorageServiceKeyOperationContext>()
                  .ForMember(c => c.Primary, o => o.MapFrom(r => r.PrimaryKey))
                  .ForMember(c => c.Secondary, o => o.MapFrom(r => r.SecondaryKey));
            Mapper.CreateMap<OperationStatusResponse, StorageServiceKeyOperationContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            Mapper.CreateMap<StorageOperationStatusResponse, ManagementOperationContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            //Hosted Services mapping
            Mapper.CreateMap<OperationStatusResponse, DeploymentInfoContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            Mapper.CreateMap<DeploymentGetResponse, DeploymentInfoContext>()
                  .ForMember(c => c.Slot, o => o.MapFrom(r => r.DeploymentSlot.ToString()))
                  .ForMember(c => c.DeploymentName, o => o.MapFrom(r => r.Name))
                  .ForMember(c => c.Url, o => o.MapFrom(r => r.Uri))
                  .ForMember(c => c.DeploymentId, o => o.MapFrom(r => r.PrivateId))
                  .ForMember(c => c.VNetName, o => o.MapFrom(r => r.VirtualNetworkName))
                  .ForMember(c => c.RollbackAllowed, o => o.MapFrom(r => !string.IsNullOrEmpty(r.RollbackAllowed)))
                  .ForMember(c => c.RoleInstanceList, o => o.MapFrom(r => r.RoleInstances))
                  .ForMember(c => c.CurrentUpgradeDomain, o => o.MapFrom(r => r.UpgradeStatus.CurrentUpgradeDomain))
                  .ForMember(c => c.CurrentUpgradeDomainState, o => o.MapFrom(r => r.UpgradeStatus.CurrentUpgradeDomainState))
                  .ForMember(c => c.UpgradeType, o => o.MapFrom(r => r.UpgradeStatus.UpgradeType));
        }
    }

}