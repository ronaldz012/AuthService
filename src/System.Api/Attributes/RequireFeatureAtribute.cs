using System.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace System.Api.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireFeatureAttribute : TypeFilterAttribute
{
    public RequireFeatureAttribute(string route, string permission, bool multiBranch = false) 
        : base(typeof(RequireFeatureFilter))
    {
        Arguments = [route, permission, multiBranch];
    }
}