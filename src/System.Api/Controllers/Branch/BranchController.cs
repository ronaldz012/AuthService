using System.Api.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Auth.Application.UseCases.Branches;
using Module.Auth.Application.UseCases.Branches.CreateBranch;
using Module.Auth.Application.UseCases.Branches.GetBranches;
using Module.Auth.Application.UseCases.Branches.GetBranchDetails;
using Module.Auth.Application.UseCases.Branches.UpdateBranch;

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
        public async Task<IActionResult> GetBranches([FromQuery] bool? isActive = true)
        {
            return await features.ListBranches.Execute(isActive).ToValueOrProblemDetails();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateBranch([FromRoute] Guid id, [FromBody] UpdateBranchRequest request)
        {
            return await features.UpdateBranch.Execute(id, request).ToValueOrProblemDetails();
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> ToggleBranchStatus([FromRoute] Guid id)
        {
            return await features.ToggleBranchStatus.Execute(id).ToValueOrProblemDetails();
        }

        [HttpGet("{id:guid}/details")]
        public async Task<IActionResult> GetBranchDetails([FromRoute] Guid id)
        {
            return await features.GetBranchDetails.Execute(id).ToValueOrProblemDetails();
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetBranchTypes()
        {
            return await features.GetBranchTypes.Execute().ToValueOrProblemDetails();
        }
    }
}
