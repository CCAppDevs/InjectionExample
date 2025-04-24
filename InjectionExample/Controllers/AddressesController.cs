using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InjectionExample.Data;
using InjectionExample.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace InjectionExample.Controllers
{
    [Authorize]
    public class AddressesController : Controller
    {
        private readonly IAuthorizationService _authService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public AddressesController(
            ApplicationDbContext context,
            IAuthorizationService authService,
            UserManager<IdentityUser> userManager,
            ILogger<Program> logger
        )
        {
            _context = context;
            _authService = authService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Addresses
        public async Task<IActionResult> Index()
        {
            // not using the authorization service yet

            var applicationDbContext = _context.Addresses
                .Where(a => a.UserId == _userManager.GetUserId(User));

            if (applicationDbContext == null)
            {
                return NotFound();
            }

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Addresses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var address = await _context.Addresses
                .FirstOrDefaultAsync(m => m.AddressId == id);

            if (address == null)
            {
                _logger.LogWarning("The address was not found in the database");
                return NotFound();
            }

            // needs to use the auth service to check ownership
            var authResult = await _authService.AuthorizeAsync(User, address, "OwnersOnly");

            if (authResult.Succeeded)
            {
                _logger.LogInformation("The user was authorized to access the address");
                return View(address);
            }
            else
            {
                // TODO: log failure here
                _logger.LogCritical($"The authorization result was denied for invalid credentials. {address.UserId} vs {User.Identity?.Name}");
                return new ForbidResult();
            }
        }

        // GET: Addresses/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = _userManager.GetUserId(User);
            return View();
        }

        // POST: Addresses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AddressId,StreetNumber,Street,City,State,Country,UserId")] Address address)
        {
            _logger.LogInformation("Entered the create function, pre userid lookup");
            // TODO: manually attach the user id to the address before validation

            if (address.UserId == null)
            {
                return new ChallengeResult();
            }

            if (ModelState.IsValid)
            {
                _context.Add(address);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(address);
        }

        // GET: Addresses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
            {
                return NotFound();
            }

            var authResult = await _authService.AuthorizeAsync(User, address, "OwnersOnly");

            if (authResult.Succeeded)
            {
                ViewData["UserId"] = _userManager.GetUserId(User);
                return View(address);
            }
            else
            {
                return new ForbidResult();
            }
                
        }

        // POST: Addresses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AddressId,StreetNumber,Street,City,State,Country,UserId")] Address address)
        {
            if (id != address.AddressId)
            {
                return NotFound();
            }

            var authResult = await _authService.AuthorizeAsync(User, address, "OwnersOnly");

            if (!authResult.Succeeded)
            {
                return new ForbidResult();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(address);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AddressExists(address.AddressId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", address.UserId);
            return View(address);
        }

        // GET: Addresses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var address = await _context.Addresses
                .FirstOrDefaultAsync(m => m.AddressId == id);
            if (address == null)
            {
                return NotFound();
            }

            var authResult = await _authService.AuthorizeAsync(User, address, "OwnersOnly");

            if (authResult.Succeeded)
            {
                return View(address);
            }
            else
            {
                return new ForbidResult();
            }
        }

        // POST: Addresses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var address = await _context.Addresses.FindAsync(id);
            
            if (address == null)
            {
                return NotFound();
            }

            var authResult = await _authService.AuthorizeAsync(User, address, "OwnersOnly");

            if (authResult.Succeeded)
            {
                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                _logger.LogCritical($"UserId: {User.Identity?.Name} was denied access to delete addressId: {address.AddressId}");
                return new ForbidResult();
            }
        }

        private bool AddressExists(int id)
        {
            return _context.Addresses.Any(e => e.AddressId == id);
        }
    }
}
