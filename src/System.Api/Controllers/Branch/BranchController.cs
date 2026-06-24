using System.Api.Result;
using Common.Contracts.branches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Auth.Application.UseCases.Branches;
using Module.Auth.Application.UseCases.Branches.CreateBranch;
using Module.Auth.Application.UseCases.Branches.GetBranches;

namespace System.Api.Controllers.Branch
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Branch")]
    public class BranchController (BranchesUseCases features): ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateBranch([FromBody]CreateBranchRequest request)
        { 
            return await features.CreateBranch.Execute(request).ToValueOrProblemDetails();
        }

        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            return await features.ListBranches.Execute().ToValueOrProblemDetails();
        }
    }
}
