using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Lidarr.Http;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Datastore.Events;
using System;

namespace Lidarr.Api.V1.RootFolders
{
    public class RootFolderModule : LidarrRestModuleWithSignalR<RootFolderResource, RootFolder>
    {
        private readonly IRootFolderService _rootFolderService;

        public RootFolderModule(IRootFolderService rootFolderService,
                                IBroadcastSignalRMessage signalRBroadcaster,
                                RootFolderValidator rootFolderValidator,
                                PathExistsValidator pathExistsValidator,
                                MappedNetworkDriveValidator mappedNetworkDriveValidator,
                                StartupFolderValidator startupFolderValidator,
                                SystemFolderValidator systemFolderValidator,
                                FolderWritableValidator folderWritableValidator,
                                ProfileExistsValidator profileExistsValidator,
                                MetadataProfileExistsValidator metadataProfileExistsValidator

        )
            : base(signalRBroadcaster)
        {
            _rootFolderService = rootFolderService;

            GetResourceAll = GetRootFolders;
            GetResourceById = GetRootFolder;
            CreateResource = CreateRootFolder;
            UpdateResource = UpdateRootFolder;
            DeleteResource = DeleteFolder;

            SharedValidator.RuleFor(c => c.Path)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .IsValidPath()
                .SetValidator(mappedNetworkDriveValidator)
                .SetValidator(startupFolderValidator)
                .SetValidator(pathExistsValidator)
                .SetValidator(systemFolderValidator)
                .SetValidator(folderWritableValidator);

            PostValidator.RuleFor(c => c.Path)
                .SetValidator(rootFolderValidator);

            SharedValidator.RuleFor(c => c.Name)
                .NotEmpty();

            SharedValidator.RuleFor(c => c.DefaultMetadataProfileId)
                .SetValidator(metadataProfileExistsValidator);

            SharedValidator.RuleFor(c => c.DefaultQualityProfileId)
                .SetValidator(profileExistsValidator);
        }

        private RootFolderResource GetRootFolder(int id)
        {
            return _rootFolderService.Get(id).ToResource();
        }

        private int CreateRootFolder(RootFolderResource rootFolderResource)
        {
            var model = rootFolderResource.ToModel();

            return _rootFolderService.Add(model).Id;
        }

        private void UpdateRootFolder(RootFolderResource rootFolderResource)
        {
            var model = rootFolderResource.ToModel();

            if (model.Path != rootFolderResource.Path)
            {
                throw new Exception("Cannot edit root folder path");
            }

            _rootFolderService.Update(model);
        }

        private List<RootFolderResource> GetRootFolders()
        {
            var folders = _rootFolderService.AllWithSpaceStats().ToResource();
            return folders;
        }

        private void DeleteFolder(int id)
        {
            _rootFolderService.Remove(id);
        }
    }
}
