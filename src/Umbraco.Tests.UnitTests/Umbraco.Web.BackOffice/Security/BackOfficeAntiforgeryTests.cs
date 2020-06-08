﻿using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Web.BackOffice.Security;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.BackOffice.Security
{
    [TestFixture]
    public class BackOfficeAntiforgeryTests
    {
        private HttpContext GetHttpContext()
        {
            var httpContext = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new UmbracoBackOfficeIdentity(-1, "test", "test", Enumerable.Empty<int>(), Enumerable.Empty<int>(), "en-US",
                    Guid.NewGuid().ToString(), Enumerable.Empty<string>(), Enumerable.Empty<string>()))
            };
            httpContext.Request.IsHttps = true;
            return httpContext;
        }

        [Test]
        public async Task Validate_Tokens()
        {
            // This is the only way to get the DefaultAntiforgery service from aspnet
            var container = new ServiceCollection();
            container.AddLogging();
            container.AddAntiforgery();
            var services = container.BuildServiceProvider();

            var antiforgery = services.GetRequiredService<IAntiforgery>();
            var options = services.GetRequiredService<IOptions<AntiforgeryOptions>>();

            var httpContext = GetHttpContext();

            var backofficeAntiforgery = new BackOfficeAntiforgery(antiforgery, options);
            backofficeAntiforgery.GetTokens(httpContext, out var cookieToken, out var headerToken);
            Assert.IsFalse(cookieToken.IsNullOrWhiteSpace());
            Assert.IsFalse(headerToken.IsNullOrWhiteSpace());

            // the same context cannot validate since it's already created tokens
            httpContext = GetHttpContext();

            var result = await backofficeAntiforgery.ValidateRequestAsync(httpContext);

            Assert.IsFalse(result.Success); // missing token

            // add cookie and header
            httpContext.Request.Headers[HeaderNames.Cookie] = new CookieHeaderValue(Constants.Web.CsrfValidationCookieName, cookieToken).ToString();
            httpContext.Request.Headers[Constants.Web.AngularHeadername] = headerToken;

            result = await backofficeAntiforgery.ValidateRequestAsync(httpContext);
            Assert.IsTrue(result.Success); // missing token
        }
    }
}
