using Sitecore.XA.Foundation.IoC;
using Sitecore.XA.Foundation.Mvc.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Support.XA.Feature.Media.Repositories
{
    public interface IGalleryRepository: IModelRepository, IControllerRepository, IAbstractRepository<IRenderingModelBase>
    {
    }
}