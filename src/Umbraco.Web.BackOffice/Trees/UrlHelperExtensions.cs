﻿using System;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core;
using Umbraco.Web.BackOffice.Trees;
using Umbraco.Web.WebApi;

namespace Umbraco.Extensions
{
    public static class UrlHelperExtensions
    {
        internal static string GetTreePathFromFilePath(this IUrlHelper urlHelper, string virtualPath, string basePath = "")
        {
            //This reuses the Logic from umbraco.cms.helpers.DeepLink class
            //to convert a filepath to a tree syncing path string.

            //removes the basepath from the path
            //and normalizes paths - / is used consistently between trees and editors
            basePath = basePath.TrimStart("~");
            virtualPath = virtualPath.TrimStart("~");
            virtualPath = virtualPath.Substring(basePath.Length);
            virtualPath = virtualPath.Replace('\\', '/');

            //-1 is the default root id for trees
            var sb = new StringBuilder("-1");

            //split the virtual path and iterate through it
            var pathPaths = virtualPath.Split('/');

            for (var p = 0; p < pathPaths.Length; p++)
            {
                var path = HttpUtility.UrlEncode(string.Join("/", pathPaths.Take(p + 1)));
                if (string.IsNullOrEmpty(path) == false)
                {
                    sb.Append(",");
                    sb.Append(path);
                }
            }
            return sb.ToString().TrimEnd(",");
        }

        public static string GetTreeUrl(this IUrlHelper urlHelper, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, Type treeType, string nodeId, FormCollection queryStrings)
        {
            var actionUrl = urlHelper.GetUmbracoApiService(umbracoApiControllerTypeCollection, "GetNodes", treeType)
                .EnsureEndsWith('?');

            //now we need to append the query strings
            actionUrl += "id=" + nodeId.EnsureEndsWith('&') + queryStrings.ToQueryString("id",
                //Always ignore the custom start node id when generating URLs for tree nodes since this is a custom once-only parameter
                // that should only ever be used when requesting a tree to render (root), not a tree node
                TreeQueryStringParameters.StartNodeId);
            return actionUrl;
        }

        public static string GetMenuUrl(this IUrlHelper urlHelper, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, Type treeType, string nodeId, FormCollection queryStrings)
        {
            var actionUrl = urlHelper.GetUmbracoApiService(umbracoApiControllerTypeCollection, "GetMenu", treeType)
                .EnsureEndsWith('?');

            //now we need to append the query strings
            actionUrl += "id=" + nodeId.EnsureEndsWith('&') + queryStrings.ToQueryString("id");
            return actionUrl;
        }
    }
}
