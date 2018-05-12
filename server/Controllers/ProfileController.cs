using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PodNoms.Api.Models;
using PodNoms.Api.Models.ViewModels;
using PodNoms.Api.Persistence;
using PodNoms.Api.Services.Auth;

namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("[controller]")]
    public class ProfileController : BaseAuthController {

        public IUnitOfWork _unitOfWork { get; }
        public IMapper _mapper { get; }
        private readonly IEntryRepository _entryRepository;

        public ProfileController(IMapper mapper, IUnitOfWork unitOfWork,
                    IEntryRepository entryRepository,
                UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor)
            : base(contextAccessor, userManager) {
            this._entryRepository = entryRepository;
            this._mapper = mapper;
            this._unitOfWork = unitOfWork;
        }
        [HttpGet]
        public ActionResult<ProfileViewModel> Get() {
            var result = _mapper.Map<ApplicationUser, ProfileViewModel>(_applicationUser);
            return new OkObjectResult(result);
        }

        [HttpPost]
        public async Task<ActionResult<ProfileViewModel>> Post([FromBody] ProfileViewModel item) {
            _applicationUser.Slug = item.Slug;
            _applicationUser.FirstName = item.FirstName;
            _applicationUser.LastName = item.LastName;
            await _userManager.UpdateAsync(_applicationUser);
            var ret = _mapper.Map<ApplicationUser, ProfileViewModel>(_applicationUser);
            return Ok(ret);
        }

        [HttpGet("checkslug/{slug}")]
        public async Task<IActionResult> CheckSlug(string slug) {
            var slugValid = await _userManager.CheckSlug(slug);

            if (slugValid)
                return NotFound();

            return Ok();
        }

        [HttpGet("limits")]
        public async Task<ActionResult<ProfileLimitsViewModel>> GetProfileLimits() {
            var entries = await _entryRepository.GetAllForUserAsync(_applicationUser.Id);
            var user = _mapper.Map<ApplicationUser, ProfileViewModel>(_applicationUser);
            var sum = entries.Select(x => x.AudioFileSize)
                .Sum();
            var vm = new ProfileLimitsViewModel {
                StorageQuota = 5368709120, //5Gb
                StorageUsed = sum,
                User = user
            };
            return Ok(vm);
        }
    }
}