﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using SiteServer.CMS.Caches;
using SiteServer.CMS.Core;
using SiteServer.CMS.Database.Core;
using SiteServer.CMS.Database.Models;
using SiteServer.CMS.Plugin;
using SiteServer.CMS.Plugin.Impl;
using SiteServer.Plugin;
using SiteServer.Utils;

namespace SiteServer.API.Controllers.Pages.Settings
{
    [RoutePrefix("pages/settings/adminAccessTokens")]
    public class PagesAdminAccessTokensController : ApiController
    {
        private const string Route = "";

        [HttpGet, Route(Route)]
        public IHttpActionResult GetList()
        {
            try
            {
                var rest = Request.GetAuthenticatedRequest();
                if (!rest.IsAdminLoggin ||
                    !rest.AdminPermissions.HasSystemPermissions(ConfigManager.SettingsPermissions.Admin))
                {
                    return Unauthorized();
                }

                var adminNames = new List<string>();

                if (rest.AdminPermissions.IsSuperAdmin())
                {
                    adminNames = DataProvider.Administrator.GetUserNameList().ToList();
                }
                else
                {
                    adminNames.Add(rest.AdminName);
                }

                var scopes = new List<string>(AccessTokenManager.ScopeList);

                foreach (var service in PluginManager.Services)
                {
                    if (service.IsApiAuthorization)
                    {
                        scopes.Add(service.PluginId);
                    }
                }

                return Ok(new
                {
                    Value = DataProvider.AccessToken.GetAll(),
                    adminNames,
                    scopes,
                    rest.AdminName
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpDelete, Route(Route)]
        public IHttpActionResult Delete()
        {
            try
            {
                var rest = Request.GetAuthenticatedRequest();
                if (!rest.IsAdminLoggin ||
                    !rest.AdminPermissions.HasSystemPermissions(ConfigManager.SettingsPermissions.Admin))
                {
                    return Unauthorized();
                }

                var id = Request.GetQueryInt("id");
                DataProvider.AccessToken.Delete(id);

                return Ok(new
                {
                    Value = DataProvider.AccessToken.GetAll()
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route(Route)]
        public IHttpActionResult Submit([FromBody] AccessTokenInfo itemObj)
        {
            try
            {
                var rest = Request.GetAuthenticatedRequest();
                if (!rest.IsAdminLoggin ||
                    !rest.AdminPermissions.HasSystemPermissions(ConfigManager.SettingsPermissions.Admin))
                {
                    return Unauthorized();
                }

                if (itemObj.Id > 0)
                {
                    var tokenInfo = DataProvider.AccessToken.Get(itemObj.Id);

                    if (tokenInfo.Title != itemObj.Title && DataProvider.AccessToken.IsTitleExists(itemObj.Title))
                    {
                        return BadRequest("保存失败，已存在相同标题的API密钥！");
                    }

                    tokenInfo.Title = itemObj.Title;
                    tokenInfo.AdminName = itemObj.AdminName;
                    tokenInfo.Scopes = itemObj.Scopes;

                    DataProvider.AccessToken.Update(tokenInfo);

                    LogUtils.AddAdminLog(rest.AdminName, "修改API密钥", $"Access Token:{tokenInfo.Title}");
                }
                else
                {
                    if (DataProvider.AccessToken.IsTitleExists(itemObj.Title))
                    {
                        return BadRequest("保存失败，已存在相同标题的API密钥！");
                    }

                    var tokenInfo = new AccessTokenInfo
                    {
                        Title = itemObj.Title,
                        AdminName = itemObj.AdminName,
                        Scopes = itemObj.Scopes
                    };

                    DataProvider.AccessToken.Insert(tokenInfo);

                    LogUtils.AddAdminLog(rest.AdminName, "新增API密钥", $"Access Token:{tokenInfo.Title}");
                }

                return Ok(new
                {
                    Value = DataProvider.AccessToken.GetAll()
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
