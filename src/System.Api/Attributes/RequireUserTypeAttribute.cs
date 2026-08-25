using System.Api.Filters;
using Microsoft.AspNetCore.Mvc;
using Module.Auth.Domain;

namespace System.Api.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireUserTypeAttribute : TypeFilterAttribute
{
    public RequireUserTypeAttribute(UserType userType) : base(typeof(RequireUserTypeFilter))
    {
        Arguments = [(int)userType];
    }
}
