using Sitecore.Support.XA.Feature.Media.Repositories;
using Sitecore.XA.Feature.Media.Models;
using Sitecore.XA.Feature.Media.Services;
using Sitecore.XA.Foundation.IoC;
using Sitecore.XA.Foundation.Mvc.Controllers;
using Sitecore.XA.Foundation.Mvc.Repositories.Base;
using Sitecore.XA.Foundation.RenderingVariants.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Support.XA.Feature.Media.Controllers
{
    public class GalleryController : StandardController
    {
        private readonly IGalleryRepository _galleryRepository;

        public GalleryController(IGalleryRepository repository)
        {
            _galleryRepository = repository;
        }

        protected override object GetModel()
        {
            return _galleryRepository.GetModel();
        }
    }
}