
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Support.XA.Feature.Media
{
    using Sitecore.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.Support.XA.Feature.Media.Repositories;
    using Sitecore.Support.XA.Feature.Media.Controllers;

    public class RegisterDependencies : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IPlaylistRepository, PlaylistRepository>();
            serviceCollection.AddTransient<IGalleryRepository, GalleryRepository>();
            serviceCollection.AddTransient<PlaylistController>();
            serviceCollection.AddTransient<GalleryController>();
        }
}
}