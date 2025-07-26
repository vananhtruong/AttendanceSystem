using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Repository;
using BusinessObject.Models;
using BusinessObject.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : ODataController
    {
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepo, IMapper mapper)
        {
            _userRepo = userRepo;
            _mapper = mapper;
        }

        [EnableQuery]
        [HttpGet]
        public IActionResult Get()
        {
            var users = _userRepo.GetAllAsync().Result;
            var dtos = _mapper.Map<List<UserDTO>>(users);
            return Ok(dtos);
        }
    }
}