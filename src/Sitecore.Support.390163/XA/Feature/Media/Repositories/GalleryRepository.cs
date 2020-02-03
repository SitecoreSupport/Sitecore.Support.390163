namespace Sitecore.Support.XA.Feature.Media.Repositories
{
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.Data;
    using Sitecore.Data.Fields;
    using Sitecore.Data.Items;
    using Sitecore.DependencyInjection;
    using Sitecore.XA.Feature.Media;
    using Sitecore.XA.Feature.Media.Models;
    using Sitecore.XA.Feature.Media.Repositories;
    using Sitecore.XA.Feature.Media.Services;
    using Sitecore.XA.Foundation.IoC;
    using Sitecore.XA.Foundation.Multisite.LinkManagers;
    using Sitecore.XA.Foundation.Mvc.Repositories.Base;
    using Sitecore.XA.Foundation.RenderingVariants.Repositories;
    using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web.Script.Serialization;


    public class GalleryRepository : VariantsRepository, IGalleryRepository, IModelRepository, IControllerRepository, IAbstractRepository<IRenderingModelBase>
    {
            private GallerySettings _settings;

        protected IMediaItemService MediaItemService
        {
            get;
        }

        protected virtual GallerySettings Settings => _settings ?? (_settings = GetGallerySettings());

        public GalleryRepository()
        {
            MediaItemService = ServiceLocator.ServiceProvider.GetService<IMediaItemService>();
        }

        public override IRenderingModelBase GetModel()
        {
            GalleryRenderingModel galleryRenderingModel = new GalleryRenderingModel();
            FillBaseProperties(galleryRenderingModel);
            galleryRenderingModel.Media = GetMedia().Select(CreateGalleryMediaItem);
            galleryRenderingModel.JsonDataProperties = GetJsonDataProperties();
            return galleryRenderingModel;
        }

        protected virtual IEnumerable<Item> GetMedia()
        {
            return Rendering.DataSourceItem?.GetChildrenWithVersion();
        }

        protected virtual string GetJsonDataProperties()
        {
            if (Settings != null)
            {
                return GetGalleryJsonDataProperties(Settings, base.IsControlEditable);
            }
            return string.Empty;
        }

        protected virtual GallerySettings GetGallerySettings()
        {
            Item dataSourceItem = Rendering.DataSourceItem;
            if (dataSourceItem != null)
            {
                GallerySettings gallerySettings = new GallerySettings
                {
                    Height = Rendering.Parameters.ParseInt("GalleryHeight")
                };
                LinkField linkField = dataSourceItem.Fields[Templates.Gallery.Fields.GalleriaTheme];
                if (linkField != null)
                {
                    gallerySettings.GalleriaThemeLink = new LinkItem(linkField).TargetUrl;
                }
                return gallerySettings;
            }
            return null;
        }

        protected virtual string GetGalleryJsonDataProperties(GallerySettings settings, bool isControlEditable)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            if (settings != null)
            {
                dictionary.Add("theme", settings.GalleriaThemeLink);
                dictionary.Add("height", settings.Height);
                dictionary.Add("isControlEditable", isControlEditable);
            }
            return new JavaScriptSerializer().Serialize(dictionary);
        }

        protected virtual string GetVideoUrl(Item item)
        {
            switch (item.Fields[Templates.GalleryVideo.Fields.VideoProvider].ToEnum<VideoProvider>())
            {
                case VideoProvider.YouTube:
                    {
                        Match match = Regex.Match(item[Templates.GalleryVideo.Fields.VideoID], "^(?:https?\\:\\/\\/)?(?:www\\.)?(?:youtu\\.be\\/|youtube\\.com\\/(?:embed\\/|v\\/|watch\\?v\\=))?([\\w-]{10,12})(?:$|\\&|\\?\\#).*");
                        if (match.Success)
                        {
                            return $"https://www.youtube.com/watch?v={match.Groups[1]}";
                        }
                        break;
                    }
                case VideoProvider.Vimeo:
                    {
                        Match match = Regex.Match(item[Templates.GalleryVideo.Fields.VideoID], "^(?:https?\\:\\/\\/)?(?:www\\.)?(?:vimeo\\.com\\/)?(?:.*#|.*/videos/)?([0-9]+)");
                        if (match.Success)
                        {
                            return $"https://vimeo.com/{match.Groups[1]}";
                        }
                        break;
                    }
                case VideoProvider.Dailymotion:
                    {
                        Match match = Regex.Match(item[Templates.GalleryVideo.Fields.VideoID], "^(?:https?\\:\\/\\/)?(?:www\\.)?(?:dailymotion\\.com\\/)?(?:video\\/([^_]+))?[^#]*(?:#video=([^_&]+))?");
                        for (int num = 2; num > 0; num--)
                        {
                            if (match.Success && match.Groups[num].Success)
                            {
                                return $"https://www.dailymotion.com/video/{match.Groups[num].Value}";
                            }
                        }
                        break;
                    }
            }
            return null;
        }

        protected virtual string GetImageUrl(Item item, ID imageFieldId)
        {
            string imageFieldTargetUrl = MediaItemService.GetImageFieldTargetUrl(item, imageFieldId);
            if (!string.IsNullOrWhiteSpace(imageFieldTargetUrl))
            {
                return imageFieldTargetUrl;
            }
            return "#";
        }

        protected virtual string GetVideoThumbnailUrl(Item item)
        {
            if (!string.IsNullOrWhiteSpace(item.Fields[Templates.GalleryVideo.Fields.VideoThumbnail]?.Value))
            {
                return GetVideoThumbnailUrlFromVideoThumbnailField(item);
            }
            if (!string.IsNullOrWhiteSpace(item.Fields[Templates.GalleryVideo.Fields.VideoProvider]?.Value))
            {
                return GetVideoThumbnailUrlFromVideoProvider(item);
            }
            return null;
        }

        protected virtual string GetVideoThumbnailUrlFromVideoProvider(Item item)
        {
            string result = null;
            switch (item.Fields[Templates.GalleryVideo.Fields.VideoProvider].ToEnum<VideoProvider>())
            {
                case VideoProvider.YouTube:
                    {
                        Match match = Regex.Match(item[Templates.GalleryVideo.Fields.VideoID], "^(?:https?\\:\\/\\/)?(?:www\\.)?(?:youtu\\.be\\/|youtube\\.com\\/(?:embed\\/|v\\/|watch\\?v\\=))?([\\w-]{10,12})(?:$|\\&|\\?\\#).*");
                        if (match.Success)
                        {
                            result = $"https://img.youtube.com/vi/{match.Groups[1]}/hqdefault.jpg";
                        }
                        break;
                    }
                case VideoProvider.Dailymotion:
                    {
                        Match match = Regex.Match(item[Templates.GalleryVideo.Fields.VideoID], "^(?:https?\\:\\/\\/)?(?:www\\.)?(?:dailymotion\\.com\\/)?(?:video\\/([^_]+))?[^#]*(?:#video=([^_&]+))?");
                        for (int num = 2; num > 0; num--)
                        {
                            if (match.Success && match.Groups[num].Success)
                            {
                                result = $"https://www.dailymotion.com/thumbnail/video/{match.Groups[num].Value}";
                            }
                        }
                        break;
                    }
            }
            return result;
        }

        protected virtual string GetVideoThumbnailUrlFromVideoThumbnailField(Item item)
        {
            return MediaItemService.GetGeneralLinkFieldTargetUrl(item, Templates.GalleryVideo.Fields.VideoThumbnail);
        }

        protected virtual GalleryMediaItem CreateGalleryMediaItem(Item item)
        {
            GalleryMediaItem galleryMediaItem = new GalleryMediaItem
            {
                Item = item
            };
            if (item.InheritsFrom(Templates.GalleryVideo.ID))
            {
                galleryMediaItem.ThumbnailUrl = GetVideoThumbnailUrl(item);
            }
            galleryMediaItem.Href = delegate (Item dataSource, string linkFieldName)
            {
                string result = string.Empty;
                bool num = dataSource.InheritsFrom(Templates.GalleryImage.ID);
                bool flag = dataSource.InheritsFrom(Templates.GalleryVideo.ID);
                if (num)
                {
                    result = GetImageUrl(dataSource, Templates.GalleryImage.Fields.Image);
                }
                else if (flag)
                {
                    result = GetVideoUrl(dataSource);
                }
                return result;
            };
            return galleryMediaItem;
        }

        protected override bool ShouldShowMessage()
        {
            return base.IsControlEditable;
        }
    }
}