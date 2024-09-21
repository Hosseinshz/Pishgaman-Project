using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pishgaman_Project.Models;
using Pishgaman_Project.ViewModels;
using System.Security.Claims;

namespace Pishgaman_Project.Controllers
{
    //[Authorize]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly DBTestProject _dbContext;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;

        public UserController(ILogger<UserController> logger, DBTestProject dbContext, IEmailService emailService, ISmsService smsService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _emailService = emailService;
            _smsService = smsService;
        }
        public async Task<bool> CheckDuplicatePhoneNumber(string phoneNumber, int ID)
        {
            var existingUser = await _dbContext.UserInfo.FirstOrDefaultAsync(x => x.ID == ID);

            if (existingUser != null && existingUser.PhoneNumber == phoneNumber)
            {
                return true;
            }

            var duplicateUser = await _dbContext.UserInfo.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);

            if (duplicateUser == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public IActionResult Index(string searchFirstName, string searchLastName, string sortOrder, int pageSize = 25, int pageNumber = 1)
        {
            var users = _dbContext.UserInfo.AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(searchFirstName))
            {
                users = users.Where(u => u.FirstName.Contains(searchFirstName));
            }
            if (!string.IsNullOrEmpty(searchLastName))
            {
                users = users.Where(u => u.LastName.Contains(searchLastName));
            }

            // Sorting
            switch (sortOrder)
            {
                case "name_asc":
                    users = users.OrderBy(u => u.FirstName);
                    break;
                case "name_desc":
                    users = users.OrderByDescending(u => u.FirstName);
                    break;
                case "lastname_asc":
                    users = users.OrderBy(u => u.LastName);
                    break;
                case "lastname_desc":
                    users = users.OrderByDescending(u => u.LastName);
                    break;
                default:
                    users = users.OrderBy(u => u.FirstName); // Default sort
                    break;
            }

            // Count total records after filtering
            int totalRecords = users.Count();

            // Paging
            users = users.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            // Prepare ViewModel (or ViewBag)
            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.SortOrder = sortOrder;
            ViewBag.SearchFirstName = searchFirstName;
            ViewBag.SearchLastName = searchLastName;
            ViewBag.TotalRecords = totalRecords; // Added to show total records

            // Calculate start and end record for display
            int startRecord = (pageNumber - 1) * pageSize + 1;
            int endRecord = Math.Min(pageNumber * pageSize, totalRecords); // Explicitly cast to int

            ViewBag.StartRecord = startRecord;
            ViewBag.EndRecord = endRecord;

            return View(users.ToList());
        }
        [HttpGet]
        public JsonResult GetUserDetails(int id)
        {
            var user = _dbContext.UserInfo
                .Where(u => u.ID == id)
                .Select(u => new
                {
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    phoneNumber = u.PhoneNumber,
                    email = u.Email,
                    role = u.Role,
                    createDate = u.CreateDate
                })
                .FirstOrDefault();

            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            return Json(user);
        }

        public IActionResult AddUser()
        {
            var model = new UserInfoViewModel();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> AddUserConfirm(UserInfoViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Instantiate the password hasher
                var passwordHasher = new PasswordHasher<UserInfo>();

                // Retrieve the UserID of the currently logged-in user
                var registrarUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                int? registrarUserId = registrarUserIdClaim != null ? int.Parse(registrarUserIdClaim.Value) : (int?)null;

                // Create a new user
                var newUser = new UserInfo
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    CreateDate = DateTime.UtcNow,
                    Role = "Admin",
                    CreatedByUserID = registrarUserId 
                };

                // Hash the password
                newUser.Password = passwordHasher.HashPassword(newUser, model.Password);

                // Add and save the new user to the database
                _dbContext.UserInfo.Add(newUser);
                await _dbContext.SaveChangesAsync();

                // Send email and SMS notifications
                if (!string.IsNullOrWhiteSpace(newUser.Email))
                {
                    await _emailService.SendEmailAsync(newUser.Email, "Welcome!", "Your account has been created.");
                }
                await _smsService.SendSmsAsync(newUser.PhoneNumber, "Welcome! Your account has been created.");

                return RedirectToAction("Index");
            }

            return View("AddUser", model);
        }

        [HttpPost]
        public IActionResult DeleteUsers([FromBody] List<int> userIds)
        {
            if (userIds == null || !userIds.Any())
            {
                return BadRequest("No users selected for deletion.");
            }

            try
            {
                // Perform the deletion of the users from the database
                foreach (var id in userIds)
                {
                    var user = _dbContext.UserInfo.Find(id);
                    if (user != null)
                    {
                        _dbContext.UserInfo.Remove(user);
                    }
                }

                _dbContext.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting users.");
                TempData["msgType"] = "error";
                TempData["msg"] = "خطا در حذف کاربران";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            var user = _dbContext.UserInfo.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            _dbContext.UserInfo.Remove(user);
            _dbContext.SaveChanges();

            return Ok();
        }
        public IActionResult EditUser(int id)
        {
            var user = _dbContext.UserInfo.Find(id);
            if (user == null)
            {
                TempData["msgType"] = "error";
                TempData["msg"] = "کاربر یافت نشد";
                return RedirectToAction("Index");
            }
            else
            {
                var editModel = new UserEditViewModel
                {
                    ID = user.ID,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                };
                return View(editModel);
            }
        }
        public IActionResult EditUserConfirm(UserEditViewModel model)
        {
            var user = _dbContext.UserInfo.Find(model.ID);
            if (user == null)
            {
                TempData["msgType"] = "error";
                TempData["msg"] = "خطا در ویرایش";
                return RedirectToAction("EditUser", new { id = model.ID });
            }
            else
            {
                try
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Email = model.Email;
                    user.PhoneNumber = model.PhoneNumber;

                    _dbContext.UserInfo.Update(user);
                    _dbContext.SaveChanges();
                }
                catch (Exception ex)
                {

                    _logger.LogError(ex, "Error occurred while Editing User.");

                    TempData["msgType"] = "error";
                    TempData["msg"] = "خطا در ویرایش";
                    return RedirectToAction("EditUser", new { id = model.ID });
                }
            }
            return RedirectToAction("Index");
        }

        public IActionResult UserCreationReport(int pageSize = 25, int pageNumber = 1)
        {
            var groupedData = _dbContext.UserInfo
                .Where(u => u.CreatedByUserID.HasValue) 
                .Join(_dbContext.UserInfo,
                      userInfo => userInfo.CreatedByUserID,
                      user => user.ID,
                      (userInfo, user) => new { userInfo, user }) 
                .GroupBy(u => new { u.userInfo.CreateDate.Date, u.user.FirstName, u.user.LastName })
                .Select(g => new
                {
                    CreatedDate = g.Key.Date,
                    CreatedByFullName = g.Key.FirstName + " " + g.Key.LastName, 
                    UserCount = g.Count()
                })
                .ToList();

            var viewModel = groupedData.Select(g => new UserCreationReportViewModel
            {
                CreatedDate = g.CreatedDate,
                CreatedByFullName = g.CreatedByFullName,
                UserCount = g.UserCount
            }).ToList();

            int totalRecords = viewModel.Count;

            var pagedData = viewModel
                .OrderBy(g => g.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalRecords;

            return View(pagedData);
        }

    }
}
