using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using NUnit.Framework;
using Synergy.Common.Security.Attributes;

namespace Synergy.Common.Security.Tests
{
    public class CheckPermissionAttributeTests
    {
        [Test]
        public void Attribute_EmptyPermissions_ThrowException()
        {
            Assert.Throws(typeof(ArgumentException), () =>
            {
                _ = new CheckPermissionAttribute(new string [0]);
            });

            Assert.Throws(typeof(ArgumentException), () =>
            {
                _ = new CheckPermissionAttribute(new [] { "" });
            });

            Assert.Throws(typeof(ArgumentException), () =>
            {
                _ = new CheckPermissionAttribute(new[] { " " });
            });

            Assert.Throws(typeof(ArgumentException), () =>
            {
                _ = new CheckPermissionAttribute(new[] { " ", "" });
            });
        }

        [Test]
        public void Attribute_WithOneMatchedPrmission_CanBeFound()
        {
            var context = this.CreateFakeContext(
                "Admin.CRMMailMerge.Read", "Admin.CRMMailMerge.Write",
                "CRM.MailMerge.Read", "CRM.MailMerge.Write");

            var attribute = new CheckPermissionAttribute(new[] { "Admin.CRMMailMerge.Read" });
            attribute.OnAuthorization(context);

            Assert.IsNull(context.Result);
        }

        [Test]
        public void Attribute_WithMoreThenOneMatchedPrmission_CanBeFound()
        {
            var context = this.CreateFakeContext(
                "Admin.CRMMailMerge.Read", "Admin.CRMMailMerge.Write",
                "CRM.MailMerge.Read", "CRM.MailMerge.Write");

            var attribute = new CheckPermissionAttribute(new[] { "CRM.MailMerge.Write", "Other" });
            attribute.OnAuthorization(context);

            Assert.IsNull(context.Result);
        }

        [Test]
        public void Attribute_WithWildcardAtTheEnd_CanBeFound()
        {
            var context = this.CreateFakeContext(
                "Admin.CRMMailMerge.Read", "Admin.CRMMailMerge.Write",
                "CRM.MailMerge.Read", "CRM.MailMerge.Write");

            var context2 = this.CreateFakeContext(
                "Admin.CRMMailMerge.Read", "Admin.CRMMailMerge.Write",
                "CRM.MailMerge.Read", "CRM.MailMerge.Write");

            var attribute = new CheckPermissionAttribute(new[] { "CRM.MailMerge.*" });
            attribute.OnAuthorization(context);

            var attribute2 = new CheckPermissionAttribute(new[] { "CRM.**" });
            attribute2.OnAuthorization(context2);

            Assert.IsNull(context.Result);
            Assert.IsNull(context2.Result);
        }

        [Test]
        public void Attribute_WithWildcardAtTheBeginning_CanBeFound()
        {
            var context = this.CreateFakeContext(
                "Admin.CRMMailMerge.Read", "Admin.CRMMailMerge.Write",
                "CRM.MailMerge.Read", "CRM.MailMerge.Write");

            var context2 = this.CreateFakeContext(
                "Admin.CRMMailMerge.Read", "Admin.CRMMailMerge.Write",
                "CRM.MailMerge.Read", "CRM.MailMerge.Write");

            var attribute = new CheckPermissionAttribute(new[] { "*.MailMerge.Read" });
            attribute.OnAuthorization(context);

            var attribute2 = new CheckPermissionAttribute(new[] { "**.Read" });
            attribute2.OnAuthorization(context2);

            Assert.IsNull(context.Result);
            Assert.IsNull(context2.Result);
        }

        [Test]
        public void Attribute_WithWildcardInTheMiddle_CanBeFound()
        {
            var context = this.CreateFakeContext(
                "Admin.CRMMailMerge.Read", "Admin.CRMMailMerge.Any.Write",
                "CRM.MailMerge.Read", "CRM.MailMerge.Write");

            var context2 = this.CreateFakeContext(
                "Admin.CRMMailMerge.Read", "Admin.CRMMailMerge.Any.Write",
                "CRM.MailMerge.Read", "CRM.MailMerge.Write");

            var attribute = new CheckPermissionAttribute(new[] { "Admin.*.Any.Write" });
            attribute.OnAuthorization(context);

            var attribute2 = new CheckPermissionAttribute(new[] { "Admin.**.Write" });
            attribute2.OnAuthorization(context2);

            Assert.IsNull(context.Result);
            Assert.IsNull(context2.Result);
        }

        [Test]
        public void Attribute_WithoutMatchingPermission_ReturnForbidden()
        {
            var context = this.CreateFakeContext(
                "Admin.CRMMailMerge.Read", "Admin.CRMMailMerge.Write",
                "CRM.MailMerge.Read", "CRM.MailMerge.Write");

            var context1 = this.CreateFakeContext(
                "Admin.CRMMailMerge.Read", "Admin.CRMMailMerge.Write",
                "CRM.MailMerge.Read", "CRM.MailMerge.Write");

            var context2 = this.CreateFakeContext(
                "Admin.CRMMailMerge.Read", "Admin.CRMMailMerge.Write",
                "CRM.MailMerge.Read", "CRM.MailMerge.Write");

            var attribute = new CheckPermissionAttribute(new[] { "Test" });
            attribute.OnAuthorization(context);

            var attribute1 = new CheckPermissionAttribute(new[] { "Admin.*" });
            attribute1.OnAuthorization(context1);

            var attribute2 = new CheckPermissionAttribute(new[] { "**.Test.**" });
            attribute2.OnAuthorization(context2);

            Assert.IsAssignableFrom(typeof(ForbidResult),  context.Result);
            Assert.IsAssignableFrom(typeof(ForbidResult),  context1.Result);
            Assert.IsAssignableFrom(typeof(ForbidResult),  context2.Result);
        }

        [Test]
        public void Attribute_NotAuthorizedUser_ReturnUnauthorized()
        {
            var context = this.CreateNotAuthorizedContext(
                "Admin.CRMMailMerge.Read", "Admin.CRMMailMerge.Write",
                "CRM.MailMerge.Read", "CRM.MailMerge.Write");

            var attribute = new CheckPermissionAttribute(new[] { "Test" });
            attribute.OnAuthorization(context);

            Assert.IsAssignableFrom(typeof(UnauthorizedResult), context.Result);
        }

        private AuthorizationFilterContext CreateFakeContext(params string[] permissions)
        {
            var identity = new ClaimsIdentity(new List<Claim>()
            {
                new Claim("Permissions", JsonConvert.SerializeObject(permissions)),
            }, "fake");
            var context = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new[] {identity})
            };

            return new AuthorizationFilterContext(new ActionContext(context, new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>())
            {
                HttpContext = context
            };

        }

        private AuthorizationFilterContext CreateNotAuthorizedContext(params string[] permissions)
        {
            var identity = new ClaimsIdentity(new List<Claim>()
            {
                new Claim("Permissions", JsonConvert.SerializeObject(permissions)),
            });
            var context = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new[] { identity })
            };

            return new AuthorizationFilterContext(new ActionContext(context, new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>())
            {
                HttpContext = context
            };

        }
    }
}