﻿using System.Threading.Tasks;

namespace SiteServer.Abstractions
{
    public partial interface IContentRepository
    {
        Task UpdateAsync(Site site, Channel channel, Content content);

        Task SetAutoPageContentToSiteAsync(Site site);

        Task UpdateArrangeTaxisAsync(Site site, Channel channel, string attributeName, bool isDesc);

        Task<bool> SetTaxisToUpAsync(Site site, Channel channel, int contentId, bool isTop);

        Task<bool> SetTaxisToDownAsync(Site site, Channel channel, int contentId, bool isTop);

        Task AddDownloadsAsync(string tableName, int channelId, int contentId);
    }
}