﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using SiteServer.CMS.Fx;
using SiteServer.CMS.Plugin.Impl;
using SiteServer.Utils;

namespace SiteServer.CMS.Core.RestRoutes.V1
{
    public class ApiContentsParameters
    {
        public ApiContentsParameters(HttpRequestMessage request)
        {
            ChannelIds = TranslateUtils.StringCollectionToIntList(request.GetQueryString("channelIds"));
            ChannelGroup = StringUtils.Trim(AttackUtils.FilterSql(request.GetQueryString("channelGroup")));
            ContentGroup = StringUtils.Trim(AttackUtils.FilterSql(request.GetQueryString("contentGroup")));
            Tag = StringUtils.Trim(AttackUtils.FilterSql(request.GetQueryString("tag")));
            Top = request.GetQueryInt("top", 20);
            Skip = request.GetQueryInt("skip");
            Likes = TranslateUtils.StringCollectionToStringList(StringUtils.Trim(AttackUtils.FilterSql(request.GetQueryString("like"))));
            OrderBy = StringUtils.Trim(AttackUtils.FilterSql(request.GetQueryString("orderBy")));

            //var queryDict = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            //queryDict.AddRange(request.QueryDict);
            QueryString = request.GetQueryDirectory();

            QueryString.Remove("siteId");
            QueryString.Remove("channelIds");
            QueryString.Remove("channelGroup");
            QueryString.Remove("contentGroup");
            QueryString.Remove("tag");
            QueryString.Remove("top");
            QueryString.Remove("skip");
            QueryString.Remove("like");
            QueryString.Remove("orderBy");
        }

        public List<int> ChannelIds { get; set; }

        public string ChannelGroup { get; set; }

        public string ContentGroup { get; set; }

        public string Tag { get; set; }

        public int Top { get; set; }

        public int Skip { get; set; }

        public List<string> Likes { get; set; }

        public string OrderBy { get; set; }

        public IDictionary<string, string> QueryString { get; set; }
    }
}
