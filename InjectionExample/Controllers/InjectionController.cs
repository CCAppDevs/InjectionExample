using InjectionExample.Data;
using InjectionExample.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace InjectionExample.Controllers
{
    [Authorize]
    public class InjectionController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<Program> _logger;
        private readonly ApplicationDbContext _context;

        public InjectionController(ILogger<Program> logger, UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = "0; DROP DATABASE; SELECT * FROM Addresses where userId = 0";

            var query = $"SELECT * FROM Addresses WHERE userId = 0; DROP DATABASE; SELECT * FROM Addresses where userId = 0 AND state = 'wa'";

            var result = await _context.Addresses.FromSqlRaw(query).ToListAsync();

            ViewData["UserId"] = _userManager.GetUserId(User);

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("AddressId,StreetNumber,Street,City,State,Country,UserId")] Address address)
        {
            return RedirectToAction(nameof(Index));
        }
    }
}
